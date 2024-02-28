
using UnityEngine;
using UnityEngine.UI;

public class TabButton : MonoBehaviour {

    public Sprite activeImage; //点击按钮，切换到激活图片，需要指定 
    Sprite normalImage; //按钮正常图片，在初始化时直接获取，无需指定

    public TabView tabView; //TabButton对应的TabView

    public int tabIndex = 0; //页面索引，需要手动填写
    public bool selected = false;

    Image tabImage;

	void Start () {
        tabImage = this.GetComponent<Image>();
        normalImage = tabImage.sprite;
        this.GetComponent<Button>().onClick.AddListener(OnClick);//动态给按钮添加事件
    }

    public void Select(bool select)
    {
        tabImage.overrideSprite = select ? activeImage : normalImage;
    }

    void OnClick() //点击按钮
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_Return1);

        this.tabView.SelectTab(this.tabIndex);//根据索引，切换到 选择页
    }
}
