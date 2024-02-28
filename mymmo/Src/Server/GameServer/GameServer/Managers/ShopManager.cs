
using Common;
using Common.Data;
using SkillBridge.Message;
using Network;
using GameServer.Services;


namespace GameServer.Managers
{
    class ShopManager : Singleton<ShopManager> //服务器中ShopManager能做成单例，因为商店是不跟随角色的，不论角色创建与否，商店必须要先有
    {

        public Result BuyItem(NetConnection<NetSession> sender, int shopId, int shopItemId) //参数需要传入：NetConnection<NetSession> sender 买家网络连接
        {
            if (!DataManager.Instance.Shops.ContainsKey(shopId)) //安全校验,查看服务端ShopDefine配置表中 是否存在此商店ID？
            {
                return Result.Failed;
            }
            ShopItemDefine shopItem;
            if (DataManager.Instance.ShopItems[shopId].TryGetValue(shopItemId, out shopItem))//查看服务端ShopItemDefine中，shopId商店 是否存在 此shopItemId？
            {
                Log.InfoFormat("BuyItem: character:{0} Item:{1} Count:{2} Price:{3}", sender.Session.Character.Id, shopItem.ItemID, shopItem.Count, shopItem.Price);
                if (sender.Session.Character.Gold >= shopItem.Price)//卖家金币数是否足够？
                {
                    sender.Session.Character.ItemManager.AddItem(shopItem.ItemID, shopItem.Count); //校验通过后，调用ItemManager增加道具
                    sender.Session.Character.Gold -= shopItem.Price; //对金币赋值，触发状态管理器中 的金币变化

                    DBService.Instance.Save();
                    return Result.Success;
                }
            }
            return Result.Failed;
        }

    }
}
