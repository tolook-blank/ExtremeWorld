
using System.Collections.Generic;
using SkillBridge.Message;
using Common;
using Common.Utils;
using GameServer.Entities;

namespace GameServer.Models
{
    class Team
    { //设定：一个队伍，5个人共享。且创建的队伍不会销毁，所以队伍数量只会增加
        public int Id; //队伍ID
        public Character Leader;

        public List<Character> Members = new List<Character>();//队员列表

        public double timestamp; //时间戳，记录 队伍信息发生更改的时间（因为玩家进入队伍的时间不同，所以timestamp也不同）

        public Team(Character leader)
        {
            AddMember(leader);
        }


        public void AddMember(Character member)//member进入队伍，member的队伍面板 显示
        {
            if (this.Members.Count == 0)//第一个入队的是队长
            {
                this.Leader = member;
            }
            this.Members.Add(member);
            member.Team = this;//成员的队伍，为当前的Team
            timestamp = TimeUtil.timestamp;//有成员入队时，记录当前时间的时间戳
        }

        public void RemoveMember(Character member) //退出队伍
        {
            Log.InfoFormat("Leave Team: {0}_{1}", member.Id, member.Info.Name);
            this.Members.Remove(member);//队伍移除该成员
            if (member == this.Leader)//若是队长退出
            {
                if (this.Members.Count > 0)//若还有队员，则剩下第一位队员 成为队长
                {
                    this.Leader = this.Members[0];
                }
                else //没有队员了，只剩一个空队伍（设定机制：创建的队伍不会销毁，所以队伍数量只会增加）
                    this.Leader = null;
            }
            member.Team = null; //member的队伍面板 隐藏
            timestamp = TimeUtil.timestamp;//队员离开时，也记录当前时间的时间戳
        }

        public void PostProcess(NetMessageResponse message)//Team中 后处理要让队伍中的每个玩家 都能收到队伍信息变化通知，通过时间戳来实现，（FriendManager中的方法 只能通知到单个人）
        {
            if (message.teamInfo == null)
            {
                message.teamInfo = new TeamInfoResponse();
                message.teamInfo.Result = Result.Success;
                message.teamInfo.Team = new NTeamInfo();
                message.teamInfo.Team.Id = this.Id; //队伍ID
                message.teamInfo.Team.Leader = this.Leader.Id; //队长ID
                foreach (var member in this.Members) //成员列表
                {
                    message.teamInfo.Team.Members.Add(member.GetBasicInfo());
                }
            }
        }

        public bool ExecuteAdmin(TeamAdminCommand command, int target, Character leader)//命令，目标ID，发出命令者s
        {
            Character targetChar = leader.Team.Members.Find((Character c) => c.Id == target);//从leader的队伍中找到该成员
            if (targetChar == null) //若目标不存在，返回失败
            {
                return false;
            }
            switch (command)
            {
                case TeamAdminCommand.Kickout:
                    leader.Team.RemoveMember(targetChar);//t出该玩家
                    break;
                case TeamAdminCommand.Transfer:
                    leader.Team.Leader = targetChar; //更换队长
                    timestamp = TimeUtil.timestamp; //更新时间戳
                    break;
            }
            return true;
        }
    }
}
