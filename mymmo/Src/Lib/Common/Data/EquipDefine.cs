using SkillBridge.Message;

namespace Common.Data
{
    public class EquipDefine
    {
        public int ID { get; set; } //装备ID
        public string Name { get; set; } //装备名
        public EquipSlot Slot { get; set; } //装备部位枚举值（装备槽位置），Weapon、Accessory...
        public string Category { get; set; } //装备职业限制，战士、法师、弓箭手

        public float STR { get; set; } //力量（一级属性）
        public float INT { get; set; } //智力（一级属性）
        public float DEX { get; set; } //敏捷（一级属性）
        public float MaxHP { get; set; } //最大生命值
        public float MaxMP { get; set; } //最大法力值
        public float AD { get; set; } //物理攻击（二级属性）
        public float AP { get; set; } //法术攻击（二级属性）
        public float DEF { get; set; } //物理防御（二级属性）
        public float MDEF { get; set; } //法防（二级属性）
        public float SPD { get; set; } //攻速（二级属性）
        public float CRI { get; set; } //暴击率（二级属性）

    }
}
