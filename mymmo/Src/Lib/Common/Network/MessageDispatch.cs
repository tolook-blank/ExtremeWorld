using Common;

namespace Network
{
    public class MessageDispatch<T> : Singleton<MessageDispatch<T>>
    {
        //服务端 和 客户端 的消息分发，调用 消息分发器MessageDistributer
        //在message.proto的NetMessageRequest/Response中 添加协议字段后，一定要在MessageDispatch 加上消息分发处理。否则可能遇到 服务器上未显示消息处理的问题
        public void Dispatch(T sender, SkillBridge.Message.NetMessageResponse message)//客户端分发响应, 对应message.proto 中的 message NetMessageResponse{}
        {
            if (message.userRegister != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.userRegister); }
            if (message.userLogin != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.userLogin); }
            if (message.createChar != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.createChar); }
            if (message.gameEnter != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.gameEnter); }
            if (message.gameLeave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.gameLeave); }

            if (message.mapCharacterEnter != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.mapCharacterEnter); }
            if (message.mapCharacterLeave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.mapCharacterLeave); }
            if (message.mapEntitySync != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.mapEntitySync); }

            if (message.bagSave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.bagSave); }

            if (message.itemBuy != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.itemBuy); }
            if (message.itemEquip != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.itemEquip); }


            if (message.questList != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.questList); }
            if (message.questAccept != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.questAccept); }
            if (message.questSubmit != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.questSubmit); }

            if (message.friendAddReq != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.friendAddReq); }
            if (message.friendAddRes != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.friendAddRes); }
            if (message.firendList != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.firendList); }
            if (message.friendRemove != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.friendRemove); }

            if (message.teamInviteReq != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamInviteReq); }
            if (message.teamInviteRes != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamInviteRes); }
            if (message.teamInfo != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamInfo); }
            if (message.teamLeave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamLeave); }

            if (message.guildCreate != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildCreate); }
            if (message.guildJoinReq != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildJoinReq); }
            if (message.guildJoinRes != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildJoinRes); }
            if (message.Guild != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.Guild); }
            if (message.guildLeave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildLeave); }
            if (message.guildList != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildList); }
            if (message.guildAdmin != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildAdmin); }

            if (message.Chat != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.Chat); }
            if (message.teamAdmin != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamAdmin); }

            if (message.statusNotify != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.statusNotify); }
        }

        public void Dispatch(T sender, SkillBridge.Message.NetMessageRequest message) //服务端分发请求（给服务端Service层的订阅者），对应message.proto 中的 message NetMessageRequest{}
        {
            if (message.userRegister != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.userRegister); }
            if (message.userLogin != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.userLogin); }
            if (message.createChar != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.createChar); }
            if (message.gameEnter != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.gameEnter); }
            if (message.gameLeave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.gameLeave); }

            if (message.mapCharacterEnter != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.mapCharacterEnter); }
            if (message.mapEntitySync != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.mapEntitySync); }
            if (message.mapTeleport != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.mapTeleport); }

            if (message.bagSave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.bagSave); }

            if (message.itemBuy != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.itemBuy); }
            if (message.itemEquip != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.itemEquip); }

            if (message.questList != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.questList); }
            if (message.questAccept != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.questAccept); }
            if (message.questSubmit != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.questSubmit); }

            if (message.friendAddReq != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.friendAddReq); }
            if (message.friendAddRes != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.friendAddRes); }
            if (message.firendList != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.firendList); }
            if (message.friendRemove != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.friendRemove); }

            if (message.teamInviteReq != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamInviteReq); }
            if (message.teamInviteRes != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamInviteRes); }
            if (message.teamInfo != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamInfo); }
            if (message.teamLeave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamLeave); }

            if (message.guildCreate != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildCreate); }
            if (message.guildJoinReq != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildJoinReq); }
            if (message.guildJoinRes != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildJoinRes); }
            if (message.Guild != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.Guild); }
            if (message.guildLeave != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildLeave); }
            if (message.guildList != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildList); }
            if (message.guildAdmin != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.guildAdmin); }

            if (message.Chat != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.Chat); }
            if (message.teamAdmin != null) { MessageDistributer<T>.Instance.RaiseEvent(sender, message.teamAdmin); }


        }
    }
}