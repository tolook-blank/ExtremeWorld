using Common.Data;
using Managers;
using Models;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : UIWindow
{
    //设计细节：购买商店道具后，玩家金币会减少，玩家的背包道具增加 ，这个状态变化的通知逻辑 不是直接在商店、道具系统中添加，而是通过一个状态系统完成，简化逻辑。
    public Text title;//商店名
    public Text money;//玩家的金币数，

    public GameObject shopItemPrefab;//商品(prefab)
    ShopDefine shop;
    public Transform[] itemRoot;//商品页

    void Start()
    {
        StartCoroutine(InitItems());
    }
    //客户端不需要从服务端拉取 商店道具列表、商店装备列表 ，因为客户端本地也有这些配置表，可以直接从本地读取 DataManager.Instance.ShopItems[shop.ID]
    //玩家们看到的商店配置是一样的，不是动态的，所以可以由配置表来决定 ； 而玩家背包里的道具 必须存放在数据库，从服务器中拉取
    IEnumerator InitItems() //初始化商品道具
    {
        int count = 0; //商品数量
        int page = 0;  //分页功能，每页16个商品
        foreach (var kv in DataManager.Instance.ShopItems[shop.ID])//遍历商店中的 商品
        {
            if (kv.Value.Status > 0) //若商品是启用状态
            {
                GameObject go = Instantiate(shopItemPrefab, itemRoot[page]);//实例化prefab，创建商品列表项
                UIShopItem ui = go.GetComponent<UIShopItem>();//获取商品对象
                ui.SetShopItem(kv.Key, kv.Value, this);//初始化商品
                count++; 
                if (count >= 16) //当一页商品数量 >=16，分页
                {
                    count = 0;
                    page++;
                    itemRoot[page].gameObject.SetActive(true);
                }
            }
        }
        yield return null;
    }

    public void SetShop(ShopDefine shop) //点击NPC，打开商店UI界面，转到SetShop函数 设置商店信息
    {
        this.shop = shop;
        this.title.text = shop.Name; //商店名
        this.money.text = User.Instance.CurrentCharacter.Gold.ToString();
    }

    private UIShopItem selectedItem;//存储选中的商品信息
    public void SelectShopItem(UIShopItem item) //item是当前选择商品
    {
        if (selectedItem != null)//若已经选中过一个商品
        {
            selectedItem.Selected = false;//再点击，则取消该商品的选中
        }
        selectedItem = item;//更改为选中当前商品
    }

    public void OnClickBuy()
    {
        if (this.selectedItem == null)
        {
            MessageBox.Show("请选择要购买的道具", "购买提示");
            return;
        }
        //购买道具，需要发送 商店ID 和 购买的商品ID， 调用流程：UIShop.OnClickBuy()->ShopManager.BuyItem()->ItemService.SendBuyItem()发送消息给服务器
        if (!ShopManager.Instance.BuyItem(this.shop.ID, this.selectedItem.ShopItemID))
        {
            //若购买失败后续可以添加 弹出提示
        }
        SoundManager.Instance.PlaySound(SoundDefine.SFX_Shop_Purchase);
        UpdateMoney();
    }

    public void UpdateMoney()//刷新金币数量，优化体验：当购买完道具后，已经收到了金币状态通知 ，但是金币文本数量未即使更新，需要重新打开界面才刷新
    {
        this.money.text = User.Instance.CurrentCharacter.Gold.ToString();
    }
}
