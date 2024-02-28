using Services;
using SkillBridge.Message;
using UnityEngine.UI;


public class UIGuildApplyItem : ListView.ListViewItem
{

    public Text nickname;
    public Text level;
    public Text @class;

    public NGuildApplyInfo Info;//当前选中公会申请项的信息

    public void SetItemInfo(NGuildApplyInfo item)
    {
        this.Info = item;
        if (this.nickname != null) this.nickname.text = this.Info.Name;
        if (this.level != null) this.level.text = this.Info.Level.ToString();
        if (this.@class != null) this.@class.text = this.Info.Class.ToString();
    }

    public void OnAccept()
    {
        MessageBox.Show(string.Format("要通过【{0}】的公会申请吗？",this.Info.Name),"审批申请",MessageBoxType.Confirm,"同意加入","取消").OnYes = () => {
            GuildService.Instance.SendGuildJoinApply(true, this.Info);
        };
    }

    public void OnReject()
    {
        MessageBox.Show(string.Format("要拒绝【{0}】的公会申请吗？", this.Info.Name), "审批申请", MessageBoxType.Confirm, "拒绝加入", "取消").OnYes = () => {
            GuildService.Instance.SendGuildJoinApply(false, this.Info);
        };
    }
}
