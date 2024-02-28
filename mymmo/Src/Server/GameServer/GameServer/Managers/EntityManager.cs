
using System.Collections.Generic;
using GameServer.Entities;
using Common;

namespace GameServer.Managers
{
    class EntityManager : Singleton<EntityManager>
    {
        private int idx = 0; //AllEntities总列表的索引
        public List<Entity> AllEntities = new List<Entity>();//用总列表来 管理所有Entity
        public Dictionary<int, List<Entity>> MapEntities = new Dictionary<int, List<Entity>>();//区分不同地图上的 entities列表

        public void AddEntity(int mapId, Entity entity)
        {
            AllEntities.Add(entity);//先添加到实体总列表中
            //加入管理器，使用AllEntities列表索引 生成唯一ID，实体ID只在这里赋值
            entity.EntityData.Id = ++this.idx;

            List<Entity> entities = null;
            if (!MapEntities.TryGetValue(mapId, out entities))//若mapId对应的地图上不存在entities列表，则先创建此地图的entities列表
            {
                entities = new List<Entity>();
                MapEntities[mapId] = entities;
            }
            entities.Add(entity);//添加实体 到该地图的entities列表中
        }

        public void RemoveEntity(int mapId, Entity entity)
        {
            this.AllEntities.Remove(entity);
            this.MapEntities[mapId].Remove(entity);
        }

    }
}
