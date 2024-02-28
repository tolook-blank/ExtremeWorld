
using UnityEngine;
using Managers;
using Services;


public class UIGuildApplyList : UIWindow
{//公会申请列表，显示 公会收到的加入申请
    public GameObject itemPrefab; //列表项prefab
    public ListView listMain;  //列表框
    public Transform itemRoot; //根节点

    void Start()
    {
        GuildService.Instance.OnGuildUpdate += UpdateList; //监听加入申请列表更新
        GuildService.Instance.SendGuildListRequest(); //请求公会列表，服务器返回List<NGuildInfo>
        this.UpdateList();
    }
    private void OnDestroy()
    {
        GuildService.Instance.OnGuildUpdate -= UpdateList;
    }

    private void UpdateList()//所有列表UI的初始化，都是 先Clear、后Init
    {
        ClearList();
        InitItems();
    }

    //初始化公会申请列表 
    private void InitItems()
    {
        foreach (var item in GuildManager.Instance.guildInfo.Applies)//遍历当前公会的申请列表
        {
            GameObject go = Instantiate(itemPrefab, this.listMain.transform);//实例化每个申请项
            UIGuildApplyItem ui = go.GetComponent<UIGuildApplyItem>();
            ui.SetItemInfo(item);
            this.listMain.AddItem(ui);
        }
    }

    private void ClearList()
    {
        this.listMain.RemoveAll();
    }
}
