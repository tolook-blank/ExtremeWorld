using System;
using System.Collections.Generic;
using UnityEngine;

using Network;
using SkillBridge.Message;
using Managers;
using UnityEngine.Events;
using Models;

namespace Services
{
    class GuildService : Singleton<GuildService>, IDisposable
    {
        public UnityAction OnGuildUpdate; //监听 公会更新事件
        public UnityAction<bool> OnGuildCreateResult; //监听 创建公会结果 事件
        public UnityAction<List<NGuildInfo>> OnGuildListResult; //监听 公会列表请求更新 事件

        public void Init()
        {

        }

        public GuildService()
        {
            MessageDistributer.Instance.Subscribe<GuildCreateResponse>(this.OnGuildCreate); //接收创建公会响应
            MessageDistributer.Instance.Subscribe<GuildListResponse>(this.OnGuildList);     //接收请求公会列表响应
            MessageDistributer.Instance.Subscribe<GuildJoinRequest>(this.OnGuildJoinRequest); //接收加入公会请求
            MessageDistributer.Instance.Subscribe<GuildJoinResponse>(this.OnGuildJoinResponse);//接收加入公会响应
            MessageDistributer.Instance.Subscribe<GuildResponse>(this.OnGuild);                //接收公会信息响应
            MessageDistributer.Instance.Subscribe<GuildLeaveResponse>(this.OnGuildLeave);      //接收退出公会响应
            MessageDistributer.Instance.Subscribe<GuildAdminResponse>(this.OnGuildAdmin);      //公会管理响应

        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<GuildCreateResponse>(this.OnGuildCreate);
            MessageDistributer.Instance.Unsubscribe<GuildListResponse>(this.OnGuildList);
            MessageDistributer.Instance.Unsubscribe<GuildJoinRequest>(this.OnGuildJoinRequest);
            MessageDistributer.Instance.Unsubscribe<GuildJoinResponse>(this.OnGuildJoinResponse);
            MessageDistributer.Instance.Unsubscribe<GuildResponse>(this.OnGuild);
            MessageDistributer.Instance.Unsubscribe<GuildLeaveResponse>(this.OnGuildLeave);
            MessageDistributer.Instance.Unsubscribe<GuildAdminResponse>(this.OnGuildAdmin);

        }

