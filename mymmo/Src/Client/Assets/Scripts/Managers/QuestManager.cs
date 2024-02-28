
using System.Collections.Generic;
using System.Linq;
using Models;
using Services;
using SkillBridge.Message;
using UnityEngine.Events;

namespace Managers
{

    public enum NpcQuestStatus //npc身上的任务状态
    {
        None = 0, //该NPC身上无任务
        Complete,//拥有已完成，可提交任务
        Available,//拥有可接受任务
        Incomplete,//拥有进行中任务，即已领取，未完成
    }

    class QuestManager : Singleton<QuestManager>
    {
        //缓存 服务器返回的任务信息
        public List<NQuestInfo> questInfos;

        //管理所有可用任务
        public Dictionary<int, Quest> allQuests = new Dictionary<int, Quest>();//参数：任务id, 任务信息

        //管理每个NPC身上的 每个任务状态 对应的任务列表， 参数：NpcID，npc身上的任务状态，任务列表
        //NPC身上的三个任务列表：可接取任务、已完成任务、未完成任务列表 ， Dictionary<NpcQuestStatus, List<Quest>> 是 npc任务状态管理器
        public Dictionary<int, Dictionary<NpcQuestStatus, List<Quest>>> npcQuests = new Dictionary<int, Dictionary<NpcQuestStatus, List<Quest>>>();

        public UnityAction<Quest> onQuestStatusChanged;//任务状态变化 发布器

        public void Init(List<NQuestInfo> quests)
        {
            this.questInfos = quests; //缓存 服务器返回的任务信息（可领取、进行中）
            allQuests.Clear(); //先清空本地 所有可用任务
            this.npcQuests.Clear();
            InitQuests(); //初始化任务信息
        }

        void InitQuests()
        {
            foreach (var info in this.questInfos) //遍历服务器返回的任务信息列表（包括可领取、进行中任务）
            {
                Quest quest = new Quest(info); //初始化每个任务 
                this.allQuests[quest.Info.QuestId] = quest; //添加到 所有可用任务管理器
            }

            //再读取本地配置表中的任务，将 可接取任务 添加到可用任务管理器
            this.CheckAvailableQuests();

            foreach (var kv in this.allQuests) //遍历所有可用任务管理器
            {
                this.AddNpcQuest(kv.Value.Define.AcceptNPC, kv.Value); //将可接取任务 添加到 任务接取NPC的 Dictionary<NpcQuestStatus, List<Quest>>管理器上
                this.AddNpcQuest(kv.Value.Define.SubmitNPC, kv.Value); //把可提交任务 添加到 任务提交NPC的 Dictionary<NpcQuestStatus, List<Quest>>管理器上
            }

        }

        //将 可接取任务 添加到可用任务管理器
        void CheckAvailableQuests()
        {
            foreach (var kv in DataManager.Instance.Quests) //读取本地配置表中的任务，将 可接取任务 添加到可用任务管理器
            {
                if (kv.Value.LimitClass != CharacterClass.None && kv.Value.LimitClass != User.Instance.CurrentCharacter.Class) //职业限制不符合
                    continue;  //此任务不可接取，跳过，遍历配置表中的下一个任务

                if (kv.Value.LimitLevel > User.Instance.CurrentCharacter.Level) //不满足等级要求 
                    continue;

                if (this.allQuests.ContainsKey(kv.Key)) //已接受过该任务
                    continue;

                if (kv.Value.PreQuest > 0) //若当前任务 有前置任务要求 ，只要前置任务未完成，则不可领取此任务
                {
                    Quest preQuest; //保存 前置任务信息
                    if (this.allQuests.TryGetValue(kv.Value.PreQuest, out preQuest))//若进行中和可领取任务中 包括前置任务
                    {
                        if (preQuest.Info == null) //若前置任务未接取，此任务不可接取，跳过，遍历配置表中的下一个任务
                            continue;
                        if (preQuest.Info.Status != QuestStatus.Finished)//前置任务已领取，未完成，此任务不可接取
                            continue;
                    }
                    else //若进行中和可领取任务中 不包括前置任务，此任务不可接取
                        continue;
                }
                Quest quest = new Quest(kv.Value);//若当前任务可领取
                this.allQuests[quest.Define.ID] = quest; //将任务添加到 可用任务管理器
            }
        }

