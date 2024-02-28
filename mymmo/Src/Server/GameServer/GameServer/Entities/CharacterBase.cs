using Common.Data;
using GameServer.Core;
using SkillBridge.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServer.Managers;

namespace GameServer.Entities
{
    class CharacterBase : Entity
    {
        //CharacterBase 分为3种阵营CharacterType:Player、Npc、Monster
        public int Id { get; set; } //唯一的DB_id
        public string Name { get { return this.Info.Name; } }

        public NCharacterInfo Info; //用来与客户端之间通讯，传递角色身上的数据
        public CharacterDefine Define;//XXXDefine是配置表的数据

        public CharacterBase(Vector3Int pos, Vector3Int dir) : base(pos, dir) //Character构造方法中调用
        {

        }

        //创建Monster时调用
        public CharacterBase(CharacterType type, int configId, int level, Vector3Int pos, Vector3Int dir) :
           base(pos, dir) 
        {
            this.Info = new NCharacterInfo();
            this.Info.Type = type; //CharacterBase 分为3种阵营CharacterType:Player、Npc、Monster
            this.Info.Level = level;
            this.Info.ConfigId = configId;//CharacterDefine中的TID（对应玩家的职业Class 或 怪物的类别）
            this.Info.Entity = this.EntityData;
            this.Info.EntityId = this.entityId;
            this.Define = DataManager.Instance.Characters[this.Info.ConfigId];//角色职业的配置表信息
            this.Info.Name = this.Define.Name;

        }
    }
}
