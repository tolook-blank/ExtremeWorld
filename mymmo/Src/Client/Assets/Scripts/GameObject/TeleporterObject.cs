using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.Data;
using Services;

public class TeleporterObject : MonoBehaviour
{

    public int ID; //传送点ID，标识主城、副本、野外的传送点
    Mesh mesh = null;
    void Start()
    {
        this.mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnTriggerEnter(Collider other)//传送点的触发事件
    {
        PlayerInputController pc = other.GetComponent<PlayerInputController>();//检测触发者other 是否是玩家，只有玩家绑定了PlayerInputController组件
        if (pc != null && pc.isActiveAndEnabled)//若是还没有调用OnEnable的脚本，虽然gameObject.isActiveInHierachy、enabled 是true，但是isActiveAndEnabled是false
        {
            TeleporterDefine td = DataManager.Instance.Teleporters[this.ID];//通过传送点ID,获取传送点信息（TeleporterDefine.xlsx中定义的）
            if (td == null)//若传送点不存在，报错
            {
                Debug.LogErrorFormat("TeleporterObject: Character [{0}] Enter Teleporter [{1}],But TeleporterDefine not existed", pc.character.Info.Name, this.ID);
                return;
            }
            Debug.LogFormat("TeleporterObject: Character [{0}] Enter Teleporter [{1}:{2}]", pc.character.Info.Name, td.ID, td.Name);
            if (td.LinkTo > 0) //td.LinkTo 是传送目标点ID， 传入点触发器的LinkTo =0,不会触发传送请求
            {
                if (DataManager.Instance.Teleporters.ContainsKey(td.LinkTo))//当传送点、传送目的地 都存在 
                {
                    MapService.Instance.SendMapTeleport(this.ID);//发送传送请求
                }
                else
                {
                    Debug.LogErrorFormat("TeleporterObject: ID:{0} LinkID {1} error", td.ID, td.LinkTo);
                }

            }
        }

    }

    //编辑器拓展（宏），使传送点在游戏视图不显示，只在编辑视图显示
#if UNITY_EDITOR
    void OnDrawGizmos()//删除传送点的Mesh Render组件后，绘制线框Gizmos，标识出传送点
    {
        Gizmos.color = Color.green;
        if (this.mesh != null)
        {
            Gizmos.DrawWireMesh(this.mesh, this.transform.position + Vector3.up * this.transform.localScale.y * 0.5f,
                this.transform.rotation, this.transform.localScale);//绘制线框，标识传送点
        }
        UnityEditor.Handles.color = Color.red;
        //绘制小箭头，箭头朝向为 传送后的角色朝向
        UnityEditor.Handles.ArrowHandleCap(0, this.transform.position, this.transform.rotation, 1f, EventType.Repaint);
    }
#endif




}
