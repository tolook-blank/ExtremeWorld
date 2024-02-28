using System.Collections.Generic;
using Common;
using GameServer.Models;

namespace GameServer.Managers
{
    class SpawnManager : Singleton<SpawnManager> //刷怪管理器，全局存在
    {
        private Map Map;
        private List<Spawner> Rules = new List<Spawner>(); //刷怪规则管理器， Spawner列表
        public void Init(Map map)
        {
            this.Map = map;//刷怪点所属地图
            if (DataManager.Instance.SpawnRules.ContainsKey(map.Define.ID))//读取刷怪规则表中的所有 刷怪规则
            {
                foreach(var define in DataManager.Instance.SpawnRules[map.Define.ID].Values)//根据地图ID，提取刷怪规则
                {
                    this.Rules.Add(new Spawner(define, this.Map));//生成每条刷怪器
                }
            }
        }

        public void Update()
        {
            if (Rules.Count == 0)
                return;

            for(int i = 0; i < Rules.Count; i++)
            {
                this.Rules[i].Update();
            }
        }
    }
}
