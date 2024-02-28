
using System.Collections.Generic;
using Common.Data;
using UnityEngine;

namespace Managers
{
    class NPCManager : Singleton<NPCManager>
    {
        //事件 是NPC系统和其他系统之间 唯一的接口
        public delegate bool NpcActionHandler(NpcDefine npc);
        //功能型NPC 事件表
        Dictionary<NpcFunction, NpcActionHandler> eventMap = new Dictionary<NpcFunction, NpcActionHandler>();//使用eventMap[NpcFunction](NpcDefine);

        Dictionary<int, Vector3> npcPositions = new Dictionary<int, Vector3>(); //保持NPC的位置，用于寻路

        public void RegisterNpcEvent(NpcFunction function, NpcActionHandler action) // 注册功能型NPC事件表， NpcActionHandler action是该npc的职能事件
        {
            if (!eventMap.ContainsKey(function))
            {
                eventMap[function] = action;
            }
            else
                eventMap[function] += action;
        }

        public NpcDefine GetNpcDefine(int npcID)
        {
            NpcDefine npc = null;
            DataManager.Instance.Npcs.TryGetValue(npcID, out npc);//简洁 安全
            return npc;
        }

        public bool Interactive(int npcID)
        {
            if (DataManager.Instance.Npcs.ContainsKey(npcID))
            {
                var npc = DataManager.Instance.Npcs[npcID];
                return Interactive(npc);
            }
            return false;
        }

        public bool Interactive(NpcDefine npc)
        {
            if (DoTaskInteractive(npc))//和有任务的NPC，进行任务交互
            {
                return true;
            }
            else if (npc.Type == NpcType.Functional) //若是功能型NPC，打开商店之类
            {
                return DoFunctionInteractive(npc); //进行功能交互
            }
            return false;
        }

        private bool DoTaskInteractive(NpcDefine npc) //做任务交互，（只在此方法中，NPC系统 调用任务系统，此设计是为了 降低两系统之间的耦合度）
        {
            NpcQuestStatus status = QuestManager.Instance.GetQuestStatusByNpc(npc.ID);//获取NPC的任务状态管理器
            if (status == NpcQuestStatus.None) //若该npc无任何 任务状态，任务交互失败
                return false;

            return QuestManager.Instance.OpenNpcQuest(npc.ID);//打开NPC任务对话框
        }
        private bool DoFunctionInteractive(NpcDefine npc) //功能型交互，能打开商店、副本
        {
            if (npc.Type != NpcType.Functional)
                return false;
            if (!eventMap.ContainsKey(npc.Function))//功能型NPC事件表中 是否存在此npc
                return false;

            return eventMap[npc.Function](npc); //若存在，调用该Npc的职能事件
        }

        public void UpdateNpcPosition(int npcId, Vector3 pos) //收集NPC的位置，在NpcController中调用
        {
            this.npcPositions[npcId] = pos;//存入NPC位置表
        }
        public Vector3 GetNpcPositon(int npcId)
        {
            return this.npcPositions[npcId];
        }
    }
}
