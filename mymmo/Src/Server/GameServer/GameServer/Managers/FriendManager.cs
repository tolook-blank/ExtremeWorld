
using System.Collections.Generic;
using System.Linq;
using Common;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Services;

namespace GameServer.Managers
{
    class FriendManager
    {//好友信息的增删 涉及到数据库
        Character Owner;
        List<NFriendInfo> friends = new List<NFriendInfo>();//每个玩家的好友列表

        bool friendChanged = false;//每当好友列表有变动，标记为true

        public FriendManager(Character owner)
        {
            this.Owner = owner;
            this.InitFriends();
        }

        public void InitFriends()
        {
            this.friends.Clear();                          //Owner.Data.Friends 的类型为：Character.TCharacter.ICollection<TCharacterFriend>
            foreach (var friend in this.Owner.Data.Friends)//读取数据库中的好友信息TCharacterFriend,转成NFriendInfo存到好友管理器
            {
                this.friends.Add(GetFriendInfo(friend));
            }
        }
        public void GetFriendInfos(List<NFriendInfo> friendslist)
        {
            foreach (var f in this.friends)
            {
                friendslist.Add(f);
            }
        }

        public NFriendInfo GetFriendInfo(TCharacterFriend friend)
        {
            NFriendInfo friendInfo = new NFriendInfo();
            var character = CharacterManager.Instance.GetCharacter(friend.FriendID);//从在线玩家管理器中，获取好友的Character
            friendInfo.friendInfo = new NCharacterInfo();
            friendInfo.Id = friend.Id; //DB表[TCharacterFriends]的ID
            if (character == null)//若好友不在线
            {
                friendInfo.friendInfo.Id = friend.FriendID; //好友ID
                friendInfo.friendInfo.Name = friend.FriendName;
                friendInfo.friendInfo.Class = (CharacterClass)friend.Class;
                friendInfo.friendInfo.Level = friend.Level;
                friendInfo.Status = 0;//离线状态设为0
            }
            else //若好友在线
            {
                friendInfo.friendInfo = character.GetBasicInfo();
                friendInfo.friendInfo.Name = character.Info.Name;//已经在GetBasicInfo中，赋过一次值
                friendInfo.friendInfo.Class = character.Info.Class;
                friendInfo.friendInfo.Level = character.Info.Level;
                if (friend.Level != character.Info.Level) //若 好友当前等级 ！= 好友数据库中等级，更新
                {
                    friend.Level = character.Info.Level;
                }

                character.FriendManager.UpdateFriendInfo(this.Owner.Info, 1);//status = 1表示在线
                friendInfo.Status = 1; //在线状态设为1
            }
            Log.InfoFormat("{0}_{1} GetFriendInfo: {2}_{3} Status:{4}", this.Owner.Id, this.Owner.Info.Name, friendInfo.friendInfo.Id, friendInfo.friendInfo.Name, friendInfo.Status);
            return friendInfo;
        }


        public NFriendInfo GetFriendInfo(int friendId)
        {
            foreach (var f in this.friends)
            {
                if (f.friendInfo.Id == friendId)
                {
                    return f;
                }
            }
            return null;
        }


        public void AddFriend(Character friend)
        {
            TCharacterFriend tf = new TCharacterFriend()
            {
                FriendID = friend.Id,
                FriendName = friend.Data.Name,
                Class = friend.Data.Class,
                Level = friend.Data.Level
            };
            this.Owner.Data.Friends.Add(tf);//添加到数据库中
            friendChanged = true;
        }

        public bool RemoveFriendByFriendID(int friendid)//根据(好友列表项中)好友的ID,删除好友
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.FriendID == friendid);//从好友的DB_好友表中 查找满足 v.FriendID == friendid的记录
            if (removeItem != null)
            {
                DBService.Instance.Entities.TCharacterFriends.Remove(removeItem);
            }
            friendChanged = true;
            return true;
        }

        public bool RemoveFriendByID(int id)//好友列表项的ID
        {
            var removeItem = this.Owner.Data.Friends.FirstOrDefault(v => v.Id == id);
            if (removeItem != null)
            {
                DBService.Instance.Entities.TCharacterFriends.Remove(removeItem);
            }
            friendChanged = true;
            return true;
        }

        public void UpdateFriendInfo(NCharacterInfo friendInfo, int status)//更新好友的在线状态,status = 0：下线 ， 1：在线
        {
            foreach (var f in friends)
            {
                if (f.friendInfo.Id == friendInfo.Id)
                {
                    f.Status = status;
                    break;
                }
            }
            this.friendChanged = true;
        }

        public void OfflineNotify()//若我自己要下线了，把我的状态通知给好友
        {
            foreach (var friendInfo in this.friends) //遍历我的所有好友
            {
                var friend = CharacterManager.Instance.GetCharacter(friendInfo.friendInfo.Id);//若好友在线，可以从CharacterManager中获取到好友角色
                if (friend != null) //若好友在线
                {
                    friend.FriendManager.UpdateFriendInfo(this.Owner.Info, 0); //通知好友，更改我的状态 为离线（0）
                }
            }
        }

        public void PostProcess(NetMessageResponse message)//若好友列表中发生变化,调用 PostProcess后处理函数
        {
            if (friendChanged)//只当（A玩家）好友列表信息 变动，才执行后处理，且后处理完 会把状态清除，以便接收下次的信息变动
            {
                Log.InfoFormat("PostProcess > FriendManager :characterID:{0}_{1}", this.Owner.Id, this.Owner.Info.Name);
                this.InitFriends();
                if (message.firendList == null)
                {
                    message.firendList = new FriendListResponse();
                    message.firendList.Friends.AddRange(this.friends);
                }
                friendChanged = false;
            }
        }
    }
}
