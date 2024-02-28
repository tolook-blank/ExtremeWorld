using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Data;
using Models;
using Services;
using SkillBridge.Message;
using UnityEngine;

namespace Managers
{
    //当存在道具用完后的空格子占用 ，占用的道具格看起来不连续时，使用 一键整理功能解决；当背包解锁的格子不够用时，可以自动扩容；可以有上限的 一次添加、删除多个道具
    public class ItemManager : Singleton<ItemManager> //客户端中可以用单例，因为每个玩家各有自己的道具管理器
    {
        public Dictionary<int, Item> Items = new Dictionary<int, Item>(); //ID作为Key,Item 为 Value

        public void Init(List<NItemInfo> NItems)//在OnGameEnter中初始化
        {
            this.Items.Clear();
            foreach (var info in NItems)
            {
                Item item = new Item(info);
                this.Items.Add(item.Id, item);

                Debug.LogFormat("ItemManager：Init [{0}]", item);//显示每个道具的日志
            }
            StatusService.Instance.RegisterStatusNotify(StatusType.Item, OnItemNotify);//道具管理器 注册 道具状态变化通知
        }

        public ItemDefine GetItem(int itemID)//客户端没有权限删除、增加道具
        {
            ItemDefine itemDefine = null;
            DataManager.Instance.Items.TryGetValue(itemID, out itemDefine);
            return itemDefine;
        }

        private bool OnItemNotify(NStatus status) //处理 状态管理器的通知
        {
            if (status.Action == StatusAction.Add)
            {
                this.AddItem(status.Id, status.Value);
            }
            if (status.Action == StatusAction.Delete)
            {
                this.RemoveItem(status.Id, status.Value);
            }
            return true;
        }

        private void AddItem(int itemId, int count)//道具管理器中 增加道具
        {
            Item item = null;
            ushort limit = (ushort)DataManager.Instance.Items[itemId].Stacklimit;//该道具叠加限制
            if (count > limit * 5) return; //一次最多添加装满5个格子的道具
            if (this.Items.TryGetValue(itemId, out item))//若已存在，只用增加数量
            {
                item.Count += count;
            }
            else
            {
                item = new Item(itemId, count);//若道具管理器中不存在此道具，则创建new
                this.Items.Add(itemId, item); //再添加到道具管理器
            }
            BagManager.Instance.AddItem(itemId, count); //道具系统更新后，同时更新到背包
        }


        private void RemoveItem(int itemId, int count)//删除道具
        {
            if (!this.Items.ContainsKey(itemId))//若不存在，删除失败返回
            {
                return;
            }
            Item item = this.Items[itemId]; //若存在，再判断够不够删除
            if (item.Count < count) { return; }//数量不足，删除失败
            item.Count -= count; //只需减少数量

            BagManager.Instance.RemoveItem(itemId, count);//再更新到背包
        }

        public bool UseItem(int itemID)
        {
            return false;
        }

        public bool UseItem(ItemDefine item)
        {
            return false;
        }

    }
}
