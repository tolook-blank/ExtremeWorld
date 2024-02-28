
using UnityEngine.UI;
using SkillBridge.Message;

public class UITeamItem : ListView.ListViewItem
{
    //管理队员项
    public Text nickname;
    public Image classIcon;//职业图标
    public Image leaderIcon;

    public Image background;

    public int idx;
    public NCharacterInfo Info; //队伍中成员项（玩家）消息

    public override void onSelected(bool selected) //点击选中时触发
    {
        this.background.enabled = selected ? true : false;
    }


    void Start()
    {
        this.background.enabled = false;
    }

    public void SetMemberInfo(int idx, NCharacterInfo item, bool isLeader)//队伍列表的成员索引，成员项信息，是否是队长
    {
        this.idx = idx;
        this.Info = item;
        if (this.nickname != null) this.nickname.text = this.Info.Level.ToString().PadRight(4) + this.Info.Name; //等级 名称
        if (this.classIcon != null) this.classIcon.overrideSprite = SpriteManager.Instance.classIcons[(int)this.Info.Class - 1];//更新职业图标
        if (this.leaderIcon != null) this.leaderIcon.gameObject.SetActive(isLeader); //当isLeader是true，才启用 队长图标
    }
}
