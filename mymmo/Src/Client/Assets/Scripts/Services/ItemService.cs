using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Data;
using Network;
using SkillBridge.Message;
using Models;
using Managers;

namespace Services
{
    class ItemService : Singleton<ItemService>, IDisposable
    {

        public ItemService()
        {
            //客户端不需要从服务端拉取 商店道具列表、商店装备列表 ，因为客户端本地也有这些配置表，可以直接从本地读取
            MessageDistributer.Instance.Subscribe<ItemBuyResponse>(this.OnItemBuy);//订阅 道具购买的响应
            MessageDistributer.Instance.Subscribe<ItemEquipResponse>(this.OnItemEquip);//订阅 装备的穿、脱响应
        }


        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<ItemBuyResponse>(this.OnItemBuy);
            MessageDistributer.Instance.Unsubscribe<ItemEquipResponse>(this.OnItemEquip);
        }
        /*
        客户端发送道具购买请求协议，如果没有状态系统，服务端会返回三个网络协议：购买道具结果、增加道具状态变化 、减少金币状态变化
        而添加了状态系统后，接收服务器返回的一次协议statusNotify，就能接收其中的所有状态变化statusNotify.Status；
        但是服务端每次给客户端发消息都需要三次构建消息，构建完成后再打包发给客户端。每一次接收到客户端发来的消息，必定会构建新消息（网络协议）回去
        构建消息：NetMessage message = new NetMessage(); message.Response = new NetMessageResponse(); message.Response.XXX = new XXX(); message.Response.XXX.a = a;
        打包发送消息：byte[] data = PackageHandler.PackMessage(message); sender.SendData(data, 0, data.Length);

        因此还需要变更网络消息机制，为了在服务端实现一个请求来，一个请求回的类似HTTP机制，将任何消息 订阅接收到的时机 作为请求的开始 ，将任何消息send发给客户端时 作为请求的结束
        而在设计的网络协议中: NetMessageResponse 本身结构就满足服务端的这个需求，理论上可以一次response 整合它的所有协议发给客户端
        怎么整合？ 从 NetSession 入手，而NetSession和 NetConnection<NetSession> 密不可分， 因为sender就是NetConnection，
        所以将发送消息的过程 封装打包NetConnection中，void SendResponse(){ byte[] data = session.GetResponse(); SendData(data, 0, data.Length); }
        客户端每次登录会有一个唯一的 NetSession ，如果想要response消息共用，绑定在NetSession中最合适
         */

        public void SendBuyItem(int shopId, int shopItemId)
        {
            Debug.Log("SendBuyItem");

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.itemBuy = new ItemBuyRequest();
            message.Request.itemBuy.shopId = shopId;
            message.Request.itemBuy.shopItemId = shopItemId;
            NetClient.Instance.SendMessage(message);
        }

        private void OnItemBuy(object sender, ItemBuyResponse message)//道具购买 事件处理
        {
            MessageBox.Show("购买结果: " + message.Result + "\n" + message.Errormsg, "购买完成");
        }


        Item pendingEquip = null; //记录当前穿的哪件装备
        bool isEquip; //记录此次的 穿、脱操作
        public bool SendEquipItem(Item equip, bool isEquip) //发送装备穿脱脱请求 ，isEquip=true 穿装备
        {
            if (pendingEquip != null)
                return false;
            Debug.Log("SendEquipItem");

            pendingEquip = equip; //要穿脱的 装备
            this.isEquip = isEquip;

            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.itemEquip = new ItemEquipRequest();
            message.Request.itemEquip.Slot = (int)equip.EquipInfo.Slot;//装备槽位ID,什么部位的装备
            message.Request.itemEquip.itemId= equip.Id;  //装备id
            message.Request.itemEquip.isEquip = isEquip; //穿装备 或是 脱
            NetClient.Instance.SendMessage(message);
            return true;
        }


        private void OnItemEquip(object sender, ItemEquipResponse message)
        {
            if(message.Result == Result.Success) //成功穿/脱
            {
                if (pendingEquip != null)
                {
                    if (this.isEquip)
                        EquipManager.Instance.OnEquipItem(pendingEquip);
                    else
                        EquipManager.Instance.OnUnEquipItem(pendingEquip.EquipInfo.Slot);//传入装备槽位ID
                    pendingEquip = null;
                }
            }

        }

    }
}
