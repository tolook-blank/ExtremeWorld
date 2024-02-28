using System;
using System.Collections.Generic;
using System.Linq;
using SkillBridge.Message;
using Common;
using GameServer.Entities;
using GameServer.Managers;
using GameServer.Services;
using Common.Utils;



namespace GameServer.Models
{
    class Guild
    { //公会   注意：DB中，角色和公会 没有直接的连线强关联，而是通过给TCharacter增加GuildId属性，来获取角色所在公会
        public int Id; //公会ID
        public string Name { get { return this.Data.Name; } } //公会名称

        public TGuild Data; //内存中存储当前的公会数据

        public double timestamp; //时间戳，记录 公会信息发生更改的时间，是用于 判断后处理 生效的条件

        public Guild(TGuild guild)//读取DB中的公会数据，加载到内存Data中
        {
            this.Data = guild;
            this.Id = guild.Id;
        }

        public bool JoinApply(NGuildApplyInfo apply) //公会是否接收申请， 设定：同一时刻内，玩家对 同个公会的入会申请 只能存在一个
        {
            //先DB中 查看该玩家 是否已经有过申请记录
            var oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId);
            if (oldApply != null)
            {
                //之前处理：查到申请过，直接返回
                this.Data.Applies.Remove(oldApply);
                DBService.Instance.Entities.TGuildApplies.Remove(oldApply);//现在改为：先删除老的记录
            }
            //可以添加 公会申请限制校验：等级、战力等等

            //将新申请记录 插入DB中
            var dbApply = DBService.Instance.Entities.TGuildApplies.Create();//创建 申请信息实体
            dbApply.GuildId = apply.GuildId; //公会ID
            dbApply.CharacterId = apply.characterId; //玩家ID
            dbApply.Name = apply.Name; //玩家名称
            dbApply.Class = apply.Class; //玩家职业
            dbApply.Level = apply.Level;
            dbApply.ApplyTime = DateTime.Now;//入会申请时间

            DBService.Instance.Entities.TGuildApplies.Add(dbApply);//插入DB的公会申请列表中
            this.Data.Applies.Add(dbApply); //再添加到 当前公会中的申请列表

            DBService.Instance.Save();
            this.timestamp = TimeUtil.timestamp;//更新时间戳
            return true;
        }
        
        //审批申请
        public bool JoinApprove(NGuildApplyInfo apply)
        {
            //先DB中 查看该玩家 是否已经有过申请记录 且 还未处理
            TGuildApply oldApply = this.Data.Applies.FirstOrDefault(v => v.CharacterId == apply.characterId && v.Result == 0);//=0为未处理状态
            if (oldApply == null)//无申请则返回
                return false;

            oldApply.Result = (int)apply.Result;
            if (apply.Result == ApplyResult.Accept)//若审批通过，同意入会
            {
                this.AddMember(apply.characterId, apply.Name, apply.Class, apply.Level, GuildTitle.None);
            }

            DBService.Instance.Save();
            this.timestamp = TimeUtil.timestamp;//更新时间戳
            return true;
        }

        public void AddMember(int characterId, string name, int @class, int level, GuildTitle title)//公会添加成员
        {
            DateTime now = DateTime.Now;
            TGuildMember dbMember = new TGuildMember()//new对象的同时赋值, 也可以 new完后再赋值 如dbMember.CharacterId = characterId;
            {
                CharacterId = characterId,
                Name = name,
                Class = @class,
                Level = level,
                Title = (int)title,
                JoinTime = now,
                LastTime = now
            };
            //或者用Create(), TGuildMember dbMember2 = DBService.Instance.Entities.TGuildMembers.Create();

            DBService.Instance.Entities.TGuildMembers.Add(dbMember); //插入DB的公会成员列表
            this.Data.Members.Add(dbMember);//公会添加成员 （公会知道了自己的成员是谁）
            var character = CharacterManager.Instance.GetCharacter(characterId);//找到此角色
            if (character != null)//若该角色在线
            {
                character.Data.GuildId = this.Id; //设置该玩家的公会ID，也要让成员 知道自己的公会是谁  Bug: this.Id=0?
            }
            else //若该角色离线了，将其信息更新到数据库中
            {
                //若不使用DBService.Instance.Entities，则直接执行下行Sql语句（更新角色表，设置其公会ID）
                //DBService.Instance.Entities.Database.ExecuteSqlCommand("UPDATE Characters SET GuildId = @p0 WHERE CharacterId == @p1", this.Id, characterId);
                TCharacter dbChar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == characterId);
                dbChar.GuildId = this.Id;//设置该玩家的公会ID
            }

