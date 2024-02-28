using Models;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainPlayerCamera : MonoSingleton<MainPlayerCamera>
{//主角摄像机（即跟随主角的摄像机，让我们看到主角的第一视角），作为Mono单例脚本 挂载在主城的 MainPlayerCamera 物体上
    public Camera playerCamera;
    public Transform viewPoint; //未使用
    public GameObject player; //摄像机跟随的主角 ，在GameObjectManager中执行InitGameObject脚本时，初始化MainPlayerCamera的player

    //单例类，不要使用Start ()，避免覆盖了父类 MonoSingleton中的Start()，若要初始化，则使用 继承父类的OnStart()
    protected override void OnStart()
    {
    }

    private void LateUpdate()
    {
        if (player == null && User.Instance.CurrentCharacterObject != null)// 此帧，如果摄像机未找到玩家角色，玩家目前为空
        {
            player = User.Instance.CurrentCharacterObject.gameObject;//给玩家重新赋值，让摄像头 跟随 当前控制的角色游戏对象
        }
        if (player == null) //若User.Instance.CurrentCharacterObject也为null,返回
        {
            return;
        }
        //摄像机跟随主角 
        this.transform.position = player.transform.position;//将玩家的位置 赋值给 摄像机，摄像机保持与玩家的位置同步
        this.transform.rotation = player.transform.rotation;//保持与玩家的旋转朝向同步
    }
}
