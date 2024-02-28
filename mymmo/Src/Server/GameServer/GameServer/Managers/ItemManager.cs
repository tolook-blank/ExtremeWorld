
using System.Collections.Generic;
using Common;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Models;

namespace GameServer.Managers
{
    class ItemManager //服务器中ItemManager不能做成单例，因为道具是跟随角色的，随角色的创建而创建
    {
        Character Owner; //道具拥有者， 每个Character 的ItemManager不同

        //注意：为了避免多次从DB中增删字段，数量为0的道具也不会从字典中删除，当再次获得道具时，只需更新一次DB,优化性能。（DB中执行速度 Update > insert >delete >select）
        public Dictionary<int, Item> Items = new Dictionary<int, Item>();//管理角色身上的所有道具，(道具ID,道具)

        public ItemManager(Character owner) //构造时，传入角色owner，表示 此ItemManager管理owner的道具
        {
            this.Owner = owner; //传入Character道具拥有者

            foreach (var item in owner.Data.Items) //获取角色owner的DB所有道具，添加到道具管理器中
            {
                this.Items.Add(item.ItemID, new Item(item));
            }
        }

        public bool UseItem(int itemId, int count = 1) //使用道具的id，使用数量(默认一次使用一 个)
        {
            Log.InfoFormat("[{0}] UseItem [{1}:{2}]", this.Owner.Data.ID, itemId, count);
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item)) //若角色身上存在该道具。注意：TryGetValue只执行一次查询，就能判断是否存在，且能获得该值，也不会报错
            {
                if (item.Count < count) //若该道具的数量 不够
                {
                    return false; //则使用失败
                }
                //TODO:增加使用逻辑

                item.Remove(count); //足够，则减去消耗的道具数量。 即使道具数量为0了，也没必要从道具字典中删除，能减少DB增删字段的频率，提升性能
                return true;//使用成功
            }
            return false; //若角色身上不存在此道具，使用失败
        }

        public bool HasItem(int itemId)//是否拥有该道具
        {
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item))
            {
                return item.Count > 0; 
            }
            return false;
            //return this.Items.TryGetValue(itemId, out item);//避免刚好消耗完道具的情况，即 item.Count == 0 ，但是 道具依然在管理器字典中留有记录
        }

        public Item GetItem(int itemId)//获取该道具
        {
            Item item = null;
            this.Items.TryGetValue(itemId, out item);
            Log.InfoFormat("[{0}] GetItem [{1}:{2}]", this.Owner.Data.ID, itemId, item);
            return item;
        }

        //增加道具
        public bool AddItem(int itemId, int count)//道具ID,增加数量
        {
            Item item = null;
            if (this.Items.TryGetValue(itemId, out item)) //若道具已经存有，只需增加数量
            {
                item.Add(count);
            }
            else //若道具不存在，需要在DB道具表中 插入一条新数据
            {
                //var DBitem = DBService.Instance.Entities.CharacterItems.Create(); //可以用Create(）函数，也可以 new TCharacterItem()
                TCharacterItem dbItem = new TCharacterItem();
                dbItem.CharacterID = Owner.Data.ID;
                dbItem.Owner = Owner.Data;
                dbItem.ItemID = itemId;
                dbItem.ItemCount = count;
                Owner.Data.Items.Add(dbItem); //插入DB中

                item = new Item(dbItem);
                this.Items.Add(itemId, item); //还要添加到道具管理器中
            }
            this.Owner.StatusManager.AddItemChange(itemId, count, StatusAction.Add); //把道具的增加状态 记录到状态管理器
            Log.InfoFormat("[{0}] AddItem [{1}] addCount:{2}", Owner.Data.ID, item, count);
            //DBService.Instance.Save(); 在调用处已经Save过了。 一旦DB发生增删，都需要保存，但是若每一次Save都立即保存，会给服务器很大压力。
            return true;
        }

        public bool RemoveItem(int ItemId, int count)
        {
            if (!this.Items.ContainsKey(ItemId))//没有此道具，删除失败
            {
                return false;
            }
            Item item = this.Items[ItemId];
            if (item.Count < count) //数量不足，删除失败
            {
                return false;
            }
            item.Remove(count);
            this.Owner.StatusManager.AddItemChange(ItemId, count, StatusAction.Delete);//把道具的变化量 记录到状态管理器
            Log.InfoFormat("[{0}] RemoveItem [{1}] RemoveCount:{2}", Owner.Data.ID, item, count);
            //DBService.Instance.Save();
            return true;
        }

        public void GetItemInfos(List<NItemInfo> list)//NItemInfo 是 NetWork网络上的 ItemInfo道具信息
        {
            foreach (var item in this.Items)//将Items管理器 的道具 转换成 网络道具数据
            {
                list.Add(new NItemInfo() { Id = item.Value.ItemID, Count = item.Value.Count });//把数据从内存 转换到 网络上的list中,可能存在item.Count = 0
            }
        }

    }
}
