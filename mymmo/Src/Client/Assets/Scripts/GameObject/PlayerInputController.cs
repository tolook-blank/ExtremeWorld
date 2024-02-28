
using UnityEngine;
using UnityEngine.AI;
using Entities;
using SkillBridge.Message;//由协议生成
using Services;
using Managers;


public class PlayerInputController : MonoBehaviour
{//玩家输入控制器 附加到Character的Prefab上。PlayerInputController 把玩家的输入转换成 Rigidbody、Character(Entity)的行为

    //Entity、Character.position等等 都是逻辑坐标系，而绑定在Character Prefab上的this、Rigidbody等等 是世界坐标系
    public Rigidbody rb;//角色绑定的刚体，刚体控制角色移动, Rigidbody的position、direction 都是世界坐标系
    SkillBridge.Message.CharacterState state;

    public float rotateSpeed = 2.0f;//控制角色旋转的速度。

    public float turnAngle = 10;//角色旋转的角度阈值。

    public int speed; //计算显示速度

    //在GameObjectManager中执行InitGameObject脚本时，初始化PlayerInputController的character、entityController
    public Character character;//控制的角色，character的position、direction 都是逻辑坐标系

    public EntityController entityController;//PlayerInputController 通过控制 Character,来间接的控制EntityController

    public bool onAir = false; //跳跃约束，空中不能跳跃

    private NavMeshAgent agent;//导航代理

    private bool autoNav = false; //角色的自动寻路

    const float navDis = 3f;

    // Use this for initialization
    void Start()
    {
        state = SkillBridge.Message.CharacterState.Idle;
        //entityController = GetComponent<EntityController>();
        if (this.character == null)
        {
            DataManager.Instance.Load();
            NCharacterInfo cinfo = new NCharacterInfo();
            cinfo.Id = 1;
            cinfo.Name = Models.User.Instance.CurrentCharacter.Name;
            cinfo.ConfigId = 1;
            cinfo.Entity = new NEntity();
            cinfo.Entity.Position = new NVector3();
            cinfo.Entity.Direction = new NVector3();
            cinfo.Entity.Direction.X = 0;
            cinfo.Entity.Direction.Y = 100;
            cinfo.Entity.Direction.Z = 0;
            this.character = new Character(cinfo);

            if (entityController != null) entityController.entity = this.character;
        }
        if (agent == null)
        {
            agent = this.gameObject.AddComponent<NavMeshAgent>();
            agent.stoppingDistance = navDis;
            agent.enabled = false; //默认先禁用
        }

    }

    //public void StartNav(Vector3 target) //传入target的世界坐标，开始寻路
    //{
    //    agent.enabled = true; //开始寻路后，再启用
    //    StartCoroutine(BeginNav(target));
    //}
    //IEnumerator BeginNav(Vector3 target)
    //{
    //    agent.SetDestination(target);//代理设置目的地，开始自动计算路径
    //    yield return null;
    //    autoNav = true; //开启自动寻路
    //    if (state != SkillBridge.Message.CharacterState.Move)//模拟移动状态
    //    {
    //        state = SkillBridge.Message.CharacterState.Move;
    //        this.character.MoveForward();
    //        this.SendEntityEvent(EntityEvent.MoveFwd);//广播状态，同步行走
    //        agent.speed = (this.character.speed / 100f) * 2; //初始代理速度 ，和真实速度保持一致
    //    }
    //}
    //public void StopNav() //停止寻路
    //{
    //    autoNav = false;//关闭自动寻路
    //    agent.ResetPath();
    //    if (state != CharacterState.Idle)
    //    {
    //        state = CharacterState.Idle;
    //        this.rb.velocity = Vector3.zero;
    //        this.character.Stop();
    //        this.SendEntityEvent(EntityEvent.Idle);
    //    }
    //    NavPathRenderer.Instance.SetPath(null, Vector3.zero);

    //}

    //public void NavMove()
    //{

    //    //计算路径时，pathPending 将为 true。几帧之后，如果有效路径可用，代理将恢复移动。
    //    if (agent.pathPending) return;

    //    //寻路失败，路径无效
    //    if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
    //    {
    //        StopNav();//停止寻路
    //        return;
    //    }

    //    //寻路未完成，返回，等待寻路完成
    //    if (agent.pathStatus != NavMeshPathStatus.PathComplete)
    //        return;

    //    //当玩家主动输入 控制方向，打断自动寻路
    //    if (Mathf.Abs(Input.GetAxis("Vertical")) > 0.1 || Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1)
    //    {
    //        StopNav();
    //        return;
    //    }

    //    //绘制寻路线路
    //    NavPathRenderer.Instance.SetPath(agent.path, agent.destination);// NavMeshAgent.path 返回 NavMeshPath由导航系统计算的路径
    //    //agent.path.corners路径以路标列表的形式表示，存储在 corners 数组中

    //    if (agent.isStopped || agent.remainingDistance < navDis)//代理停止，或 已到达目的地
    //    {
    //        StopNav();
    //        return;
    //    }
    //}

