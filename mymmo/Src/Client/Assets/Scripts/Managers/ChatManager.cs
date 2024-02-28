using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using Models;
using Services;
using SkillBridge.Message;

namespace Managers
{
    class ChatManager : Singleton<ChatManager>
    {
        public enum LocalChannel
        {
            All = 0, //综合频道
            Local = 1, //本地频道
            World = 2, //世界
            Team = 3,  //队伍
            Guild = 4,  //公会
            Private = 5,//私聊
        }

        private ChatChannel[] ChannelFilter = new ChatChannel[6]
        {
            ChatChannel.Local |ChatChannel.World|ChatChannel.Guild|ChatChannel.Team|ChatChannel.Private|ChatChannel.System,//综合频道
            ChatChannel.Local,
            ChatChannel.World,
            ChatChannel.Team,
            ChatChannel.Guild,
            ChatChannel.Private,
        };

        public void StartPrivateChat(int targetId, string targetName)//发起私聊
        {
            this.PrivateID = targetId;
            this.PrivateName = targetName;

            this.sendChannel = LocalChannel.Private;//先切换成私聊频道
            if (this.OnChat != null)
                this.OnChat();//OnChat+=RefreshUI，显示私聊频道 聊天框
        }

        public List<ChatMessage>[] Messages = new List<ChatMessage>[6] //6个频道的消息列表
        {
            new List<ChatMessage>(),
            new List<ChatMessage>(),
            new List<ChatMessage>(),
            new List<ChatMessage>(),
            new List<ChatMessage>(),
            new List<ChatMessage>(),
        };

        public LocalChannel sendChannel; //当前的发送频道
        internal LocalChannel displayChannel; //当前显示的频道

        public int PrivateID = 0;
        public string PrivateName = "";

        public ChatChannel SendChannel
        {
            get
            {
                switch (sendChannel)
                {
                    case LocalChannel.Local: return ChatChannel.Local;
                    case LocalChannel.World: return ChatChannel.World;
                    case LocalChannel.Team: return ChatChannel.Team;
                    case LocalChannel.Guild: return ChatChannel.Guild;
                    case LocalChannel.Private: return ChatChannel.Private;
                }
                return ChatChannel.Local;
            }
        }

        // public UnityAction OnChat { get; internal set; } //using UnityEngine.Events;
        public Action OnChat { get; internal set; }//UIChat中 OnChat += RefreshUI，一旦调用OnChat，就会触发RefreshUI

        public void Init()
        {
            foreach (var messages in this.Messages) //每次初始化时，先清空聊天记录
            {
                messages.Clear();
            }
        }

        public void SendChat(string content, int toId = 0, string toName = "")
        {
            if(this.SendChannel == ChatChannel.Private)
            {
                toId = PrivateID;
                toName = PrivateName;
            }
            ChatService.Instance.SendChat(this.SendChannel, content, toId, toName);
        }

        public bool SetSendChannel(LocalChannel channel)//（在下拉框中任选一个频道选项）设置发送频道
        {
            if (channel == LocalChannel.Team)
            {
                if (User.Instance.TeamInfo == null)
                {
                    this.AddSystemMessage("你尚未加入任何队伍，无法使用队伍频道");
                    return false;
                }
            }
            if (channel == LocalChannel.Guild)
            {
                if (User.Instance.CurrentCharacter.Guild == null)
                {
                    this.AddSystemMessage("你尚未加入任何公会，无法使用公会频道");
                    return false;
                }
            }
            this.sendChannel = channel;
            Debug.LogFormat("Set Channel:{0}", this.sendChannel);
            return false;
        }

        public void AddMessages(ChatChannel channel, List<ChatMessage> messages)
        {
            for (int cha = 0; cha < 6; cha++)
            {
                if ((this.ChannelFilter[cha] & channel) == channel)//&运算过滤 频道（自己&自己 = 自己）
                {
                    this.Messages[cha].AddRange(messages);
                }
            }
            if (this.OnChat != null)
                this.OnChat();
        }

        public void AddSystemMessage(string message, string from = "")//系统消息归属到 所有频道
        {
            this.Messages[(int)LocalChannel.All].Add(new ChatMessage()
            {
                Channel = ChatChannel.System,
                Message = message,
                FromName = from,
            });
            if (this.OnChat != null)
                this.OnChat();
        }

        public string GetCurrentMessage()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var message in this.Messages[(int)displayChannel]) //从显示频道中 获取当前消息
            {
                sb.AppendLine(FormatMessage(message));
            }
            return sb.ToString();
        }

        private string FormatMessage(ChatMessage message)//格式化消息、加彩色标签<color=#332211>彩色字体</color>
        {
            switch (message.Channel)
            {
                case ChatChannel.Local:
                    return string.Format("[本地]{0}{1}", FormatFromPlayer(message), message.Message);
                case ChatChannel.World:
                    return string.Format("<color=cyan>[世界]{0}{1}</color>", FormatFromPlayer(message), message.Message);
                case ChatChannel.System:
                    return string.Format("<color=yellow>[系统]{0}</color>", message.Message);
                case ChatChannel.Private:
                    return string.Format("<color=magenta>[私聊]{0}{1}</color>", FormatFromPlayer(message), message.Message);
                case ChatChannel.Team:
                    return string.Format("<color=green>[队伍]{0}{1}</color>", FormatFromPlayer(message), message.Message);
                case ChatChannel.Guild:
                    return string.Format("<color=blue>[公会]{0}{1}</color>", FormatFromPlayer(message), message.Message);
            }
            return "";
        }

        private string FormatFromPlayer(ChatMessage message)//定义 来自不同玩家的格式信息，<a> 标签定义超链接
        {
            if (message.FromId == User.Instance.CurrentCharacter.Id)//自己发的消息
            {
                return "<a name =\"\" class =\"player\">[我]</a>"; //<a name ="" class ="player">[你]</a>
            }
            else //别的玩家发的消息
                return string.Format("<a name =\"c:{0}:{1}\" class =\"player\">[{1}]</a>", message.FromId, message.FromName);
        }
    }
}
