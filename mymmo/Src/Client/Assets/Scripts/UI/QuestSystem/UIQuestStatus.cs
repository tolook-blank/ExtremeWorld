using Managers;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestStatus : MonoBehaviour
{//NPC任务状态图标 ，绑定在 主城中的UIWorldElementManager/UIQuestStatus

    public Image[] statusImages; //按 NpcQuestStatus枚举值定义顺序绑定，[0]None(没有图片) ,[1]Complete ,[2]Available,[3]Incomplete

    private NpcQuestStatus questStatus;//npc身上的任务状态

    public void SetQuestStatus(NpcQuestStatus status)//根据npc身上的任务状态 设置状态图标
    {
        this.questStatus = status;
        for (int i = 0; i < 4; i++)//按 NpcQuestStatus枚举值定义的顺序，[0]None(没有图片) ,[1]Complete完成 ,[2]Available可接,[3]Incomplete进行中
        {
            if (this.statusImages[i] != null)
            {
                this.statusImages[i].gameObject.SetActive(i == (int)status);
            }
        }
    }
}
