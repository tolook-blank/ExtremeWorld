
using UnityEngine;
using Models;
using Managers;
using Services;

public class UIFriends : UIWindow
{

    public GameObject itemPrefab; //列表元素
    public ListView listMain; //列表框
    public Transform itemRoot; //根节点
    public UIFriendItem selectedItem; //当前选中项

    void Start()
    {
        FriendService.Instance.OnFriendUpdate = RefreshUI; //委托调用时，更新UI
        this.listMain.onItemSelected += this.OnFriendSelected;
        RefreshUI();
    }

    private void OnDestroy()
    {
        FriendService.Instance.OnFriendUpdate -= RefreshUI;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnFriendSelected(ListView.ListViewItem item) //选中好友处理事件
    {
        this.selectedItem = item as UIFriendItem; //选择项
    }

    public void OnClickFriendAdd()//点击添加好友按钮
    {
        InputBox.Show("输入要添加的好友名称或ID", "添加好友").OnSubmit += OnFriendAddSubmit; //当点击输入框的 确定时，执行OnFriendAddSubmit
    }

    private bool OnFriendAddSubmit(string input, out string tips)
    {
        tips = "";
        int friendId = 0;
        string friendName = "";
        if (!int.TryParse(input, out friendId)) //若输入的不是ID(不能转成int类型),则输入了好友名称
            friendName = input;
        if (friendId == User.Instance.CurrentCharacter.Id || friendName == User.Instance.CurrentCharacter.Name)
        {
            tips = "不能添加自己为好友";
            return false;
        }

        FriendService.Instance.SendFriendAddRequest(friendId, friendName);
        return true;
    }

    public void OnClickFriendChat()//点击私聊按钮
    {
        if(selectedItem.Info.Status == 0)
        {
            MessageBox.Show("只能私聊在线的好友");
            return;
        }
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        ChatManager.Instance.StartPrivateChat(selectedItem.Info.friendInfo.Id, selectedItem.Info.friendInfo.Name);
    }

    public void OnClickFriendRemove()//点击删除好友按钮
    {
        if (selectedItem == null) //还未选中
        {
            MessageBox.Show("请选择要删除的好友");
            return;
        }
        MessageBox.Show(string.Format("确定要删除好友[{0}]吗？", selectedItem.Info.friendInfo.Name), "删除好友", MessageBoxType.Confirm, "删除", "取消").OnYes = () =>
        {
            FriendService.Instance.SendFriendRemoveRequest(this.selectedItem.Info.Id, this.selectedItem.Info.friendInfo.Id);
        };

    }

    public void OnClickFriendTeamInvite() //邀请好友组队
    {
        if (selectedItem == null)
        {
            MessageBox.Show("请选择要邀请的好友");
            return;
        }
        if (selectedItem.Info.Status == 0)
        {
            MessageBox.Show("只能邀请在线的好友");
            return;
        }
        MessageBox.Show(string.Format("确定邀请好友[{0}]加入队伍吗？", selectedItem.Info.friendInfo.Name), "邀请好友组队", MessageBoxType.Confirm, "邀请", "取消").OnYes = () =>
        {
            TeamService.Instance.SendTeamInviteRequest(this.selectedItem.Info.friendInfo.Id, this.selectedItem.Info.friendInfo.Name);
        };
    }

    private void RefreshUI()
    {
        ClearFriendList();
        InitFriendItems();
    }

    private void InitFriendItems()
    {
        foreach (var item in FriendManager.Instance.allFriends)
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIFriendItem ui = go.GetComponent<UIFriendItem>();
            ui.SetFriendInfo(item);
            this.listMain.AddItem(ui);
        }

    }

    private void ClearFriendList()
    {
        this.listMain.RemoveAll();
    }

}
