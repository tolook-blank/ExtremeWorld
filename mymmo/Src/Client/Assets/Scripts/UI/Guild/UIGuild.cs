
using UnityEngine;
using Models;
using Managers;
using Services;
using SkillBridge.Message;

public class UIGuild : UIWindow
{
    public GameObject itemPrefab; //列表元素
    public ListView listMain; //列表框
    //public Transform itemRoot; //根节点

    public UIGuildInfo uiInfo; //显示选择的公会信息
    public UIGuildMemberItem selectedItem; //当前选中 公会成员

    public GameObject panelAdmin; //管理员面板  独有的公会管理权限
    public GameObject panelLeader;//会长面板（最高级的管理员） 独有的管理权限

    void Start()
    {
        GuildService.Instance.OnGuildUpdate += UpdateUI; //监听 公会界面信息更新
        this.listMain.onItemSelected += this.OnGuildMemberSelected;
        UpdateUI();
    }

    private void OnDestroy()
    {
        GuildService.Instance.OnGuildUpdate -= UpdateUI;
    }

    public void OnGuildMemberSelected(ListView.ListViewItem item) //选中某个公会成员 的处理事件
    {
        this.selectedItem = item as UIGuildMemberItem; //选择项
    }

    private void UpdateUI()
    {
        this.uiInfo.Info = GuildManager.Instance.guildInfo; //更新公会面板左侧的 Info面板信息

        ClearList();//清空成员列表
        InitItems();//初始化成员项
        //当前角色的职位符合，才显示对应的UI面板
        this.panelAdmin.SetActive(GuildManager.Instance.myMemberInfo.Title > GuildTitle.None); //非普通成员（即正副会长），显示管理面板
        this.panelLeader.SetActive(GuildManager.Instance.myMemberInfo.Title == GuildTitle.President);//只有会长显示
    }

    private void InitItems()
    {
        foreach (var item in GuildManager.Instance.guildInfo.Members)
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIGuildMemberItem ui = go.GetComponent<UIGuildMemberItem>();
            ui.SetGuildMemberInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    private void ClearList()
    {
        this.listMain.RemoveAll();
    }

  

    //公会管理职能按钮
    public void OnClickAppliesList()//点击查看申请列表（管理审批）
    {
        UIManager.Instance.Show<UIGuildApplyList>();
        UpdateUI();
    }

    public void OnClickLeave()//点击退出公会按钮
    {
        MessageBox.Show("确定要退出公会吗", "退出公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendGuildLeaveRequest();//当前角色退出公会
        };
    }

    public void OnClickChat()//点击私聊按钮
    {
        if (selectedItem.Info.Status == 0)
        {
            MessageBox.Show("只能私聊在线的成员");
            return;
        }
        ChatManager.Instance.StartPrivateChat(selectedItem.Info.Info.Id, selectedItem.Info.Info.Name);
    }

    public void OnClickKickout()//点击踢出成员按钮
    {
        if (selectedItem == null) //还未选中
        {
            MessageBox.Show("请选择要踢出的成员");
            return;
        }
        //若当前角色不是管理员职位，没有发送管理指令的权力， 防止非法操作
        if (GuildManager.Instance.myMemberInfo.Title == GuildTitle.None)
        {
            MessageBox.Show("您的职位权限不足");
            return;
        }
        MessageBox.Show(string.Format("确定要踢成员[{0}]出公会吗？", this.selectedItem.Info.Info.Name), "踢出公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Kickout, this.selectedItem.Info.Info.Id);
        };
    }

    public void OnClickPromote() //晋升  (职位只定了3个等级, 普通成员、副会长、会长)
    {
        if (selectedItem == null) //还未选中
        {
            MessageBox.Show("请选择要晋升的成员");
            return;
        }
        if (selectedItem.Info.Title != GuildTitle.None) //只能晋升 普通成员 为 副会长
        {
            MessageBox.Show("该成员已经是公会管理员");//会长、副会长
            return;
        }
        //若当前角色不是会长，没有发送晋升指令的权力
        if (GuildManager.Instance.myMemberInfo.Title != GuildTitle.President)
        {
            MessageBox.Show("您的职位权限不足");
            return;
        }
        MessageBox.Show(string.Format("确定要晋升成员[{0}]为副会长吗？", this.selectedItem.Info.Info.Name), "晋升职位", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Promote, this.selectedItem.Info.Info.Id);
        };
    }
    public void OnClickDepose() //罢免
    {
        if (selectedItem == null) //还未选中
        {
            MessageBox.Show("请选择要罢免的成员");
            return;
        }
        if (selectedItem.Info.Title == GuildTitle.None) //只能罢免 副会长 为 普通成员 
        {
            MessageBox.Show("该成员已经是普通成员");
            return;
        }
        if (selectedItem.Info.Title == GuildTitle.President)
        {
            MessageBox.Show("不能罢免会长哟");
            return;
        }
        //若当前角色不是会长，没有发送罢免指令的权力
        if (GuildManager.Instance.myMemberInfo.Title != GuildTitle.President)
        {
            MessageBox.Show("您的职位权限不足");
            return;
        }
        MessageBox.Show(string.Format("确定要罢免副会长[{0}]为普通成员吗？", this.selectedItem.Info.Info.Name), "罢免职位", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Depose, this.selectedItem.Info.Info.Id);
        };
    }

    public void OnClickTransfer()//转让会长
    {
        if (selectedItem == null) //还未选中
        {
            MessageBox.Show("请选择一个成员来接任会长");
            return;
        }
        //若当前角色不是会长，没有发送转让会长指令的权力
        if (GuildManager.Instance.myMemberInfo.Title != GuildTitle.President)
        {
            MessageBox.Show("您的职位权限不足");
            return;
        }
        MessageBox.Show(string.Format("确定要把会长转让给[{0}]吗？", this.selectedItem.Info.Info.Name), "转让会长", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendAdminCommand(GuildAdminCommand.Promote, this.selectedItem.Info.Info.Id);
        };
    }

    public void OnClickSetNotice()//更改宣言uiInfo.notice（属于会长职能）
    {
        if (GuildManager.Instance.myMemberInfo.Title == GuildTitle.President)//会长才允许编辑公会宣言
        {
            uiInfo.notice.readOnly = false;//将notice 从Text组件 更改为使用 InputField组件
            uiInfo.notice.Select();//设置焦点，选中输入框
            uiInfo.notice.ActivateInputField();//设置 InputField 为活跃状态并将焦点置于它上面，就可以允许用户输入文本了
            User.Instance.CurrentCharacter.Guild.Notice = uiInfo.notice.text;//将更改过的宣言 更新保存
            GuildManager.Instance.guildInfo.Notice = uiInfo.notice.text;//将更改过的宣言 更新保存
        }
        else
        {
            MessageBox.Show("您的职位权限不足, 只有会长才能更改宣言");
            return;
        }
    }


}
