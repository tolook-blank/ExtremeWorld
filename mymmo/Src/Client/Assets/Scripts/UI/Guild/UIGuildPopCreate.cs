
using UnityEngine.UI;
using Services;


public class UIGuildPopCreate : UIWindow
{
    //创建公会面板
    public InputField InputName; //公会名称
    public InputField InputNotice; //公会宣言

    void Start()
    {
        GuildService.Instance.OnGuildCreateResult += OnGuildCreated; //监听 服务器返回公会创建结果 的事件
    }

    void OnDestroy()
    {
        GuildService.Instance.OnGuildCreateResult -= OnGuildCreated;
    }

    public override void OnYesClick() //如果创建公会失败，不希望关闭创建公会面板，所以重写OnYesClick()
    {
        //一系列输入检查
        if (string.IsNullOrEmpty(InputName.text))
        {
            MessageBox.Show("请输入公会名称", "创建失败", MessageBoxType.Error);
            return;
        }
        if (InputName.text.Length < 1 || InputName.text.Length > 10)
        {
            MessageBox.Show("公会名称限定为 1-10 个字符", "创建失败", MessageBoxType.Error);
            return;
        }
        if (string.IsNullOrEmpty(InputNotice.text))
        {
            MessageBox.Show("请输入公会宣言", "创建失败", MessageBoxType.Error);
            return;
        }
        if (InputNotice.text.Length < 3 || InputNotice.text.Length > 50)
        {
            MessageBox.Show("公会宣言限定为 3-50 个字符", "创建失败", MessageBoxType.Error);
            return;
        }

        GuildService.Instance.SendGuildCreate(InputName.text, InputNotice.text); //直接 发送创建公会协议 给服务器
    }

    private void OnGuildCreated(bool result)
    {
        if (result)//当服务器返回创建成功的结果时，才把创建公会界面关闭掉；如果创建公会失败，不希望关闭创建公会面板
            this.Close(WindowResult.Yes);
    }
}
