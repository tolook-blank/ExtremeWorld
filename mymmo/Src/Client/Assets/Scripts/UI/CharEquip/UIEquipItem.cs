
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Models;
using Managers;


public class UIEquipItem : MonoBehaviour, IPointerClickHandler
{ //IPointerClickHandler 指针点击处理器，通过检测UIEquipItem 上的鼠标点击 来穿脱装备
    //UIEquipItem的实现 借鉴了 UIShopItem
    public Image icon;
    public Text title;
    public Text level;       //装备等级，也即角色穿戴限制等级
    public Text limitClass;  //职业限制
    public Text limitCategory; //类别部位限制

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    private bool selected;
    public bool Selected
    {
        get { return selected; }
        set
        {
            selected = value;
            this.background.overrideSprite = selected ? selectedBg : normalBg; //实现高亮
        }
    }

    public int index { get; set; } //装备槽索引
    private UICharEquip owner; //装备面板主界面
    private Item item; //维护当前装备信息

    void Start()
    {

    }

    bool isEquiped = false; //此装备的穿戴状态

    //设置信息，参数：装备槽ID,装备Item,UICharEquip装备面板, 穿戴状态
    public void SetEquipItem(int idx, Item item, UICharEquip owner, bool equiped)
    {
        this.owner = owner;
        this.index = idx;
        this.item = item;
        this.isEquiped = equiped;

        if (this.title != null) this.title.text = this.item.Define.Name;
        if (this.level != null) this.level.text = this.item.Define.Level.ToString();
        if (this.limitClass != null) this.limitClass.text = item.Define.LimitClass.ToString(); //职业限制
        if (this.limitCategory != null) this.limitCategory.text = item.Define.Category; //装备类别
        if (this.icon != null) this.icon.overrideSprite = Resloader.Load<Sprite>(this.item.Define.Icon);
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    if (this.isEquiped) //点击已经装备的，脱下 
    //    {
    //        UnEquip();
    //    }
    //    else //点击未穿戴的(列表中的装备） （先选中，再次点击，才能穿上装备）
    //    {
    //        if(this.selected)
    //        {
    //            DoEquip();
    //            this.Selected = false; //装备完毕，取消当前装备高亮
    //        }
    //        else 
    //        {
    //            this.Selected = true; //高亮 当前选中装备 
    //        }
    //    }
    //}


    private static UIEquipItem selectedEquipment; //表示之前的选中装备，实现一次只能选中（高亮）一个单位， 只有当前选中的才高亮。

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this.isEquiped) //点击已经装备的
        {
            UnEquip();//脱下装备
        }
        else //点击未穿戴的(列表中的装备） （先选中，再次点击，才能穿上装备）
        {
            if (selectedEquipment == this) //如果连续两次选中当前装备
            {
                DoEquip();//穿上装备
                selectedEquipment = null; //将之前选中装备置空
                this.Selected = false; //穿上装备后，取消选中高亮
            }
            else //若还未选中过 或者 连续两次选中的不一致
            {
                if (selectedEquipment != null)//两次若选中的不是同一件装备
                {
                    selectedEquipment.Selected = false;// 取消之前选中的装备的高亮
                }
                selectedEquipment = this; //更新selectedEquipment
                this.Selected = true; //设为选中状态，并高亮当前选中装备 
            }
        }
    }

    //知识点：Lambda 运算符 =>，读“goes to”。Lambda 表达式，需要在  => 左侧输入参数（如果有），在另一侧 输入表达式或语句块 ， (输入参数) => {...}
    //OnYes 是无参无返回值 的UnityAction类型方法， 所以 它的lambda表达式 左侧没有参数，即 .OnYes = () => {...}
    private void DoEquip()
    {
        if (User.Instance.CurrentCharacter.Class != this.item.Define.LimitClass) //要穿戴的装备 与角色的职业不匹配，不可穿
        {
            MessageBox.Show(string.Format("要穿戴的装备[{0}]不匹配角色的职业", this.item.Define.Name), "穿戴失败", MessageBoxType.Error);
            return;
        }
        var msg = MessageBox.Show(string.Format("要穿上装备[{0}]吗？", this.item.Define.Name), "确认", MessageBoxType.Confirm);
        msg.OnYes = () =>
        {
            Item oldEquip = EquipManager.Instance.GetEquip(item.EquipInfo.Slot); //查看该装备槽中 是否已有装备
            if (oldEquip != null) //若已有，是否做装备替换
            {
                var newmsg = MessageBox.Show(string.Format("要替换掉[{0}]吗？", oldEquip.Define.Name), "确认", MessageBoxType.Confirm);
                newmsg.OnYes = () =>
                {
                    this.owner.DoEquip(this.item);//再穿上替换的装备this.item
                    this.owner.UnEquip(oldEquip); //先脱下oldEquip
                    
                };
            }
            else //若装备槽中本来没装备，直接穿戴
                this.owner.DoEquip(this.item);
        };
    }

    private void UnEquip()
    {
        var msg = MessageBox.Show(string.Format("要取下装备[{0}]吗？", this.item.Define.Name), "确认", MessageBoxType.Confirm);
        msg.OnYes = () =>
        {
            this.owner.UnEquip(this.item);
        };
    }
}

