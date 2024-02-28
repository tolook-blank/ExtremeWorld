using System;
using UnityEngine;

using Network;
using SkillBridge.Message;
using Models;
using Managers;
using UnityEngine.Events;

namespace Services
{
    class TeamService : Singleton<TeamService>, IDisposable
    {
        public UnityAction OnTeamUpdate;

        public void Init()
        {

        }

        public TeamService()
        {
            MessageDistributer.Instance.Subscribe<TeamInviteRequest>(this.OnTeamInviteRequest);
            MessageDistributer.Instance.Subscribe<TeamInviteResponse>(this.OnTeamInviteResponse);
            MessageDistributer.Instance.Subscribe<TeamInfoResponse>(this.OnTeamInfo);
            MessageDistributer.Instance.Subscribe<TeamLeaveResponse>(this.OnTeamLeave);
            MessageDistributer.Instance.Subscribe<TeamAdminResponse>(this.OnTeamAdmin);      //队伍管理响应
        }



        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<TeamInviteRequest>(this.OnTeamInviteRequest);
            MessageDistributer.Instance.Unsubscribe<TeamInviteResponse>(this.OnTeamInviteResponse);
            MessageDistributer.Instance.Unsubscribe<TeamInfoResponse>(this.OnTeamInfo);
            MessageDistributer.Instance.Unsubscribe<TeamLeaveResponse>(this.OnTeamLeave);
            MessageDistributer.Instance.Unsubscribe<TeamAdminResponse>(this.OnTeamAdmin);      //队伍管理响应
        }

        //A玩家 发送添加组队请求 给服务器,传递 受邀请玩家B的信息
        public void SendTeamInviteRequest(int friendId, string friendName)
        {
            Debug.Log("SendTeamInviteRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamInviteReq = new TeamInviteRequest();
            message.Request.teamInviteReq.FromId = User.Instance.CurrentCharacter.Id;
            message.Request.teamInviteReq.FromName = User.Instance.CurrentCharacter.Name;
            message.Request.teamInviteReq.ToId = friendId;
            message.Request.teamInviteReq.ToName = friendName;
            NetClient.Instance.SendMessage(message);
        }

        //B玩家 收到 A玩家发来的组队邀请请求，选择是否同意，调用发送对应响应的方法
        private void OnTeamInviteRequest(object sender, TeamInviteRequest request)//TeamInviteRequest request 即 A玩家发来的组队邀请请求
        {
            Debug.Log("OnTeamInviteRequest");
            var confirm = MessageBox.Show(string.Format("{0} 邀请您加入队伍", request.FromName), "组队请求", MessageBoxType.Confirm, "接受", "拒绝");
            confirm.OnYes = () =>
             {
                 this.SendTeamInviteResponse(true, request);
             };
            confirm.OnNo = () =>
            {
                this.SendTeamInviteResponse(false, request);
            };
        }

        //B玩家发送 对A玩家组队邀请的响应 给服务器,表明是否同意 A玩家的组队请求
        public void SendTeamInviteResponse(bool accpet, TeamInviteRequest request)
        {
            Debug.Log("SendTeamInviteResponse");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamInviteRes = new TeamInviteResponse();
            message.Request.teamInviteRes.Result = accpet ? Result.Success : Result.Failed;
            message.Request.teamInviteRes.Errormsg = accpet ? "同意" : "拒绝了你的邀请";
            message.Request.teamInviteRes.Request = request;//B玩家 将对应的组队邀请请求 填充到响应中， 发回给服务器
            NetClient.Instance.SendMessage(message);
        }


        //A玩家收到 B玩家 对组队邀请的响应
        private void OnTeamInviteResponse(object sender, TeamInviteResponse message)
        {
            Debug.Log("OnTeamInviteResponse");
            if (message.Result == Result.Success)
                MessageBox.Show(message.Request.ToName + "接受了您的组队邀请", "组队成功");
            else
                MessageBox.Show(message.Errormsg, "组队失败");
        }


        //收到拉取队伍列表请求的响应 （调用时，刷新列表UI）
        private void OnTeamInfo(object sender, TeamInfoResponse message)
        {
            Debug.Log("OnTeamInfo");
            TeamManager.Instance.UpdateTeamInfo(message.Team);
        }

        //(请求者)发送退出组队请求 
        public void SendTeamLeaveRequest(int teamId, int memberId) //退出的队伍ID,退出人的ID
        {
            Debug.Log("SendTeamLeaveRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamLeave = new TeamLeaveRequest();
            message.Request.teamLeave.TeamId = teamId; //队伍ID
            message.Request.teamLeave.characterId = memberId; //退出队伍的角色ID
            NetClient.Instance.SendMessage(message);
        }

        //收到退出组队响应
        private void OnTeamLeave(object sender, TeamLeaveResponse message)
        {
            var character = CharacterManager.Instance.GetCharacter(message.characterId);//前提条件：玩家退出队伍，但未下线
            if (message.Result == Result.Success)
            {
                User.Instance.TeamInfo.Members.Remove(character.Info);//退出队伍时，队伍中删除该成员的信息
                TeamManager.Instance.UpdateTeamInfo(null);
                MessageBox.Show("成功", "退出队伍");
            }
            else
                MessageBox.Show(message.Errormsg, "退出队伍", MessageBoxType.Error);
        }


        /// <summary>
        /// 队长 发送管理指令（目前共2类：T人、转让队长）
        /// </summary>
        /// <param name="command"></param>
        /// <param name="tragetId"></param>
        public void SendTeamAdminCommand(TeamAdminCommand command, int characterId)
        {
            Debug.Log("SendTeamAdminCommand");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.teamAdmin = new TeamAdminRequest();
            message.Request.teamAdmin.Command = command;
            message.Request.teamAdmin.Target = characterId;
            NetClient.Instance.SendMessage(message);
        }

        private void OnTeamAdmin(object sender, TeamAdminResponse response)
        {
            Debug.LogFormat("OnTeamAdmin：{0} {1}", response.Command, response.Result);
            MessageBox.Show(string.Format("执行指令:{0} 结果:{1},{2}", response.Command, response.Result, response.Errormsg));
            if (response.Command.Command == TeamAdminCommand.Kickout && response.Command.Target == User.Instance.CurrentCharacter.Id)
            {//如果是T人指令，且目标是自己，关闭自己的队伍面板
                TeamManager.Instance.UpdateTeamInfo(null);
            }
        }
    }
}
