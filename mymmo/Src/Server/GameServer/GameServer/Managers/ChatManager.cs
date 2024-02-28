
using System.Collections.Generic;
using Common;
using Common.Utils;
using SkillBridge.Message;
using GameServer.Entities;

namespace GameServer.Managers
{
    class ChatManager : Singleton<ChatManager>
    {//（全局的聊天记录）聊天消息直接保存在内存中（各类聊天管理器中），不用 数据库存储
        public List<ChatMessage> System = new List<ChatMessage>(); //系统消息，因为全局唯一，所以使用List存储
        public List<ChatMessage> World = new List<ChatMessage>(); //世界频道消息，全局唯一
        public Dictionary<int, List<ChatMessage>> Local = new Dictionary<int, List<ChatMessage>>(); //每个地图维护一个，本地消息管理器(地图ID,本地消息）
        public Dictionary<int, List<ChatMessage>> Team = new Dictionary<int, List<ChatMessage>>(); //每个队伍维护一个 (队伍ID,队伍消息）
        public Dictionary<int, List<ChatMessage>> Guild = new Dictionary<int, List<ChatMessage>>(); //每个公会维护一个 (公会ID,公会消息）

        //私聊消息未作保存处理，可自行补充
        //public Dictionary<int, List<ChatMessage>> Private //玩家各自维护一个私聊频道 (玩家自己ID,私聊消息），私聊双方各自维护，相当于存储两份聊天消息，以空间换时间

        public void Init()
        {

        }

        public void AddMessage(Character from, ChatMessage message)
        {
            message.FromId = from.Id;
            message.FromName = from.Name;
            message.Time = TimeUtil.timestamp;
            switch (message.Channel)
            {
                case ChatChannel.Local:
                    this.AddLocalMessage(from.Info.mapId, message); //添加本地消息，需要传入地图ID
                    break;
                case ChatChannel.World:
                    this.AddWorldMessage(message);
                    break;
                case ChatChannel.System:
                    this.AddSystemMessage(message);
                    break;
                case ChatChannel.Team:
                    this.AddTeamMessage(from.Team.Id, message);//添加队伍消息，要传入队伍ID
                    break;
                case ChatChannel.Guild:
                    this.AddGuildMessage(from.Guild.Id, message);
                    break;
            }
        }

        private void AddLocalMessage(int mapId, ChatMessage message)
        {
            //在 调用AddLocalMessage的AddMessage中添加了校验
            if (!this.Local.TryGetValue(mapId, out List<ChatMessage> messages))//若此地图没有聊天管理器，则创建一个，并添加到 本地总聊天管理器中
            {
                messages = new List<ChatMessage>();
                this.Local[mapId] = messages;
            }
            messages.Add(message);
        }

        private void AddSystemMessage(ChatMessage message)
        {
            this.System.Add(message);
        }
        private void AddWorldMessage(ChatMessage message)
        {
            this.World.Add(message);
        }

        private void AddTeamMessage(int teamId, ChatMessage message)
        {
            //在中添加了校验
            if (!this.Team.TryGetValue(teamId, out List<ChatMessage> messages))//若此队伍没有聊天管理器，则创建一个，并添加到 队伍总聊天管理器中
            {
                messages = new List<ChatMessage>();
                this.Team[teamId] = messages;
            }
            messages.Add(message);
        }

        private void AddGuildMessage(int guildId, ChatMessage message)
        {
            //在AddMessage中添加了校验
            if (!this.Guild.TryGetValue(guildId, out List<ChatMessage> messages))//若此公会没有聊天管理器，则创建一个，并添加到 公会总聊天管理器中
            {
                messages = new List<ChatMessage>();
                this.Guild[guildId] = messages;
            }
            messages.Add(message);
        }

        public int GetLocalMessages(int mapId,int idx, List<ChatMessage> result)
        {
            if(!this.Local.TryGetValue(mapId,out List<ChatMessage> messages))//若此地图无聊天管理器
            {
                return 0; //返回0，表示没有信息
            }
            return GetNewMessages(idx, result, messages);
        }

        public int GetSystemMessages(int idx, List<ChatMessage> result)
        {
            return GetNewMessages(idx, result, this.System);
        }

        public int GetWorldMessages(int idx,List<ChatMessage> result)
        {
            return GetNewMessages(idx, result, this.World);
        }

        public int GetTeamMessages(int teamId, int idx, List<ChatMessage> result)
        {
            if (!this.Team.TryGetValue(teamId, out List<ChatMessage> messages))//若此队伍无聊天管理器
            {
                return 0; //返回0，表示没有信息
            }
            return GetNewMessages(idx, result, messages);
        }

        public int GetGuildMessages(int guildId, int idx, List<ChatMessage> result)
        {
            if (!this.Guild.TryGetValue(guildId, out List<ChatMessage> messages))//若此公会无聊天管理器
            {
                return 0; //返回0，表示没有信息
            }
            return GetNewMessages(idx, result, messages);
        }

        private int GetNewMessages(int idx, List<ChatMessage> result, List<ChatMessage> messages)
        {
            //若聊天历史中，消息数量过多，只拉取最新的MaxChatRecordNums条 记录
            if (idx == 0)//第一次加载
            {
                if(messages.Count > GameDefine.MaxChatRecordNums)//若记录数量>MaxChatRecordNums, 取前MaxChatRecordNums条
                {
                    idx = messages.Count - GameDefine.MaxChatRecordNums;
                }
            }
            for (; idx < messages.Count; idx++)
            {
                result.Add(messages[idx]);
            }
            return idx;//返回 此频道中消息数量
        }
    }
}
