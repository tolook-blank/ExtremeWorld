using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    //UIManager管理的 各个游戏系统的UI界面，当用到才打开，用完关闭
    //UITipsManager 管理动态UI
    class UIElement //UI元素类
    {
        public string Resources;//UI窗口prefab的资源路径，例如：Resources = "UI/UISetting"。动态加载的资源放在 Assets\Resources目录下
        public bool Cache; //是否开启Cache，当 Cache = true时，界面的打开、关闭，相当于启用、禁用，不会销毁
        public GameObject Instance; //当 Cache = true时，使用Instance 存放 UI实例，Instance.SetActive(true/false)控制UI启用、禁用
    }

    private Dictionary<Type, UIElement> UIResources = new Dictionary<Type, UIElement>(); //UI资源管理器

    public UIManager()//在构造函数中（调用时机靠前），初始化UI资源管理器（ 每次新增UI界面时，都需要 增加对应资源初始化逻辑）
    {
        // UIResources.Add(typeof(UINameBar), new UIElement() { Resources = "UI/UINameBar", Cache = true });
        //cache = true时，界面的打开、关闭，相当于启用、禁用，不会销毁，所以 start()函数不会再次执行，UI界面里面的就元素不会刷新。
        UIResources.Add(typeof(UISetting), new UIElement() { Resources = "UI/UISetting", Cache = true });//UISetting设置界面，不用刷新，Cache设为true

        UIResources.Add(typeof(UIBag), new UIElement() { Resources = "UI/UIBag", Cache = false });//UIBag是 背包界面的prefab。Cache = false 表示关闭UI再打开，界面元素会刷新
        UIResources.Add(typeof(UIShop), new UIElement() { Resources = "UI/UIShop", Cache = false });//UIShop商店界面。测试为了简化，将 Cache = false ，替代刷新逻辑。
        UIResources.Add(typeof(UICharEquip), new UIElement() { Resources = "UI/UICharEquip", Cache = false });//装备界面
        UIResources.Add(typeof(UIQuestSystem), new UIElement() { Resources = "UI/UIQuestSystem", Cache = false }); //任务系统界面
        UIResources.Add(typeof(UIQuestDialog), new UIElement() { Resources = "UI/UIQuestDialog", Cache = false }); //任务对话界面
        UIResources.Add(typeof(UIFriends), new UIElement() { Resources = "UI/UIFriends", Cache = false }); //好友界面
        UIResources.Add(typeof(UIGuild), new UIElement() { Resources = "UI/Guild/UIGuild", Cache = false });//公会主界面
        UIResources.Add(typeof(UIGuildList), new UIElement() { Resources = "UI/Guild/UIGuildList", Cache = false });//公会列表界面（选择其一加入公会）
        UIResources.Add(typeof(UIGuildPopNoGuild), new UIElement() { Resources = "UI/Guild/UIGuildPopNoGuild", Cache = false });//创建或加入公会弹窗
        UIResources.Add(typeof(UIGuildPopCreate), new UIElement() { Resources = "UI/Guild/UIGuildPopCreate", Cache = false });//创建公会弹窗
        UIResources.Add(typeof(UIGuildApplyList), new UIElement() { Resources = "UI/Guild/UIGuildApplyList", Cache = false });//公会的加入申请列表
        UIResources.Add(typeof(UIPopCharMenu), new UIElement() { Resources = "UI/UIPopCharMenu", Cache = false });//点击聊天玩家姓名的弹窗

        UIResources.Add(typeof(UIRide), new UIElement() { Resources = "UI/UIRide", Cache = false });//坐骑界面
        UIResources.Add(typeof(UISystemConfig), new UIElement() { Resources = "UI/UISystemConfig", Cache = false });//系统设置界面

    }
    ~UIManager()
    {

    }

    public T Show<T>()//打开T类型的UI（从Resources目录下 加载UIprefab资源）
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Open);//播放 打开弹窗音效
        Type type = typeof(T); //type 为 UI 类型
        if (this.UIResources.ContainsKey(type)) //若UI资源管理器中 存在此UI类型
        {
            UIElement info = this.UIResources[type];
            if (info.Instance != null) //若 此UI存在实例，即 Cache = true
            {
                info.Instance.SetActive(true); //激活实例
            }
            else //若UI的instance为空，Cache = false
            {
                UnityEngine.Object prefab = Resources.Load(info.Resources);//从Resources目录下 加载UIprefab资源
                if (prefab == null) //资源路径中找不到prefab
                {
                    return default(T);
                }
                info.Instance = (GameObject)GameObject.Instantiate(prefab);//实例化UI窗口

            }
            return info.Instance.GetComponent<T>();//返回该类型UI
        }
        return default(T);
    }


    public void Close(Type type)//关闭UI
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Win_Close);//播放 关闭弹窗音效
        if (this.UIResources.ContainsKey(type))//判断 要关闭的UI 是否存在
        {
            UIElement info = this.UIResources[type];
            if (info.Cache) //若Cache = true启用，不能销毁UI实例
            {
                info.Instance.SetActive(false);//将其禁用
            }
            else //若Cache未启用，直接销毁UI实例
            {
                GameObject.Destroy(info.Instance);
                info.Instance = null;
            }
        }
    }

    public void Close<T>()
    {
        this.Close(typeof(T));
    }
}