        //将任务quest ，添加到 任务NPC的 Dictionary<NpcQuestStatus, List<Quest>>管理器上
        private void AddNpcQuest(int npcid, Quest quest)
        {
            if (!this.npcQuests.ContainsKey(npcid)) //若该npc 还未添加到 总Npc任务状态管理器中
                this.npcQuests[npcid] = new Dictionary<NpcQuestStatus, List<Quest>>();//创建 该npc 的任务状态管理器

            //NPC身上的三个列表：可接取任务、已完成任务、未完成任务列表
            List<Quest> availables;
            List<Quest> completes;
            List<Quest> incompletes;

            if (!this.npcQuests[npcid].TryGetValue(NpcQuestStatus.Available, out availables)) //若该npc身上不存在 可接取任务列表，创建
            {
                availables = new List<Quest>();
                this.npcQuests[npcid][NpcQuestStatus.Available] = availables; //availables 可接任务列表
            }
            if (!this.npcQuests[npcid].TryGetValue(NpcQuestStatus.Complete, out completes))//若该npc身上不存在 已完成任务列表，创建
            {
                completes = new List<Quest>();
                this.npcQuests[npcid][NpcQuestStatus.Complete] = completes; //completes 已完成任务列表
            }
            if (!this.npcQuests[npcid].TryGetValue(NpcQuestStatus.Incomplete, out incompletes)) //若该npc身上不存在 进行中任务列表，创建
            {
                incompletes = new List<Quest>();
                this.npcQuests[npcid][NpcQuestStatus.Incomplete] = incompletes; //Incompletes 进行中任务列表
            }

            //若任务的网络信息为空，表示还未接取过该任务
            if (quest.Info == null) 
            {
                if (npcid == quest.Define.AcceptNPC && !this.npcQuests[npcid][NpcQuestStatus.Available].Contains(quest))//若该npc是此任务的接取NPC，且该NPC的可接任务列表中没有该任务
                    this.npcQuests[npcid][NpcQuestStatus.Available].Add(quest);//添加到npcid 的 可接任务列表
            }
            else //已经接取过的任务
            {
                if (quest.Define.SubmitNPC == npcid && quest.Info.Status == QuestStatus.Completed) //若该npc是此任务的提交NPC，且任务是已完成、但未提交状态
                {
                    if (!this.npcQuests[npcid][NpcQuestStatus.Complete].Contains(quest)) //如果 该NPC的Complete任务列表中 没有该任务
                        this.npcQuests[npcid][NpcQuestStatus.Complete].Add(quest);//添加到npcid 的 完成任务列表
                }
                if (quest.Define.SubmitNPC == npcid && quest.Info.Status == QuestStatus.InProgress)//若该npc是此任务的提交NPC，且任务是进行中状态
                {
                    if (!this.npcQuests[npcid][NpcQuestStatus.Incomplete].Contains(quest)) //如果 该NPC的Incomplete任务列表中 没有该任务
                        this.npcQuests[npcid][NpcQuestStatus.Incomplete].Add(quest);//添加到npcid 的 进行中任务列表
                }
            }

        }


        //获取NPC的任务状态管理器（NPCManager 的DoTaskInteractive方法 调用 QuestManager.GetQuestStatusByNpc接口，获取NPC任务状态）
        public NpcQuestStatus GetQuestStatusByNpc(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();
            if (this.npcQuests.TryGetValue(npcId, out status)) //获取NPC身上的任务状态管理器（NPC头顶小图标- UIQuestStatus）
            {
                if (status[NpcQuestStatus.Complete].Count > 0) //先查看该NPC的Complete任务列表，优先显示已完成任务（黄色问号）
                    return NpcQuestStatus.Complete;
                if (status[NpcQuestStatus.Available].Count > 0)//再查看是否有可接取的任务（黄色感叹号）
                    return NpcQuestStatus.Available;
                if (status[NpcQuestStatus.Incomplete].Count > 0)//最后查看是否有未完成的任务（灰色问号）
                    return NpcQuestStatus.Incomplete;
            }
            return NpcQuestStatus.None; //该NPC身上没有任务
        }

