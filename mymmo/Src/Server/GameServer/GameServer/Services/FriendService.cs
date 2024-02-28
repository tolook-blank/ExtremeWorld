
using System.Linq;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;


namespace GameServer.Services
{
    class FriendService : Singleton<FriendService>
    {
        //List<FriendAddRequest> friendAddRequests = new List<FriendAddRequest>(); //（旧）用来保存 好友请求信息（谁添加谁？）, 现在优化的协议中，让服务器、客户端中都有好友添加请求、响应，无须此结构来管理数据
        public FriendService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddRequest>(this.OnFriendAddRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendAddResponse>(this.OnFriendAddResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<FriendRemoveRequest>(this.OnFriendRemove);
        }

        public void Init()
        {

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


        //服务器收到A玩家sender 发来的添加好友请求，将该请求转发给玩家B
        private void OnFriendAddRequest(NetConnection<NetSession> sender, FriendAddRequest request)
        {
            Character character = sender.Session.Character;//A玩家的角色
            Log.InfoFormat("OnFriendAddRequest:: FromId:{0} FromName:{1} ToId:{2} ToName:{3} ", request.FromId, request.FromName, request.ToId, request.ToName);

            if (request.ToId == 0)//如果没有传入玩家B的ID,则使用名称查找
            {
                foreach (var cha in CharacterManager.Instance.Characters) //此处设计目的：只在当前所有 在线 玩家中查找
                {
                    if (cha.Value.Data.Name == request.ToName)
                    {
                        request.ToId = cha.Key;
                        break;
                    }
                }
            }
            NetConnection<NetSession> friend = null; //存放B玩家(被添加好友者)的Session
            if (request.ToId > 0) //B玩家在线
            {
                if (character.FriendManager.GetFriendInfo(request.ToId) != null)//从A玩家自己的好友管理器中，查询是否已经舔加过B玩家为好友
                {
                    sender.Session.Response.friendAddRes = new FriendAddResponse();
                    sender.Session.Response.friendAddRes.Result = Result.Failed;
                    sender.Session.Response.friendAddRes.Errormsg = "已经是好友了";
                    sender.SendResponse();//发送添加失败 响应给A玩家
                    return;
                }
                //若未添加过该玩家
                friend = SessionManager.Instance.GetSession(request.ToId);//获取对方的 session在线会话状态
            }
            if (friend == null)//B玩家不存在 或 下线了
            {
                sender.Session.Response.friendAddRes = new FriendAddResponse();
                sender.Session.Response.friendAddRes.Result = Result.Failed;
                sender.Session.Response.friendAddRes.Errormsg = "好友不存在 或者 不在线";
                sender.SendResponse();//发送添加失败 响应给A玩家
                return;
            }
            //若B玩家在线，且未被A玩家添加过
            friend.Session.Response.friendAddReq = request;//(协议设计点)把A玩家发来的添加好友请求 附加在响应中，直接发给 玩家B
            friend.SendResponse(); //发送响应给B玩家
        }

        //服务器收到 玩家B sender 发送的 对A玩家添加好友请求的响应，将该响应 发给玩家A
        private void OnFriendAddResponse(NetConnection<NetSession> sender, FriendAddResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendAddResponse:: characterId:{0} Result:{1} FromId:{2} ToId:{3} ", character.Id, response.Result, response.Request.FromId, response.Request.ToId);
            sender.Session.Response.friendAddRes = response;//填充一份响应 给玩家B

            var requester = SessionManager.Instance.GetSession(response.Request.FromId);//获取对方的 session在线会话状态

            if (response.Result == Result.Success)//B玩家 接受玩家A的好友请求
            {
                if (requester == null) //但是玩家A下线了，发送添加失败（只能发给玩家B）
                {
                    sender.Session.Response.friendAddRes.Result = Result.Failed;
                    sender.Session.Response.friendAddRes.Errormsg = "请求者已下线";//发给接收者玩家B
                }
                else //双方都在线，也同意添加好友 ，添加成功
                {
                    //互为好友，互相都要添加到FriendManager
                    character.FriendManager.AddFriend(requester.Session.Character);//玩家B 把请求者A 加入好友管理器
                    requester.Session.Character.FriendManager.AddFriend(character);//请求者A 把玩家B 加入好友管理器
                    DBService.Instance.Save();

                    requester.Session.Response.friendAddRes = response;//填充一份响应 给请求者A
                    requester.Session.Response.friendAddRes.Result = Result.Success;
                    requester.Session.Response.friendAddRes.Errormsg = "添加好友成功";
                    requester.SendResponse();//发送给请求者A
                }
            }
            else
            {//若B玩家 拒绝A玩家的好友请求
                if (requester != null)//若玩家A在线，要发送添加失败响应给玩家A
                {
                    requester.Session.Response.friendAddRes = response;
                    requester.Session.Response.friendAddRes.Result = Result.Failed;
                    requester.Session.Response.friendAddRes.Errormsg = "对方拒绝，添加好友失败";
                    requester.SendResponse();//发送给请求者A
                }
            }
            sender.SendResponse();//发给接收者玩家B
        }

        //服务器 收到删除好友请求
        private void OnFriendRemove(NetConnection<NetSession> sender, FriendRemoveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnFriendRemove:: characterId:{0} 选中项FriendRelationId:{1}  ", character.Id, request.Id);
            sender.Session.Response.friendRemove = new FriendRemoveResponse();
            sender.Session.Response.friendRemove.Id = request.Id;//好友列表 当前选中项的ID

            //删除好友也是双向的，自己既要删除对方，也要在对方的好友列表中删除自己
            if (character.FriendManager.RemoveFriendByID(request.Id))//自己 从管理器中删除对方, 通过 好友列表选中项的ID，来删除好友
            {
                sender.Session.Response.friendRemove.Result = Result.Success;

                var friend = SessionManager.Instance.GetSession(request.friendId);//通过当前选中项中好友的ID，获取好友的Session在线会话状态
                if (friend != null)
                {//好友在线（先删内存，再删数据库）
                    friend.Session.Character.FriendManager.RemoveFriendByFriendID(character.Id);//对方从管理器中 删除自己，通过好友的ID，来删除好友
                }
                else
                {//好友不在线（直接删数据库）
                    this.RemoveFriend(request.friendId, character.Id);//对方从数据库实体中 删除自己
                }
            }
            else
                sender.Session.Response.friendRemove.Result = Result.Failed;

            DBService.Instance.Save();
            sender.SendResponse();
        }

        private void RemoveFriend(int charId, int friendId)//直接从DB表[TCharacterFriends]中删除 FriendID==friendId && TCharacterID == charId 的记录（行）
        {
            var removeItem = DBService.Instance.Entities.TCharacterFriends.FirstOrDefault(v => v.TCharacterID == charId && v.FriendID == friendId);//（lambda）为筛选条件
            if (removeItem != null)
            {
                DBService.Instance.Entities.TCharacterFriends.Remove(removeItem);
            }
        }
    }
}
