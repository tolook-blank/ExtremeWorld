using System;
using System.Collections.Generic;
using SkillBridge.Message;
using GameServer.Entities;


namespace GameServer.Managers
{
    class StatusManager  //和道具管理器一样，在角色身上
    {
        Character Owner;
        private List<NStatus> Status { get; set; } //状态管理器列表
        public bool HasStatus
        {
            get { return this.Status.Count > 0; }
        }

        public StatusManager(Character owner)
        {
            this.Owner = owner;
            this.Status = new List<NStatus>();
        }

        public void AddStatus(StatusType type, int id, int value, StatusAction action)
        {
            this.Status.Add(new NStatus()
            {
                Type = type,
                Id = id,
                Value = value,
                Action = action
            });
        }

        public void AddGoldChange(int goldDelta)//加减金币 ,金币的ID是0
        {
            if (goldDelta > 0)
            {
                this.AddStatus(StatusType.Money, 0, goldDelta, StatusAction.Add);
            }
            if (goldDelta < 0)
            {
                this.AddStatus(StatusType.Money, 0, -goldDelta, StatusAction.Delete);
            }
        }

        public void AddItemChange(int id, int count, StatusAction action)//道具ID,数量,行为(增/减/更新)
        {
            this.AddStatus(StatusType.Item, id, count, action);
        }

        public void PostProcess(NetMessageResponse message)//处理NetMessageResponse响应
        {
            if (message.statusNotify == null)
            {
                message.statusNotify = new StatusNotify();
            }
            foreach (var status in this.Status) //遍历状态管理器中 存储的未处理的状态变化
            {
                message.statusNotify.Status.Add(status);//添加 所有的状态变化 到状态通知网络协议里，客户端订阅服务器返回的 statusNotify响应 来接收状态变化通知
            }
            this.Status.Clear(); // 清空处理过的状态列表
        }
    }
}
