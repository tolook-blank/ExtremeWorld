using System;
using System.Collections.Generic;
using UnityEngine;

using Common.Data;
using Network;
using SkillBridge.Message;
using Models;
using Managers;

namespace Services
{
    class QuestService : Singleton<QuestService>, IDisposable
    {

        public QuestService()
        {
            MessageDistributer.Instance.Subscribe<QuestAcceptResponse>(this.OnQuestAccept);//订阅 道具购买的响应事件
            MessageDistributer.Instance.Subscribe<QuestSubmitResponse>(this.OnQuestSubmit);
        }
        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<QuestAcceptResponse>(this.OnQuestAccept);//订阅 道具购买的响应事件
            MessageDistributer.Instance.Unsubscribe<QuestSubmitResponse>(this.OnQuestSubmit);
        }

        public bool SendQuestAccept(Quest quest)
        {
            Debug.Log("SendQuestAccept");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.questAccept = new QuestAcceptRequest();
            message.Request.questAccept.QuestId = quest.Define.ID;
            NetClient.Instance.SendMessage(message);
            return true;
        }

        //处理服务器返回的接取任务响应
        private void OnQuestAccept(object sender, QuestAcceptResponse message)
        {
            Debug.LogFormat("OnQuestAccept：{0}，ERROR:{1}",message.Result,message.Errormsg);
            if (message.Result == Result.Success)
            {
                QuestManager.Instance.OnQuestAccepted(message.Quest); //QuestSevice调用QuestManager，执行
            }
            else
                MessageBox.Show("任务接受失败", "错误", MessageBoxType.Error);
        }

        public bool SendQuestSubmit(Quest quest)
        {
            Debug.Log("SendQuestSubmit");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.questSubmit = new QuestSubmitRequest();
            message.Request.questSubmit.QuestId = quest.Define.ID;
            NetClient.Instance.SendMessage(message);
            return true;
        }

        private void OnQuestSubmit(object sender, QuestSubmitResponse message)
        {
            Debug.LogFormat("OnQuestSubmit：{0}，ERROR:{1}", message.Result, message.Errormsg);
            if (message.Result == Result.Success)
            {
                QuestManager.Instance.OnQuestSubmited(message.Quest);
            }
            else
                MessageBox.Show("任务提交失败", "错误", MessageBoxType.Error);
        }





    }
}
