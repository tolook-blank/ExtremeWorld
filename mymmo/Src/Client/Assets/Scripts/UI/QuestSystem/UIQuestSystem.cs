
using UnityEngine;
using UnityEngine.UI;
using Common.Data;
using Managers;

public class UIQuestSystem : UIWindow
{
    //任务系统主界面
    public Text title;

    public GameObject itemPrefab; //任务项

    public TabView Tabs; //进行中、可接任务 两个按钮
    public ListView listMain; //主线任务列表框
    public ListView listBranch; //支线任务列表框

    public UIQuestInfo questInfo; //嵌套脚本

    private bool showAvailableList = false; //代表 是否显示可接任务面板， 默认显示进行中的任务面板

    void Start()
    {
        //任务项之间 会发生来回的切换选中 ，让 上级ListView任务列表 来关注任务选中事件
        this.listMain.onItemSelected += this.OnQuestSelected; // 订阅主线任务列表框选中事件
        this.listBranch.onItemSelected += this.OnQuestSelected;// 支线
        this.Tabs.OnTabSelect += OnSelectTab;  //选中Tab按钮
        RefreshUI();
        //QuestManager.Instance.OnQuestChanged += RefreshUI;
    }

    void OnSelectTab(int idx)
    {
        showAvailableList = idx == 1; //1 可接任务面板， ；0 进行中任务面板
        RefreshUI();
    }

    private void OnDestroy()
    {
        //QuestManager.Instance.OnQuestChanged -= RefreshUI;
    }

    void RefreshUI() //刷新UI
    {
        ClearAllQuestList(); //先清除
        InitAllQuestItems(); //再初始化
    }

    // Update is called once per frame
    void Update()
    {

    }

    //初始化所有任务列表
    void InitAllQuestItems()
    {
        foreach (var kv in QuestManager.Instance.allQuests) //从任务管理器中 拉取所有 可用任务（包括可接取、已接取的任务）
        {
            if (showAvailableList) //如果打开的是 可接任务面板
            {
                if (kv.Value.Info != null) //若该任务NQuestInfo不为空，说明接取过此任务，不在可接任务面板中创建
                    continue;
            }
            else //如果打开的是 进行中任务面板
            {
                if (kv.Value.Info == null) //若该任务NQuestInfo为空，说明还未接取此任务，不在进行中任务面板中创建
                    continue;
            }
            //可接任务面板中创建可接任务的UIQuestItem 、进行中面板 创建已接取任务的UIQuestItem

            //实例化任务项itemPrefab，若当前遍历项是主线任务，则放在主线任务列表下
            GameObject go = Instantiate(itemPrefab, kv.Value.Define.Type == QuestType.Main ? this.listMain.transform : this.listBranch.transform);
            UIQuestItem ui = go.GetComponent<UIQuestItem>(); 
            ui.SetQuestItem(kv.Value); //再设置当前任务项的信息

            if (kv.Value.Define.Type == QuestType.Main) //若当前遍历项是主线任务
                this.listMain.AddItem(ui);//主线列表添加此任务项
            else
                this.listBranch.AddItem(ui as ListView.ListViewItem);//子类可以隐式转换为父类/基类 ，as ListView.ListViewItem可省略
        }
    }

    void ClearAllQuestList()
    {
        this.listMain.RemoveAll();
        this.listBranch.RemoveAll();
    }

    public void OnQuestSelected(ListView.ListViewItem item) //选中任务，设置 任务面板信息
    {//如果设置任务面板信息 和 切换选中列表 的顺序颠倒，会存在小BUG:只会执行到if、else if的判断语句，不会向下执行到this.questInfo.SetQuestInfo(questItem.quest)

        UIQuestItem questItem = item as UIQuestItem; // 获取选中的任务项
        this.questInfo.SetQuestInfo(questItem.quest);// 设置任务面板信息

        //切换选中列表
        if (item.owner == this.listMain)// 如果选中的是主线任务列表中的项
        {
            // 清除分支任务列表中的选中状态 ，全都设为非选中
            foreach(var uiQuestItem in listBranch.GetComponentsInChildren<UIQuestItem>())
            {
                uiQuestItem.onSelected(false);
            }
        }
        else if (item.owner == this.listBranch)// 如果选中的是分支任务列表中的项
        {
            // 清除主线任务列表中的选中状态
            foreach (var uiQuestItem in listMain.GetComponentsInChildren<UIQuestItem>())
            {
                uiQuestItem.onSelected(false);
            }
        }
    }
}
