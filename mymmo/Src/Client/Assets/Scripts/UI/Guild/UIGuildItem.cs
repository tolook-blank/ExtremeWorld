
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;

public class UIGuildItem : ListView.ListViewItem
{
    public Text id;
    public Text name;
    public Text memberCount;
    public Text leadername;

    public Image background;
    public Sprite normalBg;
    public Sprite selectedBg;

    public NGuildInfo Info; 
    public override void onSelected(bool selected)
    {
        this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

    public void SetGuildInfo(NGuildInfo item)
    {
        this.Info = item;
        if (this.id != null) this.id.text = this.Info.Id.ToString();
        if (this.name != null) this.name.text = this.Info.GuildName;
        if (this.memberCount != null) this.memberCount.text = this.Info.memberCount.ToString();
        if (this.leadername != null) this.leadername.text = this.Info.leaderName;
    }
}
