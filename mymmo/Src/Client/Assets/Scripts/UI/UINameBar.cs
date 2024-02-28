using Entities;
using UnityEngine;
using UnityEngine.UI;
using Models;

[ExecuteInEditMode]
public class UINameBar : MonoBehaviour
{
    public Text characterName;
    //public Text Level;
    public Image avatar;

    public Sprite warrior;
    public Sprite wizard;
    public Sprite archer;
    public Character character;//哪个角色的 名称信息面板

    void Start()
    {
        if (User.Instance.CurrentCharacter == character.Info)
        {
            switch (User.Instance.CurrentCharacter.Class)
            {
                case SkillBridge.Message.CharacterClass.Warrior:
                    avatar.overrideSprite = warrior;
                    break;
                case SkillBridge.Message.CharacterClass.Wizard:
                    avatar.overrideSprite = wizard;
                    break;
                case SkillBridge.Message.CharacterClass.Archer:
                    avatar.overrideSprite = archer;
                    break;
                default: break;
            }
        }
        if (this.character != null)
        {
            if (character.Info.Type == SkillBridge.Message.CharacterType.Monster)
                this.avatar.gameObject.SetActive(false);//怪物不显示头像，只有 "名称 LV.等级 "
            else
                this.avatar.gameObject.SetActive(true);
        }
        //if (Level != null)
        //{
        //    Level.text = this.character.Info.Level.ToString();
        //}
    }


    void Update()
    {
        this.UpdateInfo();//在Update中，随时更新角色的信息面板
        //让血条和摄像机保持同方向, 同方向 和同旋转 的效果相同, 移动到 UIWorldElement中处理
        //this.transform.forward = Camera.main.transform.forward; 
    }

    void UpdateInfo()
    {
        if (this.character != null)
        {
            //if (this.Level.text != this.character.Info.Level.ToString())
            //{
            //    Level.text = this.character.Info.Level.ToString();
            //}
            string name = " Lv." + this.character.Info.Level +" " + this.character.Name;
            if (name != this.characterName.text)//角色的等级或姓名变更
            {
                this.characterName.text = name;//更新显示
            }
        }
    }
}
