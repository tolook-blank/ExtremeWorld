using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SkillBridge.Message;

using Common;
using Common.Data;

using Network;
using GameServer.Managers;
using GameServer.Services;
using GameServer.Entities;


namespace GameServer.Models
{
    class Item
    {
        TCharacterItem dbItem; //DB中的道具，为了避免DB读取操作过于频繁，在角色登录后，会首先拉取一次数据库中的道具数据，存放在内存中，用于平常客户端和服务器通讯

        public int ItemID;//道具ID

        public int Count; //道具数量

        public Item(TCharacterItem item)
        {
            this.dbItem = item;
            this.ItemID = (short)item.ItemID;
            this.Count = (short)item.ItemCount;
        }

        public void Add(int count)
        {
            this.Count += count;
            dbItem.ItemCount = this.Count;
        }

        public void Remove(int count)
        {
            this.Count -= count;
            dbItem.ItemCount = this.Count;
        }

        public bool Use(int count = 1)//使用道具，留空 后续战斗系统使用
        {
            //暂时留空
            return false;
        }

        public override string ToString()//简化输出
        {
            return string.Format("ItemID = {0},ItemCount = {1}", ItemID, Count);
        }

    }
}
