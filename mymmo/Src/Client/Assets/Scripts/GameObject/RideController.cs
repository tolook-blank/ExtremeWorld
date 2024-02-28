using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using SkillBridge.Message;//协议

public class RideController : MonoBehaviour
{//坐骑控制器 绑定在坐骑身上
    public Transform mountPoint; //绑定骑乘点
    public EntityController rider;//骑乘者
    public Vector3 offset; //偏移量，用于调节骑乘点
    private Animator anim; //动画状态机
   
    void Start()
    {
        this.anim = GetComponent<Animator>();
    }

    void Update()
    {
        if (this.mountPoint == null || this.rider == null) return;

        //坐骑转向时 对偏移量做基于方向的变换
        this.rider.SetRidePotision(this.mountPoint.position + this.mountPoint.TransformDirection(this.offset));
    }

    public void SetRider(EntityController rider)//设置骑乘者
    {
        this.rider = rider;
    }
    public void OnEntityEvent(EntityEvent entityEvent, int param)
    {
        switch (entityEvent)//坐骑的实体动画事件
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
        }
    }

}
