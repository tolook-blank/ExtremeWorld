using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HideUISlider : MonoBehaviour
{
    //侧边栏隐藏UI功能

    public GameObject UIPanel; //隐藏的UI界面
    void Start()
    {
        UIPanel.SetActive(true); //开始要启用，后面可以再隐藏
    }

    public void ShowHideUIPanel()
    {
        if (UIPanel != null)
        {
            Animator animator = UIPanel.GetComponent<Animator>(); //获取该UI界面上的动画器
            if (animator != null)
            {   //切换当前 打开/隐藏状态
                animator.SetBool("IsShow", !animator.GetBool("IsShow")); //IsShow为true打开/ false隐藏
            }
        }
    }

}
