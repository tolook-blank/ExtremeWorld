
using System.Collections.Generic;
using UnityEngine;
using Services;
using SkillBridge.Message;

public class UIGuildList : UIWindow
{//公会列表，显示可申请加入的公会
    public GameObject itemPrefab; //列表元素
    public ListView listMain; //列表框
    //public Transform itemRoot; //根节点
    
    public UIGuildInfo uiInfo; //显示选择的公会信息
    public UIGuildItem selectedItem; //当前选中项
    void Start()
    {
        this.listMain.onItemSelected += this.OnGuildMemberSelected; //监听 选中列表项
        this.uiInfo.Info = null;
        GuildService.Instance.OnGuildListResult += UpdateGuildList; //监听列表刷新，可以实现分页

        GuildService.Instance.SendGuildListRequest(); //发送请求公会列表给 服务器 ，服务器返回List<NGuildInfo>
    }

    private void OnDestroy()
    {
        GuildService.Instance.OnGuildListResult -= UpdateGuildList;
    }

    private void UpdateGuildList(List<NGuildInfo> guilds) //服务器返回guilds
    {
        ClearList();
        InitItems(guilds);
    }

    public void OnGuildMemberSelected(ListView.ListViewItem item) //选中好友处理事件
    {
        this.selectedItem = item as UIGuildItem; //设置选择公会项 为 UIGuildItem
        this.uiInfo.Info = this.selectedItem.Info; //刷新Info面板，显示选择的公会信息
    }

    //刷新公会列表 （如：获取下一页公会列表）
    private void InitItems(List<NGuildInfo> guilds)
    {
        foreach (var item in guilds)
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);
            UIGuildItem ui = go.GetComponent<UIGuildItem>();
            ui.SetGuildInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    private void ClearList()
    {
        this.listMain.RemoveAll();
    }

    public void OnClickJoin()//点击申请加入 按钮
    {
        if (selectedItem == null) //还未选中
        {
            MessageBox.Show("请选择要加入的公会");
            return;
        }
        MessageBox.Show(string.Format("确定要加入公会[{0}]吗？", selectedItem.Info.GuildName), "申请加入公会", MessageBoxType.Confirm, "确定", "取消").OnYes = () =>
        {
            GuildService.Instance.SendGuildJoinRequest(this.selectedItem.Info.Id);
        };
    }

}
