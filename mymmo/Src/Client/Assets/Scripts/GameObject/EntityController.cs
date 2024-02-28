using SkillBridge.Message;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Entities;
using Managers;
using Models;

public class EntityController : MonoBehaviour, IEntityNotify //继承接口IEntityNotify，实现接口方法 来接收EntityManager的通知
{//实体控制器 也绑定到Character的Prefab上。EntityController控制Character的Animator动画状态机

    public Animator anim;
    public Rigidbody rb;
    private AnimatorStateInfo currentBaseState;//预留变量

    //在GameObjectManager中执行InitGameObject脚本时，初始化EntityController的entity
    public Entity entity; //控制器控制的实体

    public UnityEngine.Vector3 position;
    public UnityEngine.Vector3 direction;
    Quaternion rotation;

    public UnityEngine.Vector3 lastPosition;
    Quaternion lastRotation;

    public float speed;
    public float animSpeed = 1.5f;
    public float jumpPower = 3.0f;

    public bool isPlayer = false;//是否是 当前玩家控制的角色

    public RideController rideController;//坐骑控制器
    public Transform rideBone;//绑定坐骑点（接触点骨骼），用来对齐坐骑

    private int currentRide = 0;

    // Use this for initialization
    void Start()
    {
        if (entity != null)
        {
            EntityManager.Instance.RegisterEntityChangeNotify(entity.entityId, this);//（每个玩家都要）注册通知事件
            this.UpdateTransform();
        }

        if (!this.isPlayer) //若不是当前玩家的角色
            rb.useGravity = false;
    }

    void UpdateTransform()
    {
        //entity.position是逻辑坐标，以cm为单位； this.position是世界坐标，以m为单位
        this.position = GameObjectTool.LogicToWorld(entity.position);
        this.direction = GameObjectTool.LogicToWorld(entity.direction);

        this.rb.MovePosition(this.position);//刚体rb移动到this.position的位置
        this.transform.forward = this.direction;
        this.lastPosition = this.position;//记录上一次位置
        this.lastRotation = this.rotation;
    }

    void OnDestroy() //角色死亡，角色对象销毁
    {
        if (entity != null)
            Debug.LogFormat("{0} OnDestroy :ID:{1} POS:{2} DIR:{3} SPD:{4} ", this.name, entity.entityId, entity.position, entity.direction, entity.speed);

        if (UIWorldElementManager.Instance != null)//在角色死亡(EntityController绑定在角色游戏对象上)时，删除角色信息栏
        {
            UIWorldElementManager.Instance.RemoveCharacterNameBar(this.transform);

        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (this.entity == null)
            return;

        this.entity.OnUpdate(Time.fixedDeltaTime);

        if (!this.isPlayer)//若不是当前玩家的角色
        {
            this.UpdateTransform();
        }
    }

    public void OnEntityRemoved()//接收删除Entity的通知
    {
        if (UIWorldElementManager.Instance != null)
        {
            UIWorldElementManager.Instance.RemoveCharacterNameBar(this.transform);//删除实体的信息栏

            //foreach(var kv in DataManager.Instance.Npcs)
            //{
            //    if(kv.Value.Type == Common.Data.NpcType.Task)
            //    {
            //        Transform trans = NPCManager.Instance.GetNpcPositon(kv.Key);
            //        UIWorldElementManager.Instance.RemoveNpcQuestStatus(trans); //删除任务项NPC头顶的状态图标
            //    }
            //}
        }
        Destroy(this.gameObject); //删除实体的游戏对象物体
    }

    public void OnEntityChanged(Entity entity) //实体位置数据更新
    {
        Debug.LogFormat("OnEntityChanged: ID:{0} POS:{1} DIR:{2} SPD:{3}", entity.entityId, entity.position, entity.direction, entity.speed);
    }

    //通过实体事件 控制动画
    public void OnEntityEvent(EntityEvent entityEvent, int param)
    {
        switch (entityEvent)//角色实体动画事件
        {
            case EntityEvent.Idle:
                anim.SetBool("Move", false);
                anim.SetTrigger("Idle");
                break;
            case EntityEvent.MoveFwd:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.MoveBack:
                anim.SetBool("Move", true);
                break;
            case EntityEvent.Jump:
                anim.SetTrigger("Jump");
                break;
            case EntityEvent.Ride:
                {
                    this.Ride(param);
                }
                break;
        }
        if (this.rideController != null) this.rideController.OnEntityEvent(entityEvent, param);//同步坐骑的动画事件
    }

    public void Ride(int rideId) //角色上马/下马
    {
        if (currentRide == rideId) return;
        currentRide = rideId;
        if (rideId > 0)//上马
        {
            this.rideController = GameObjectManager.Instance.LoadRide(rideId, this.transform);//原地召唤坐骑，获取此坐骑控制器
        }
        else//下马
        {
            Destroy(this.rideController.gameObject);//销毁坐骑（即坐骑控制器所绑定的游戏对象）
            this.rideController = null;
        }

        if (this.rideController == null)//下马之后
        {
            this.anim.transform.localPosition = Vector3.zero;
            this.anim.SetLayerWeight(1, 0);//设置动画层（动画状态机中的层起到了动画混合和组织的作用，通过调整权重和设置过渡条件，实现更复杂的动画效果）
        }
        else//上马之后
        {
            this.rideController.SetRider(this);//坐骑设置骑乘者
            this.anim.SetLayerWeight(1, 1);
        }
    }

    public void SetRidePotision(Vector3 position)//position是坐垫位置 
    {
        //设置骑乘坐骑时 角色的动画位置，以显示出平稳的骑乘效果（坐骑可能跳跃）
        this.anim.transform.position = position + (this.anim.transform.position - this.rideBone.position);
    }
}
