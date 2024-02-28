using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Common.Data;
using Services;

[ExecuteInEditMode]
public class SpawnPoint : MonoBehaviour
{
    //类似于传送点
    public int ID; //刷怪点ID，标识主城、副本、野外的刷怪点
    Mesh mesh = null;

    void Start()
    {
        this.mesh = GetComponent<MeshFilter>().sharedMesh;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //编辑器拓展（宏），使传送点在游戏视图不显示，只在编辑视图显示（打包后无效）
#if UNITY_EDITOR
    void OnDrawGizmos()//绘制线框Gizmos，标识出刷怪点
    {
        Vector3 pos = this.transform.position + Vector3.up * this.transform.localScale.y * 0.5f; // 刷怪点位置 上移一半的高度，保证怪物站在地面上
        Gizmos.color = Color.red;
        if (this.mesh != null)
        {
            Gizmos.DrawWireMesh(this.mesh, pos,this.transform.rotation, this.transform.localScale);//绘制线框，标识刷怪点
        }
        UnityEditor.Handles.color = Color.red;
        //绘制小箭头，箭头朝向为 怪物朝向
        UnityEditor.Handles.ArrowHandleCap(0, this.transform.position, this.transform.rotation, 1f, EventType.Repaint);
        UnityEditor.Handles.Label(pos, "SpawnPoint:" + this.ID);
    }
#endif




}
