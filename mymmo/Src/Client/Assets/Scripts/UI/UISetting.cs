

public class UISetting : UIWindow
{
    public void ExitToCharSelect() //返回角色选择按钮
    {
        Services.UserService.Instance.SendGameLeave(); //发送离开游戏 请求
        SceneManager.Instance.LoadScene("CharSelect"); //加载角色选择场景
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Select); //播放角色选择Bgm
    }

    public void SystemConfig() //系统设置按钮
    {
        UIManager.Instance.Show<UISystemConfig>();
        this.Close();
    }

    public void ExitGame() //退出游戏按钮
    {//保证SendGameLeave 消息发送成功之后，才能执行退出游戏
        Services.UserService.Instance.SendGameLeave(true);//发送退出游戏请求，true
    }
}
