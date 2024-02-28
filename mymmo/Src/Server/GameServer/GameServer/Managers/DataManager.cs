
using System.Collections.Generic;
using System.IO;
using Common;
using Common.Data;
using Newtonsoft.Json;

namespace GameServer.Managers
{
    public class DataManager : Singleton<DataManager>
    {//��ȡ���ñ�Ŀ¼��Src\Server\GameServer\GameServer\bin\Debug\Data\XXXDefine
        internal string DataPath;
        internal Dictionary<int, MapDefine> Maps = null;
        internal Dictionary<int, CharacterDefine> Characters = null;//��ɫְҵ����TID����Ӧ��ɫְҵ��������Ϣ
        internal Dictionary<int, TeleporterDefine> Teleporters = null;
        public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null; //ˢ�ֵ㣬��ͼID,ˢ�ֵ�ID,ˢ�ֵ�
        public Dictionary<int, Dictionary<int, SpawnRuleDefine>> SpawnRules = null; //ˢ�ֹ��򣬵�ͼID,ˢ�ֵ�ID,ˢ�ֹ���
        public Dictionary<int, NpcDefine> Npcs = null;
        public Dictionary<int, ItemDefine> Items = null; //���߱�
        public Dictionary<int, ShopDefine> Shops = null;
        public Dictionary<int,Dictionary<int, ShopItemDefine>> ShopItems = null; //������Key, �������ֵ���
        public Dictionary<int, EquipDefine> Equips = null;
        public Dictionary<int, QuestDefine> Quests = null;
        public Dictionary<int, RideDefine> Rides = null;

        public DataManager()
        {
            this.DataPath = "Data/"; //DataPath������·�� Src\Server\GameServer\GameServer\bin\Debug\Data
            Log.Info("DataManager > DataManager()");
        }

        public void Load() 
        {
            string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
            this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);

            json = File.ReadAllText(this.DataPath + "CharacterDefine.txt");
            this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

            json = File.ReadAllText(this.DataPath + "TeleporterDefine.txt");
            this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);

            json = File.ReadAllText(this.DataPath + "NpcDefine.txt");
            this.Npcs = JsonConvert.DeserializeObject<Dictionary<int, NpcDefine>>(json);

            json = File.ReadAllText(this.DataPath + "ItemDefine.txt");
            this.Items = JsonConvert.DeserializeObject<Dictionary<int, ItemDefine>>(json);

            json = File.ReadAllText(this.DataPath + "ShopDefine.txt");
            this.Shops = JsonConvert.DeserializeObject<Dictionary<int, ShopDefine>>(json);

            json = File.ReadAllText(this.DataPath + "ShopItemDefine.txt");
            this.ShopItems = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, ShopItemDefine>>>(json);

            json = File.ReadAllText(this.DataPath + "EquipDefine.txt");
            this.Equips = JsonConvert.DeserializeObject<Dictionary<int, EquipDefine>>(json);

            json = File.ReadAllText(this.DataPath + "QuestDefine.txt");
            this.Quests = JsonConvert.DeserializeObject<Dictionary<int, QuestDefine>>(json);

            json = File.ReadAllText(this.DataPath + "SpawnPointDefine.txt");
            this.SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);

            json = File.ReadAllText(this.DataPath + "SpawnRuleDefine.txt");
            this.SpawnRules = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnRuleDefine>>>(json);

            json = File.ReadAllText(this.DataPath + "RideDefine.txt");
            this.Rides = JsonConvert.DeserializeObject<Dictionary<int, RideDefine>>(json);
        }
    }
}