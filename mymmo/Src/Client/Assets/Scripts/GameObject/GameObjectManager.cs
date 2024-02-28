using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Entities;
using Services;
using Managers;
using Models;
using Common;


public class GameObjectManager : MonoSingleton<GameObjectManager> //使用Mono单例（全局的），保证在切换地图场景时，游戏对象不销毁
{//游戏对象管理器，作为Mono单例脚本 挂载在主城的 GameObjectManager 物体上

    Dictionary<int, GameObject> Characters = new Dictionary<int, GameObject>();//游戏对象管理器（entityId，角色预制体）

    protected override void OnStart() //在单例MonoSingleton中，必须要重载单例类中的OnStart()，不能使用Start()
    {
        StartCoroutine(InitGameObjects());
        CharacterManager.Instance.OnCharacterEnter += OnCharacterEnter;//订阅CharacterManager中的角色进入主城事件
        CharacterManager.Instance.OnCharacterLeave += OnCharacterLeave;//订阅角色离开主城事件
    }

    IEnumerator InitGameObjects()//进入游戏时初始化一遍，通过协程，查找当前场景中所有角色，分别为每个角色创建游戏对象
    {
        foreach (var cha in CharacterManager.Instance.Characters.Values)
        {
            CreateCharacterObject(cha);
            yield return null;
        }
    }


    private void OnDestroy()//离开主城地图时，调用OnDestroy
    {
        CharacterManager.Instance.OnCharacterEnter -= OnCharacterEnter;//取消订阅角色进入主城事件
        CharacterManager.Instance.OnCharacterLeave -= OnCharacterLeave;//取消订阅
    }

    // Update is called once per frame
    void Update()
    {

    }

    //MapService.OnMapCharacterEnter -> CharacterManager.AddCharacter -> GameObjectManager.OnCharacterEnter -> GameObjectManager.CreateCharacterObject
    //在游戏进程中，每当收到CharacterManager的角色进入地图 通知，就创建新角色对象 （每次进入地图都需要重新创建游戏对象）
    void OnCharacterEnter(Character cha)
    {
        CreateCharacterObject(cha);
    }

    private void CreateCharacterObject(Character character)//创建单个角色游戏对象
    {
        Log.InfoFormat("CreateCharacterObject : character.Info.Name: {0}", character.Info.Name);
        //当游戏对象管理器中 不包含character.entityId 或 character.entityId对应的游戏对象已销毁，才能创建
        if (!Characters.ContainsKey(character.entityId) || Characters[character.entityId] == null)
        {
            Object obj = Resloader.Load<Object>(character.Define.Resource);//导入角色预制体，character.Define.Resource为prefab的资源路径
            if (obj == null)
            {
                Debug.LogErrorFormat("Character[{0}] Resource[{1}] not existed.", character.Define.TID, character.Define.Resource);
                return;
            }
            GameObject go = (GameObject)Instantiate(obj, this.transform); //创建角色游戏对象，作为GameObjectManager（this.transform）的子物体
            go.name = "Character_" + character.Id + "_" + character.Name;//角色统一改名
            Characters[character.entityId] = go;//添加到GameObjectManager的 游戏对象管理器

            UIWorldElementManager.Instance.AddCharacterNameBar(go.transform, character);//添加角色信息栏
        }
        this.InitGameObject(Characters[character.entityId], character);
    } 

    //创建角色时CreateCharacterObject，做初始化InitGameObject
    void InitGameObject(GameObject go, Character character)//go 是 character的游戏对象实例
    {
        //初始化角色坐标，转换为Unity世界坐标
        go.transform.position = GameObjectTool.LogicToWorld(character.position);
        go.transform.forward = GameObjectTool.LogicToWorld(character.direction);
        EntityController ec = go.GetComponent<EntityController>();
        if (ec != null)
        {
            ec.entity = character;
            ec.isPlayer = character.IsCurrentPlayer; //是否是 当前玩家控制的角色
            ec.Ride(character.Info.Ride);//角色上马（传入坐骑ID）（当进入游戏时，初始化 周围骑坐骑的人）
        }
        PlayerInputController pc = go.GetComponent<PlayerInputController>();//获取go的PlayerInputController组件，判断其是否是玩家
        if (pc != null)//玩家的Character prefab实例上才有PlayerInputController脚本
        {
            if (character.IsCurrentPlayer)//如果是当前玩家的角色（自己控制的）
            {
                User.Instance.CurrentCharacterObject = pc; //当前玩家的游戏对象
                MainPlayerCamera.Instance.player = go; //摄像头跟随 当前控制角色的游戏对象
                pc.enabled = true;//启用PlayerInputController
                pc.character = character; //当前控制的角色
                pc.entityController = ec;//将 玩家输入控制器 和 EntityController 关联 ，或在玩家输入控制器中直接entityController = GetComponent<EntityController>();
            }
            else//如果是别的玩家的角色，我不能控制
            {
                pc.enabled = false;//禁用PlayerInputController输入控制
            }
        }
    }

    void OnCharacterLeave(Character character)//每当角色离开地图，就销毁游戏对象  
    {
        if (!Characters.ContainsKey(character.entityId))//如果游戏对象管理器 中不存在该角色 ，则说明该角色已经删除
        {
            return;
        }

        if (Characters[character.entityId] != null)//该角色的游戏对象 还存在
        {
            Destroy(Characters[character.entityId]); //销毁游戏对象 
            Characters.Remove(character.entityId); //游戏对象管理器中 删除
        }
    }

    //在parent位置处 召唤坐骑，返回此坐骑控制器
    public RideController LoadRide(int rideId, Transform parent)
    {
        var rideDefine = DataManager.Instance.Rides[rideId];
        Object obj = Resloader.Load<Object>(rideDefine.Resource);//"加载路径Resources/Ride/...下的 坐骑游戏体，如R001"
        if (obj == null)//先判断此坐骑  在配置表中是否存在
        {
            Debug.LogErrorFormat("Ride[{0}] Resource[{1}] not existed.", rideDefine.ID, rideDefine.Resource);
            return null;
        }
        GameObject go = (GameObject)Instantiate(obj, parent);//实例化坐骑
        go.name = "Ride_" + rideDefine.ID + "_" + rideDefine.Name;
        return go.GetComponent<RideController>(); //返回坐骑身上绑定的 坐骑控制器
    }

}

