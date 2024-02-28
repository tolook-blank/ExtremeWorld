
using UnityEngine;
using UnityEngine.UI;
using Candlelight.UI;
using Managers;


public class UIChat : MonoBehaviour
{

    public HyperText textArea;//聊天内容显示区域

    public TabView channelTab;

    public InputField chatText;//聊天输入框
    public Text chatTarget;

    public Dropdown channelSelect;//下拉框

    void Start()
    {
        this.channelTab.OnTabSelect += OnDisplayChannelSelected;
        ChatManager.Instance.OnChat += RefreshUI; //一旦ChatManager调用OnChat，就会触发RefreshUI，刷新UI界面
    }

    void OnDestroy()
    {
        ChatManager.Instance.OnChat -= RefreshUI;
    }

    void Update()
    {
        InputManager.Instance.IsInputMode = chatText.isFocused;//鼠标若点击了聊天输入框（即输入框获取焦点），则是聊天输入模式
    }

    private void OnDisplayChannelSelected(int idx)//切换选择频道时，更新频道显示
    {
        ChatManager.Instance.displayChannel = (ChatManager.LocalChannel)idx;//枚举值的顺序 和 UI中顺序一致
        RefreshUI();
    }

    public void RefreshUI()//发送消息、切换频道都会刷新UI
    {
        this.textArea.text = ChatManager.Instance.GetCurrentMessage();//给HyperText图文控件 赋值
        this.channelSelect.value = (int)ChatManager.Instance.sendChannel - 1;
        if (ChatManager.Instance.SendChannel == SkillBridge.Message.ChatChannel.Private) //若当前的发送频道 是私聊频道
        {
            this.chatTarget.gameObject.SetActive(true);//激活显示 聊天目标框
            if (ChatManager.Instance.PrivateID != 0)//若私聊对象存在，显示聊天框
            {
                this.chatTarget.text = ChatManager.Instance.PrivateName + ":";//
            }
            else
                this.chatTarget.text = "<无>:";
        }
        else// 不是私聊频道，禁用聊天目标框
        {
            this.chatTarget.gameObject.SetActive(false);
        }
    }

    public void OnClickChatLink(HyperText text, HyperText.LinkInfo link) //点击链接的触发事件, 绑定在HyperText组件的Events下 的OnClick事件中
    {//玩家发送的<a> 标签超链接示例：<a name="" class="player">Hello World</a> ，只支持 name=""和 class=""，其中class定义样式（颜色、显示）
     //约定：取"c:角色ID:Name" 表示 Character的link.Name，例如：<a name="c:1:Name" class="player">Name</a>
     //约定：取"i:道具ID:Name" 表示 Item的link.Name，例如：<a name="c:1001:Name" class="item">Name</a>
        if (string.IsNullOrEmpty(link.Name))//name="link.Name"
            return;

        if (link.Name.StartsWith("c:"))//如果点击的是角色链接
        {
            string[] strs = link.Name.Split(":".ToCharArray());//以:分割，拆成：c、ID 、Name 分别存到strs[0][1][2]中
            UIPopCharMenu menu = UIManager.Instance.Show<UIPopCharMenu>(); //点击链接后，弹出菜单
            int.TryParse(strs[1], out menu.targetId);//设置弹出菜单的ID、Name
            //menu.targetId = int.Parse(strs[1]);//设置弹出菜单的ID、Name
            menu.targetName = strs[2];
        }
    }

    public void OnClickSend() //点击发送按钮
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Confirm);
        OnEndInput(this.chatText.text);
    }

    public void OnEndInput(string text)//结束输入
    {
        if (!string.IsNullOrEmpty(text.Trim())) // 去除首尾空格，输入的字符串是否为空
        {
            this.SendChat(text);//发送聊天
        }
        this.chatText.text = "";//清空聊天框
    }

    public void SendChat(string content)
    {
        ChatManager.Instance.SendChat(content);
    }

    public void OnSendChannelChanged(int idx)//下拉框设置切换发送频道。 idx从0开始，对应本地 世界 队伍 公会 私聊 ，不包含综合频道
    {
        if (ChatManager.Instance.sendChannel == (ChatManager.LocalChannel)(idx + 1))//因为不包含综合频道，需要 idx + 1 偏移
        {
            SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Confirm);
            return; //发送频道正是当前频道，不用变
        }
        if (!ChatManager.Instance.SetSendChannel((ChatManager.LocalChannel)idx + 1))//设置发送频道失败（例如：没有队伍 却设置为队伍频道）
        {
            SoundManager.Instance.PlaySound(SoundDefine.SFX_Message_Error);
            this.channelSelect.value = (int)ChatManager.Instance.sendChannel - 1;//恢复原值
        }
        else //设置发送频道成功，刷新UI
        {
            SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Confirm);
            RefreshUI();
        }
    }

}
