
using System.Collections.Generic;
using System.Linq;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Services;
using Common.Data;


namespace GameServer.Managers
{
    class QuestManager //任务管理器也是跟随角色创建而创建的，不适合单例
    {
        Character Owner;
        public QuestManager(Character owner)
        {
            this.Owner = owner;
        }

        public void GetQuestInfos(List<NQuestInfo> list)
        {
            foreach (var quest in Owner.Data.Quests)
            {
                list.Add(GetQuestInfo(quest));
            }
        }

        public NQuestInfo GetQuestInfo(TcharacterQuest quest)//从TcharacterQuest 转换成NQuestInfo，即用数据库任务数据 构造网络任务数据
        {
            return new NQuestInfo()
            {
                QuestId = quest.QuestID,
                QuestGuid = quest.Id,
                Status = (QuestStatus)quest.Status,
                Targets = new int[3]
                {
                    quest.Target1,
                    quest.Target2,
                    quest.Target3,
                }
            };
        }

        public Result AcceptQuest(NetConnection<NetSession> sender, int questId)//服务器处理接受任务请求，若接受任务成功，要修改到数据库
        {
            Character character = sender.Session.Character; //获取当前角色信息
            QuestDefine quest;
            if (DataManager.Instance.Quests.TryGetValue(questId, out quest))//如果任务ID存在，读取该任务的配置表消息到quest
            {
                var dbquest = DBService.Instance.Entities.characterQuests.Create();//相当于new 一个TcharacterQuest实例
                dbquest.QuestID = questId;
                if (quest.Target1 == QuestTarget.None)//没有任务目标，直接完成（只由服务器来判断任务是否完成）
                {
                    dbquest.Status = (int)QuestStatus.Completed;
                }
                else//有任务目标
                {
                    dbquest.Status = (int)QuestStatus.InProgress;//进行中
                }
                DBService.Instance.Entities.characterQuests.Add(dbquest);
                sender.Session.Response.questAccept.Quest = GetQuestInfo(dbquest);//把DB转换成网络数据,返回给客户端
                character.Data.Quests.Add(dbquest);//把DB任务数据 添加到DB角色身上，（把任务插入数据库中）
                DBService.Instance.Save(); //并保存
                return Result.Success;
            }
            sender.Session.Response.questAccept.Errormsg = "配置表中任务不存在";
            return Result.Failed;
        }

        public Result SubmitQuest(NetConnection<NetSession> sender, int questId)//服务器处理提交任务请求
        {
            Character character = sender.Session.Character; //获取当前角色信息
            QuestDefine quest;
            if (DataManager.Instance.Quests.TryGetValue(questId, out quest))//如果任务ID存在，读取该任务的配置表消息
            {
                var dbquest = character.Data.Quests.Where(q => q.QuestID == questId).FirstOrDefault();//查询数据库中 是否已经接取过该任务
                if (dbquest != null)
                {
                    if (dbquest.Status != (int)QuestStatus.Completed)//任务还不是已完成、未提交状态
                    {
                        sender.Session.Response.questSubmit.Errormsg = "任务未完成";
                        return Result.Failed;//返回失败
                    }
                    dbquest.Status = (int)QuestStatus.Finished; //若任务是已完成、未提交状态，改为 已完成、已提交状态
                    sender.Session.Response.questSubmit.Quest = this.GetQuestInfo(dbquest);
                    DBService.Instance.Save();//先保存任务状态

                    //处理任务奖励
                    if (quest.RewardGold > 0)
                        character.Gold += quest.RewardGold;
                    //if (quest.RewardExp > 0)
                    //    character.Exp += quest.RewardExp;
                    if(quest.RewardItem1 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem1, quest.RewardItem1Count);
                    }
                    if (quest.RewardItem2 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem2, quest.RewardItem2Count);
                    }
                    if (quest.RewardItem3 > 0)
                    {
                        character.ItemManager.AddItem(quest.RewardItem3, quest.RewardItem3Count);
                    }
                    DBService.Instance.Save();//再保存任务奖励
                    return Result.Success;//返回成功
                }
                sender.Session.Response.questAccept.Errormsg = "数据库中任务不存在[2]";
                return Result.Failed;
            }
            sender.Session.Response.questAccept.Errormsg = "配置表中任务不存在[1]";
            return Result.Failed;
        }
    }
}