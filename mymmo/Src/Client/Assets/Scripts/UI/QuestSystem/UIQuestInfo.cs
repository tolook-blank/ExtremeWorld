using Models;
using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UIQuestInfo : MonoBehaviour
{//挂载到 任务主界面UIQuestSystem的QuestInfo面板上，当点击QuestList面板的任务时， QuestInfo面板显示对应任务信息
    //也挂载到 UIQuestDialog 的 Panel上， 在与任务NPC交互时，显示接取对话框中的 任务信息
    public Text title;
    public Text[] targets;
    public Text overview; //添加了Content Size Fitter 内容自适应组件，优点：简单易用  缺点：不支持多级嵌套，当description内容清空后，无法自动恢复原大小

    public UIIconItem rewardItems; //奖励道具
    public Text rewardMoney;  //奖励金币
    public Text rewardExp;    //奖励经验

    //public Button navButton;//领取任务后，寻找任务目标点导航
    private int npc = 0; //npcId


    public void SetQuestInfo(Quest quest) //传入任务信息 Quest
    {
        this.title.text = string.Format("[{0}]{1}", quest.Define.Type, quest.Define.Name);//例如：[MAIN]拜访埃布尔
        if (this.overview != null)
        {
            if (quest.Info == null) //任务还未接取
            {
                this.overview.text = quest.Define.Dialog; //设置任务描述（显示任务对话）
            }
            else //任务已经接取
            {
                if (quest.Info.Status == SkillBridge.Message.QuestStatus.Completed) //若任务是完成状态
                {
                    this.overview.text = quest.Define.DialogFinish; //显示任务完成对话
                }
            }
        }
        //if(quest.Define.Target1 == Common.Data.QuestTarget.None)//若任务目标为空
        //{

        //}

        this.rewardMoney.text = quest.Define.RewardGold.ToString();//任务奖励
        this.rewardExp.text = quest.Define.RewardExp.ToString();

        if (quest.Info == null)//未获取任务时
        {
            this.npc = quest.Define.AcceptNPC; //设置为接取任务npcid
        }
        else if (quest.Info.Status == SkillBridge.Message.QuestStatus.Completed)//任务完成后
        {
            this.npc = quest.Define.SubmitNPC; //设置为提交任务npc
        }
        //this.navButton.gameObject.SetActive(this.npc > 0); //根据是否有目标npc 显示寻路按钮

        foreach (var fitter in this.GetComponentsInChildren<ContentSizeFitter>()) //查找UIQuestInfo挂载的游戏物体下 的所有子节点的ContentSizeFitter自适应组件
        {
            fitter.SetLayoutVertical(); //将所有的文本内容设置完成后，强制刷新布局，适应overview的内容框变化
        }
    }

    public void OnClickAbandon()
    {

    }
    //public void OnClickNav()
    //{
    //    if(this.npc != 0)
    //    {
    //        Vector3 pos = NPCManager.Instance.GetNpcPositon(this.npc);
    //        User.Instance.CurrentCharacterObject.StartNav(pos);
    //        UIManager.Instance.Close<UIQuestSystem>();
    //    }
    //    else
    //    {
    //        MessageBox.Show("导航目标npc未设定", "导航失败");
    //    }
    //}
}
