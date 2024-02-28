
using System.ComponentModel;
using SkillBridge.Message;

namespace Common.Data
{
    public enum QuestType
    {
        [Description("主线")]
        Main,
        [Description("支线")]
        Branch
    }

    public enum QuestTarget
    {
        None, //对话任务
        Kill, //杀怪任务
        Item, //道具任务
    }


    public class QuestDefine //任务系统配置表
    {
        public int ID { get; set; } //任务ID
        public string Name { get; set; } //任务名称

        public int LimitLevel { get; set; }//任务等级
        public CharacterClass LimitClass { get; set; }//职业限制
        public int PreQuest { get; set; } //前置任务
        public QuestType Type { get; set; } //任务类型：主线、支线...
        public int AcceptNPC { get; set; }//任务接取NPC
        public int SubmitNPC { get; set; }//任务提交NPC
        public string Overview { get; set; } //任务概述
        public string Dialog { get; set; } //任务对话
        public string DialogAccept { get; set; } //任务接受对话
        public string DialogDeny { get; set; } //任务拒绝对话
        public string DialogIncomplete { get; set; } //任务未完成对话
        public string DialogFinish { get; set; } //任务完成对话

        public QuestTarget Target1 { get; set; } //任务目标1，可以是 对话、杀怪、道具
        public int Target1ID { get; set; } //目标1的ID
        public int Target1NUM { get; set; }//目标1的数量

        public QuestTarget Target2 { get; set; }
        public int Target2ID { get; set; }
        public int Target2NUM { get; set; }

        public QuestTarget Target3 { get; set; }
        public int Target3ID { get; set; }
        public int Target3NUM { get; set; }

        public int RewardGold { get; set; }
        public int RewardExp { get; set; }
        public int RewardItem1 { get; set; }//奖励道具1的ID
        public int RewardItem1Count { get; set; }//奖励道具1的数量
        public int RewardItem2 { get; set; }
        public int RewardItem2Count { get; set; }
        public int RewardItem3 { get; set; }
        public int RewardItem3Count { get; set; }

    }
}