    //执行顺序：FixedUpdate -> Update() ->LateUpdate() 
    void FixedUpdate()//处理玩家输入，使用 Rigidbody`进行移动,应该在 `FixedUpdate` 方法中进行
    {
        if (character == null) return;//没有控制对象，无法进行输入控制

        //if (autoNav) //寻路状态下，暂时屏蔽玩家输入（防止冲突）
        //{
        //    NavMove();
        //    return;
        //}

        if (InputManager.Instance != null && InputManager.Instance.IsInputMode == true) return; //当前角色处于 聊天输入模式，此时键盘输入不控制角色（避免打字造成角色移动）

        float v = Input.GetAxis("Vertical");//获取玩家垂直（前后）输入的值，通常是W键或者S键。

        if (v > 0.01)//玩家按下了前进键（例如W键）
        {
            if (state != SkillBridge.Message.CharacterState.Move)
            {
                state = SkillBridge.Message.CharacterState.Move;//设置为移动状态
                this.character.MoveForward();
                this.SendEntityEvent(EntityEvent.MoveFwd);
            }
            //使用 `Rigidbody` 的 `velocity` 属性来实现角色的移动， 刚体的 垂直+ 水平 方向速度分量 
            this.rb.velocity = this.rb.velocity.y * Vector3.up + 2 * GameObjectTool.LogicToWorld(character.direction) * (this.character.speed + 9.81f) / 100f;
            //this.rb.velocity *= 2; 为什么会突然飞天、遁地，是velocity的影响吗？

        }
        else if (v < -0.01)//玩家按下了后退键（例如S键）
        {
            if (state != SkillBridge.Message.CharacterState.Move)
            {
                state = SkillBridge.Message.CharacterState.Move;
                this.character.MoveBack();
                this.SendEntityEvent(EntityEvent.MoveBack);
            }
            this.rb.velocity = this.rb.velocity.y * Vector3.up + 2 * GameObjectTool.LogicToWorld(character.direction) * (this.character.speed + 9.81f) / 100f;
            //this.rb.velocity *= 2;
        }
        else//玩家没有按下前进或后退键，空闲状态
        {
            if (state != SkillBridge.Message.CharacterState.Idle)
            {
                state = SkillBridge.Message.CharacterState.Idle;
                this.rb.velocity = Vector3.zero;
                this.character.Stop();
                this.SendEntityEvent(EntityEvent.Idle);
            }
        }

        if (Input.GetButtonDown("Jump"))
        {
            this.SendEntityEvent(EntityEvent.Jump);
        }

        //通过水平输入来转移视角、方向， 也可以换种方式实现鼠标控制视角转动 Input.GetAxis("Mouse X")
        float h = Input.GetAxis("Horizontal");//获取玩家的水平输入，通常是A键和D键。
        if (h < -0.1 || h > 0.1)//如果玩家正在左右移动，则执行旋转操作，使角色的方向朝向输入方向
        {
            this.transform.Rotate(0, h * rotateSpeed, 0);//使其朝着水平输入 h 的方向旋转。
            Vector3 dir = GameObjectTool.LogicToWorld(character.direction);//角色的方向
            Quaternion rot = new Quaternion();
            rot.SetFromToRotation(dir, this.transform.forward);//将角色的朝向从 dir 转到 this.transform.forward

            //检查 rot 的欧拉角度是否在指定的角度范围内，用于确定是否需要更新角色的朝向。
            if (rot.eulerAngles.y > this.turnAngle && rot.eulerAngles.y < (360 - this.turnAngle))
            {//
                character.SetDirection(GameObjectTool.WorldToLogic(this.transform.forward));//将角色的方向设置为 this.transform.forward，也就是当前的前方方向。
                rb.transform.forward = this.transform.forward;//使角色的刚体rb运动与角色朝向一致。
                this.SendEntityEvent(EntityEvent.None);
            }

        }
        //Debug.LogFormat("FixedUpdate：velocity {0}", this.rb.velocity.magnitude);
    }

    Vector3 lastPos;
    float lastSync = 0;

    private void LateUpdate()//LateUpdate 方法通常用于处理 在每一帧的渲染之后 执行的逻辑。
    {
        if (character == null) return;//确保只有在character存在时才进行处理。

        Vector3 offset = this.rb.transform.position - lastPos;//计算当前帧的角色位置和上一帧位置之间的偏移量，用于计算速度。
        this.speed = (int)(offset.magnitude * 100f / Time.deltaTime);//计算显示速度 = 位移/时间，其中offset.magnitude 是向量的长度，即位移
        //Debug.LogFormat("LateUpdate velocity {0} : {1}", this.rb.velocity.magnitude, this.speed);
        this.lastPos = this.rb.transform.position;//将当前帧位置 赋值给上一帧位置 ，形成迭代

        Vector3Int goLogicPos = GameObjectTool.WorldToLogic(this.rb.transform.position);
        float logicOffset = (goLogicPos - this.character.position).magnitude;
        //当刚体位置rb.transform.position和实体位置character.position 之间的误差 >500,做一次强制同步SetPosition
        if (logicOffset > 500)
        {
            this.character.SetPosition(GameObjectTool.WorldToLogic(this.rb.transform.position));
            this.SendEntityEvent(EntityEvent.None);
        }
        this.transform.position = this.rb.transform.position;//位置同步，把刚体位置 重新赋值给 角色位置

        //角色方向同步
        Vector3 dir = GameObjectTool.LogicToWorld(character.direction);
        Quaternion rot = new Quaternion();
        rot.SetFromToRotation(dir, this.transform.forward);

        if (rot.eulerAngles.y > this.turnAngle && rot.eulerAngles.y < (360 - this.turnAngle))//当角色方向偏差过大，同步方向
        {
            character.SetDirection(GameObjectTool.WorldToLogic(this.transform.forward));
            this.SendEntityEvent(EntityEvent.None);
        }
    }

    public void SendEntityEvent(EntityEvent entityEvent, int param = 0)
    {
        if (entityController != null)
        {
            entityController.OnEntityEvent(entityEvent, param); //通知entityController，控制动画事件
        }

        MapService.Instance.SendMapEntitySync(entityEvent, this.character.EntityData, param);//用MapService 把角色的移动同步事件 发送给服务器
    }
}
