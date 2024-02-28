
using UnityEngine;

public abstract class UIWindow : MonoBehaviour
{
    //所有UI窗口界面的抽象父类，实现通用的UI功能
    public delegate void CloseHandler(UIWindow sender, WindowResult result);
    public event CloseHandler OnClose; //关闭UI窗口事件

    //例如：在聊天窗口中处理 点击玩家姓名，弹出（私聊、添加好友、邀请组队）窗口时可用到Root
    public GameObject Root;//每个UI窗口都有一个Panel，Root 代表该 Panel

    public virtual System.Type Type
    {
        get
        {
            return this.GetType();
        }
    }
    public enum WindowResult//内置结果类型
    {
        None = 0,
        Yes,
        NO
    }

    public void Close(WindowResult result = WindowResult.None)
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);
        UIManager.Instance.Close(this.Type);//调用UIManager关闭UI窗口
        if (this.OnClose != null)
        {
            this.OnClose(this, result);
        }
        this.OnClose = null;
    }
    public virtual void OnCloseClick()//关闭
    {
        this.Close();
    }

    public virtual void OnYesClick()//确认
    {
        this.Close(WindowResult.Yes);
        this.Close();
    }

    public virtual void OnNoClick()//取消
    {
        this.Close(WindowResult.NO);
        this.Close();
    }

    void OnMouseDown()//临时测试函数
    {
        Debug.LogFormat("{0} Clicked", this.name);
    }
}
