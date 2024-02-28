using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Common.Data;
using UnityEngine;
using SkillBridge.Message;
using Services;

namespace Models
{
    //Models层用来存储从网络返回的各种信息，而不必所有数据都从服务器拉取。如果数据不改变，只需要一个Singleton就足够
    //在Models层创建一个单例用户类，这个User是 本地的 一个映射。
    class User : Singleton<User>
    {
        SkillBridge.Message.NUserInfo userInfo; //本地保存的 当前用户信息

        public SkillBridge.Message.NUserInfo Info//本地保存的 当前用户信息
        {
            get { return userInfo; }
        }

        public void SetupUserInfo(SkillBridge.Message.NUserInfo info) //把info信息记录到本地
        {
            this.userInfo = info;
        }

        public MapDefine CurrentMapData { get; set; } //角色当前所在地图，方便加载小地图

        public SkillBridge.Message.NCharacterInfo CurrentCharacter { get; set; } //网络当前角色，在 OnGameEnter中（因为此时获得了Entity_ID）初始化赋值

        public PlayerInputController CurrentCharacterObject { get; set; } //当前角色的游戏对象，在 OnMapCharacterEnter 后得到赋值

        public NTeamInfo TeamInfo { get; set; } //当前队伍信息

        public void AddGold(int value)
        {
            this.CurrentCharacter.Gold += value;
        }

        public int CurrentRide = 0;
        public void Ride(int rideId)
        {

            if (CurrentRide == 0 && CurrentRide != rideId)//上马
            {
                CurrentRide = rideId;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, CurrentRide);
            }
            else //下马
            {
                CurrentRide = 0;
                CurrentCharacterObject.SendEntityEvent(EntityEvent.Ride, 0);

            }
        }

    }
}
