using Common;
using GameServer.Entities;
using Network;
using SkillBridge.Message;
using GameServer.Managers;


namespace GameServer.Services
{//服务端，只用维护背包数据，需要一个BagService.  登陆创建角色时，把数据从数据库中取出来，给角色初始化 ；当收到客户端的保存请求时，把数据从网络上保存到服务器中
    class ItemService : Singleton<ItemService>
    {
        public ItemService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemBuyRequest>(this.OnItemBuy);//订阅 客户端的购买道具请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ItemEquipRequest>(this.OnItemEquip);//订阅 客户端的装备穿、脱请求
        }

        //处理道具购买
        private void OnItemBuy(NetConnection<NetSession> sender, ItemBuyRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnItemBuy: character:{0} Shop:{1} ShopItem:{2}", character.Id, request.shopId, request.shopItemId);
            var result = ShopManager.Instance.BuyItem(sender, request.shopId, request.shopItemId);//调用 ShopManager管理购买商店道具
            //sender.Session.Response 是响应消息整合包NetMessageResponse，只需new一个ItemBuyResponse()就能完成构建道具购买响应
            sender.Session.Response.itemBuy = new ItemBuyResponse();
            sender.Session.Response.itemBuy.Result = result;
            sender.SendResponse();
        }

        private void OnItemEquip(NetConnection<NetSession> sender, ItemEquipRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnItemEquip: character:{0} Slot:{1} Item:{2} Equip:{3}", character.Id, request.Slot, request.itemId, request.isEquip);
            var result = EquipManager.Instance.EquipItem(sender, request.Slot, request.itemId, request.isEquip);
            sender.Session.Response.itemEquip = new ItemEquipResponse();
            sender.Session.Response.itemEquip.Result = result;
            sender.SendResponse();
        }

        public void Init()
        {

        }


    }
}