        /// <summary>
        /// 发送创建公会
        /// </summary>
        /// <param name="guildName">公会名称 </param>
        /// <param name="notice"> 公会宣言</param>
        public void SendGuildCreate(string guildName, string notice)
        {
            Debug.Log("SendGuildCreateRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildCreate = new GuildCreateRequest();
            message.Request.guildCreate.GuildName = guildName;
            message.Request.guildCreate.GuildNotice = notice;
            NetClient.Instance.SendMessage(message);
        }

        //收到服务器返回的 公会创建响应
        private void OnGuildCreate(object sender, GuildCreateResponse response)
        {
            Debug.LogFormat("OnGuildCreateResponse:{0}", response.Result);
            if (OnGuildCreateResult != null)
            {
                this.OnGuildCreateResult(response.Result == Result.Success);
            }
            if (response.Result == Result.Success)
            {
                GuildManager.Instance.Init(response.guildInfo);
                MessageBox.Show(string.Format("{0} 公会创建成功", response.guildInfo.GuildName), "公会");
            }
            else
                MessageBox.Show(string.Format("{0} 公会创建失败", response.guildInfo.GuildName), "公会");
        }

        /// <summary>
        /// 申请人 发送 加入公会请求
        /// </summary>
        /// <param name="guildId">公会ID</param>
        public void SendGuildJoinRequest(int guildId)
        {
            Debug.Log("SendGuildJoinRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildJoinReq = new GuildJoinRequest();
            message.Request.guildJoinReq.Apply = new NGuildApplyInfo();
            message.Request.guildJoinReq.Apply.GuildId = guildId;
            NetClient.Instance.SendMessage(message);
        }

        //申请人 收到加入公会响应
        private void OnGuildJoinResponse(object sender, GuildJoinResponse response)
        {
            Debug.LogFormat("OnGuildJoinResponse:{0}", response.Result);
            if (response.Result == Result.Success)
            {
                MessageBox.Show("加入公会成功", "公会");
            }
            else
                MessageBox.Show("加入公会失败", "公会");

        }

        /// <summary>
        /// （审批员）发送 加入公会审批请求，在 审批公会申请时，点击UIGuildApplyItem的通过/拒绝的按钮,来调用此函数 发送此审批请求到服务器
        /// </summary>
        /// <param name="accept"></param>
        /// <param name="apply"></param>
        public void SendGuildJoinApply(bool accept, NGuildApplyInfo apply)
        {
            Debug.Log("SendGuildJoinApply");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildJoinRes = new GuildJoinResponse();
            message.Request.guildJoinRes.Result = Result.Success;
            message.Request.guildJoinRes.Apply = apply;
            message.Request.guildJoinRes.Apply.Result = accept ? ApplyResult.Accept : ApplyResult.Reject;//审批结果
            NetClient.Instance.SendMessage(message);
        }


        /// <summary>
        /// (审批员)发送处理加入公会请求的 响应
        /// </summary>
        /// <param name="accept">审批员 是否同意此加入申请</param>
        /// <param name="request">加入公会请求（请帖） </param>
        public void SendGuildJoinResponse(bool accept, GuildJoinRequest request)
        {
            Debug.Log("SendGuildJoinResponse");
            NetMessage message = new NetMessage();
            message.Response = new NetMessageResponse();
            message.Response.guildJoinRes = new GuildJoinResponse();
            message.Response.guildJoinRes.Result = Result.Success;
            message.Response.guildJoinRes.Apply = request.Apply;
            message.Response.guildJoinRes.Apply.Result = accept ? ApplyResult.Accept : ApplyResult.Reject;//审批结果
            NetClient.Instance.SendMessage(message);
        }

        //（审批员）收到（服务器转发的）申请人发来的加入公会请求
        private void OnGuildJoinRequest(object sender, GuildJoinRequest request)
        {
            var confirm = MessageBox.Show(string.Format("{0} 申请加入公会", request.Apply.Name), "公会申请", MessageBoxType.Confirm, "同意", "拒绝");
            confirm.OnYes = () =>
            {
                this.SendGuildJoinResponse(true, request);//若同意，管理员 发送 同意加入公会响应
            };
            confirm.OnNo = () =>
            {
                this.SendGuildJoinResponse(false, request);
            };
        }


        /// <summary>
        /// 请求 公会列表信息
        /// </summary>
        public void SendGuildListRequest()
        {

            Debug.Log("SendGuildListRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildList = new GuildListRequest();
            NetClient.Instance.SendMessage(message);
        }

        //收到 服务器返回的公会列表信息响应
        private void OnGuildList(object sender, GuildListResponse response)
        {
            if (OnGuildListResult != null) //通知 监听公会列表刷新的对象(UIGuildList)
            {
                this.OnGuildListResult(response.Guilds);
            }
        }

        /// <summary>
        /// 请求 退出公会
        /// </summary>
        public void SendGuildLeaveRequest()
        {
            Debug.Log("SendGuildLeaveRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildLeave = new GuildLeaveRequest();
            NetClient.Instance.SendMessage(message);
        }

        //收到退出公会响应
        private void OnGuildLeave(object sender, GuildLeaveResponse response)
        {
            if (response.Result == Result.Success)
            {
                GuildManager.Instance.Init(null);//置空当前角色的公会信息
                MessageBox.Show("退出成功", "公会");
            }
            else
                MessageBox.Show("退出失败", "公会", MessageBoxType.Error);
        }


        /// <summary>
        /// （前提：已经成功加入一个公会）发送 公会界面信息 请求
        /// </summary>
        public void SendGuildRequest()
        {
            Debug.Log("SendGuildRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.Guild = new GuildRequest();
            NetClient.Instance.SendMessage(message);
        }

        //收到 服务器发来的 公会信息响应
        private void OnGuild(object sender, GuildResponse response)
        {
            Debug.LogFormat("OnGuild：{0} {1}_{2}", response.Result, response.guildInfo.Id, response.guildInfo.GuildName);
            GuildManager.Instance.Init(response.guildInfo);//更新当前公会信息
            if (this.OnGuildUpdate != null)
                this.OnGuildUpdate();
        }


        /// <summary>
        /// 管理员 发送管理指令（共4类：T人(副会长)、晋升、罢免、转让会长）
        /// </summary>
        /// <param name="command"></param>
        /// <param name="tragetId"></param>
        public void SendAdminCommand(GuildAdminCommand command, int characterId)
        {
            Debug.Log("SendAdminCommand");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.guildAdmin = new GuildAdminRequest();
            message.Request.guildAdmin.Command = command;
            message.Request.guildAdmin.Target = characterId;
            NetClient.Instance.SendMessage(message);
        }

        private void OnGuildAdmin(object sender, GuildAdminResponse response)
        {
            Debug.LogFormat("OnGuildAdmin：{0} {1}", response.Command, response.Result);
            MessageBox.Show(string.Format("执行指令:{0} 结果:{1},{2}", response.Command, response.Result, response.Errormsg));
            if (response.Command.Command == GuildAdminCommand.Kickout && response.Command.Target == User.Instance.CurrentCharacter.Id) //自己被T
            {
                GuildManager.Instance.Init(null);//置空当前角色的公会信息
            }

        }


    }
}
