
using Entities;
using SkillBridge.Message;
using System.Collections.Generic;

namespace Managers
{
    interface IEntityNotify //注意：使用接口实现事件订阅、通知  好处：一个接收者可以接收多个事件，能实现一系列委托的效果  
    {
        void OnEntityRemoved(); //相当于委托   public UnityAction OnEntityRemoved;
        void OnEntityChanged(Entity entity); //实体位置更新，相当于委托 public UnityAction<Entity> OnEntityChanged;
        void OnEntityEvent(EntityEvent entityEvent, int param); //实体动画事件，相当于委托 public UnityAction<EntityEvent,int> OnEntityEvent;
    }

    class EntityManager : Singleton<EntityManager>
    {
        Dictionary<int, Entity> entities = new Dictionary<int, Entity>();//管理客户端本地的Entity列表，参数:entityId,Entity

        Dictionary<int, IEntityNotify> notifies = new Dictionary<int, IEntityNotify>();//管理 实体的订阅事件，EntityManager用来通知 EntityController

        public void RegisterEntityChangeNotify(int entityId, IEntityNotify notify)//注册 实体变更通知,相当于订阅事件
        {
            this.notifies[entityId] = notify;
        }

        public void AddEntity(Entity entity)
        {
            entities[entity.entityId] = entity;
        }

        public void RemoveEntity(NEntity entity)
        {
            entities.Remove(entity.Id);
            if (notifies.ContainsKey(entity.Id))//通知订阅者 删除实体事件
            {
                notifies[entity.Id].OnEntityRemoved();
                notifies.Remove(entity.Id);
            }
        }

        //处理实体移动同步 ： 1、先知道谁做的移动同步？ entitysync.Entity 做的   2、再更新他的数据
        public void OnEntitySync(NEntitySync entitysync)//entitysync是移动同步消息
        {
            Entity entity = null;
            entities.TryGetValue(entitysync.Id, out entity);//获取移动者的 Entity， 检测entitysync.Id这个key存在与否，同时得到对应Value
            if (entity != null) //若 实体列表中存在 此移动者的Entity
            {
                if (entitysync.Entity != null)//若发出移动同步的实体 非空
                {
                    entity.EntityData = entitysync.Entity;//更新移动者的实体位置（坐标、方向、速度）
                }
                if (notifies.ContainsKey(entitysync.Id))
                {
                    notifies[entity.entityId].OnEntityChanged(entity);//通知移动者的实体 位置数据更新
                    notifies[entity.entityId].OnEntityEvent(entitysync.Event, entitysync.Param);//通知移动者的实体 播放相应事件动画 entitysync.Event
                }
            }
        }


    }


}
