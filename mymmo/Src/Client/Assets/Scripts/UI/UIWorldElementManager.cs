using Entities;
using Managers;
using System.Collections.Generic;
using UnityEngine;

public class UIWorldElementManager : MonoSingleton<UIWorldElementManager>
{
    //作为Mono单例脚本，绑定在主城 场景中的 UIWorldElementManager 游戏物体上
    public GameObject nameBarPrefab;//管理信息栏，直接开放public，再手动拖拽；也可以 Resources.Load<T>(path) 动态导入prefab资源
    public GameObject npcStatusPrefab; //NPC 任务状态图标

    //各世界元素管理器
    private Dictionary<Transform, GameObject> elementNames = new Dictionary<Transform, GameObject>();//信息栏管理器
    private Dictionary<Transform, GameObject> elementNPCStatus = new Dictionary<Transform, GameObject>();//NPC的任务状态管理器

    //单例类，不要使用Start ()，避免覆盖了父类 MonoSingleton中的Start()，若要初始化则使用 继承父类提供的OnStart()
    protected override void OnStart()
    {
        nameBarPrefab.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    //在GameObjectManager.CreateCharacterObject函数中调用，
    public void AddCharacterNameBar(Transform owner, Character character)//角色创建时添加信息栏
    {
        GameObject gonameBar = Instantiate(nameBarPrefab, this.transform);//Prefab实例化，且以this为父节点，因为管理器创建的元素都要在管理器节点下。
        gonameBar.name = "NameBar" + character.entityId;//UI信息栏重命名
        gonameBar.GetComponent<UIWorldElement>().owner = owner; //属于玩家角色的世界元素
        gonameBar.GetComponent<UINameBar>().character = character;
        gonameBar.SetActive(true);
        this.elementNames[owner] = gonameBar;//将信息栏添加到管理器
    }

    public void RemoveCharacterNameBar(Transform owner) //角色死亡时销毁信息栏
    {
        if (this.elementNames.ContainsKey(owner))
        {
            Destroy(this.elementNames[owner]);//先销毁信息栏
            this.elementNames.Remove(owner);//管理器删除信息栏数据
        }
    }

    //刷新NPC 头顶任务状态图标显示
    public void AddNpcQuestStatus(Transform owner, NpcQuestStatus status) //owner是NPC位置，添加 任务状态显示status
    {
        if (this.elementNPCStatus.ContainsKey(owner)) //NPC已有任务状态图标
        {
            elementNPCStatus[owner].GetComponent<UIQuestStatus>().SetQuestStatus(status);//更新 其头顶任务状态图标
        }
        else //若该NPC还没有
        {
            GameObject go = Instantiate(npcStatusPrefab, this.transform); //先实例化 NPC任务状态图标
            go.name = "NpcQuestStatus" + owner.name;
            go.GetComponent<UIWorldElement>().owner = owner; //将NPC的任务状态图标 跟随显示在NPC头顶上
            go.GetComponent<UIQuestStatus>().SetQuestStatus(status);//设置对应状态图标
            go.SetActive(true);
            this.elementNPCStatus[owner] = go;//添加到管理器
        }
    }

    public void RemoveNpcQuestStatus(Transform owner)//移除NPC状态图标
    {
        if (this.elementNPCStatus.ContainsKey(owner))
        {
            Destroy(this.elementNPCStatus[owner]);//删除状态图标
            this.elementNPCStatus.Remove(owner);//从管理器中删除
        }
    }

}
