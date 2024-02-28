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
{//���ݼ�����
    public string DataPath;
    public Dictionary<int, MapDefine> Maps = null;
    public Dictionary<int, CharacterDefine> Characters = null;   //��ɫ���ñ�ID����ְҵ��Ӧö��ֵ����ְҵ���ñ�����
    public Dictionary<int, TeleporterDefine> Teleporters = null; //������
    public Dictionary<int, Dictionary<int, SpawnPointDefine>> SpawnPoints = null; //ˢ�ֵ�
    public Dictionary<int, Dictionary<int, SpawnRuleDefine>> SpawnRules = null; //ˢ�ֹ���
    public Dictionary<int, NpcDefine> Npcs = null;
    public Dictionary<int, ItemDefine> Items = null;
    public Dictionary<int, ShopDefine> Shops = null;
    public Dictionary<int, Dictionary<int, ShopItemDefine>> ShopItems = null; //������Key���̵�ID,��ƷID��, �������ֵ���
    public Dictionary<int, EquipDefine> Equips = null;
    public Dictionary<int, QuestDefine> Quests = null;
    public Dictionary<int, RideDefine> Rides = null; 

    public DataManager()
    {
        this.DataPath = "Data/";  //DataPath: src\client\Data
        Debug.LogFormat("DataManager > DataManager()");
    }

    public void Load()//Load()�������ʹ�ã���Ϊ����˲���ʹ��Unity��Э��  ��
    {
        string json = File.ReadAllText(this.DataPath + "MapDefine.txt");
        //�����л�����json�ļ��еĶ���������ȡ���� ����ԭ��ԭ�ȵ����ʵ��������
        //JsonConvert.DeserializeObject��������json�ļ������л���ת��ΪDictionary<int, MapDefine>���͵Ķ���
        this.Maps = JsonConvert.DeserializeObject<Dictionary<int, MapDefine>>(json);//��ɶ�ȡ���ݲ�����

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

    public IEnumerator LoadData()//LoadData()�ͻ���ʹ�ã���ͨ��Э������
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
    public void SaveTeleporters() //���洫�͵㣬���������͵�json
    {
        //public static string SerializeObject( Object value , Formatting formatting)
        //��ָ��Ҫ���л��Ķ���Object value��������������ĸ�ʽ Formatting formatting�����л�Ϊ JSON string�ַ�����ʽ
        string json = JsonConvert.SerializeObject(this.Teleporters, Formatting.Indented);
        File.WriteAllText(this.DataPath + "TeleporterDefine.txt", json);
    }

    public void SaveSpawnPoints()//����ˢ�ֵ㣬������ˢ�ֵ�json
    {
        string json = JsonConvert.SerializeObject(this.SpawnPoints, Formatting.Indented);
        File.WriteAllText(this.DataPath + "SpawnPointDefine.txt", json);
    }

#endif
}
