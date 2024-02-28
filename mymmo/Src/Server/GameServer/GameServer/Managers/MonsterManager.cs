using System.Collections.Generic;
using Common;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Models;

namespace GameServer.Managers
{
    class MonsterManager : Singleton<MonsterManager> //怪物管理器，怪物要全局存在，不是随角色的创建而创建，适合做成单例
    {
        private Map Map;//标记当前所属地图
        public Dictionary<int, Monster> Monsters = new Dictionary<int, Monster>();//字典，管理本地图上的所有怪物

        public void Init(Map map)//怪物所属地图
        {
            this.Map = map;
        }

        //类似于CharacterManager的AddCharacter方法 ， 生成怪物
        public Monster Create(int spawnMonID, int spawnLevel, NVector3 position, NVector3 direction)
        {
            Monster monster = new Monster(spawnMonID, spawnLevel, position, direction);//创建 等级为spawnLevel 的怪物，位于position处，面朝direction
            EntityManager.Instance.AddEntity(this.Map.ID, monster);//添加到EntityManager
            monster.Id = monster.entityId;//怪物没有DB_id,使用的是Entity_id 
            monster.Info.EntityId = monster.entityId;//移动同步，使用Entity_id
            monster.Info.mapId = this.Map.ID; //怪物所属地图
            Monsters[monster.Id] = monster;//添加到怪物管理器

            this.Map.MonsterEnter(monster);//通知怪物所属地图上玩家，刷怪点position处 生成了怪物
            return monster;
        }

    }
}
