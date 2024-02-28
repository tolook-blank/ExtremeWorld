
using UnityEngine;
using UnityEngine.UI;
using Services;
using SkillBridge.Message;

public class UILogin : MonoBehaviour {
    //登录面板
    public InputField username;    //账号
    public InputField password;    //密码
    public Button buttonLogin;     //登录
    public Button buttonRegister;  //注册

    public Toggle IsRemember; // 记住密码单选框，利用unity自带的PlayerPrefs方法实现

    // Use this for initialization
    void Start () {
        //订阅器则是一个接收事件并提供事件处理程序的对象，发布器类中的委托 调用订阅器类中的方法（事件处理程序）。
        //UILogin 告诉 UserService,我希望监听 UserService中的登陆事件，
        UserService.Instance.OnLogin = OnLogin;//订阅 UserService中的OnLogin事件

        //若想实现 保存多个账号、密码， 可使用 $"username.text"将 账号名作为 Key
        //查找key是否存在， bool exist = PlayerPrefs.HasKey("key")
        if (PlayerPrefs.HasKey("username"))
        {
            username.text = PlayerPrefs.GetString("username");//若存在，获取保存在本地的账号
        }
        if (PlayerPrefs.HasKey("password"))
        {
            password.text = PlayerPrefs.GetString("password");//获取保存在本地的密码
        }
    }


    public void OnClickLogin()
    {
        //先进行一系列的判断+登录操作
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
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        //发送登录请求
        UserService.Instance.SendLogin(this.username.text,this.password.text);

        //如果勾选了 记住密码框
        if (IsRemember.isOn == true)
        {
            //PlayerPrefs.SetString($"username.text", username.text);
            PlayerPrefs.SetString("username", username.text); //保存用户名
            PlayerPrefs.SetString("password", password.text); //保存密码
        }
        else
        {
            //PlayerPrefs.DeleteAll();//删除全部数据
            PlayerPrefs.DeleteKey("password");//否则不保存，删除保存的密码，下次登录时，需要重新输入
        }

    }

    
    void OnLogin(Result result, string message)
    {
        if (result == Result.Success)
        {
            //MessageBox.Show("登录成功,准备角色选择" + message,"提示", MessageBoxType.Information);
            //当用户登录成功，进入角色选择场景
            SceneManager.Instance.LoadScene("CharSelect");
            SoundManager.Instance.PlaySound(SoundDefine.Music_Select);
        }
        else//登录失败，报错
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }
}
