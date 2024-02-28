using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

using SkillBridge.Message;
using Common.Data;


namespace Models
{
    //Models倾向于纯维护本地数据，与服务器关联少，也不需要在游戏世界中和服务端做同步

    //结构布局属性， Sequential 表示结构体 在内存中顺序排列， 即内存里先排ItemId，再Count，两者内存地址相差一个ushort 2字节。
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct BagItem // 使用结构体（值类型） ，方便 两个背包格子之间数据交换 temp=a;a=b;b=temp
    {
        public ushort ItemId; //道具Id, ushort占2字节，范围 0 - 65535
        public ushort Count; //道具数量

        public static BagItem zero = new BagItem { ItemId = 0, Count = 0 }; //空格子  ItemId = 0

        public BagItem(int itemId, int count)
        {
            this.ItemId = (ushort)itemId;
            this.Count = (ushort)count;
        }

        public static bool operator ==(BagItem lhs, BagItem rhs)
        {
            return (lhs.ItemId == rhs.ItemId && lhs.Count == rhs.Count);
        }
        public static bool operator !=(BagItem lhs, BagItem rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object other)
        {
            if (other is BagItem)
            {
                return Equals((BagItem)other);
            }
            return false;
        }

        public bool Equals(BagItem other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return ItemId.GetHashCode() ^ (Count.GetHashCode() << 2);
        }
    }
}


