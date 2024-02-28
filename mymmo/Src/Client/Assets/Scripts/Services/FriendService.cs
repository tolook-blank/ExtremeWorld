using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Data;
using Network;
using SkillBridge.Message;
using Models;
using Managers;
using UnityEngine.Events;

namespace Services
{
    class FriendService : Singleton<FriendService>, IDisposable
    {
        public UnityAction OnFriendUpdate;

        public void Init()
        {

        }

        public FriendService()
        {
            MessageDistributer.Instance.Subscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer.Instance.Subscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer.Instance.Subscribe<FriendListResponse>(this.OnFriendList);
            MessageDistributer.Instance.Subscribe<FriendRemoveResponse>(this.OnFriendRemove);
        }


        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer.Instance.Unsubscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer.Instance.Unsubscribe<FriendListResponse>(this.OnFriendList);
            MessageDistributer.Instance.Unsubscribe<FriendRemoveResponse>(this.OnFriendRemove);
        }


        /* A玩家 添加B玩家 为好友 的流程：
         1、先进入Client SendFriendAddRequest，  A玩家客户端 发送添加好友请求 => 给服务器. 
         2、GameServer OnFriendAddRequest 服务器 收到A玩家发来的添加好友请求，将其转发给 => 玩家B的客户端.  
         3、Client OnFriendAddRequest    B玩家客户端  收到 A玩家发来的添加好友请求，选择是否同意，=> 调用此客户端中 发送对应响应的方法 .
         4、Client SendFriendAddResponse B玩家客户端  发送 对A添加好友请求的响应 => 给服务器,表明是否同意 A玩家的好友请求.
         5、GameServer OnFriendAddResponse， 服务器收到 B玩家发送的 对A玩家添加好友请求的响应，将该响应 => 发给双方，通知双方添加成功或是失败. 
         6、Client OnFriendAddResponse  A、B玩家 客户端双方 收到 添加好友的结果响应
         */

        /*A玩家 删除 好友B玩家 的流程：
         1.先进入Client SendFriendRemoveRequest， A玩家客户端 发送删除好友请求 => 给服务器.
         2.GameServer OnFriendRemove， 服务器 收到A玩家 发来的删除好友请求，完成双向删除好友，完成后发回响应=> 给A玩家
         3.Client OnFriendRemove， A玩家 收到删除好友响应
         */

        //A玩家 发送添加好友请求 给服务器
        public void SendFriendAddRequest(int friendId, string friendName)
        {
            Debug.Log("SendFriendAddRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendAddReq = new FriendAddRequest();
            message.Request.friendAddReq.FromId = User.Instance.CurrentCharacter.Id;
            message.Request.friendAddReq.FromName = User.Instance.CurrentCharacter.Name;
            message.Request.friendAddReq.ToId = friendId;
            message.Request.friendAddReq.ToName = friendName;
            NetClient.Instance.SendMessage(message);
        }

        //B玩家发送 对添加好友请求的响应 给服务器,表明是否同意 A玩家的好友请求，
        public void SendFriendAddResponse(bool accept, FriendAddRequest request) 
        {
            Debug.Log("SendFriendAddResponse");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendAddRes = new FriendAddResponse();
            message.Request.friendAddRes.Result = accept ? Result.Success : Result.Failed;
            message.Request.friendAddRes.Errormsg = accept ? "对方同意" : "对方拒绝了你的请求";
            message.Request.friendAddRes.Request = request;//B玩家 将对应的添加好友请求 填充到响应中， 发回给服务器

            NetClient.Instance.SendMessage(message);
        }

        //B玩家 收到 A玩家发来的添加好友请求，选择是否同意，调用发送对应响应的方法
        private void OnFriendAddRequest(object sender, FriendAddRequest request)//FriendAddRequest request 即 A玩家发来的添加好友请求
        {
            Debug.Log("OnFriendAddRequest");
            var confirm = MessageBox.Show(string.Format("{0} 请求加你为好友", request.FromName), "好友请求", MessageBoxType.Confirm, "接受", "拒绝");
            confirm.OnYes = () =>
             {
                 this.SendFriendAddResponse(true, request);
             };
            confirm.OnNo = () =>
            {
                this.SendFriendAddResponse(false, request);
            };
        }

        //A玩家收到 B玩家 对添加好友请求的响应结果
        private void OnFriendAddResponse(object sender, FriendAddResponse message)
        {
            Debug.Log("OnFriendAddResponse");
            if (message.Result == Result.Success)
                MessageBox.Show(message.Request.ToName + "接受了您的好友请求", "添加好友成功");
            else
                MessageBox.Show(message.Errormsg, "添加好友失败");
        }


        //收到拉取好友列表请求的响应 （在添加好友，删除好友时调用，来刷新好友列表UI）
        private void OnFriendList(object sender, FriendListResponse message)
        {
            Debug.Log("OnFriendList");
            FriendManager.Instance.allFriends = message.Friends;
            if (this.OnFriendUpdate != null)
                this.OnFriendUpdate();
        }

        //发送删除好友请求
        public void SendFriendRemoveRequest(int id, int friendId) //当前选中好友列表项的ID,选中项中好友的ID
        {
            Debug.Log("SendFriendRemoveRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.friendRemove = new FriendRemoveRequest();
            message.Request.friendRemove.Id = id;
            message.Request.friendRemove.friendId = friendId;

            NetClient.Instance.SendMessage(message);
        }

        //收到删除好友响应
        private void OnFriendRemove(object sender, FriendRemoveResponse message)
        {
            if (message.Result == Result.Success)
                MessageBox.Show("删除成功", "删除好友");
            else
                MessageBox.Show("删除失败", "删除好友", MessageBoxType.Error);
        }
    }
}