            this.timestamp = TimeUtil.timestamp;//更新时间戳
        }

        public bool Leave(Character member) //退出公会（在线的 主动操作）
        {
            Log.InfoFormat("Leave Guild: {0}_{1}", member.Id, member.Info.Name);
            var dbMember = DBService.Instance.Entities.TGuildMembers.FirstOrDefault(g => g.CharacterId == member.Id);
            if (dbMember.Title != (int)GuildTitle.None)//有职务要先卸任, 职位为普通成员时，才能退出公会
            {
                Log.Info("职务在身，不能退出，需要先卸任");
                return false;
            }
            DBService.Instance.Entities.TGuildMembers.Remove(dbMember);
            this.Data.Members.Remove(dbMember);//公会移除该成员
            member.Data.GuildId = 0; //清空此玩家的公会ID
            this.timestamp = TimeUtil.timestamp;//更新时间戳
            return true;
        }

        public void PostProcess(Character from, NetMessageResponse message)//公会中的每个玩家 都收到公会信息变化通知，通过时间戳来实现
        {
            if (message.Guild == null)
            {
                message.Guild = new GuildResponse();
                message.Guild.Result = Result.Success;
                message.Guild.guildInfo = this.GuildInfo(from);//填充 玩家from的公会信息到，任何发给客户端的协议中
            }
        }

        public NGuildInfo GuildInfo(Character from)//获取玩家from的公会信息（根据申请人from的 公会职位 来决定返回的信息量）
        {
            NGuildInfo info = new NGuildInfo() //公会基本信息
            {
                Id = this.Id,
                GuildName = this.Name,
                Notice = this.Data.Notice,
                leaderId = this.Data.LeaderID,
                leaderName = this.Data.LeaderName,
                createTime = (long)TimeUtil.GetTimestamp(this.Data.CreateDate),
                memberCount = this.Data.Members.Count
            };

            if (from != null)//若from 是 当前公会成员 ，才能看到 Members成员信息；只有当前公会的成员 才能查看 成员信息
            {
                info.Members.AddRange(GetMemberInfos());//网络信息中，添加成员列表信息
                if (from.Id == this.Data.LeaderID)//若from 是会长，才能查看 Applies申请列表信息
                    info.Applies.AddRange(GetApplyInfos());
            }
            return info;
        }


        private List<NGuildMemberInfo> GetMemberInfos()//获取此公会中所有成员的NGuildMemberInfo， 将 数据库记录 转换成 网络协议记录
        {
            List<NGuildMemberInfo> members = new List<NGuildMemberInfo>();
            foreach (var member in this.Data.Members)//内存中的当前公会的成员
            {
                var memberInfo = new NGuildMemberInfo()//将TGuildMember 转换成 NGuildMemberInfo
                {
                    Id = member.Id,
                    characterId = member.CharacterId,
                    Title = (GuildTitle)member.Title,
                    joinTime = (long)TimeUtil.GetTimestamp(member.JoinTime),
                    lastTime = (long)TimeUtil.GetTimestamp(member.LastTime),
                };
                //安全检查
                var character = CharacterManager.Instance.GetCharacter(member.CharacterId);//用于判断此成员是否在线
                if (character != null)//若此成员在线
                {
                    memberInfo.Info = character.GetBasicInfo();
                    memberInfo.Status = 1;//在线状态
                    member.Level = character.Data.Level; //更新公会中的成员信息，同步角色的当前信息
                    member.Name = character.Data.Name;
                    member.LastTime = DateTime.Now; //上次在线时间
                }
                else
                {
                    memberInfo.Info = this.GetMemberInfo(member);
                    memberInfo.Status = 0;//离线状态
                }
                members.Add(memberInfo);
            }
            return members;
        }

        private NCharacterInfo GetMemberInfo(TGuildMember member) //获取单个公会成员的角色信息
        {
            return new NCharacterInfo()
            {
                Id = member.CharacterId,
                Name = member.Name,
                Class = (CharacterClass)member.Class,
                Level = member.Level
            };
        }

        private List<NGuildApplyInfo> GetApplyInfos()//将 数据库记录 转换成 网络协议记录
        {
            List<NGuildApplyInfo> applies = new List<NGuildApplyInfo>();
            foreach (var apply in this.Data.Applies)//遍历DB中的申请列表
            {
                if (apply.Result != (int)ApplyResult.None) continue; //如果此申请已经 被通过/拒绝了，就不再发给客户端，只有待处理的才发送
                applies.Add(new NGuildApplyInfo() //将TGuildApply 转换成 NGuildApplyInfo
                {
                    characterId = apply.CharacterId,
                    GuildId = apply.GuildId,
                    Name = apply.Name,
                    Class = apply.Class,
                    Level = apply.Level,
                    Result = (ApplyResult)apply.Result
                });
            }
            return applies;
        }

        TGuildMember GetDBMember(int characterId)//获取 characterId对应数据库中的公会成员
        {
            foreach (var member in this.Data.Members)
            {
                if (member.CharacterId == characterId)
                {
                    return member;
                }
            }
            return null;
        }

        public void ExecuteAdmin(GuildAdminCommand command, int targetId, int sourceId) //命令，目标ID，发出命令者ID
        {//管理：修改成员的信息。 且无论该成员是否在线，都要保证生效，所以 要直接操作数据库
            var target = GetDBMember(targetId);
            var source = GetDBMember(sourceId);
            switch (command)
            {
                case GuildAdminCommand.Promote:
                    target.Title = (int)GuildTitle.VicePresident; //目标从普通成员 晋升为 副会长
                    break;
                case GuildAdminCommand.Depose:
                    target.Title = (int)GuildTitle.None; //目标从副会长 降职为 普通成员 
                    break;
                case GuildAdminCommand.Transfer:
                    target.Title = (int)GuildTitle.President; //目标成为会长
                    source.Title = (int)GuildTitle.None; //自己退休为 普通成员
                    this.Data.LeaderID = targetId; //更改公会的 会长ID、name
                    this.Data.LeaderName = target.Name;
                    break;
                case GuildAdminCommand.Kickout:
                    var dbMember = DBService.Instance.Entities.TGuildMembers.FirstOrDefault(g => g.CharacterId == targetId);
                    DBService.Instance.Entities.TGuildMembers.Remove(dbMember);
                    this.Data.Members.Remove(dbMember);//公会中，移除该玩家信息
                    var character = CharacterManager.Instance.GetCharacter(targetId);//角色管理器中 找到此角色
                    if (character != null)//该玩家在线
                    {
                        character.Data.GuildId = 0; //直接清除玩家的公会ID
                        //character.Info.Guild.Id = 0;
                    }
                    else//若该玩家已经离线
                    {
                        TCharacter dbChar = DBService.Instance.Entities.Characters.SingleOrDefault(c => c.ID == targetId);//查找DB
                        dbChar.GuildId = 0;//清除玩家的公会ID
                    }
                    break;
            }
            DBService.Instance.Save();
            this.timestamp = TimeUtil.timestamp;//更新时间戳 (每个管理命令 都会更改公会信息)
        }
    }
}
