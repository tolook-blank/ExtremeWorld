using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Text;
using System;
using System.IO;
using Common.Data;
using Newtonsoft.Json;

public class DataManager : Singleton<DataManager>
{//数据加载类
    public string DataPath;
    public Dictionary<int, MapDefine> Maps = null;
    public Dictionary<int, CharacterDefine> Characters = null;   //角色配置表ID（即职业对应枚举值），职业配置表属性
    public Dictionary<int, TeleporterDefine> Teleporters = null; //传送门
    public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null; //刷怪点
    public Dictionary<int, Dictionary<int, SpawnRuleDefine>> SpawnRules = null; //刷怪规则
    public Dictionary<int, NpcDefine> Npcs = null;
    public Dictionary<int, ItemDefine> Items = null;
    public Dictionary<int, ShopDefine> Shops = null;
    public Dictionary<int, Dictionary<int, ShopItemDefine>> ShopItems = null; //有两列Key（商店ID,商品ID）, 用两个字典存放
    public Dictionary<int, EquipDefine> Equips = null;
    public Dictionary<int, QuestDefine> Quests = null;
    public Dictionary<int, RideDefine> Rides = null; 

    public DataManager()
    {
        this.DataPath = "Data/";  //DataPath: src\client\Data
        Debug.LogFormat("DataManager > DataManager()");
    }

    public void Load()//Load()供服务端使用，因为服务端不能使用Unity的协程  ，
    {
        string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
        //反序列化：将json文件中的二进制数据取出来 并还原成原先的类的实例化对象
        //JsonConvert.DeserializeObject方法，将json文件反序列化，转换为Dictionary<int, MapDefine>类型的对象
        this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);//完成读取数据并加载

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

    public IEnumerator LoadData()//LoadData()客户端使用，可通过协程启用
    {
        string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
        this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "CharacterDefine.txt");
        this.Characters = JsonConvert.DeserializeObject<Dictionary<int, CharacterDefine>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "TeleporterDefine.txt");
        this.Teleporters = JsonConvert.DeserializeObject<Dictionary<int, TeleporterDefine>>(json);


        json = File.ReadAllText(this.DataPath + "NpcDefine.txt");
        this.Npcs = JsonConvert.DeserializeObject<Dictionary<int, NpcDefine>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "ItemDefine.txt");
        this.Items = JsonConvert.DeserializeObject<Dictionary<int, ItemDefine>>(json);

        json = File.ReadAllText(this.DataPath + "ShopDefine.txt");
        this.Shops = JsonConvert.DeserializeObject<Dictionary<int, ShopDefine>>(json);

        json = File.ReadAllText(this.DataPath + "ShopItemDefine.txt");
        this.ShopItems = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, ShopItemDefine>>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "RideDefine.txt");
        this.Rides = JsonConvert.DeserializeObject<Dictionary<int, RideDefine>>(json);

        json = File.ReadAllText(this.DataPath + "EquipDefine.txt");
        this.Equips = JsonConvert.DeserializeObject<Dictionary<int, EquipDefine>>(json);

        yield return null;

        json = File.ReadAllText(this.DataPath + "QuestDefine.txt");
        this.Quests = JsonConvert.DeserializeObject<Dictionary<int, QuestDefine>>(json);

        json = File.ReadAllText(this.DataPath + "SpawnPointDefine.txt");
        this.SpawnPoints = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnPointDefine>>>(json);

        json = File.ReadAllText(this.DataPath + "SpawnRuleDefine.txt");
        this.SpawnRules = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<int, SpawnRuleDefine>>>(json);

        yield return null;
    }

#if UNITY_EDITOR
    public void SaveTeleporters() //保存传送点，并导出传送点json
    {
        //public static string SerializeObject( Object value , Formatting formatting)
        //将指定要序列化的对象Object value，按照设置输出的格式 Formatting formatting，序列化为 JSON string字符串形式
        string json = JsonConvert.SerializeObject(this.Teleporters, Formatting.Indented);
        File.WriteAllText(this.DataPath + "TeleporterDefine.txt", json);
    }

    public void SaveSpawnPoints()//保存刷怪点，并导出刷怪点json
    {
        string json = JsonConvert.SerializeObject(this.SpawnPoints, Formatting.Indented);
        File.WriteAllText(this.DataPath + "SpawnPointDefine.txt", json);
    }

#endif
}
