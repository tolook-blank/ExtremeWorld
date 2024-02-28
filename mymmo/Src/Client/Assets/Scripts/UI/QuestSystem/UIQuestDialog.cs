
using UnityEngine;
using Models;


public class UIQuestDialog : UIWindow {
	//点击任务NPC后弹出的，领取任务 对话框

	public UIQuestInfo questInfo;//绑定Panel
	public Quest quest; //当前的任务信息 

	public GameObject openButtons; //领取任务按钮组
	public GameObject submitButtons; //提交任务按钮组

	/*
	 enum QUEST_STATUS //任务状态
	{
		IN_PROGRESS = 0;//已接受,未完成
		COMPLETED = 1; //已完成,未提交
		FINISHED = 2; //已完成,已提交
		FAILED = 3; //任务失败
	}*/

	public void SetQuest(Quest quest) //设置任务信息
    {
		this.quest = quest;//保存当前的任务信息 
		this.UpdateQuest(); //更新任务信息，调用questInfo.SetQuestInfo(this.quest)

		if (this.quest.Info == null) //判断是否是 可接任务(新任务)，如果是
        {
			openButtons.SetActive(true); //打开 可接任务按钮组合
			submitButtons.SetActive(false); //关闭 提交任务按钮组
        }
        else //如果是进行中任务
        {
			if(this.quest.Info.Status == SkillBridge.Message.QuestStatus.Completed)//若是已完成，但未提交 的状态
			{
				openButtons.SetActive(false);
				submitButtons.SetActive(true);//打开 提交任务按钮组
            }
			else //已接受,未完成 状态
			{   //两个按钮组都不打开
				openButtons.SetActive(false);
				submitButtons.SetActive(false);
			}
        }
    }

    void UpdateQuest()
    {
        if(this.quest != null)
        {
			if(this.questInfo != null)
            {
				this.questInfo.SetQuestInfo(this.quest);
            }
        }
    }

}
