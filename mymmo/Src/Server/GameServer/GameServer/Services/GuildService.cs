
using System.Linq;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;


namespace GameServer.Services
{
    class GuildService : Singleton<GuildService>
    {
        public GuildService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildCreateRequest>(this.OnGuildCreate); //接收创建公会请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildListRequest>(this.OnGuildList);     //接收公会列表请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinRequest>(this.OnGuildJoinRequest); //接收加入公会请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildJoinResponse>(this.OnGuildJoinResponse);//接收加入公会响应
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildRequest>(this.OnGuild);                //接收公会信息请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildLeaveRequest>(this.OnGuildLeave);      //接收退出公会请求
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<GuildAdminRequest>(this.OnGuildAdmin);      //管理指令请求

        }

        public void Init()
        {
            GuildManager.Instance.Init();
        }

        //服务器收到sender 发来的创建公会请求，将处理该请求的响应 发回给sender
        private void OnGuildCreate(NetConnection<NetSession> sender, GuildCreateRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildCreate:: GuildName:{0} Id_Name:{1}_{2} ", request.GuildName, character.Id, character.Name);
            sender.Session.Response.guildCreate = new GuildCreateResponse();
            if (character.Guild != null) //已经有公会，不可创建
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "已经有公会，不可创建";
                sender.SendResponse();
                return;
            }
            if (GuildManager.Instance.CheckNameExisted(request.GuildName))//检查公会名称是否已经存在，已存在则创建失败
            {
                sender.Session.Response.guildCreate.Result = Result.Failed;
                sender.Session.Response.guildCreate.Errormsg = "公会名称已存在";
                sender.SendResponse();
                return;
            }
            GuildManager.Instance.CreateGuild(request.GuildName, request.GuildNotice, character);//公会名称、宣言、会长
            sender.Session.Response.guildCreate.guildInfo = character.Guild.GuildInfo(character);
            sender.Session.Response.guildCreate.Result = Result.Success;
            sender.SendResponse();

        }

        /// <summary>
        /// 服务器收到 申请人sender发来的 加入公会请求,返回响应给sender,并转发此请求 给会长（审查人）
        /// </summary>
        /// <param name="sender">申请人</param>
        /// <param name="request">加入公会请求</param>
        private void OnGuildJoinRequest(NetConnection<NetSession> sender, GuildJoinRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinRequest:: GuildID:{0} CharacterId_Name:{1}_{2} ", request.Apply.GuildId, request.Apply.characterId, request.Apply.Name);
            var guild = GuildManager.Instance.GetGuild(request.Apply.GuildId);
            if (guild == null) //没有找到此公会ID 对应的公会
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Failed;
                sender.Session.Response.guildJoinRes.Errormsg = "公会不存在";
                sender.SendResponse();
                return;
            }
            //客户端sender的 GuildJoinRequest请求中 只要填充公会ID, 服务器 会补充申请人信息
            request.Apply.characterId = character.Data.ID;
            request.Apply.Name = character.Data.Name;
            request.Apply.Class = character.Data.Class;
            request.Apply.Level = character.Data.Level;

            if (guild.JoinApply(request.Apply))//对该公会 第一次发送加入申请
            {
                var leader = SessionManager.Instance.GetSession(guild.Data.LeaderID);//获取会长的 session在线会话状态
                if (leader != null)//若会长在线
                {//给会长（审查人）发送 sender的加入公会请求
                    leader.Session.Response.guildJoinReq = request;
                    leader.SendResponse();
                }
            }
            else
            {
                sender.Session.Response.guildJoinRes = new GuildJoinResponse();
                sender.Session.Response.guildJoinRes.Result = Result.Failed;
                sender.Session.Response.guildJoinRes.Errormsg = "请勿重复申请";
                sender.SendResponse();
                return;
            }
        }

        //服务器收到 审查人sender发来的 加入公会响应，将其 发送给申请人requester
        private void OnGuildJoinResponse(NetConnection<NetSession> sender, GuildJoinResponse response)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildJoinResponse:: GuildID:{0} CharacterId_Name:{1}_{2} ", response.Apply.GuildId, response.Apply.characterId, response.Apply.Name);
            var guild = GuildManager.Instance.GetGuild(response.Apply.GuildId);//要加入的公会

            //if(response.Apply.Result == ApplyResult.Accept)
            if (response.Result == Result.Success)
            {//接受了公会请求
                guild.JoinApprove(response.Apply);//公会审批
            }

            var requester = SessionManager.Instance.GetSession(response.Apply.characterId);//获取申请人的Session在线会话状态
            if (requester != null)//申请人在线
            {
                requester.Session.Character.Guild = guild;
                requester.Session.Response.guildJoinRes = response;
                requester.Session.Response.guildJoinRes.Result = Result.Success;
                requester.Session.Response.guildJoinRes.Errormsg = "加入公会成功";
                //requester.Session.Response.guildJoinRes.Apply = response.Apply;
                requester.SendResponse();
            }
        }

        //服务器接收公会列表请求,返回公会列表信息给sender
        private void OnGuildList(NetConnection<NetSession> sender, GuildListRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildList:： Id_Name:{0}_{1} ", character.Id, character.Name);
            sender.Session.Response.guildList = new GuildListResponse();
            sender.Session.Response.guildList.Guilds.AddRange(GuildManager.Instance.GetGuildsInfo());//从管理器中 获取当前所有公会的信息，添加到公会列表中
            sender.Session.Response.guildList.Result = Result.Success;
            sender.SendResponse();
        }


        //服务器收到 sender 发来的 拉取自身所在公会的信息请求，将信息填充 返回给sender
        private void OnGuild(NetConnection<NetSession> sender, GuildRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuild:: guildId:{0} ", character.Guild.Id);
            //TGuild guild = DBService.Instance.Entities.TGuilds.Where(g => g.Id == character.Guild.Id).FirstOrDefault();
            TGuild guild = DBService.Instance.Entities.TGuilds.FirstOrDefault(g => g.Id == character.Guild.Id);
            sender.Session.Response.Guild = new GuildResponse();
            sender.Session.Response.Guild.guildInfo = GuildManager.Instance.GetGuildInfo(guild);
            sender.Session.Response.Guild.Result = Result.Success;
            sender.SendResponse();
        }

        //sender 请求退出公会
        private void OnGuildLeave(NetConnection<NetSession> sender, GuildLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildLeave:: CharacterId_Name:{0}_{1} ", character.Id, character.Name);
            sender.Session.Response.guildLeave = new GuildLeaveResponse();

            if (character.Guild == null)
            {
                sender.Session.Response.guildLeave.Result = Result.Failed;
                sender.Session.Response.guildLeave.Errormsg = "您当前还没有加入公会，退会失败";
            }
            else
            {
                character.Guild.Leave(character);
                DBService.Instance.Save();//公会数据要存储在DB中（持久化数据）
                sender.Session.Response.guildLeave.Result = Result.Success;
            }

            sender.SendResponse();
        }

        //服务器 收到管理员sender 发来的管理指令
        private void OnGuildAdmin(NetConnection<NetSession> sender, GuildAdminRequest message)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnGuildAdmin:: 执行指令:{0} target:{1} ", message.Command, message.Target);
            sender.Session.Response.guildAdmin = new GuildAdminResponse();
            if(character.Guild == null)
            {
                sender.Session.Response.guildAdmin.Result = Result.Failed;
                sender.Session.Response.guildAdmin.Errormsg = "非法操作，你没有公会不要乱来";
                sender.SendResponse();
                return;
            }

            character.Guild.ExecuteAdmin(message.Command, message.Target, character.Id);//命令，目标，发出命令者
            //此处默认执行命令成功，但是可以再多一些判断

            var target = SessionManager.Instance.GetSession(message.Target);//获取对方的 session在线会话状态
            if (target != null)
            {//若目标在线，把命令执行结果发给 目标target
                target.Session.Response.guildAdmin = new GuildAdminResponse();
                target.Session.Response.guildAdmin.Result = Result.Success;
                target.Session.Response.guildAdmin.Command = message;
                target.SendResponse();
            }
            //反馈命令调用结果给 发出命令者sender
            sender.Session.Response.guildAdmin.Result = Result.Success;
            sender.Session.Response.guildAdmin.Command = message;
            sender.SendResponse();
        }

    }
}
