using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;

namespace GameServer.Models
{
    class Chat
    {//Models.Chat保存 当前玩家(个人)的聊天记录 ， ChatManager保存所有频道中（全局)的聊天记录
        Character Owner;

        //每个人当前已获取到聊天消息 的索引值
        public int localIdx;
        public int worldIdx;
        public int systemIdx;
        public int teamIdx;
        public int guildIdx;

        public Chat(Character owner)
        {
            this.Owner = owner;
        }

        public void PostProcess(NetMessageResponse message)//玩家获取各个频道消息的后处理（除了私聊消息，私聊消息是双方在线收发）
        {
            if (message.Chat == null)
            {
                message.Chat = new ChatResponse();
                message.Chat.Result = Result.Success;
            }
            //传入上一次获取消息的 索引值， GetXXXMessage返回游戏当前最新的 消息索引值，并更新保存的索引值
            this.localIdx = ChatManager.Instance.GetLocalMessages(this.Owner.Info.mapId, this.localIdx, message.Chat.localMessages);
            this.worldIdx = ChatManager.Instance.GetWorldMessages(this.worldIdx, message.Chat.worldMessages);
            this.systemIdx = ChatManager.Instance.GetSystemMessages (this.systemIdx, message.Chat.systemMessages);
            //有组队、公会，才拉取对应频道的聊天消息
            if (this.Owner.Team != null)
            {
                this.teamIdx = ChatManager.Instance.GetTeamMessages(this.Owner.Team.Id, this.teamIdx, message.Chat.teamMessages);
            }
            if (this.Owner.Guild != null)
            {
                this.guildIdx = ChatManager.Instance.GetGuildMessages(this.Owner.Guild.Id, this.guildIdx, message.Chat.guildMessages);
            }
        }

    }
}
