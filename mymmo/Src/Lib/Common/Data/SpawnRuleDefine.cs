
using SkillBridge.Message;

namespace Common.Data
{
    public class SpawnRuleDefine
    {
        public int ID { get; set; }
        public int MapID { get; set; }
        public int SpawnMonID { get; set; } //怪物ID
        public int SpawnLevel{ get;set; } //怪物等级
        public SPAWN_TYPE SpawnType { get; set; }
        public int SpawnPoint { get; set; } //刷怪点ID
        public int SpawnPoints { get; set; } 
        public float SpawnPeriod { get; set; } //刷怪周期
    }
}
