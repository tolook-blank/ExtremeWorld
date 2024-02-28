using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SkillBridge.Message;
using Common.Data;

//Model和Entity的区别，Model 并不是 服务器与客户端之间进行数据传输，或是相关联的东西， model倾向于纯维护本地数据，不需要在游戏世界中与服务端同步
namespace Models
{
    public class Quest
    {
        public QuestDefine Define;  //本地 任务配置信息
        public NQuestInfo Info; //网络 任务配置信息 ,若任务还未被接取，该任务就不会存在网络信息

        public Quest()
        {

        }

        public Quest(NQuestInfo info)
        {
            this.Info = info;
            this.Define = DataManager.Instance.Quests[info.QuestId];
        }


        public Quest(QuestDefine define)
        {
            this.Define = define;
            this.Info = null;
        }

        public string GetTypeName()
        {
            return EnumUtil.GetEnumDescription(this.Define.Type);
        }
    }

}


