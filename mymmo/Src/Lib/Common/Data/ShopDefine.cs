
using SkillBridge.Message;

namespace Common.Data
{
    public class ShopDefine //商店配置表
    {
        public int ID { get; set; } //商店ID
        public string Name { get; set; }
        public string Icon { get; set; } //商店图标
        public string Descript { get; set; }
        public int Status { get; set; }

    }
}
