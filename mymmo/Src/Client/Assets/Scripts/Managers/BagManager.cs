
using Models;
using Network;
using SkillBridge.Message;
using UnityEngine;

namespace Managers
{
    //客户端将 服务端发来的 网络背包数据NBagInfo 的字节道具列表 解析出来
    class BagManager : Singleton<BagManager> //可以用单例，因为在客户端中，道具管理器是每人一份，管理自己的道具
    {
        //背包管理器中负责的数据结构
        public int Unlocked; //解锁的背包格子数
        public BagItem[] Items; //解锁的格子中的道具
        NBagInfo BagInfo; //包含 Unlocker解锁格子数 、Items背包存放的道具列表（可变长字节数组byte[]）
        //背包格子上限

        unsafe public void Init(NBagInfo info)//info.Items是字节数组类型
        {
            this.BagInfo = info;
            this.Unlocked = info.Unlocked;

            Items = new BagItem[this.Unlocked];  //Items 是BagItem[] 类型
            if (info.Items.Length != 0 && info.Items != null)//有道具
            {
                Analyze(info.Items);//解析出来的Items[] 可能存在item.count = 0 的占用格子情况，若每次都Reset整理，性能开销则过大，
            }
            else//首次登陆
            {
                BagInfo.Items = new byte[sizeof(BagItem) * this.Unlocked];
            }
            Reset(); //获取角色的道具管理器中的数据，整理更新背包
        }

        public void ExpandCapacity(int Capacity)//新的容量
        {
            BagItem[] newItems = new BagItem[Capacity];

            for (int i = 0; i < Items.Length; i++) // 将旧数组中的数据复制到新数组
            {
                newItems[i] = Items[i];//Items 引用了新数组 newItems 的内存，C#中，没有任何引用的旧数组将会由垃圾回收器回收
            }
            Items = newItems;// 将新数组赋值给 Items
        }

        public void Reset() //背包一键整理按钮 ,整理Items[]， 覆盖 道具数量为0的空格子
        {
            int i = 0;
            foreach (var kv in ItemManager.Instance.Items)//遍历角色身上的道具管理器（可能存在item.Count = 0），填充背包，一种道具填充完 才继续向后遍历
            {
                if (kv.Value.Count == 0)
                {
                    continue; //若此种道具数量为空，则不占用格子，直接填充下一种道具
                }
                if (kv.Value.Count <= kv.Value.Define.Stacklimit)
                {
                    this.Items[i].ItemId = (ushort)kv.Key;
                    this.Items[i].Count = (ushort)kv.Value.Count;
                }
                else
                {
                    int count = kv.Value.Count;
                    while (count > kv.Value.Define.Stacklimit) //超出叠加限制的部分，需要拆分，直到不能拆。 例如：200 = 限制99 + 99 + 2
                    {
                        this.Items[i].ItemId = (ushort)kv.Key;
                        this.Items[i].Count = (ushort)kv.Value.Define.Stacklimit;
                        i++;//下个格子
                        count -= kv.Value.Define.Stacklimit;
                    }
                    this.Items[i].ItemId = (ushort)kv.Key;
                    this.Items[i].Count = (ushort)count;
                }
                i++;

                if (i >= (this.Unlocked - 1)) //背包解锁的格子不够用时,扩容
                {
                    this.Unlocked += 10; //一次额外解锁10个格子
                    ExpandCapacity(this.Unlocked);
                }
            }
            SaveBag(); //整理时自动保存背包
        }

