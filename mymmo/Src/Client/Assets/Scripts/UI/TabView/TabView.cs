using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TabView : MonoBehaviour
{

    public TabButton[] tabButtons;//切换页面按钮 
    public GameObject[] tabPages;//页面数组，和切换页面按钮一一对应

    public UnityAction<int> OnTabSelect;

    public int index = -1; //当前页索引
    // Use this for initialization
    IEnumerator Start () {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            tabButtons[i].tabView = this;
            tabButtons[i].tabIndex = i;
        }
        yield return new WaitForEndOfFrame(); //等一帧
        SelectTab(0); //默认先选择第一页（索引为0）
    }

    public void SelectTab(int index)//根据索引，切换到 选择页
    {
        if (this.index != index)
        {
            for (int i = 0; i < tabButtons.Length; ++i)//tabButtons.Length 一般= tabPages.Length
            {
                tabButtons[i].Select(i == index);
                if (i < tabPages.Length)
                    tabPages[i].SetActive(i == index);
            }
            if (OnTabSelect != null)
                OnTabSelect(index);
        }
    }
}
