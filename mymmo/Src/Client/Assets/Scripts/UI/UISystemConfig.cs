
using UnityEngine;
using UnityEngine.UI;

public class UISystemConfig : UIWindow
{
    //UISetting中 点击系统设置按钮 弹出的界面为UISystemConfig
    public Image musicOff;
    public Image soundOff;

    public Toggle toggleMusic;
    public Toggle toggleSound;

    public Slider sliderMusic;
    public Slider sliderSound;

    void Start()
    {
        toggleMusic.isOn = Config.MusicOn; //音乐的开关
        toggleSound.isOn = Config.SoundOn; //音效的开关
        sliderMusic.value = Config.MusicVolume; //音量的调节
        sliderSound.value = Config.SoundVolume; //音效的调节

    }
    public override void OnYesClick() //关闭保存按钮
    {
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);//播放点击音效
        PlayerPrefs.Save(); //保存玩家的偏好设置（如音量级别，分辨率等），Unity的PlayerPrefs类： https://blog.csdn.net/qq_33795300/article/details/131727488
        base.OnYesClick();
    }

    //绑定在 toggleMusic组件的 OnValueChanged事件中
    public void MusicToggle(bool on)//点击音乐Toggle按钮,切换音乐开关
    {
        musicOff.enabled = !on;
        Config.MusicOn = on;
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

    //绑定在 toggleSound组件的 OnValueChanged事件中
    public void SoundToggle(bool on)//点击音效Toggle按钮,切换音效开关
    {
        soundOff.enabled = !on;
        Config.SoundOn = on;
        SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
    }

    //绑定在 sliderMusic组件的 OnValueChanged事件中,拉动Slider的滑块调节，传入的是slider的value
    public void MusicVolume(float vol)//调节音乐音量
    {
        Config.MusicVolume = (int)vol;
        PlaySound();
    }

    //绑定在 sliderSound组件的 OnValueChanged事件中
    public void SoundVolume(float vol)//调节音效音量
    {
        Config.SoundVolume = (int)vol;
        PlaySound();
    }

    float lastPlay = 0;

    private void PlaySound()//当调节音量时，至少隔 0.1s，播放一次点击音效，来让玩家感受调节后的音量大小
    {
        if (Time.realtimeSinceStartup - lastPlay > 0.1)
        {
            lastPlay = Time.realtimeSinceStartup;//游戏启动后的真实时间，不受帧率和暂停影响
            SoundManager.Instance.PlaySound(SoundDefine.SFX_UI_Click);
        }
    }

}
