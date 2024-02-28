using Common.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIShopItem : MonoBehaviour, ISelectHandler
{
    //实现ISelectHandler选中处理接口的OnSelect方法，在UIShopItem的预制体上(即被选中的UI)，添加一个 Selectable(script)组件 ，实现选中功能
    //商店的商品道具
    public Image icon;
    public Text title;
    public Text price;
    public Text count;
    public Text limitClass; //限制职业

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    private bool selected;
    public bool Selected //判断是否选中当前道具
    {
        get { return selected; }
        set
        {
            selected = value;
            this.background.overrideSprite = selected ? selectedBg : normalBg;
        }
    }

    public int ShopItemID { get; set; }//商品道具的ID

    private UIShop shop; //决定属于哪个商店
    private ItemDefine item; //该商品道具的配置信息
    private ShopItemDefine ShopItem { get; set; }

    public void SetShopItem(int id, ShopItemDefine shopItem, UIShop shop)
    {
        this.shop = shop; //哪个商店的道具
        this.ShopItemID = id; //商品道具的ID
        this.ShopItem = shopItem; //商品的配置表 
        this.item = DataManager.Instance.Items[this.ShopItem.ItemID];//读出商品道具的配置表

        this.title.text = this.item.Name;//读取对应的道具信息，初始化商品

        this.count.text = "x" + ShopItem.Count.ToString();
        this.price.text = ShopItem.Price.ToString();
        if (this.item.LimitClass != SkillBridge.Message.CharacterClass.None)//有职业限制的道具，才显示职业限制
            this.limitClass.text = this.item.LimitClass.ToString();
        this.icon.overrideSprite = Resloader.Load<Sprite>(item.Icon);
    }



    public void OnSelect(BaseEventData eventData)//重写ISelectHandler的选择事件，简便实现出 点击选中功能
    {
        this.Selected = true; //将自己标记为 选中状态
        this.shop.SelectShopItem(this); //告诉商店，选择了自己
    }

    //private float lastClickTime = 0f;
    //private float currClickTime = 0f;
    //private bool clicked;
    //public void OnPointerClick(PointerEventData pointerEventData) //也可以用双击实现选中
    //{
    //    if (clicked == false)//还未点击过
    //    {
    //        clicked = true; //第一次点击设为true
    //        currClickTime = Time.realtimeSinceStartup; //记录第一次点击的时间
    //    }
    //    else if (currClickTime - lastClickTime < 0.3f) //点击过至少一次，若这次和上次的时间间隔<0.3s,满足双击判定
    //    {
    //        //执行双击逻辑
    //        this.Selected = true; //将自己标记为 选中状态
    //        this.shop.SelectShopItem(this); //告诉商店，选择了自己
    //    }
    //    lastClickTime = currClickTime; //迭代上次点击的时间
    //}
}
