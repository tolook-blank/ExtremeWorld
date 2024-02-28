using SkillBridge.Message;

namespace Common.Data
{
    public class ShopItemDefine //商品 配置表
    {
        public int ItemID { get; set; } //道具ID
        public int Count { get; set; } //数量
        public int Price { get; set; } //价格
        public int Status { get; set; } //表示商品道具的状态： 1启用、 0禁用

    }
}
