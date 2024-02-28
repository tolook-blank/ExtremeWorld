using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SkillBridge.Message;
using Models;

public class UIRideItem : ListView.ListViewItem
{
	//列表项的功能： 1.显示选中状态 2.给列表项元素赋值
	public Image icon;
	public Text title;
	public Text level;

	public Image background;
	public Sprite normalBg;
	public Sprite selectedBg;

	public Item item;
	public override void onSelected(bool selected)
	{
		this.background.overrideSprite = selected ? selectedBg : normalBg;
	}

	public void SetRideItem(Item item)
	{
		this.item = item;
		if (this.title != null) this.title.text = this.item.RideInfo.Name;
		if (this.level != null) this.level.text = this.item.RideInfo.Level.ToString();
		if (this.icon != null) this.icon.overrideSprite = Resloader.Load<Sprite>(this.item.RideInfo.Icon);
	}
}
