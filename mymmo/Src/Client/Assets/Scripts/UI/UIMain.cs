using Models;
using UnityEngine.UI;
using Managers;

public class UIMain : MonoSingleton<UIMain> {
    //作为Mono单例脚本，绑定在主城 场景中的 UIMain 游戏物体上 ，且在游戏中不会被销毁
    //UIMain管理的UI，大部分都常驻在界面中，不会销毁（可以做隐藏） 如：角色状态栏、小地图 、技能栏等等
    public Text avatarName;
    public Text avatarLevel;

    public UITeam TeamWindow;

	// Use this for initialization
	protected override void OnStart () {
        this.UpdateAvatar();
	}

    void UpdateAvatar()
    {
        this.avatarName.text = string.Format("{0}[{1}]", User.Instance.CurrentCharacter.Name, User.Instance.CurrentCharacter.Id);
        this.avatarLevel.text = User.Instance.CurrentCharacter.Level.ToString();
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //将返回角色选择  添加到设置中了，原来的按钮移除了
    //public void ExitToCharSelect() //返回角色选择按钮
    //{
    //    SceneManager.Instance.LoadScene("CharSelect"); //加载角色选择场景
    //    Services.UserService.Instance.SendGameLeave(); //发送离开游戏请求
    //}

    public void OnClickBag()
    {
        UIBag bag =  UIManager.Instance.Show<UIBag>();
    }

    public void OnClickCharEquip()
    {
        UICharEquip equip = UIManager.Instance.Show<UICharEquip>();
    }
    public void OnClickQuest()
    {
        UIQuestSystem uIQuest = UIManager.Instance.Show<UIQuestSystem>();
    }
    
    public void OnClickFriend()
    {
        UIFriends uIQuest = UIManager.Instance.Show<UIFriends>();
    }

    public void ShowTeamUI(bool show)
    {
        TeamWindow.ShowTeam(show);
    }

    public void OnClickGuild()
    {
        GuildManager.Instance.ShowGuild(); //由公会管理器调用显示UI
    }
    public void OnClickRide() //点击打开坐骑面板
    {
        UIManager.Instance.Show<UIRide>();
    }
    public void OnClickSetting()
    {
        UIManager.Instance.Show<UISetting>();
    }
    public void OnClickSkill()
    {

    }
}
