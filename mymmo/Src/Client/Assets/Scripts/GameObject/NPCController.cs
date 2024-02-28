using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Common.Data;
using Managers;
using Models;

public class NPCController : MonoBehaviour
{//XXController，一般绑定在XX场景中的游戏物体上， NPCController 绑定在了 MainCity 的 MapRoot/NPC上
    /* NPCController(绑定NPC)：
     1、与NPC执行交互
     2、鼠标选中NPC高亮
     3、NPC小动作
     */
    public int npcID; //此脚本绑定的npcId

    SkinnedMeshRenderer renderer;
    Animator anim;
    Color orignColor;

    private bool inInteractive = false; //此NPC是否 正处于交互的状态
    private float timer = 0;

    NpcDefine npc;

    NpcQuestStatus questStatus;

    void Start()
    {
        renderer = this.GetComponentInChildren<SkinnedMeshRenderer>(); //获取骨骼动画渲染器对象，因为SkinnedMeshRenderer在子组件上，使用GetComponentInChildren 
        //renderer = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
        orignColor = renderer.sharedMaterial.color;

        anim = this.GetComponent<Animator>();
        npc = NPCManager.Instance.GetNpcDefine(npcID);
        NPCManager.Instance.UpdateNpcPosition(this.npcID, this.transform.position);//收集NPC的位置
        this.StartCoroutine(Actions());
        RefreshNpcStatus(); //刷新NPC状态
        QuestManager.Instance.onQuestStatusChanged += OnQuestStatusChanged; //订阅NPC的任务状态改变事件
    }

    void OnQuestStatusChanged(Quest quest) //每次NPC任务状态变化，就刷新 该NPC头顶任务状态显示
    {
        this.RefreshNpcStatus();
    }

    void RefreshNpcStatus()
    {
        questStatus = QuestManager.Instance.GetQuestStatusByNpc(this.npcID);
        UIWorldElementManager.Instance.AddNpcQuestStatus(this.transform, questStatus); //刷新NPC 头顶任务状态显示
    }

    void OnDestroy() 
    {
        QuestManager.Instance.onQuestStatusChanged -= OnQuestStatusChanged;
        if (UIWorldElementManager.Instance != null)
        {
            UIWorldElementManager.Instance.RemoveNpcQuestStatus(this.transform);//在NPC销毁时，删除头顶状态图标
        }
    }

    IEnumerator Actions() //NPC随机动作，
    {
        while (true)//在协程中可以写死循环，因为不会卡住主线程，除非做耗时的逻辑
        {
            if (inInteractive)
                yield return new WaitForSeconds(2f); //处于交互状态，间隔2秒 做休闲动作
            else
                yield return new WaitForSeconds(Random.Range(3f, 5f));//不是交互状态，随机间隔3-5秒 做休闲动作
            this.Relax(); //休闲动作动画
        }
    }

    void Relax()
    {
        anim.SetTrigger("Relax");
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
    }

    void Interactive()//与NPC触发交互
    {
        //if (!inInteractive) //当不处于交互状态，才执行交互操作。 防止重复点击，重复交互
        //{
        //    inInteractive = true;
        //    StartCoroutine(DoInteractive());
        //}
        if (timer > 3f) //交互结束3s后，才执行交互操作。 防止短时间重复点击，重复交互
        {
            inInteractive = true;
            StartCoroutine(DoInteractive());
        }
    }

    IEnumerator DoInteractive()//执行交互操作
    {
        yield return FaceToPlayer(); //让NPC面向玩家
        if (NPCManager.Instance.Interactive(this.npc)) //执行NPC交互
        {
            timer = 0; //结束交互，将计时器归0
            anim.SetTrigger("Talk"); //Task、Function 型NPC 都要播放说话动画
        }
        yield return new WaitForSeconds(2f); //2秒内无法重复点击，需要等待2秒
        inInteractive = false; //交互结束
    }

    IEnumerator FaceToPlayer()
    {
        Vector3 faceTo = (User.Instance.CurrentCharacterObject.transform.position - this.transform.position).normalized;//玩家位置 - NPC位置，得到NPC的交互面朝方向
        while (Mathf.Abs(Vector3.Angle(this.gameObject.transform.forward, faceTo)) > 5) //调整NPC正面朝向，直到面向玩家
        {
            this.gameObject.transform.forward = Vector3.Lerp(this.gameObject.transform.forward, faceTo, Time.deltaTime * 5f);//使用插值，让NPC 平滑转向，面对玩家
            yield return null;
        }
    }

    void OnMouseDown() //MonoBehaviour的函数，当鼠标点击collider时调用OnMouseDown，可在此与NPC交互
    {
        //if (Vector3.Distance(this.transform.position, User.Instance.CurrentCharacterObject.transform.position) > 2f)
        //{
        //    User.Instance.CurrentCharacterObject.StartNav(this.transform.position);
        //}
        //用户体验细节：可以等寻路完成后，再打开界面交互
        Interactive();
    }

    private void OnMouseOver()//当鼠标悬停在NPC(Collider)上时，每帧调用一次。
    {
        Highlight(true);
    }

    private void OnMouseEnter() //鼠标选中，高亮
    {
        Highlight(true);
    }
    private void OnMouseExit()  //鼠标离开，退出高亮
    {
        Highlight(false);
    }
    void Highlight(bool highlight)//当鼠标选中NPC时，高亮NPC
    {
        if (highlight)
        {
            if (renderer.sharedMaterial.color != Color.white) //高亮，材质颜色 设为白色
                renderer.sharedMaterial.color = Color.white;
        }
        else
        {
            if (renderer.sharedMaterial.color != orignColor) //不是高亮，原始颜色
                renderer.sharedMaterial.color = orignColor;
        }
    }

}
