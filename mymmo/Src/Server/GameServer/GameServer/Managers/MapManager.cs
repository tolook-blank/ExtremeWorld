using System.Collections.Generic;
using Common;
using GameServer.Models;

namespace GameServer.Managers
{
    class MapManager : Singleton<MapManager> //管理当前有哪张地图，地图中有哪些角色？
    {
        //地图管理器，管理所有地图
        Dictionary<int, Map> Maps = new Dictionary<int, Map>();//地图ID, 地图

        public void Init()
        {
            foreach (var mapdefine in DataManager.Instance.Maps.Values)
            {
                Map map = new Map(mapdefine);
                Log.InfoFormat("MapManager.Init > Map:{0}:{1}", map.Define.ID, map.Define.Name);
                this.Maps[mapdefine.ID] = map;
            }
        }


        //若不重载[]，需要先公开Maps，使用方法 ：MapManager.Instance.Maps[mapId]
        //效果相当于重载[] 运算符，使用方法：MapManager.Instance[mapId].方法 = Map.方法
        public Map this[int key]
        {
            get
            {
                return this.Maps[key];
            }
        }

        //因为地图需要 刷新BOSS、刷怪点周期生成怪物 等等自主服务，所以只有地图管理器需要 更新Update() ，而其他的管理器都是请求响应式，不需要Update() 
        public void Update()
        {
            foreach(var map in this.Maps.Values)//地图管理器 遍历所有的地图
            {
                map.Update();
            }
        }
    }
}
