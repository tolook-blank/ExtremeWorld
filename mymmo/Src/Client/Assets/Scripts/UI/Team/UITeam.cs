
using UnityEngine;
using UnityEngine.UI;
using Models;
using Services;


public class UITeam : MonoBehaviour
{
    //管理队伍面板
    public Text teamTitle;
    public UITeamItem[] Members; //队伍固定有5个成员位置（通过启用、禁止，显示队友人数）
    public ListView list;

    public UITeamItem selectedItem; //当前选中项（UIFriends同款）

    void Start()
    {
        this.list.onItemSelected += this.OnTeamSelected;
        if (User.Instance.TeamInfo == null) //队伍信息为空
        {
            this.gameObject.SetActive(false);//则隐藏队伍面板
            return;
        }
        foreach (var item in Members)
        {
            this.list.AddItem(item); //初始化List列表框
        }
    }

    public void OnTeamSelected(ListView.ListViewItem item) //选中队员处理事件
    {
        this.selectedItem = item as UITeamItem; //选择项
    }

    void OnEnable()
    {
        UpdateTeamUI();
    }

    void OnDisable()
    {
        UpdateTeamUI();
    }

    public void ShowTeam(bool show)
    {
        this.gameObject.SetActive(show);//显示组队面板
        if (show)//show为true时
        {
            UpdateTeamUI();//刷新UI
        }
    }
    private void UpdateTeamUI()
    {
        if (User.Instance.TeamInfo == null) return;
        this.teamTitle.text = string.Format("我的队伍 {0}/5", User.Instance.TeamInfo.Members.Count);

        for (int i = 0; i < 5; ++i)//写死，队伍中只有5个位置
        {
            if (i < User.Instance.TeamInfo.Members.Count) // 出现BUG：队员OnClickLeave后，其他成员的队伍面板中，依然显示（已经退出的成员），且已经退出的成员的状态是 已有队伍
            {
                this.Members[i].SetMemberInfo(i, User.Instance.TeamInfo.Members[i], User.Instance.TeamInfo.Members[i].Id == User.Instance.TeamInfo.Leader);
                this.Members[i].gameObject.SetActive(true);
            }
            else
                this.Members[i].gameObject.SetActive(false);
        }
    }

    public void OnClickLeave() //退出队伍按钮
    {
        MessageBox.Show("确定要退出队伍吗", "退出队伍", MessageBoxType.Confirm, "确定退出", "取消").OnYes = () =>
             {
                 TeamService.Instance.SendTeamLeaveRequest(User.Instance.TeamInfo.Id, User.Instance.CurrentCharacter.Id);//队伍Id,当前角色id
             };

        ShowTeam(false);
    }

    //补充：队长T人功能（1.确认队长身份 2.让被T玩家发送退出队伍请求）
    public void OnClickKickMember() //T人按钮
    {
        if (User.Instance.TeamInfo == null || User.Instance.TeamInfo.Leader != User.Instance.CurrentCharacter.Id)//队长才有T人权限
        {
            MessageBox.Show("不是队长，没有T人的权限哦", "失败");
            return;
        }
        if (selectedItem == null) //还未选中
        {
            MessageBox.Show("请选择 被T的倒霉蛋");
            return;
        }
        if (selectedItem.Info.Id == User.Instance.TeamInfo.Leader) //选中队长
        {
            MessageBox.Show("？你在赣神魔！");
            return;
        }
        //this.selectedItem是选中的 被T玩家
        TeamService.Instance.SendTeamAdminCommand(SkillBridge.Message.TeamAdminCommand.Kickout, this.selectedItem.Info.Id);//T人命令,被T玩家的ID
    }

    //补充：队长解散队伍功能（相当于T出所有人，清空队伍）
    public void OnClickKillTeam()
    {
        if (User.Instance.TeamInfo == null || User.Instance.TeamInfo.Leader != User.Instance.CurrentCharacter.Id)//队长才有解散队伍权限
        {
            MessageBox.Show("你不是队长，不能解散队伍", "失败");
            return;
        }
        for (int i = 0; i < User.Instance.TeamInfo.Members.Count; ++i)//T出队内所有成员，包括自己
        {
            //队长先T走所有人，再自己退出队伍;
            if (User.Instance.TeamInfo.Members[i].Id == User.Instance.TeamInfo.Leader)
            {
                continue;
            }
            TeamService.Instance.SendTeamAdminCommand(SkillBridge.Message.TeamAdminCommand.Kickout, User.Instance.TeamInfo.Members[i].Id);//T人命令,被T玩家的ID
        }
        TeamService.Instance.SendTeamLeaveRequest(User.Instance.TeamInfo.Id, User.Instance.CurrentCharacter.Id);//队伍Id,当前角色id
    }

}
