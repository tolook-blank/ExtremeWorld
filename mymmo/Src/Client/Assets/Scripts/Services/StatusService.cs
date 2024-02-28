using System;
using System.Collections.Generic;
using UnityEngine;

using Models;
using Network;
using SkillBridge.Message;


namespace Services
{
    class StatusService : Singleton<StatusService>, IDisposable
    {
        public delegate bool StatusNotifyHandler(NStatus status); //和NPCManager是一样的设计
        //状态变化管理器，存储 StatusType状态类型（即Money、Exp、Skill、item）的各种 状态变化通知(add\delete\update)
        Dictionary<StatusType, StatusNotifyHandler> eventMap = new Dictionary<StatusType, StatusNotifyHandler>(); 

        HashSet<StatusNotifyHandler> handles = new HashSet<StatusNotifyHandler>(); //HashSet的检索效率高，方便判断 收到的状态通知 是否重复

        public void Init()
        {
        }
        public StatusService()
        {
            //服务器会将未处理的状态变化都添加到状态管理器列表中，客户端 订阅服务器返回的一次协议statusNotify，能接收其中的所有状态变化statusNotify.Status
            MessageDistributer.Instance.Subscribe<StatusNotify>(this.OnStatusNotify); //客户端订阅 服务器返回的 状态通知协议
        }

        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<StatusNotify>(this.OnStatusNotify);
        }

        public void RegisterStatusNotify(StatusType type, StatusNotifyHandler action)
        {
            if (handles.Contains(action)) //防止Bug：多次更换角色=>多次OnGameEnter->多次ItemManager.Init，会导致多次触发状态通知（举例：买一件装备=> 买了一件装备n次 = 买了n件装备） 
            {
                return;
            }

            if (!eventMap.ContainsKey(type))
            {
                eventMap[type] = action;
            }
            else
            {
                eventMap[type] += action;
            }
            handles.Add(action);
        }

        //接收服务器返回的 状态通知协议statusNotify，处理状态变化
        private void OnStatusNotify(object sender, StatusNotify notify)
        {
            foreach (NStatus status in notify.Status) //遍历状态通知协议中的 状态变化列表statusNotify.Status
            {
                Notify(status); //依次调用Notify，通知status状态变化
            }
        }

        private void Notify(NStatus status) //NStatus类型 可以代表游戏内所有可能发生的状态变化 
        {
            Debug.LogFormat("StatusNotify:[{0}][{1}] {2}:{3}", status.Type, status.Action, status.Id, status.Value);
            //使用了两种写法
            //1.直接通知
            if (status.Type == StatusType.Money) //如果是 金币状态变化，通知User直接 增删改
            {
                if (status.Action == StatusAction.Add)
                {
                    User.Instance.AddGold(status.Value);
                }
                else if (status.Action == StatusAction.Delete)
                {
                    User.Instance.AddGold(-status.Value);
                }
            }
            //2.调用事件发送通知， 对于其他StatusType，如Exp、SkillPoint、Item的状态变化（后续可拓展）
            StatusNotifyHandler handler;
            if (eventMap.TryGetValue(status.Type, out handler)) //若是经验、技能点、道具的增删改，发送状态变化通知
            {
                handler(status); //委托，通知 订阅这些状态变化 的订阅者
            }

        }
    }
}