        //因为类是引用类型(属于托管类型)，我们知道类受到“垃圾收集”的影响，它的内存地址是不固定的。无法获取托管类型的 地址和大小，是不能声明为指针类型的。
        //而指针分配内存后，不受“垃圾收集”影响，地址是固定的。所以为了使用托管类型的数据，我们需要临时固定地址，需要用到fixed关键词，用fixed后，就可以操作托管类型中的值类型了。
        unsafe void Analyze(byte[] data) //将服务器发来的byte[] 解析成 BagItem[]
        {
            //byte* pt = data，将指针指向data数组的首地址 &data[0]
            fixed (byte* pt = data)//由于数组是引用类型，且在内存中可移动（堆上可被垃圾回收），为了能获取可移动数据的地址，我们需要用fixed把它固定下来
            {
                for (int i = 0; i < this.Unlocked; ++i)//按照背包格子数，依次解析出其中的道具信息
                {
                    BagItem* item = (BagItem*)(pt + i * sizeof(BagItem));// 指针按照 BagItem字节数偏移
                    Items[i] = *item;  //取出 item结构体指针 指向地址中的值，解析出BagItem ，存储到背包数组中。
                }
            }
        }

        unsafe public NBagInfo GetBagInfo() //将BagItem[] 转成 byte[] ，发送给服务器（byte[]中存储了背包的道具布局）,服务器用来存储到DB中
        {
            fixed (byte* pt = BagInfo.Items)//pt是指向 BagItem[] Items 首地址的指针
            {
                for (int i = 0; i < this.Unlocked; ++i)
                {
                    BagItem* item = (BagItem*)(pt + i * sizeof(BagItem));
                    *item = Items[i];//取出 背包数组中的值 存到 内存中
                }
            }
            this.BagInfo.Unlocked = this.Unlocked;
            return this.BagInfo;
        }


        public void AddItem(int itemId, int count) //添加道具
        {
            ushort addCount = (ushort)count; //需要添加的数量
            ushort limit = (ushort)DataManager.Instance.Items[itemId].Stacklimit;//该道具叠加限制

            for (int i = 0; i < this.Unlocked; ++i)//从前往后遍历
            {
                if (this.Items[i].ItemId == itemId || this.Items[i].Count == 0) //找到存储该道具对应的格子/或者有空格子
                {
                    ushort canAdd = (ushort)(limit - this.Items[i].Count);//该道具格剩余的存储量, 对于叠加限制为1的道具，canAdd可能 = 0

                    if (canAdd >= addCount) //若足够容纳完，不溢出
                    {
                        this.Items[i].Count += addCount;
                        addCount = 0;
                        break;
                    }
                    else  //若会溢出，继续找格子存储
                    {
                        this.Items[i].Count += canAdd;
                        addCount -= canAdd;
                    }
                }
            }
            if (addCount > 0)//格子不足够存下去，需要扩容后再存储
            {
                this.Unlocked += 10; //一次额外解锁10个格子
                ExpandCapacity(this.Unlocked);
            }
            for (int i = this.Unlocked - 10; i < this.Unlocked; ++i)//扩容完，继续往新的空格子中存道具
            {

                ushort canAdd = (ushort)(limit - this.Items[i].Count);
                if (canAdd >= addCount) //若足够容纳完，不溢出
                {
                    this.Items[i].Count += addCount;
                    addCount = 0;
                    break;
                }
                else  //若会溢出
                {
                    this.Items[i].Count += canAdd;
                    addCount -= canAdd;
                }
            }

        }

        public void RemoveItem(int itemId, int count)//调用处已经校验，必能删、够删
        {
            ushort removeCount = (ushort)count;
            for (int i = 0; i < Items.Length; ++i)
            {
                if (Items[i].ItemId == itemId) // 找到背包中的对应道具
                {
                    if (Items[i].Count > removeCount) // 如果背包中数量大于要删除的数量
                    {
                        Items[i].Count -= removeCount;
                        break; // 结束删除操作
                    }
                    else if (Items[i].Count == removeCount) // 如果背包中数量等于要删除的数量
                    {
                        Items[i] = BagItem.zero; // 道具格清空
                        break; // 结束删除操作
                    }
                    else // 如果背包中数量小于要删除的数量，还要继续向后寻找，继续删除，直到为0
                    {
                        removeCount -= Items[i].Count;
                        Items[i] = BagItem.zero; // 道具格清空
                    }
                }
            }
        }


        public void SaveBag()
        {
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.bagSave = new BagSaveRequest();
            message.Request.bagSave.BagInfo = GetBagInfo();
            NetClient.Instance.SendMessage(message);
        }


    }
}


