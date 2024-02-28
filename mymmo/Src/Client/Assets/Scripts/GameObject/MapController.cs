using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;

public class MapController : MonoBehaviour {
	//MapController 挂载在 每张地图Scence的 MapRoot上。 所以每张地图都有一个MapController脚本，可用来感知 地图的切换
	//在每次切换地图时，触发MapRoot的MapController.Start(),来控制小地图管理器 加载小地图

	public Collider minimapBoundingBox;//使用顶视图，用一个 包围盒Cube 恰好覆盖地图场景，则 Cube的长宽 就是 地图的长宽

	//MapController 初始化,控制小地图管理器 MinimapManager
	void Start () {
		MinimapManager.Instance.UpdateMinimap(minimapBoundingBox); //当前地图加载时，通知小地图管理器，更新包围盒、小地图
	}
	
	void Update () {
		
	}
}
