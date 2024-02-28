using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Services;
using Common;

namespace GameServer.Managers
{
    class EquipManager : Singleton<EquipManager>
    {
        public Result EquipItem(NetConnection<NetSession> sender, int slot, int itemId, bool isEquip)
        {
            Character character = sender.Session.Character;
            if (!character.ItemManager.Items.ContainsKey(itemId)) //若角色没有此装备道具，直接返回失败 （加校验防外挂）
                return Result.Failed;

            UpdateEquip(character.Data.Equips, slot, itemId, isEquip);//根据装备槽slot 的 装备穿、脱，来更新 character.Data.Equips中的装备ID 

            DBService.Instance.Save();
            return Result.Success;
        }

        unsafe void UpdateEquip(byte[] equipData, int slot, int itemId, bool isEquip)
        {
            fixed (byte* pt = equipData) //读取角色的装备数据character.Data.Equips
            {
                int* slotid = (int*)(pt + slot * sizeof(int)); //将指针指向 对应的装备槽
                if (isEquip)//如果是穿装备
                    *slotid = itemId; //向 equipData中 填入装备ID
                else  //脱装备
                    *slotid = 0; //清空 equipData 中对应槽位slot的 装备ID
            }
        }
    }
}