
using UnityEngine;
using UnityEngine.EventSystems;//处理用户输入和交互
using UnityEngine.UI;
using Managers;
using Services;

public class UIPopCharMenu : UIWindow, IDeselectHandler
{//IDeselectHandler是取消选中后的处理接口，此脚本（UIPopCharMenu）挂载的控件上，要添加 Selectable(script)（所有交互组件的基类）组件搭配使用
    /*
     * 处理当前界面是否取消了选中 ：接口IDeselectHandler的OnDeselect方法 + OnEnable中的一行代码this.GetComponent<Selectable>().Select() 
     */

    public int targetId;//聊天时，点击链接得到 目标ID、Name

    public string targetName;

    /// <summary>
    /// UIPopCharMenu弹窗 默认选中的是整个 Panel（Root)。 当点击Panel下的 子节点时，也会取消根结点的选中状态，触发OnDeselect
    /// 所以鼠标点击任何其他地方后，都会触发OnDeselect，来处理 是否要关闭弹窗。
    /// </summary>
    /// <param name="eventData">用基类BaseEventData来接收传参，传参一般是其子类，可通过下断点调试，来确认eventData的类型</param>
    public void OnDeselect(BaseEventData eventData)//处理取消选中事件（需要先选中，才有取消选中）
    {
        var ed = eventData as PointerEventData;
        //hovered是一个列表，包含了悬停栈中的所有物体，即包含指针当前停留位置下方的所有UI元素，前提：此UI元素需要 勾选Raycast Target（勾选表示鼠标点击到该物体后，不再穿透到下面的物体）
        if (ed.hovered.Contains(this.gameObject))//若hovered包含当前界面，即点击了当前界面中的某处，不关闭弹窗
        {
            return;
        }
        //保证选中的是Panel窗口之外的内容，才关闭弹窗
        this.Close(WindowResult.None);
    }

    public void OnEnable()
    {
        this.GetComponent<Selectable>().Select();//每次窗口一打开，先绑定Selectable脚本，把窗口设定为已选中状态，然后才可能触发OnDeselect
        this.Root.transform.position = Input.mousePosition + new Vector3(80, 0, 0); //点击聊天玩家 的弹窗,在点击位置的右侧80个单位处弹出
    }

    public void OnChat()//弹窗 中的私聊按钮
    {
        //私聊入口
        ChatManager.Instance.StartPrivateChat(targetId, targetName); //私聊时，传递ID、Name
        this.Close(WindowResult.NO);
    }

    public void OnAddFriend()//添加好友
    {
        //发送添加好友请求，入口
        FriendService.Instance.SendFriendAddRequest(targetId, targetName);
        this.Close(WindowResult.NO);
    }

    public void OnInviteTeam()//邀请组队
    {
        //发送邀请组队请求，入口
        TeamService.Instance.SendTeamInviteRequest(targetId, targetName);
        this.Close(WindowResult.NO);
    }

}
