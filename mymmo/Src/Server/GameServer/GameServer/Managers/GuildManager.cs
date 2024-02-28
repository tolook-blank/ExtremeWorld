using System;
using System.Collections.Generic;
using Common;
using Common.Utils;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Models;
using GameServer.Services;
using System.Linq;

namespace GameServer.Managers
{
    class GuildManager : Singleton<GuildManager>
    {
        //因为公会中的操作比较频繁，若每次都读取数据库，会大量耗费资源；不如读取到本地内存中，使用管理器维护公会数据
        public Dictionary<int, Guild> Guilds = new Dictionary<int, Guild>();//Guild_ID,Guild，维护公会信息
        HashSet<string> GuildNames = new HashSet<string>();//HashSet，具有高效的查询效率（应用于重名检测）

        public void Init()
        {
            this.Guilds.Clear();
            GuildNames.Clear();
            foreach (var guild in DBService.Instance.Entities.TGuilds)//遍历数据库中所有公会
            {
                this.AddGuild(new Guild(guild));//加载到内存中，提升存储效率（商业中使用memory cash 或 redis处理）
            }
        }

        private void AddGuild(Guild guild)
        {
            if (!Guilds.ContainsKey(guild.Id)) //若管理器中没有添加过此公会
            {
                this.Guilds.Add(guild.Id, guild);//添加到本地内存的公会管理器中
                this.GuildNames.Add(guild.Name);
                guild.timestamp = TimeUtil.timestamp;
            }
        }

        public bool CheckNameExisted(string guildName)
        {
            return GuildNames.Contains(guildName);//使用HashSet，提升查询效率
        }

        public bool CreateGuild(string guildName, string guildNotice, Character leader)//会长创建公会
        {
            DateTime now = DateTime.Now;
            TGuild dbGuild = DBService.Instance.Entities.TGuilds.Create();
            dbGuild.Name = guildName;
            dbGuild.Notice = guildNotice;
            dbGuild.LeaderID = leader.Id;
            dbGuild.LeaderName = leader.Name;
            dbGuild.CreateDate = now;
            DBService.Instance.Entities.TGuilds.Add(dbGuild);//将创建的公会信息 添加到DB_公会表中
            DBService.Instance.Save();
            //需要先Save()一次，让数据库生成 公会的ID,再从DB中读取出来；否则，dbGuild.Id = 0 -> guild.Id = leader.Data.GuildId = 0,这会导致Character 下次登录初始化GuildManager时出错，误以为Character没有创建公会
            TGuild GetDbGuild = DBService.Instance.Entities.TGuilds.FirstOrDefault(g => g.LeaderID == leader.Id);
            Guild guild = new Guild(GetDbGuild);
            guild.AddMember(leader.Id, leader.Name, leader.Data.Class, leader.Data.Level, GuildTitle.President);//公会，添加会长
            leader.Guild = guild;

            Log.InfoFormat("CreateGuild:: guildId:{0},leaderName:{1}", guild.Id, leader.Name);

            leader.Data.GuildId = guild.Id; //更新会长Character的guildid 
            DBService.Instance.Save();
            this.AddGuild(guild);//创建完公会，把公会信息 添加到公会管理器中
            return true;
        }

        public Guild GetGuild(int guildId)
        {
            if (guildId == 0)
            {
                return null;
            }
            Guild guild = null;
            Guilds.TryGetValue(guildId, out guild);
            return guild;
        }

        public NGuildInfo GetGuildInfo(TGuild guild)
        {
            return new NGuildInfo()
            {
                Id = guild.Id,
                GuildName = guild.Name,
                leaderId = guild.LeaderID,
                leaderName = guild.LeaderName,
                Notice = guild.Notice,
                memberCount = guild.Members.Count,
                createTime = (long)TimeUtil.GetTimestamp(guild.CreateDate)
            };
        }

        public List<NGuildInfo> GetGuildsInfo()
        {
            List<NGuildInfo> result = new List<NGuildInfo>();
            foreach (var kv in this.Guilds)
            {
                result.Add(kv.Value.GuildInfo(null));//申请加入公会时，还不是该公会的成员，from为null
            }
            return result;
        }
    }
}
