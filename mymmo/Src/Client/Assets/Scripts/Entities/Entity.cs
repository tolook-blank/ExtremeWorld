
using UnityEngine;
using SkillBridge.Message;

namespace Entities
{
    public class Entity
    {
        public int entityId;//和entityData.ID一致，每个实体的ID是唯一的，且每次进入会不同，其值由服务端的EntityManager生成

        //移动同步位置 三个属性：坐标、方向、速度
        public Vector3Int position; //逻辑坐标（Entity、Character.position都是逻辑坐标系）
        public Vector3Int direction;//逻辑方向
        public int speed;


        private NEntity entityData;//保存 从服务器上同步到客户端的网络实体信息
        public NEntity EntityData
        {
            get {
                UpdateEntityData();//游戏对象的Entity 同步到 NEntity
                return entityData;
            }
            set {
                entityData = value;
                this.SetEntityData(value);
            }
        }

        public Entity(NEntity entity)
        {
            this.entityId = entity.Id;
            this.entityData = entity;
            this.SetEntityData(entity);
        }

        public virtual void OnUpdate(float delta)
        {
            if (this.speed != 0) //当前速度不为零，就朝当前方向移动
            {
                Vector3 dir = this.direction;
                this.position += Vector3Int.RoundToInt(dir * speed * delta / 100f); //方向*速度
            }
            UpdateEntityData();
        }

        public void SetEntityData(NEntity entity)//从网络NEntity 更新到本地Entity
        {
            this.position = this.position.FromNVector3(entity.Position);
            this.direction = this.direction.FromNVector3(entity.Direction);
            this.speed = entity.Speed;
        }

        public void UpdateEntityData()//用本地Entity 更新 网络NEntity 
        {
            entityData.Position.FromVector3Int(this.position);
            entityData.Direction.FromVector3Int(this.direction);
            entityData.Speed = this.speed;
        }

    }
}
