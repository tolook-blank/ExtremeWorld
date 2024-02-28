
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoSingleton<SoundManager>
{

    public AudioMixer audioMixer;
    public AudioSource musicAudioSource;
    public AudioSource soundAudioSource;

    const string MusicPath = "Music/";
    const string SoundPath = "Sound/";

    private bool musicOn;
    public bool MusicOn
    {
        get { return musicOn; }
        set
        {
            musicOn = value;
            this.MusicMute(!musicOn);
        }
    }

    private bool soundOn;
    public bool SoundOn
    {
        get { return soundOn; }
        set
        {
            soundOn = value;
            this.SoundMute(!soundOn);
        }
    }


    private int musicVolume;
    public int MusicVolume
    {
        get { return musicVolume; }
        set
        {
            if (musicVolume != value)
            {
                musicVolume = value;
                if (musicOn)
                {
                    this.SetVolume("MusicVolume", musicVolume);
                }
            }
        }
    }

    private int soundVolume;
    public int SoundVolume
    {
        get { return soundVolume; }
        set
        {
            if (soundVolume != value)
            {
                soundVolume = value;
                if (soundOn)
                {
                    this.SetVolume("SoundVolume", soundVolume);
                }
            }
        }
    }

    void Start()
    {
        this.MusicVolume = Config.MusicVolume;
        this.SoundVolume = Config.SoundVolume;
        this.MusicOn = Config.MusicOn;
        this.SoundOn = Config.SoundOn;
    }

    //"MusicVolume" 是混音器暴露的变量
    private void MusicMute(bool mute)//背景音乐静音
    {
        this.SetVolume("MusicVolume", mute ? 0 : musicVolume);//静音:将音量设置为0 
    }

    //"SoundVolume" 是混音器暴露的变量
    private void SoundMute(bool mute)//音效静音
    {
        this.SetVolume("SoundVolume", mute ? 0 : soundVolume);
    }

    private void SetVolume(string name, int value)//设置音量大小
    {
        float volume = value * 0.5f - 50f; //音量value[0,100]转化为 分贝区间[听不见-50db，听得见0db]
        this.audioMixer.SetFloat(name, volume); //设置混音器暴露的变量
    }


    public void PlayMusic(string name)
    {
        AudioClip clip = Resloader.Load<AudioClip>(MusicPath + name);
        if (clip == null)
        {
            Debug.LogWarningFormat("PlayMusic: {0} not existed.", name);
            return;
        }
        if (musicAudioSource.isPlaying)
        {
            musicAudioSource.Stop();
        }
        musicAudioSource.clip = clip;
        musicAudioSource.Play();//背景音乐是循环播放 Play（持续的）
    }

    public void PlaySound(string name)
    {
        AudioClip clip = Resloader.Load<AudioClip>(SoundPath + name);
        if (clip == null)
        {
            Debug.LogWarningFormat("PlaySound: {0} not existed.", name);
            return;
        }
        soundAudioSource.PlayOneShot(clip); //音效是按次播放 PlayOneShot， 而背景音乐是循环播放
    }
}
