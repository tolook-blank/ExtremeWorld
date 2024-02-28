using System;
using UnityEngine;
using UnityEngine.AI;
public class NavPathRenderer : MonoSingleton<NavPathRenderer>
{
    LineRenderer pathRenderer; //绘制寻路的路径线
    NavMeshPath path; //寻路路径

    void Start()
    {
        pathRenderer = GetComponent<LineRenderer>();
        pathRenderer.enabled = false;
    }
    public void SetPath(NavMeshPath path, Vector3 target)//path是寻路完成后的路径
    {
        this.path = path;
        if (this.path == null)
        {
            pathRenderer.enabled = false;
            pathRenderer.positionCount = 0;
        }
        else
        {
            pathRenderer.enabled = true;
            pathRenderer.positionCount = path.corners.Length + 1;//path.corners: 路径以路标列表的形式表示，存储在 corners 数组中, +1 是为了包含终点
            pathRenderer.SetPositions(path.corners);//设置路标corners
            pathRenderer.SetPosition(pathRenderer.positionCount - 1, target); //设置终点
            for (int i = 0; i < pathRenderer.positionCount; i++)
            {
                pathRenderer.SetPosition(i, pathRenderer.GetPosition(i) + Vector3.up * 0.2f);//将绘制路线 上浮0.2m,方便显示在路面上
            }
        }
    }
}

