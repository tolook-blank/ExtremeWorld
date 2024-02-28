using SkillBridge.Message;

namespace Common.Data
{
    public enum NpcType
    {
        None = 0,
        Functional = 1,
        Task,
    }

    public enum NpcFunction
    {
        None = 0,
        InvokeShop = 1,     //打开商店
        InvokeInsrance = 2, //打开副本
    }

    public class NpcDefine
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Descript { get; set; }
        public NVector3 Position { get; set; }
        public NpcType Type { get; set; }  //发布任务NPC 、 功能类型
        public NpcFunction Function { get; set; } //打开商店、副本
        public int Param { get; set; } //NPC负责的商店ID

    }
}
