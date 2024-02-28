using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Network
{
    interface IPostResponse//后处理器,记录任何角色、对象的变化状态，发生在任何消息准备发送给客户端的时候
    {
        void PostProcess(NetMessageResponse response);
    }
}
