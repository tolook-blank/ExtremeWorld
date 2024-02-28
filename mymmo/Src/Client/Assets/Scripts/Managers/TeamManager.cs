
using Models;
using SkillBridge.Message;


namespace Managers
{
    class TeamManager : Singleton<TeamManager>
    {

        public void Init()
        {

        }

        public void UpdateTeamInfo(NTeamInfo team) //NTeamInfo是队伍信息，包括了 队伍ID、队长ID、队员列表信息
        {
            User.Instance.TeamInfo = team;
            ShowTeamUI(team != null && team.Members.Count != 0);
        }

        public void ShowTeamUI(bool show)
        {
            if (UIMain.Instance != null) //判断当前主场景UI是否存在（防止切换场景时报错）
            {
                UIMain.Instance.ShowTeamUI(show);
            }
        }
    }
}
