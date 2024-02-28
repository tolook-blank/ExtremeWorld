
using UnityEngine;
using UnityEngine.UI;
using Common.Data;
using Managers;
using Models;
using SkillBridge.Message;
public class UIRide : UIWindow
{
    //坐骑面板
    public Text descript; //坐骑描述

    public GameObject itemPrefab;  //列表元素 ->坐骑prefab
    public ListView listMain;  //列表框
    public UIRideItem selectedItem;//当前选中项

    void Start()
    {
        RefreshUI();
        this.listMain.onItemSelected += this.OnItemSelected;
    }

    private void OnItemSelected(ListView.ListViewItem item)
    {
        this.selectedItem = item as UIRideItem; //选择项
    }

    void OnDestroy()
    {
        
    }

    void RefreshUI()
    {
        ClearItems(); //先清除
        InitItems(); //再初始化
    }

    void InitItems() //初始化，创建坐骑列表项 
    {
        foreach (var kv in ItemManager.Instance.Items)
        {
            //筛选坐骑道具
            if (kv.Value.Define.Type == ItemType.Ride &&
                (kv.Value.Define.LimitClass == CharacterClass.None || kv.Value.Define.LimitClass == User.Instance.CurrentCharacter.Class))
            {
                GameObject go = Instantiate(itemPrefab, this.listMain.transform);//创建坐骑列表项
                UIRideItem ui = go.GetComponent<UIRideItem>();
                ui.SetRideItem(kv.Value);
                this.listMain.AddItem(ui);
            }
        }
    }

    void ClearItems() //清空列表项
    {
        this.listMain.RemoveAll();
    }


    public void DoRide() //召唤坐骑
    {
        if (this.selectedItem == null)
        {
            MessageBox.Show("请选择要召唤的坐骑", "提示");
            return;
        }
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Confirm);
        User.Instance.Ride(this.selectedItem.item.Id);
    }

}

