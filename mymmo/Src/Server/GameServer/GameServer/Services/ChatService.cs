
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;

namespace GameServer.Services
{
    class ChatService : Singleton<ChatService>
    {
        public ChatService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<ChatRequest>(this.OnChat);
        }

        public void Init()
        {
            ChatManager.Instance.Init();
        }

        //客户端玩家发送消息 -> 服务器处理返回后 ->客户端才显示消息
        //服务器处理玩家发来的 聊天请求
        private void OnChat(NetConnection<NetSession> sender, ChatRequest request)
        {
            Character character = sender.Session.Character;//sender玩家的角色
            Log.InfoFormat("OnChat:: character:{0}_{1} Channel:{2} Message:{3} ", character.Id, character.Name, request.Message.Channel, request.Message.Message);
            if(request.Message.Channel == ChatChannel.Private)//如果是私聊消息
            {
                var chatTo = SessionManager.Instance.GetSession(request.Message.ToId);//获取私聊对方 session在线会话状态
                if (chatTo == null)//若对方不在线，则消息发送失败（因为设定为只能双方在线私聊）
                {
                    sender.Session.Response.Chat = new ChatResponse();//给发送者的聊天响应
                    sender.Session.Response.Chat.Result = Result.Failed;
                    sender.Session.Response.Chat.Errormsg = "对方不在线";
                    sender.Session.Response.Chat.privateMessages.Add(request.Message);//添加到聊天响应的 私聊消息列表中
                    sender.SendResponse();//返回给发送者
                }
                else//若对方在线
                {
                    if(chatTo.Session.Response.Chat == null)
                    {
                        chatTo.Session.Response.Chat = new ChatResponse();//给私聊对象的聊天响应
                    }
                    request.Message.FromId = character.Id; //消息来源
                    request.Message.FromName = character.Name;
                    chatTo.Session.Response.Chat.Result = Result.Success;
                    chatTo.Session.Response.Chat.privateMessages.Add(request.Message);//添加到聊天响应的 私聊消息列表中
                    chatTo.SendResponse();//发送给私聊对象

                    if (sender.Session.Response.Chat == null)
                    {
                        sender.Session.Response.Chat = new ChatResponse();
                    }
                    sender.Session.Response.Chat.Result = Result.Success;
                    sender.Session.Response.Chat.privateMessages.Add(request.Message);
                    sender.SendResponse();//再把消息也返回给自己（因为客户端中，没有直接在本地添加 发送的聊天消息）
                }
            }
            else//若不是私聊消息
            {
                sender.Session.Response.Chat = new ChatResponse();
                sender.Session.Response.Chat.Result = Result.Success;
                ChatManager.Instance.AddMessage(character, request.Message);//直接把玩家发送的消息 添加到服务器的聊天管理器中
                sender.SendResponse();
            }
        }

    }
}
