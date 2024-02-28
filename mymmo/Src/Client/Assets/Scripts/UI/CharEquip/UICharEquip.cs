
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using Common.Data;
using Managers;
using Models;
using SkillBridge.Message;

public class UICharEquip : UIWindow
{
    //装备面板主界面
    public Text title;
    public Text money; //角色金币数

    public GameObject itemPrefab;  //装备列表内prefab
    public GameObject itemEquipedPrefab; //装备槽内prefab

    public Transform itemListRoot;//角色的装备列表

    public List<Transform> slots; //7个装备槽位

    void Start()
    {
        RefreshUI();
        EquipManager.Instance.OnEquipChanged += RefreshUI; //订阅装备栏改变事件，每次穿脱装备，都触发RefreshUI
    }

    void OnDestroy()
    {
        EquipManager.Instance.OnEquipChanged -= RefreshUI;
    }

    void RefreshUI()
    {
        ClearAllEquipList(); //先清除装备列表
        InitAllEquipItems(); //再初始化装备列表
        ClearEquipedSlots(); //清空装备槽
        InitEquipedSlots();//初始装备槽
        this.money.text = User.Instance.CurrentCharacter.Gold.ToString();
    }

    //初始化左侧的 全部装备列表
    void InitAllEquipItems()
    {
        foreach (var kv in ItemManager.Instance.Items)
        {
            if (kv.Value.Define.Type == ItemType.Equip) //检查是否为 装备道具
            {
                if (EquipManager.Instance.Contains(kv.Key)) //检查是否已经穿戴此装备,若已经穿戴，便不显示在装备列表中
                    continue;
                GameObject go = Instantiate(itemPrefab, itemListRoot);
                UIEquipItem ui = go.GetComponent<UIEquipItem>();
                //因为装备列表中显示的都是未穿戴的装备
                ui.SetEquipItem(kv.Key, kv.Value, this, false);//itemID,Item,UICharEquip,false未穿戴
            }

        }
    }

    //初始化装备槽装备，即已经穿戴的装备
    void InitEquipedSlots() 
    {
        for (int i = 0; i < (int)EquipSlot.SlotMax; i++)
        {
            Item item = EquipManager.Instance.Equips[i];//读取此装备槽的 道具信息
            if (item != null) //若有装备
            {
                GameObject go = Instantiate(itemEquipedPrefab, slots[i]);//实例化 已经穿戴的装备
                UIEquipItem ui = go.GetComponent<UIEquipItem>();
                ui.SetEquipItem(i, item, this, true); //设置信息，装备槽ID,装备Item,UICharEquip,已经穿戴
            }
        }
    }

    void ClearAllEquipList() //清空左侧装备列表
    {
        foreach (var item in itemListRoot.GetComponentsInChildren<UIEquipItem>())//获取itemListRoot全部子节点的UIEquipItem组件 
        {
            Destroy(item.gameObject); //销毁组件的游戏物体
        }
    }


    void ClearEquipedSlots() //清空7个装备槽
    {
        foreach (var item in slots) //遍历7个装备槽
        {
            if (item.childCount > 0) //子节点(itemEquipedPrefab) > 0,说明该槽位上 有装备
            {
                Destroy(item.GetChild(0).gameObject);//销毁该槽位上的 子节点
            }
        }
    }

    public void DoEquip(Item item) //UI界面的穿戴装备，因为是通过检测UIEquipItem 上的鼠标点击 来穿脱装备，所以从UIEquipItem 的DoEquip中调用过来
    {
        EquipManager.Instance.EquipItem(item);
    }

    public void UnEquip(Item item) //脱下装备
    {
        EquipManager.Instance.UnEquipItem(item);
    }


}
