
using System.Collections.Generic;
using Common;
using GameServer.Entities;
using GameServer.Models;


namespace GameServer.Managers
{
    class TeamManager:Singleton<TeamManager>
    {
        public List<Team> Teams = new List<Team>(); //列表方便遍历
        public Dictionary<int, Team> CharacterTeams = new Dictionary<int, Team>(); //字典方便精准查询
   
        public void Init()
        {

        }

        public Team GetTeamByCharacter(int characterId)
        {
            Team team = null;
            this.CharacterTeams.TryGetValue(characterId, out team);
            return team;
        }

        public void AddTeamMember(Character inviter, Character member)//inviter 邀请member加入组队（一般队员也可邀请玩家入队）
        {
            if(inviter.Team == null)//没有队伍
            {
                inviter.Team = CreateTeam(inviter);//邀请人先创建一个队伍，成为队长

            }
            inviter.Team.AddMember(member);//队长的队伍添加成员
        }

        private Team CreateTeam(Character leader)
        {
            Team team = null;
            //创建的队伍不会销毁，所以队伍数量只会增加
            for (int i = 0; i < this.Teams.Count; i++)//遍历所有队伍
            {
                team = Teams[i];
                if(team.Members.Count == 0)//如果找到空队伍，就使用该队伍添加成员
                {
                    team.AddMember(leader);
                    return team;
                }
            }
            team = new Team(leader); //没有空队伍了，才创建新的队伍
            this.Teams.Add(team);
            team.Id = this.Teams.Count; //队伍的数量只增不减，所以队伍的Id是唯一的（同一时刻内）
            return team;
        }
    }

}
