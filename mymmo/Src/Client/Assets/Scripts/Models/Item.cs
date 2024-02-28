using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SkillBridge.Message;
using Common.Data;

//Model和Entity的区别，Model 不是用来 服务器与客户端之间进行通讯，Models倾向于纯维护本地数据，与服务器关联少，也不需要在游戏世界中和服务端做同步
namespace Models
{
    public class Item //目前包含三种道具 ： 普通道具、装备、坐骑
    {
        public int Id;
        public int Count;
        public ItemDefine Define;  //道具信息，从配置表中加载
        public EquipDefine EquipInfo; //加载装备信息
        public RideDefine RideInfo; //加载坐骑消息

        //构造函数重载  this(其他构造函数参数)
        public Item(NItemInfo item) : this(item.Id, item.Count) //因为客户端不接触DB，所以构造函数使用 网络协议NItemInfo ;而服务端通过读取DB中的TcharacterItem来构造
        {

        }

        public Item(int id, int count) 
        {
            this.Id = id;
            this.Count = count;
            DataManager.Instance.Items.TryGetValue(this.Id, out this.Define);//当道具创建时，同时加载道具配置信息
            DataManager.Instance.Equips.TryGetValue(this.Id, out this.EquipInfo);//加载装备信息
            DataManager.Instance.Rides.TryGetValue(this.Id, out this.RideInfo);//加载坐骑信息
        }

        public override string ToString()
        {
            return string.Format("Item Id:{0},Count:{1}", this.Id, this.Count);
        }
    }

}


