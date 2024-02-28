
using Models;
using SkillBridge.Message;


namespace Managers
{
    class GuildManager : Singleton<GuildManager>
    {
        public NGuildInfo guildInfo; //当前公会信息

        public NGuildMemberInfo myMemberInfo;//（当前游戏角色）我的公会成员信息
       
        public bool HasGuild
        {
            get { return this.guildInfo != null; }
            
        }

        public void Init(NGuildInfo guild)
        {
            this.guildInfo = guild;
            if (guild == null) //当前角色无公会，我的公会成员信息 肯定为空
            {
                myMemberInfo = null;
                return;
            }
            foreach(var mem in guild.Members)
            {
                if(mem.characterId == User.Instance.CurrentCharacter.Id)//在公会成员中，查找到我（当前控制角色）
                {
                    myMemberInfo = mem;//设置我的公会成员信息
                    break;
                }
            }
        }

        public void ShowGuild()
        {
            if (HasGuild) //若有公会
            {
                UIManager.Instance.Show<UIGuild>(); //显示公会界面
            }
            else //还无公会
            {
                var pop = UIManager.Instance.Show<UIGuildPopNoGuild>(); //显示 创建/加入公会界面
                pop.OnClose += PopNoGuild_OnClose; //监听 在创建/加入公会界面的操作（OnYesClick为创建，No为创建）
            }
        }

        private void PopNoGuild_OnClose(UIWindow sender, UIWindow.WindowResult result)
        {
            if (result == UIWindow.WindowResult.Yes) //创建公会
            {
                UIManager.Instance.Show<UIGuildPopCreate>(); //显示创建公会界面
            }
            else if (result == UIWindow.WindowResult.NO) //加入公会
            {
                UIManager.Instance.Show<UIGuildList>();//显示公会列表界面
            }
        }
        
    }
}
