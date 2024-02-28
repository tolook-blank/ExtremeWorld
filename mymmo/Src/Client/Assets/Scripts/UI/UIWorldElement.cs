
using UnityEngine;

public class UIWorldElement : MonoBehaviour
{

    //世界坐标的UI元素（例如 血条、任务状态提示）
    public Transform owner;//世界UI元素跟随的目标， 在UIWorldElementManager中赋值
    public float height = 2.5f;//height >角色高度


    void Update()
    {
        //让 WorldElement.transform 自动跟踪 owner.transform
        if (owner != null)
        { //血条UI是屏幕坐标  ,人物角色：世界坐标 , 将世界坐标 转为-> 屏幕坐标 Camera.main.WorldToScreenPoint(position) 
            //this.transform.position = Camera.main.WorldToScreenPoint(owner.position + Vector3.up * height);//屏幕UI使用屏幕坐标，血条跟随显示在3D角色上方
            this.transform.position = owner.position + Vector3.up * height; //血条跟随角色移动，显示在角色头顶2.5m处，世界UI元素用世界坐标
        }
        if (Camera.main != null)
            this.transform.forward = Camera.main.transform.forward; //动态更新世界元素的朝向,让血条对准相机
    }
}
