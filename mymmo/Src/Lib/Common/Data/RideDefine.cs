using SkillBridge.Message;

namespace Common.Data
{
    public class RideDefine //坐骑配置表
    {
        public int ID { get; set; } 
        public string Name { get; set; }
        public string Descript { get; set; }
        public int Level { get; set; }
        public CharacterClass LimitClass { get; set; } //职业限制，后续开发预留字段
        public string Icon { get; set; } //坐骑图标
        public string Resource { get; set; }

    }
}
