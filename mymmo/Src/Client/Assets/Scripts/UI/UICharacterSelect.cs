using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;
using Services;
using SkillBridge.Message;

public class UICharacterSelect : MonoBehaviour
{//挂载在CharacterSelect场景中的UICharacterSelect物体上

    public GameObject panelCreate;//角色创建页面，需要 选择职业、输入角色名称，才能创建角色
    public GameObject panelSelect;//角色选择页面

    public GameObject btnCreateCancel;

    public InputField charName; //创建角色时，角色名称输入

    public Transform uiCharList;//角色选择滚动区，绑定 Scroll View/Viewport/Content节点,该节点添加了Vertical Layout Group组件
    public GameObject uiCharInfo;//角色信息框模板（设为禁用状态不显示），相当于预制体
    public List<GameObject> uiChars = new List<GameObject>();//滚动区中的角色信息框列表
    public UICharacterView characterView;//3D角色显示栏

    public Image[] titles; //创建角色时，显示选中的职业名称UI
    public Text description;//创建角色时,对应职业描述
    public Text[] names;//创建角色面板中，职业按钮下 显示的职业名

    private CharacterClass charClass; //角色职业
    private int selectCharacterIdx = -1;//选中的角色ID

    // Use this for initialization
    void Start()
    {
        InitCharacterSelect(true);
        UserService.Instance.OnCharacterCreate = OnCharacterCreate;//订阅 UserService的OnCharacterCreate角色创建事件
    }


    public void InitCharacterSelect(bool init)//初始化角色选择页面
    {
        panelCreate.SetActive(false);
        panelSelect.SetActive(true);//启用角色选择面板

        if (init)
        {
            foreach (var old in uiChars)//遍历滚动区中的所有角色框
            {
                Destroy(old);//先销毁旧的角色框
            }
            uiChars.Clear();//清空角色框的管理列表

            //再创建新的角色框
            for (int i = 0; i < User.Instance.Info.Player.Characters.Count; i++)//按照玩家创建的角色顺序
            {
                //uiCharList是滚动区域，用来选择角色
                GameObject go = Instantiate(uiCharInfo, this.uiCharList);//实例化uiCharInfo信息框，并设为uiCharList的子节点
                UICharInfo chrInfo = go.GetComponent<UICharInfo>();//获取信息框 的UICharInfo脚本
                chrInfo.info = User.Instance.Info.Player.Characters[i];//根据玩家创建的 角色列表顺序，给信息框赋值
                Button button = go.GetComponent<Button>();//获取信息框的Button组件
                int idx = i;//用角色序号 初始化 信息框序号idx
                //给该信息框button监听点击事件，使用 lambda表达式 来调用 带参方法OnSelectCharacter(idx)
                button.onClick.AddListener(() =>
                {
                    OnSelectCharacter(idx);//当点击此信息框，传入此角色序号，做角色选择切换
                });

                uiChars.Add(go); //添加到信息框列表中
                go.SetActive(true);//启用显示此信息框
            }

        }
    }

    //注意：假如需要拖到组件(button)中进行监听，那么函数需要定义为public
    public void OnSelectCharacter(int idx)//在角色选择面板中，选中 滚动区中的角色框
    {
        this.selectCharacterIdx = idx;//idx是选中的信息框序号 == 玩家创建的角色序号（从0开始）
        NCharacterInfo cha = User.Instance.Info.Player.Characters[idx];
        Debug.LogFormat("Select Char:[{0}]{1}[{2}]", cha.Id, cha.Name, cha.Class);//cha.Class是职业枚举值
        //User.Instance.CurrentCharacter = cha; //此时得到的NCharacterInfo cha还没有Entity_ID, 所以改为放到 OnGameEnter 获得Entity_ID后 做初始化

        characterView.CurrectCharacter = ((int)cha.Class - 1);//显示对应职业的3D角色，设置characterView.CurrectCharacter
        //由于 信息框的创建顺序 和 玩家创建的角色顺序是一 一对应的，
        for (int i = 0; i < User.Instance.Info.Player.Characters.Count; i++)//实现角色信息框选中、切换
        {
            UICharInfo ci = this.uiChars[i].GetComponent<UICharInfo>();//获取对应序号的信息框
            ci.Selected = (idx == i); //判断该框 是否选中(高亮)
        }
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

    public void OnClickPlay()//进入游戏
    {
        if (selectCharacterIdx >= 0)//如果 已选中了角色，角色序号从0开始
        {
            UserService.Instance.SendGameEnter(selectCharacterIdx); //发送 进入游戏请求，传入角色ID
        }
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

    public void InitCharacterCreate()//在角色选择页面 点击创建新角色按钮
    {
        panelCreate.SetActive(true);//切换到角色创建页面
        panelSelect.SetActive(false);
        OnSelectClass(1);//默认选择战士职业
    }

    // Update is called once per frame
    void Update()
    {

    }

    //注意：假如需要拖到组件(button)中进行监听，那么函数需要定义为public
    public void OnClickCreate()//创建角色面板中，点击开始冒险，则发送创建角色请求
    {
        if (string.IsNullOrEmpty(this.charName.text))
        {
            MessageBox.Show("请输入角色名称");
            return;
        }
        //UI层调用 逻辑层（Service）
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        UserService.Instance.SendCharacterCreate(this.charName.text, this.charClass);//发送创建角色请求，角色名称，职业类型
    }

    void OnCharacterCreate(Result result, string message)//订阅角色创建事件 的处理函数
    {
        if (result == Result.Success)
        {
            InitCharacterSelect(true);
        }
        else
            MessageBox.Show(message, "错误", MessageBoxType.Error);
    }

    //在角色创建面板的 3个button组件 的OnClick点击事件中，分别 监听OnSelectClass(1)\(2)\(3)，
    public void OnSelectClass(int charClass) //选择职业 charClass取值为1,2,3之一
    {
        this.charClass = (CharacterClass)charClass;

        characterView.CurrectCharacter = charClass - 1;//设置当前角色索引，并更新3D角色显示栏

        for (int i = 0; i < 3; i++)
        {
            titles[i].gameObject.SetActive(i == charClass - 1); //只显示职业对应的titleUI
            names[i].text = DataManager.Instance.Characters[i + 1].Name;
        }

        description.text = DataManager.Instance.Characters[charClass].Description;
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }


}
