using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

using Entities;
using SkillBridge.Message;

namespace Managers
{
    class CharacterManager : Singleton<CharacterManager>, IDisposable
    {
        //在客户端运行期间，和我在同一地图上的所有角色 （包括玩家角色 和 怪物角色） ，参数：(EntityId,Character)
        public Dictionary<int, Character> Characters = new Dictionary<int, Character>();


        public UnityAction<Character> OnCharacterEnter;//监听角色进入
        public UnityAction<Character> OnCharacterLeave;//监听角色离开

        public CharacterManager()
        {

        }

        public void Dispose()
        {
        }

        public void Init()
        {

        }

        public void Clear()
        {
            int[] keys = this.Characters.Keys.ToArray();
            foreach (var key in keys) //遍历角色管理器中的每个人
            {
                RemoveCharacter(key);//通过 实体ID删除角色，并通知订阅者
            }
            this.Characters.Clear();
        }

        //每当有角色进入某个地图时 调用 AddCharacter
        public void AddCharacter(SkillBridge.Message.NCharacterInfo cha)
        {
            Debug.LogFormat("AddCharacter:{0}_{1} Map:{2} Entity:{3}", cha.Id, cha.Name, cha.mapId, cha.Entity.String());
            Character character = new Character(cha);//进入地图的角色 ，做两个添加
            this.Characters[cha.EntityId] = character;//添加到角色管理器,使用EntityId
            EntityManager.Instance.AddEntity(character);//也添加到EntityManager，Character继承Entity，角色是实体的一种
            if (OnCharacterEnter != null)
            {
                OnCharacterEnter(character);//添加角色，角色进入
            }
        }

        //每当有角色退出某个地图时 调用 RemoveCharacter
        public void RemoveCharacter(int entityId)
        {
            Debug.LogFormat("RemoveCharacter:{0}", entityId);
            if (this.Characters.ContainsKey(entityId))//如果角色管理器中 包含退出角色， 做两个删除
            {
                EntityManager.Instance.RemoveEntity(this.Characters[entityId].Info.Entity);//EntityManager中删除
                if (OnCharacterLeave != null)
                {
                    OnCharacterLeave(this.Characters[entityId]);//通知订阅者 删除角色
                }
                this.Characters.Remove(entityId);//角色管理中删除
            }
        }

        public Character GetCharacter(int id)
        {
            Character character = null;
            this.Characters.TryGetValue(id, out character);
            return character;
        }
    }
}
