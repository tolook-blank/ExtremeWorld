
using Models;
using Services;
using SkillBridge.Message;


namespace Managers
{
    class EquipManager : Singleton<EquipManager>
    {
        public delegate void OnEquipChangeHandler();
        public event OnEquipChangeHandler OnEquipChanged; //装备改变事件

        public Item[] Equips = new Item[(int)EquipSlot.SlotMax]; //装备系统 只维护 7个装备槽的数据
        byte[] Data; //用于和服务端 传输装备数据 byte[28]

        unsafe public void Init(byte[] data)
        {
            this.Data = data;
            this.ParseEquipData(data); //解析装备数据
        }

        public bool Contains(int equipId) //检查 装备槽中是否穿戴了某装备
        {
            for (int i = 0; i < this.Equips.Length; i++)
            {
                if (Equips[i] != null && Equips[i].Id == equipId)
                {
                    return true;
                }
            }
            return false;

        }

        public Item GetEquip(EquipSlot slot) //获取 某装备槽 的道具信息
        {
            return Equips[(int)slot];
        }

        unsafe void ParseEquipData(byte[] data) //解析装备数据byte[28] = int[7] 成 装备道具Item[7] Equips
        {
            fixed (byte* pt = this.Data)
            {
                for (int i = 0; i < Equips.Length; i++)
                {
                    int itemId = *(int*)(pt + i * sizeof(int));//将pt指针按 int大小 偏移，解析出 装备ID 
                    if (itemId > 0)
                    {
                        Equips[i] = ItemManager.Instance.Items[itemId]; //根据装备ID 从道具管理器中取出，填充进装备栏Equips[]
                    }
                    else
                        Equips[i] = null;
                }
            }
        }

        unsafe public byte[] GetEquipData() //把装备栏Item[] 转换成 字节数组byte[]，发送给服务器
        {
            fixed (byte* pt = Data)
            {
                for (int i = 0; i < (int)EquipSlot.SlotMax; i++)
                {
                    int* itemid = (int*)(pt + i * sizeof(int));
                    if (Equips[i] == null)
                        *itemid = 0;
                    else
                        *itemid = Equips[i].Id;  //将装备ID 填入 字节数组Data
                }
            }
            return this.Data;
        }

        public void EquipItem(Item equip) //穿装备请求
        {
            ItemService.Instance.SendEquipItem(equip, true);//发送穿装备请求
        }

        public void UnEquipItem(Item equip) //脱装备请求
        {
            ItemService.Instance.SendEquipItem(equip, false);//发送脱装备请求
        }

        public void OnEquipItem(Item equip) //收到 穿装备响应
        {
            if (this.Equips[(int)equip.EquipInfo.Slot] != null && this.Equips[(int)equip.EquipInfo.Slot].Id == equip.Id) //检查是否已经穿上该装备
            {
                MessageBox.Show(string.Format("角色身上已经穿上该装备[{0}]", equip.Define.Name), "确认", MessageBoxType.Confirm);
                return;
            }
            this.Equips[(int)equip.EquipInfo.Slot] = ItemManager.Instance.Items[equip.Id];//从道具系统中取出装备，填入装备槽

            if (OnEquipChanged != null)
                OnEquipChanged(); //通知订阅者，装备栏发生改变
        }

        public void OnUnEquipItem(EquipSlot slot) //收到 脱装备响应
        {
            if (this.Equips[(int)slot] != null) //装备槽中有装备
            {
                this.Equips[(int)slot] = null; //清空该装备槽
                if (OnEquipChanged != null)
                {
                    OnEquipChanged();//通知订阅者，装备栏发生改变
                }
            }
        }
    }
}
