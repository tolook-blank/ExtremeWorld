using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Common.Data;
using Models;
using Services;
using SkillBridge.Message;
using UnityEngine.Events;

namespace Managers
{
    class FriendManager : Singleton<FriendManager>
    {
        //好友列表
        public List<NFriendInfo> allFriends;

        public void Init(List<NFriendInfo> friends)
        {
            this.allFriends = friends;
        }
       
    }
}
