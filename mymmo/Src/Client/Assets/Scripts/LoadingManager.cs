using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

using SkillBridge.Message;
using ProtoBuf;
using Services;
using Managers;

//客户端最先初始化 LoadingManager，LoadingManager可视为客户端的主入口
public class LoadingManager : MonoBehaviour
{
    //加载界面的管理器，挂载于Loading场景
    public GameObject UITips;
    public GameObject UILoading;
    public GameObject UILogin;

    public Slider progressBar; //加载进度条
    public Text progressText;  
    public Text progressNumber;//加载进度文本显示

    // Use this for initialization
    IEnumerator Start()
    {
        //初始化日志
        log4net.Config.XmlConfigurator.ConfigureAndWatch(new System.IO.FileInfo("log4net.xml"));
        UnityLogger.Init();
        Common.Log.Init("Unity");
        Common.Log.Info("LoadingManager start");


        UITips.SetActive(true); //先激活显示 Tips界面
        UILoading.SetActive(false);
        UILogin.SetActive(false);
        yield return new WaitForSeconds(2f);//等待两秒后，激活显示 加载界面
        UILoading.SetActive(true);
        yield return new WaitForSeconds(1f);//等待一秒后，停用Tips界面
        UITips.SetActive(false);


        yield return DataManager.Instance.LoadData();//开启资源加载协程（嵌套协程）

        //Init basic services，初始化 Manager、Service等模块
        MapService.Instance.Init();
        UserService.Instance.Init();
        StatusService.Instance.Init();
        FriendService.Instance.Init();
        TeamService.Instance.Init();
        GuildService.Instance.Init();

        ShopManager.Instance.Init();
        ChatService.Instance.Init();
        SoundManager.Instance.PlayMusic(SoundDefine.Music_Login);//播放登录bgm

        // Fake Loading Simulate ，模拟进度条加载
        for (float i = 50; i < 100;)
        {
            i += Random.Range(0.1f, 1.5f);
            progressBar.value = i;
            progressNumber.text = ((int)progressBar.value).ToString() + "%";
            yield return new WaitForEndOfFrame();
        }

        UILoading.SetActive(false);
        UILogin.SetActive(true); //激活显示 登录界面
        yield return null;
    }

    void Update()
    {

    }
}

/*
public GameObject UILoading;   //加载界面
public Slider progressBar;   //进度条
public Text progressText;      //加载进度文本

public void LoadNextLeaver()
{
    UILoading.SetActive(true);
    StartCoroutine(LoadLeaver());
}
IEnumerator LoadLeaver()
{
    AsyncOperation operation = SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1); //获取当前场景并加一
    //operation.allowSceneActivation = false;
    while (!operation.isDone)   //当场景没有加载完毕
    {
        progressBar.value = operation.progress;  //进度条与场景加载进度对应
        progressText.text = (operation.progress * 100).ToString() + "%";
        yield return null;
    }
}
 */