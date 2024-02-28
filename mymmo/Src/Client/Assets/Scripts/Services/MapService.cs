using System;
using Network;
using UnityEngine;

using Common.Data;
using SkillBridge.Message; //网络协议消息
using Models;
using Managers;

namespace Services
{
    class MapService : Singleton<MapService>, IDisposable
    {

        public int CurrentMapId = 0;//当前的地图Id，进入地图时记录，离开地图时清除
        public MapService()
        {
            //客户端MapService 发送的请求，在服务器MapService中 都会订阅对应的请求消息（客户端发送消息不需要订阅）
            //服务器MapService 返回的响应，在客户端MapService中 也要订阅对应的响应消息

            //地图传送不需要接收 MapTeleportResponse响应消息，只要服务器处理到传送逻辑，就会触发 MapCharacterLeaveResponse、MapCharacterEnterResponse
            MessageDistributer.Instance.Subscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);//订阅角色进入地图 响应
            MessageDistributer.Instance.Subscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);//订阅角色离开地图 响应
            MessageDistributer.Instance.Subscribe<MapEntitySyncResponse>(this.OnMapEntitySync); //订阅 服务器返回的 移动同步响应
        }


        public void Dispose()
        {
            MessageDistributer.Instance.Unsubscribe<MapCharacterEnterResponse>(this.OnMapCharacterEnter);
            MessageDistributer.Instance.Unsubscribe<MapCharacterLeaveResponse>(this.OnMapCharacterLeave);

            MessageDistributer.Instance.Unsubscribe<MapEntitySyncResponse>(this.OnMapEntitySync);
        }

        public void Init()
        {

        }

        //接收服务器返回的 玩家进入地图响应
        private void OnMapCharacterEnter(object sender, MapCharacterEnterResponse response)
        {
            Debug.LogFormat("OnMapCharacterEnter:Map:{0} Count:{1}", response.mapId, response.Characters.Count);
            foreach (var cha in response.Characters)//遍历进入的地图中 的所有角色
            {// 在CharacterSelect场景中点击进入游戏之前，User.Instance.CurrentCharacter 为null
                //如果当前地图中有 我当前控制的角色 ， User.Instance.CurrentCharacter == null 判断似乎不会触发
                if (User.Instance.CurrentCharacter == null || (cha.Type == CharacterType.Player && User.Instance.CurrentCharacter.Id == cha.Id))
                {
                    User.Instance.CurrentCharacter = cha;//更新当前控制角色（在此地图中可能有状态变动，例如升级了）
                }
                CharacterManager.Instance.AddCharacter(cha);//CharacterManager中添加 在此地图中的所有角色（包括玩家角色 和 怪物角色）
            }
            //如果是我进入地图，需要加载地图资源 （别人进入地图，也只有他自己要加载地图资源）
            if (CurrentMapId != response.mapId)//若 当前地图CurrentMapId 和 进入地图response.mapId  不同，则发生了地图切换
            {
                this.EnterMap(response.mapId);//切换地图，加载即将进入的地图资源
                this.CurrentMapId = response.mapId;
            }
        }
        public void EnterMap(int mapId) //进入地图mapId，加载地图，同时加载小地图
        {
            if (DataManager.Instance.Maps.ContainsKey(mapId))//判断配置表中 是否存在 此地图？
            {
                MapDefine map = DataManager.Instance.Maps[mapId];//获取此地图配置，可以查看client/Data/MapDefine.txt文件
                User.Instance.CurrentMapData = map;//加载地图场景之前，更新角色当前所在地图，用于加载小地图

                SceneManager.Instance.LoadScene(map.Resource);//加载地图场景，map.Resource是地图名string,如"MainCity"、"Map01"
				SoundManager.Instance.PlayMusic(map.Music);
            }
            else
                Debug.LogErrorFormat("EnterMap: Map {0} not existed", mapId);
        }

        //客户端在OnMapCharacterLeave 处理 服务器发来的 角色离开地图响应
        private void OnMapCharacterLeave(object sender, MapCharacterLeaveResponse response)
        {
            Debug.LogFormat("OnMapCharacterLeave: CharID:{0}", response.entityId);
            if (response.entityId != User.Instance.CurrentCharacter.EntityId)//是我自己离开地图吗？
            {
                CharacterManager.Instance.RemoveCharacter(response.entityId);//如果是别人的角色离开，在角色管理器中删除他的信息
            }
            else //如果是自己的角色离开，直接清空CharacterManager，退出游戏
            {
                CharacterManager.Instance.Clear();
            }
        }

        //发送角色entity的移动同步请求
        public void SendMapEntitySync(EntityEvent entityEvent, NEntity entity, int param)//EntityEvent：实体动画事件 ，NEntity 实体数据（坐标、方向、速度）
        {
            Debug.LogFormat("MapEntityUpdateRequest :ID:{0} POS:{1} DIR:{2} SPD:{3} ", entity.Id, entity.Position.String(), entity.Direction.String(), entity.Speed);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();//客户端发给服务器的消息是请求 message.Request
            message.Request.mapEntitySync = new MapEntitySyncRequest();//在服务器的MapServic中，订阅这个请求消息 Subscribe<MapEntitySyncRequest>
            message.Request.mapEntitySync.entitySync = new NEntitySync()//注意：移动同步使用entity.Id
            {
                Id = entity.Id,
                Event = entityEvent,
                Entity = entity,
                Param = param
            };

            NetClient.Instance.SendMessage(message);

        }

        //接收服务器 返回的移动同步响应， 调用EntityManager 做移动同步
        private void OnMapEntitySync(object sender, MapEntitySyncResponse response)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendFormat("OnMapEntityUpdateResponse: Entity:{0}", response.entitySyncs.Count);
            sb.AppendLine();

            foreach (var entitysync in response.entitySyncs)//服务器返回的移动同步响应中 可能包含多个角色的移动同步消息， 遍历每个移动同步消息response.entitySyncs
            {
                EntityManager.Instance.OnEntitySync(entitysync);//处理 此移动同步entitysync
                sb.AppendFormat("[{0}]event:{1} entity:{2}", entitysync.Id, entitysync.Event, entitysync.Entity.String());
                sb.AppendLine();
            }
            Debug.Log(sb.ToString());
        }

        //当触发传送点的OnTriggerEnter事件，会发送地图传送请求
        public void SendMapTeleport(int teleporterId)//起始传送点teleporterId 
        {
            Debug.LogFormat("MapTeleportRequest : teleporterId:{0}", teleporterId);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.mapTeleport = new MapTeleportRequest();
            message.Request.mapTeleport.teleporterId = teleporterId; //起始传送点ID
            NetClient.Instance.SendMessage(message);
        }

        //地图传送不需要接收MapTeleportResponse响应消息，只要服务器处理到传送逻辑，就会触发 MapCharacterLeaveResponse、MapCharacterEnterResponse

    }
}