        //打开NPC的任务对话框（NPCManager 的DoTaskInteractive方法 调用 QuestManager.OpenNpcQuest接口，打开任务对话框）
        public bool OpenNpcQuest(int npcId)
        {
            Dictionary<NpcQuestStatus, List<Quest>> status = new Dictionary<NpcQuestStatus, List<Quest>>();
            if (this.npcQuests.TryGetValue(npcId, out status)) //获取NPC任务
            {
                if (status[NpcQuestStatus.Complete].Count > 0) //先查看该NPC的Complete任务列表，优先显示已完成任务（黄色问号）
                    return ShowQuestDialog(status[NpcQuestStatus.Complete].First()); //显示Complete任务列表中 第一条 可提交的任务对话
                if (status[NpcQuestStatus.Available].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Available].First());//显示 拥有可接受任务对话
                if (status[NpcQuestStatus.Incomplete].Count > 0)
                    return ShowQuestDialog(status[NpcQuestStatus.Incomplete].First());//显示 拥有未完成任务对话
            }
            return false;
        }

        //显示任务对话框
        private bool ShowQuestDialog(Quest quest)
        {
            if (quest.Info == null || quest.Info.Status == QuestStatus.Completed) //此任务 可接取 或 任务已完成，则有对话框显示
            {
                UIQuestDialog dlg = UIManager.Instance.Show<UIQuestDialog>(); //创建任务对话框
                //先关联 任务框关闭事件,（若先setQuest(quest)，未知BUG导致后续代码执行不到，dlg.OnClose收不到关闭响应，就无法触发OnQuestDialogClose，也就无法SendQuestAccept)
                dlg.OnClose += OnQuestDialogClose; 
                dlg.SetQuest(quest); //再设置任务信息
                return true;
            }
            if (quest.Info != null || quest.Info.Status == QuestStatus.InProgress) //进行中任务 不弹出对话框
            {
                if (!string.IsNullOrEmpty(quest.Define.DialogIncomplete))
                    MessageBox.Show(quest.Define.DialogIncomplete); //用弹窗消息 显示任务进行中对话
            }
            return false;
        }

        private void OnQuestDialogClose(UIWindow sender, UIWindow.WindowResult result) //处理任务框 关闭事件
        {
            UIQuestDialog dlg = (UIQuestDialog)sender;
            if (result == UIWindow.WindowResult.Yes) //接受任务、完成任务 按钮都是WindowResult.Yes
            {
                if (dlg.quest.Info == null) //该对话框中是 新任务，对应接受任务按钮
                    QuestService.Instance.SendQuestAccept(dlg.quest); //发送接受任务请求

                else if (dlg.quest.Info.Status == QuestStatus.Completed) //该任务已完成，对应提交任务按钮
                    QuestService.Instance.SendQuestSubmit(dlg.quest);//发送提交任务请求
            }
            else if (result == UIWindow.WindowResult.NO) //拒绝任务按钮
            {
                MessageBox.Show(dlg.quest.Define.DialogDeny);//直接显示拒绝任务对话
            }
        }

        Quest RefreshQuestStatus(NQuestInfo quest)//刷新任务状态，把服务器发回的任务状态消息，同步到本地
        {
            this.npcQuests.Clear();//先清空
            Quest result; //保存更新后的任务信息
            if (this.allQuests.ContainsKey(quest.QuestId))//如果可用任务管理器中 存在该任务
            {
                //更新allQuests中 此任务的状态
                this.allQuests[quest.QuestId].Info = quest;
                result = this.allQuests[quest.QuestId];
            }
            else
            {
                result = new Quest(quest);//若不存在，添加进allQuests
                this.allQuests[quest.QuestId] = result;
            }
            //再将 可接取任务 添加到可用任务管理器
            CheckAvailableQuests();
            foreach(var kv in this.allQuests)
            {
                this.AddNpcQuest(kv.Value.Define.AcceptNPC, kv.Value);
                this.AddNpcQuest(kv.Value.Define.SubmitNPC, kv.Value);
            }
            //任务状态通知
            if(onQuestStatusChanged != null)
            {
                onQuestStatusChanged(result);
            }
            return result;
        }

        public void OnQuestAccepted(NQuestInfo info)//处理接受任务响应
        {
            var quest = this.RefreshQuestStatus(info);//刷新任务状态，把服务器发回的任务状态消息，同步到本地
            MessageBox.Show(quest.Define.DialogAccept);//显示接受任务对话
        }

        public void OnQuestSubmited(NQuestInfo info)//处理提交任务响应
        {
            var quest = this.RefreshQuestStatus(info);//刷新任务状态
            MessageBox.Show(quest.Define.DialogFinish);//显示任务完成对话
        }

    }
}
