
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;
using System;

namespace GameServer.Services
{
    class TeamService : Singleton<TeamService>
    {
        public TeamService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamInviteRequest>(this.OnTeamInviteRequest);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamInviteResponse>(this.OnTeamInviteResponse);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamLeaveRequest>(this.OnTeamLeave);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<TeamAdminRequest>(this.OnTeamAdmin);
        }



        /*邀请流程：
        A玩家发送邀请请求。
        服务器将请求转发给B玩家。
        B玩家接收邀请，可以选择同意或拒绝。
        B玩家的响应发送回服务器，服务器再将其发送给A玩家。

        退出流程：
        玩家发出退出请求。
        服务器处理退出请求，通知相关的队伍和玩家。
        退出结果发送回玩家。
         */

        public void Init()
        {
            TeamManager.Instance.Init();
        }

        //服务器收到A玩家sender 发来的组队请求，将该请求转发给玩家B
        private void OnTeamInviteRequest(NetConnection<NetSession> sender, TeamInviteRequest request)
        {
            Character character = sender.Session.Character;//A玩家的角色
            Log.InfoFormat("OnTeamInviteRequest:: FromId:{0} FromName:{1} ToId:{2} ToName:{3} ", request.FromId, request.FromName, request.ToId, request.ToName);
            //TODO:执行一些前置数据校验（待做）

            //开始逻辑
            NetConnection<NetSession> target = SessionManager.Instance.GetSession(request.ToId); //获取 被邀请人的 session在线会话状态
            if (target == null)//被邀请玩家不在线
            {
                sender.Session.Response.teamInviteRes = new TeamInviteResponse();
                sender.Session.Response.teamInviteRes.Result = Result.Failed;
                sender.Session.Response.teamInviteRes.Errormsg = "好友不在线，邀请组队失败";
                sender.SendResponse();
                return;
            }

            if (target.Session.Character.Team != null) //B玩家在线，但是已有队伍
            {
                sender.Session.Response.teamInviteRes = new TeamInviteResponse();
                sender.Session.Response.teamInviteRes.Result = Result.Failed;
                sender.Session.Response.teamInviteRes.Errormsg = "对方已有队伍，邀请组队失败";
                sender.SendResponse();
                return;
            }
            //若B玩家在线，且未加入队伍
            Log.InfoFormat("ForwardTeamInviteRequest:: FromId:{0} FromName:{1} ToId:{2} ToName:{3} ", request.FromId, request.FromName, request.ToId, request.ToName);
            target.Session.Response.teamInviteReq = request;//(协议设计点)把A玩家发来的邀请组队请求 直接发给 玩家B
            target.SendResponse(); //发送响应给B玩家
        }

        //服务器收到 玩家B sender 发送的 对A玩家邀请组队请求的响应，将该响应 发给玩家A
        private void OnTeamInviteResponse(NetConnection<NetSession> sender, TeamInviteResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnTeamInviteResponse:: characterId:{0} Result:{1} FromId:{2} ToId:{3} ", character.Id, response.Result, response.Request.FromId, response.Request.ToId);
            sender.Session.Response.teamInviteRes = response;//填充一份响应 给玩家B

            var requester = SessionManager.Instance.GetSession(response.Request.FromId);//获取 请求者A的 session在线会话状态
            if (response.Result == Result.Success)//B玩家 接受玩家A的组队请求
            {
                if (requester == null) //但是请求者玩家A下线了，发送添加失败
                {
                    sender.Session.Response.friendAddRes.Result = Result.Failed;
                    sender.Session.Response.friendAddRes.Errormsg = "请求者已下线";//发给接收者玩家B
                }
                else //双方都在线，也同意组队请求 ，添加成功
                {
                    TeamManager.Instance.AddTeamMember(requester.Session.Character, character);//请求者，接收者

                    requester.Session.Response.teamInviteRes = response;//填充一份响应 给请求者A
                    requester.Session.Response.teamInviteRes.Result = Result.Success;
                    requester.Session.Response.teamInviteRes.Errormsg = "组队成功";
                    requester.SendResponse();//发送给请求者A
                }
            }
            else
            {//若B玩家 拒绝A玩家的组队请求
                if (requester != null)//若玩家A在线，要发送添加失败响应给玩家A
                {
                    requester.Session.Response.teamInviteRes = response;
                    requester.Session.Response.teamInviteRes.Result = Result.Failed;
                    requester.Session.Response.teamInviteRes.Errormsg = "对方拒绝了您的邀请，组队失败";
                    requester.SendResponse();//发送给请求者A
                }
            }
            sender.SendResponse();//发给接收者玩家B
        }

        //服务器 收到sender的退出组队请求
        private void OnTeamLeave(NetConnection<NetSession> sender, TeamLeaveRequest request)
        {
            Character character = sender.Session.Character;//请求退出队伍的玩家
            Log.InfoFormat("OnTeamLeaveRequest: characterId:{0} TeamId:{1}  ", request.characterId, request.TeamId);
            sender.Session.Response.teamLeave = new TeamLeaveResponse();
            sender.Session.Response.teamLeave.characterId = request.characterId;
            if (sender.Session.Character.Team != null) //确认有队伍
            {
                sender.Session.Character.Team.RemoveMember(character);//请求者退出队伍
                sender.Session.Response.teamLeave.Result = Result.Success;
            }
            else
            {
                sender.Session.Response.teamLeave.Errormsg = "该玩家没有加入队伍，不能退出";
                sender.Session.Response.teamLeave.Result = Result.Failed;
            }
            sender.SendResponse();
        }


        private void OnTeamAdmin(NetConnection<NetSession> sender, TeamAdminRequest message)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnTeamAdmin:: 执行指令:{0} target:{1} ", message.Command, message.Target);
            sender.Session.Response.teamAdmin = new TeamAdminResponse();
            if (character.Team == null)
            {
                sender.Session.Response.teamAdmin.Result = Result.Failed;
                sender.Session.Response.teamAdmin.Errormsg = "非法操作，你还没有加入队伍";
                sender.SendResponse();
                return;
            }
            if (character != character.Team.Leader)
            {
                sender.Session.Response.teamAdmin.Result = Result.Failed;
                sender.Session.Response.teamAdmin.Errormsg = "非法操作，你没有队长权限";
                sender.SendResponse();
                return;
            }

            if (character.Team.ExecuteAdmin(message.Command, message.Target, character) == false) //命令，目标，发出命令者
            {
                sender.Session.Response.teamAdmin.Result = Result.Failed;
                sender.Session.Response.teamAdmin.Errormsg = "当前队伍中不存在该成员";
                sender.SendResponse();
                return;
            }

            //此处默认执行命令成功，但是可以再多一些判断
            var target = SessionManager.Instance.GetSession(message.Target);//获取对方的 session在线会话状态
            if (target != null)
            {//若目标在线，把命令执行结果发给 目标target
                target.Session.Response.teamAdmin = new TeamAdminResponse();
                target.Session.Response.teamAdmin.Result = Result.Success;
                target.Session.Response.teamAdmin.Command = message;
                //被踢出队伍后，Character.Team == null，就不会收到队伍的信息更新PostProcess, 所以主动返回一条空的teamInfo 给target 去更新队伍列表
                target.Session.Response.teamInfo = new TeamInfoResponse();
                target.SendResponse();
            }
            //反馈命令调用结果给 发出命令者sender
            sender.Session.Response.teamAdmin.Result = Result.Success;
            sender.Session.Response.teamAdmin.Command = message;
            sender.SendResponse();
        }


    }
}
