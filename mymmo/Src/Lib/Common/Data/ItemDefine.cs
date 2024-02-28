using System.Collections.Generic;
using SkillBridge.Message;

namespace Common.Data
{
    public enum ItemFunction //道具功能
    {
        RecoverHP, 
        RecoverMP,
        AddBuff,
        AddExp,
        AddMoney,
        AddItem,
        AddSkillPoint,
    }

    public class ItemDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Descript { get; set; }
        //道具类型。为了方便能通过网络，将道具发送给客户端，在协议中添加道具类型字段。
        public ItemType Type { get; set; }   //道具类型： 普通、材料、任务道具、装备、坐骑
        public string Category { get; set; } //道具类别 ，药水、事物、 钱袋、技能书、经验书、(装备的武器、副手、...)等等
        public int Level { get; set; }  //使用等级限制
        public CharacterClass LimitClass { get; set; } //限制职业 ，战士WARRIOR 、法师WIZARD 、弓箭手ARCHER

        public bool CanUse { get; set; }
        public float UseCD { get; set; }
        public int Price { get; set; }
        public int SellPrice { get; set; }
        public int Stacklimit { get; set; } //背包中，道具格堆叠限制
        public string Icon { get; set; } //道具图标
        public ItemFunction Function { get; set; }
        public int Param { get; set; } //和道具相关参数，如：红药水的回复值500hp
        public List<int> Params { get; set; } //预留，如宝箱道具 可能会开出多种道具，需要多个参数

    }
}
