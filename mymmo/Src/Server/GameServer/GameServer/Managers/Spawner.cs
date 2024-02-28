
using Common;
using Common.Data;
using GameServer.Models;

namespace GameServer.Managers
{
    class Spawner 
    {
        public SpawnRuleDefine Define { get; set; } //刷怪规则

        private SpawnPointDefine spawnPoint = null; //保存当前刷怪点
        private Map Map; //当前地图

        private float spawnTime = 0;//刷怪时间
        private float unspawnTime = 0;//记录怪物的消灭时间（杀怪的时间点 + 刷怪周期 = 下次怪物的刷新时间点）

        private bool spawned = false;//是否已经刷了怪

        public Spawner(SpawnRuleDefine define, Map map)
        {
            this.Define = define;
            this.Map = map;

            if (DataManager.Instance.SpawnPoints[this.Map.ID].ContainsKey(this.Define.SpawnPoint))//若地图中存在 此SpawnPoint刷怪点ID
            {
                spawnPoint = DataManager.Instance.SpawnPoints[this.Map.ID][this.Define.SpawnPoint]; //加载刷怪点
            }
            else
            {
                Log.ErrorFormat("SpawnRule[{0}] don't have SpawnPoint[{1}]", this.Define.ID, this.Define.SpawnPoint);
            }
        }

        public void Update()
        {
            if (this.CanSpawn()) //能刷怪，才允许刷怪
            {
                this.Spawn();
            }
        }

        private bool CanSpawn()
        {
            if (this.spawned)//已经刷过怪
                return false; //不能刷怪
            if (this.unspawnTime + this.Define.SpawnPeriod > Time.time) //还没到刷怪时间（杀怪的时间点 + 刷怪周期 = 下次怪物的刷新时间点）
                return false; //也不能刷怪

            return true;
        }
        private void Spawn()//执行刷怪，调用MonsterManager生成怪物
        {
            this.spawned = true;
            Log.InfoFormat("Map[{0}] Spawn[{1}]:Monster:[{2}],Lv:[{3}], At Point:{4}", Define.MapID, Define.ID, Define.SpawnMonID, Define.SpawnLevel,Define.SpawnPoint);
            this.Map.MonsterManager.Create(Define.SpawnMonID, Define.SpawnLevel, this.spawnPoint.Position, this.spawnPoint.Direction);//刷怪点 控制怪物生成的位置、方向 spawnPoint.Position
        }
    }
}
