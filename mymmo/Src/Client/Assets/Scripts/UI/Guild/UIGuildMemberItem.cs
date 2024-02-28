
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;
using Common.Utils;


public class UIGuildMemberItem : ListView.ListViewItem
{

    public Text nickname;
    public Text level;
    public Text @class;
    public Text title;//职位
    public Text joinTime;//入会时间
    public Text status;

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    public NGuildMemberInfo Info; //当前公会成员信息
    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

    public void SetGuildMemberInfo(NGuildMemberInfo item)
    {
        this.Info = item;
        if (this.nickname != null) this.nickname.text = this.Info.Info.Name;
        if (this.@class != null) this.@class.text = this.Info.Info.Class.ToString();
        if (this.level != null) this.level.text = this.Info.Info.Level.ToString();
        if (this.title != null) this.title.text = this.Info.Title.ToString();
        if (this.joinTime != null) this.joinTime.text = TimeUtil.GetTime(this.Info.joinTime).ToShortDateString();
        if (this.status != null) this.status.text = this.Info.Status == 1 ? "在线" : TimeUtil.GetTime(this.Info.lastTime).ToShortDateString();
    }

}
