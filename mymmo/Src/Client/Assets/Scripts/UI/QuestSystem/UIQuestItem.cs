
using UnityEngine;
using UnityEngine.UI;
using Models;
using Common.Data;

public class UIQuestItem : ListView.ListViewItem {

	//任务项之间 会发生来回的切换选中，让上级ListView任务列表 来关注选中、切换事件
	public Text title;

	public Image background;
	public Sprite normalBg;
	public Sprite selectedBg;

	public Quest quest;

	public override void onSelected(bool selected) //选中高亮
    {
		this.background.overrideSprite = selected ? selectedBg : normalBg;
    }

	public void SetQuestItem(Quest item)
    {
		this.quest = item;
		if (this.title != null)
        {
			if (item.Define.Type == QuestType.Main)
			{
				this.title.text = "<color=cyan>[主线]</color>" + this.quest.Define.Name; //显示任务名称
			}
            else
            {
				this.title.text = "<color=green>[支线]</color>" + this.quest.Define.Name; //显示任务名称
			}
		}
    }

}
