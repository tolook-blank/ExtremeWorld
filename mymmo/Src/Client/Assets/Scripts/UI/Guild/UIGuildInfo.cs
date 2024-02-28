
using UnityEngine;
using UnityEngine.UI;
using Common;
using SkillBridge.Message;


public class UIGuildInfo : MonoBehaviour
{//公会信息
    public Text guildName;
    public Text guildID;
    public Text leaderName;
    //public Text notice;
    public InputField notice;

    public Text memberNumber;

    private NGuildInfo info;
    public NGuildInfo Info
    {
        get { return info; }
        set  
        {
            info = value;
            UpdateUI(); //放在set访问器中，赋值时能自动调用UpdateUI
        }
    }

    private void UpdateUI()
    {
        if (this.Info == null)
        {
            this.guildName.text = "无";
            this.guildID.text = "ID: 0";
            this.leaderName.text = "会长: 无";
            this.notice.text = "";
            this.memberNumber.text = string.Format("成员数量: 0/{0}", GameDefine.GuildMaxMemberCount);
        }
        else
        {
            this.guildName.text = this.Info.GuildName;
            this.guildID.text = "ID: " + this.Info.Id;
            this.leaderName.text = "会长: " + this.Info.leaderName;
            this.notice.text = this.Info.Notice;
            this.memberNumber.text = string.Format("成员数量: {0}/{1}", this.Info.memberCount, GameDefine.GuildMaxMemberCount);
        }
    }

}
