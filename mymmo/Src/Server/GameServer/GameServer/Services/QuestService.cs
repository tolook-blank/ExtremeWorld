
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;


namespace GameServer.Services
{
    class QuestService : Singleton<QuestService>
    {

        public QuestService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestAcceptRequest>(this.OnQuestAccept);//订阅 任务领取请求事件
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<QuestSubmitRequest>(this.OnQuestSubmit);//订阅 任务提交请求事件
        }

        public void Init()
        {

        }

        private void OnQuestAccept(NetConnection<NetSession> sender, QuestAcceptRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("QuestAcceptRequest::character:{0} QuestId:{1}", character.Id, request.QuestId);

            sender.Session.Response.questAccept = new QuestAcceptResponse();
            Result result = character.QuestManager.AcceptQuest(sender, request.QuestId);
            sender.Session.Response.questAccept.Result = result;
            sender.SendResponse();
        }

        private void OnQuestSubmit(NetConnection<NetSession> sender, QuestSubmitRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("OnQuestSubmit::character:{0} QuestId:{1}", character.Id, request.QuestId);

            sender.Session.Response.questSubmit = new QuestSubmitResponse();
            Result result = character.QuestManager.SubmitQuest(sender, request.QuestId);
            sender.Session.Response.questSubmit.Result = result;
            sender.SendResponse();
        }
    }
}
