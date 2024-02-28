using Common;
using GameServer.Entities;
using Network;
using SkillBridge.Message;

namespace GameServer.Services
{//服务端，只用维护背包数据，需要一个BagService.  登陆创建角色时，把数据从数据库中取出来，给角色初始化 ；当收到客户端的保存请求时，把数据从网络上保存到服务器中
    class BagService: Singleton<BagService>
    {
        public BagService()
        {
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<BagSaveRequest>(this.OnBagSave);//订阅 客户端的背包保存请求
        }

        //背包保存请求处理，保存客户端的背包道具布局
        private void OnBagSave(NetConnection<NetSession> sender, BagSaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("BagSaveRequest: character:{0} :Unlocked{1}",character.Id,request.BagInfo.Unlocked);//Unlocked是背包的已用格子数量

            if(request.BagInfo != null)
            {
                character.Data.Bag.Items = request.BagInfo.Items;//将网络背包协议中 客户端背包的道具布局， 保存到服务器中
                character.Data.Bag.Unlocked = request.BagInfo.Unlocked;
                DBService.Instance.Save(); //再从服务器保存到 数据库中
            }
        }

        public void Init()
        {

        }


    }
}
