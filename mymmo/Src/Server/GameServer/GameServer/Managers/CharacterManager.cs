using Common;
using System.Collections.Generic;
using SkillBridge.Message;
using GameServer.Entities;

namespace GameServer.Managers
{
    class CharacterManager : Singleton<CharacterManager>
    {
        //在线玩家管理器  字典类型，查询效率高 ，Key是character.Id(即DB_id) 
        public Dictionary<int, Character> Characters = new Dictionary<int, Character>();

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
            this.Characters.Clear();
        }

        public Character AddCharacter(TCharacter cha)//创建角色，添加到 在线玩家管理器，并获取Entity_id
        {
            Character character = new Character(CharacterType.Player, cha);//先创建角色
            EntityManager.Instance.AddEntity(cha.MapID, character);//添加到 实体管理器中，并给 character 生成Entity_id
            character.Info.EntityId = character.entityId; //立刻把 character的 Entity_id 同步到 网络消息NCharacterInfo
            this.Characters[character.Id] = character;//添加到 在线玩家管理器
            return character;
        }

        //玩家管理器 通过character.Id(DB_id) 删除玩家
        public void RemoveCharacter(int characterId)
        {
            var cha = this.Characters[characterId];//取出待删除的玩家
            EntityManager.Instance.RemoveEntity(cha.Data.MapID,cha);//先在EntityManager中 删除该玩家实体
            this.Characters.Remove(characterId); //在线玩家管理器 删除该玩家
        }

        public Character GetCharacter(int characterId) //根据角色ID，查询角色
        {
            Character character = null;
            this.Characters.TryGetValue(characterId, out character);//查询在线玩家管理器中 characterId对应的角色
            return character;
        }
    }
}
