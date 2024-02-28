
using UnityEngine;
using UnityEngine.UI;
using Services;
using SkillBridge.Message;

public class UIRegister : MonoBehaviour {

    //注册面板，UI层只用负责UI逻辑
    public InputField username;
    public InputField password;
    public InputField passwordConfirm; //确认密码
    public Button buttonRegister;

    public GameObject uiLogin;
    // Use this for initialization
    void Start () {
        //订阅器则是一个接收事件并提供事件处理程序的对象，发布器类中的委托调用订阅器类中的方法（事件处理程序）。
        UserService.Instance.OnRegister = OnRegister;//订阅 UserService中的OnRegister事件
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void OnClickRegister()
    {
        if (string.IsNullOrEmpty(this.username.text))
        {
            MessageBox.Show("请输入账号");
            return;
        }
        if (string.IsNullOrEmpty(this.password.text))
        {
            MessageBox.Show("请输入密码");
            return;
        }
        if (string.IsNullOrEmpty(this.passwordConfirm.text))
        {
            MessageBox.Show("请输入确认密码");
            return;
        }
        if (this.password.text != this.passwordConfirm.text)
        {
            MessageBox.Show("两次输入的密码不一致");
            return;
        }
        //通过控制UI层，来调用 Service层
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        UserService.Instance.SendRegister(this.username.text,this.password.text);
    }


    void OnRegister(Result result, string message)
    {
        if (result == Result.Success)
        {
            //弹出注册成功 弹窗，点击OnYes确定后，执行CloseRegister 跳转到登录界面
            MessageBox.Show("注册成功,请登录", "提示", MessageBoxType.Information).OnYes = this.CloseRegister;
        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }

    void CloseRegister()//关闭当前注册页面，跳转到登录界面
    {
        this.gameObject.SetActive(false);
        uiLogin.SetActive(true);
    }
}
