using UnityEngine;

class Config
{

    public static bool MusicOn
    {
        get { return PlayerPrefs.GetInt("Music", 1) == 1; }
        set
        {
            PlayerPrefs.SetInt("Music", value ? 1 : 0); //设置开关的value是bool值，转换 1 或 0
            SoundManager.Instance.MusicOn = value;
        }
    }

    public static bool SoundOn
    {
        get { return PlayerPrefs.GetInt("Sound", 1) == 1; }
        set
        {
            PlayerPrefs.SetInt("Sound", value ? 1 : 0);
            SoundManager.Instance.SoundOn = value;
        }
    }

    public static int MusicVolume //音乐音量大小
    {
        get { return PlayerPrefs.GetInt("MusicVolume", 100); }
        set
        {
            PlayerPrefs.SetInt("MusicVolume", value);
            SoundManager.Instance.MusicVolume = value;
        }
    }

    public static int SoundVolume
    {
        get { return PlayerPrefs.GetInt("SoundVolume", 100); }
        set
        {
            PlayerPrefs.SetInt("SoundVolume", value);
            SoundManager.Instance.SoundVolume = value;
        }
    }

    ~Config() //销毁时，保存一次设置
    {
        PlayerPrefs.Save();
    }

}
