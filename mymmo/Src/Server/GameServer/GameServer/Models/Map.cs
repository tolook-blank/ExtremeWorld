using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillBridge.Message;

using Common;
using Common.Data;

using Network;
using GameServer.Managers;
using GameServer.Entities;
using GameServer.Services;

namespace GameServer.Models
{
    class Map
    {//移动同步UpdateEntity 在Map中添加
        public class MapCharacter //嵌套类，地图中的角色
        {
            public NetConnection<NetSession> connection; //角色的客户端网络连接
            public Character character; //角色

            public MapCharacter(NetConnection<NetSession> conn, Character cha)
            {
                this.connection = conn;
                this.character = cha;
            }
        }

        public int ID
        {
            get { return this.Define.ID; }
        }
        public MapDefine Define;

        /// <summary>
        /// 地图玩家管理器，以CharacterID为Key ，每张地图分别管理 各自的玩家
        /// </summary>
        Dictionary<int, MapCharacter> MapCharacters = new Dictionary<int, MapCharacter>();

        // 怪物在地图上刷新，和地图有紧密联系，Models.Map上 有刷怪管理器、怪物管理器
        SpawnManager SpawnManager = new SpawnManager(); //刷怪管理器（刷怪规则）

        public MonsterManager MonsterManager = new MonsterManager(); //怪物管理器（生成怪物）

        public Map(MapDefine define)
        {
            this.Define = define;
            this.SpawnManager.Init(this);//当前地图的刷怪管理器
            this.MonsterManager.Init(this);//当前地图的怪物管理器
        }

        //地图更新
        public void Update()
        {
            SpawnManager.Update();//让刷怪管理器 可以处理和时间相关的逻辑，要放在Map的Update中执行
        }

        /// <summary>
        /// 玩家的角色进入地图（两个触发时机：进入游戏时、地图传送时）
        /// </summary>
        /// <param name="character"></param>
        public void CharacterEnter(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("CharacterEnter: Map:{0} characterId:{1}", this.Define.ID, character.Id);

            character.Info.mapId = this.ID;//更新角色的所在地图
            this.MapCharacters[character.Id] = new MapCharacter(conn, character);//添加进入此地图的玩家，地图玩家管理器 

            conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();//创建 角色进入地图响应
            conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;//标识进入的地图

            foreach (var kv in this.MapCharacters)//再广播，通知地图中所有玩家（MapCharacters），有新玩家进入地图
            {
                conn.Session.Response.mapCharacterEnter.Characters.Add(kv.Value.character.Info);//所有玩家 都添加到MapCharacterEnterResponse的地图玩家列表 
                if (kv.Value.character != character)//除去进入玩家外
                {
                    //通知其他玩家kv.Value.connection ，有新玩家进入地图
                    this.AddCharacterEnterMap(kv.Value.connection, character.Info);
                }
            }
            foreach (var kv in this.MonsterManager.Monsters)
            {
                conn.Session.Response.mapCharacterEnter.Characters.Add(kv.Value.Info); //怪物进入地图
            }
            conn.SendResponse();//消息返回给发送者
        }

        void AddCharacterEnterMap(NetConnection<NetSession> conn, NCharacterInfo character)//通知其他玩家，有角色（可以是玩家 或 怪物）进入地图
        {
            if (conn.Session.Response.mapCharacterEnter == null)//创建响应消息
            {
                conn.Session.Response.mapCharacterEnter = new MapCharacterEnterResponse();
                conn.Session.Response.mapCharacterEnter.mapId = this.Define.ID;//角色进入的哪张地图
            }
            //在角色进入地图响应 的角色列表中 添加 进入角色（可以是玩家 或 怪物）
            conn.Session.Response.mapCharacterEnter.Characters.Add(character);
            conn.SendResponse();
        }

        /// <summary>
        /// 角色离开地图，（三个触发时机：网络断开退出游戏时、地图传送时）
        /// </summary>
        /// <param name="character">离开地图的玩家</param>
        public void CharacterLeave(Character character)//服务器接收角色离开地图请求，广播 角色退出地图的通知
        {
            Log.InfoFormat("CharacterLeave: Map:{0} characterID:{1}", Define.ID, character.Id);// Character_Id 是 DB_Id

            foreach (var kv in this.MapCharacters) //广播，遍历地图中的所有玩家（包括自己）
            {
                this.SendCharacterLeaveMap(kv.Value.connection, character);//通知每个玩家（包括自己），character离开地图
            }
            this.MapCharacters.Remove(character.Id); //地图玩家管理器中 删除该玩家（需要在SendCharacterLeaveMap 之后再执行，否则会漏掉退出者本人的 SendCharacterLeaveMap离开地图逻辑）

        }

        //通知其他玩家，有玩家离开
        private void SendCharacterLeaveMap(NetConnection<NetSession> conn, Character character)
        {
            Log.InfoFormat("SendCharacterLeaveMap: To:{0}_{1}  Map:{2}  characterID:{3}_{4}", conn.Session.Character.Id, conn.Session.Character.Info.Name, Define.ID, character.Id, character.Info.Name);
            conn.Session.Response.mapCharacterLeave = new MapCharacterLeaveResponse();
            conn.Session.Response.mapCharacterLeave.entityId = character.entityId;
            conn.SendResponse();
        }

        //更新地图中 玩家的移动同步 ，广播给地图上的所有玩家 MapCharacters
        public void UpdateEntity(NEntitySync entitySync)//NEntitySync包含移动同步数据
        {
            foreach (var kv in this.MapCharacters)//广播：遍历地图中的所有玩家
            {
                if (kv.Value.character.entityId == entitySync.Id)//更新自己，对于移动者本人，不需要返回响应
                {
                    kv.Value.character.Position = entitySync.Entity.Position;//更新自己的位置 到服务器中
                    kv.Value.character.Direction = entitySync.Entity.Direction;
                    kv.Value.character.Speed = entitySync.Entity.Speed;
                    if (entitySync.Event == EntityEvent.Ride)
                    {
                        kv.Value.character.Ride = entitySync.Param;//上坐骑，Ride更改为坐骑ID；下坐骑，Ride改为0
                    }
                }
                else //更新别人，对于非移动者，发送移动同步响应，来同步 移动者的位置
                {
                    MapService.Instance.SendEntityUpdate(kv.Value.connection, entitySync);//给别人发送 移动同步响应
                }
            }
        }

        /// <summary>
        /// 怪物进入地图，即怪物生成
        /// </summary>
        /// <param name="monster"></param>
        public void MonsterEnter(Monster monster)
        {
            Log.InfoFormat("MonsterEnter: Map:{0} monsterId:{1}", this.Define.ID, monster.Id);
            foreach (var kv in this.MapCharacters)//遍历地图中的所有玩家
            {
                //通知地图中的所有玩家kv.Value.connection ，怪物生成
                this.AddCharacterEnterMap(kv.Value.connection, monster.Info);
            }
        }
    }
}
