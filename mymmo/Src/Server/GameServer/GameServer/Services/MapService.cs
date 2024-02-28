using Common;
using Common.Data;
using GameServer.Entities;
using GameServer.Managers;
using Network;
using SkillBridge.Message;


namespace GameServer.Services
{
    class MapService : Singleton<MapService>
    {
        public MapService()
        {
            //MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapCharacterEnterRequest>(this.OnMapCharacterEnter);//Map中处理
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapEntitySyncRequest>(this.OnMapEntitySync);//订阅 移动同步请求

            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<MapTeleportRequest>(this.OnMapTeleport);//订阅 地图传送请求

        }

        public void Init()
        {
            MapManager.Instance.Init();//初始化MapManager
        }

        //接收sender控制的角色 发来的移动同步请求，返回
        private void OnMapEntitySync(NetConnection<NetSession> sender, MapEntitySyncRequest request)
        {
            Character character = sender.Session.Character; //明确哪个角色发送的移动同步请求
            Log.InfoFormat("OnMapEntitySync: characterID:{0}_{1} Entity.Id:{2} Event:{3} Entity:{4}", character.Id, character.Info.Name,
                request.entitySync.Id, request.entitySync.Event, request.entitySync.Entity.String());

            MapManager.Instance[character.Info.mapId].UpdateEntity(request.entitySync);//通知 该角色所处地图上的实体做更新，MapManager.Instance[character.Info.mapId]是发送移动同步的角色所处地图
        }

        // 从UpdateEntity中调用回来， 返回 移动同步响应给 该地图上的角色，来同步移动者的变动
        public void SendEntityUpdate(NetConnection<NetSession> conn, NEntitySync entitySync)
        {
            conn.Session.Response.mapEntitySync = new MapEntitySyncResponse();//创建移动同步响应
            conn.Session.Response.mapEntitySync.entitySyncs.Add(entitySync); //添加到 移动同步响应列表
            conn.SendResponse();//返回给客户端
        }

        //收到sender的地图传送请求，处理传送逻辑
        private void OnMapTeleport(NetConnection<NetSession> sender, MapTeleportRequest request)
        {
            Character character = sender.Session.Character;//请求传送的角色
            Log.InfoFormat("OnMapTeleport: characterID:{0}:{1} TeleporterId:{2}", character.Id, character.Data.Name, request.teleporterId);
            if (!DataManager.Instance.Teleporters.ContainsKey(request.teleporterId))//起始传送点request.teleporterId 不存在
            {
                Log.WarningFormat("source TeleporterID [{0}] not existed", request.teleporterId);
                return;
            }
            TeleporterDefine source = DataManager.Instance.Teleporters[request.teleporterId];//获取起始传送点信息
            if (source.LinkTo == 0 || !DataManager.Instance.Teleporters.ContainsKey(source.LinkTo))//判断传送目标点是否存在
            {
                Log.WarningFormat("source TeleporterID [{0}] LinkTo ID [{1}] not existed", request.teleporterId, source.LinkTo);
                return;
            }
            //校验通过后
            TeleporterDefine target = DataManager.Instance.Teleporters[source.LinkTo];//传送目标点信息
            MapManager.Instance[source.MapID].CharacterLeave(character);//玩家角色离开 起始传送地图
            character.Position = target.Position;// 传送点位置，通过编辑器拓展来赋值  Src\Client\Assets\Editor\MapTools.cs 
            character.Direction = target.Direction; //传送点的方向
            //似乎可以在此添加 加载画面逻辑，避免地图传送时黑屏等待
            MapManager.Instance[target.MapID].CharacterEnter(sender, character); //玩家角色进入 传送目标地图

        }
    }
}
