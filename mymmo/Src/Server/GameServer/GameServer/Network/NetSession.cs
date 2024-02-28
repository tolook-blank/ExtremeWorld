using System.Threading.Tasks;

using GameServer;
using GameServer.Entities;
using GameServer.Services;
using SkillBridge.Message;

namespace Network
{//NetSession表示 服务端与客户端的一个网络会话，主要维护会话与用户、角色等实体之间的关系，以及处理与会话相关的响应消息。
    class NetSession : INetSession
    {
        public TUser User { get; set; }//DB中与当前会话关联的用户
        public Character Character { get; set; }//当前会话关联的角色
        public NEntity Entity { get; set; }

        //PostResponser 在OnGameEnter中初始化为 进入游戏的角色 ，每个客户端都有自己的PostResponser，在每次消息发送之前，处理角色的各种状态变化，生成对应的响应消息，整合在Response中发给给客户端
        public IPostResponse PostResponser { get; set; }//响应后处理器,每次在给客户端sender.SendResponse()时触发

        //当网络断开时 主动调用
        public void Disconnected()
        {
            this.PostResponser = null;//断开连接时，清空响应后处理器
            if (this.Character != null)//如果会话关联了角色
            {
                UserService.Instance.DoGameLeave(this.Character);//网络连接断开时，服务器调用DoGameLeave删除角色
            }
        }

        //客户端每次登录会有一个唯一的 NetSession ，如果想要response消息共用，绑定在NetSession中最合适,就是 Response 放到每个Session周期内
        NetMessage response; //根消息NetMessage在打包发送消息使用，包括两大部分：NetMessageRequest(客户端发送给服务器)、NetMessageResponse（服务器返回给客户端）

        public NetMessageResponse Response //响应消息整合包NetMessageResponse,
        {
            get
            {   //自动创建，随处可用
                if (response == null)
                {
                    response = new NetMessage();
                }
                if (response.Response == null)
                {
                    response.Response = new NetMessageResponse();
                }
                return response.Response;
            }
        }

        //获取 根消息NetMessage 打包成的字节数组
        public byte[] GetResponse()
        {//不再把 Response放在每个方法周期中，而是 把Response 放到每个Session周期内。Session是全局的，在客户端登录时产生唯一Session。
            if (response != null)//如果存在响应消息，会调用响应后处理器的 PostProcess 方法
            {
                if (this.PostResponser != null)//PostResponser 在OnGameEnter中 初始化为进入游戏的character(!= null)，执行Character的PostProcess()
                {//在整合响应消息中Response，添加后处理机制，在每次消息发送之前，处理角色的各种状态变化，生成对应的响应消息，整合在Response中发给给客户端
                    this.PostResponser.PostProcess(Response);
                }
                byte[] data = PackageHandler.PackMessage(response);//将网络协议（响应消息）打包成字节数组
                response = null; //消息打包后，清空此响应消息；下次接收消息时，再重新创建new
                return data;
            }
            return null;
        }

    }
}
