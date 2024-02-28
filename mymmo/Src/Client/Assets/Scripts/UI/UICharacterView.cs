
using UnityEngine;

public class UICharacterView : MonoBehaviour
{//挂载在CharacterSelect场景中的CharacterView物体上

    public GameObject[] characters;//各个职业角色的prefab,且按顺序排列：战士 法师 弓箭手

    private int currentCharacter = 0;//当前角色索引【0,1,2】，默认=0 显示战士职业prefab

    public int CurrectCharacter
    {
        get
        {
            return currentCharacter;
        }
        set
        {
            currentCharacter = value;//设置角色索引
            this.UpdateCharacter();//更新角色显示
        }
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    void UpdateCharacter()//更新角色显示
    {
        for (int i = 0; i < 3; i++)//只开放了3个角色
        {
            characters[i].SetActive(i == this.currentCharacter);//启用 选择的职业
        }
    }
}
