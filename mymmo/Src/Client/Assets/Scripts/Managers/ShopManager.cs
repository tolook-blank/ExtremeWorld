using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Data;
using Services;
using System;

namespace Managers
{

    class ShopManager : Singleton<ShopManager>
    {
        public void Init()
        {
            NPCManager.Instance.RegisterNpcEvent(NpcFunction.InvokeShop, OnOpenShop);//商店系统 结合 NPC系统 ，通过与NPC交互来打开商店
        }

        private bool OnOpenShop(NpcDefine npc)
        {
            this.ShowShop(npc.Param);//npc.Param是 该npc关联的商店id
            return true;
        }

        public void ShowShop(int shopId) //打开商店界面，参数：商店id
        {
            ShopDefine shop;
            if (DataManager.Instance.Shops.TryGetValue(shopId, out shop))//查询该商店的配置信息
            {
                UIShop uiShop = UIManager.Instance.Show<UIShop>();//调用UIManager打开商店UI界面
                if (uiShop != null)
                {
                    uiShop.SetShop(shop); //设置商店信息
                }
            }
            else
            {
                MessageBox.Show("商店不存在", "打开商店失败");
            }
        }

        public bool BuyItem(int shopId, int shopItemId)//商店ID 和 购买的商品ID
        {
            ItemService.Instance.SendBuyItem(shopId, shopItemId);//道具购买请求
            return true;
        }
    }

}