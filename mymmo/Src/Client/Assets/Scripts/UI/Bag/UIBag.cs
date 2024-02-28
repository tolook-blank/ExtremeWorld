using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Models;
using Managers;

public class UIBag : UIWindow {
    //背包、商店 都是道具系统对外展示的一个接口

	public Text money; //背包中，显示金币数

	public Transform[] pages; //背包页，绑定 Scroll View的content节点, 道具格子Grid是它的子节点

	public GameObject bagItem;//背包道具预制件

	List<Image> grids; //道具格子列表

    void Start () {
		if(grids == null)
        {
			grids = new List<Image>();
			for(int page = 0; page < this.pages.Length; ++page)//pages.Length=2，表示有2页
            {
				grids.AddRange(this.pages[page].GetComponentsInChildren<Image>(true)); //动态获取 两页背包道具格子的总数量（包括未使用的）
            }
        }
        this.money.text = User.Instance.CurrentCharacter.Gold.ToString();
        StartCoroutine(InitBags());//用携程，初始化背包
	}

    IEnumerator InitBags() //初始化背包
    {
        for(int i = 0; i < BagManager.Instance.Items.Length; ++i)//BagManager.Instance.Items.Length是 道具占用背包格子数
        {
			var item = BagManager.Instance.Items[i]; //取出第i个格子中的BagItem
            if (item.ItemId > 0) //空格子BagItem.ItemId = 0 ，item.ItemId > 0 表示格子中有道具
            {
                GameObject go = Instantiate(bagItem, grids[i].transform);//第 i 个道具的游戏对象bagItem 创建在 第 i 个格子上
                var ui = go.GetComponent<UIIconItem>();//UIbagItem的prefab 上绑定了 UIIconItem脚本
                var def = ItemManager.Instance.Items[item.ItemId].Define;//从道具管理器上获取此道具的Define, Define中有道具图标路径，如"Icon": "UI/Items/hongp",
                ui.SetMainIcon(def.Icon, item.Count.ToString()); //设置道具图标，def.Icon 为道具图标路径， item.Count道具数量
            }
        }

        //把剩下没解锁的格子设置为灰色
        for (int i = BagManager.Instance.Unlocked; i < grids.Count; ++i)// BagManager.Instance.Unlocked 是已经解锁的道具格子数
        {
            grids[i].color = Color.gray;
        }
        yield return null;
    }

    void Clear() //清理背包格
    {
        for(int i = 0; i < grids.Count; i++)
        {
            if (grids[i].transform.childCount > 0) // slots[i].transform.childCount > 0 说明该格子中存在道具
            {
                Destroy(grids[i].transform.GetChild(0).gameObject); //清除原来背包格子中的道具
            }
        }
    }

   public void OnReset() //背包整理按钮绑定函数
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Confirm);
        BagManager.Instance.Reset();
        this.Clear();//简单清空， 也可以判断当前格子的道具ID是否改变 BagManager.Instance.Items[i].ItemId 来选择性地初始化
        StartCoroutine(InitBags());//再重新InitBags初始化背包
    }
}
