
using UnityEngine;
using UnityEngine.UI;

public class UICharInfo : MonoBehaviour
{
    public SkillBridge.Message.NCharacterInfo info;

    public Text charClass;//职业类型
    public Text charName; //玩家创建的角色昵称
    public Image highlight;

    public bool Selected
    {
        get { return highlight.IsActive(); }
        set
        {
            highlight.gameObject.SetActive(value);
        }
    }

    // Use this for initialization
    void Start()
    {
        if (info != null)
        {
            this.charClass.text = this.info.Class.ToString();
            this.charName.text = this.info.Name;
        }
    }
}
