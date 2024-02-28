using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SceneManager : MonoSingleton<SceneManager>
{//作为Mono单例脚本，绑定在Loading场景中的 SceneManager 游戏物体上
    public UnityAction<float> onProgress = null;

    public UnityAction onSceneLoadDone = null;

    // Use this for initialization
    protected override void OnStart()
    {
        
    }

    // Update is called once per frame
    void Update () {
		
	}

    public void LoadScene(string name)
    {
        //StartCoroutine(Example());（注意方法名后加括号，参数可写在括号里） 优点：灵活，性能开销小。
        //缺点：无法单独的停止这个协程，如果需要停止这个协程只能等待协同程序运行完毕或则使用StopAllCoroutine();方法。
        StartCoroutine(LoadLevel(name));//使用协程，加载场景。  协程是通过迭代器来实现功能的
    }

    IEnumerator LoadLevel(string name)//IEnumerator：是一个实现迭代器功能的接口。 yield return语句来暂停协程并提交一个唤醒条件
    {
        Debug.LogFormat("LoadLevel: {0}", name);
        AsyncOperation async = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(name); //AsyncOperation异步操作协同程序，LoadSceneAsync是异步加载，要加载场景的名称 name不区分大小写
        async.allowSceneActivation = true; //允许在场景准备就绪后，立即激活场景（跳转），如果将 allowSceneActivation 设置为 false，则progress进度将在 0.9 处停止，直到被设置为 true。
        async.completed += LevelLoadCompleted; //异步操作完成时，调用LevelLoadCompleted 事件处理函数
        while (!async.isDone) //当场景没有加载完毕 （异步操作未完成）
        {
            if (onProgress != null)
                onProgress(async.progress); //(float) AsyncOperation.progress:获取操作进度，数值为从0到1。当进度浮点值到达 1.0 并调用 isDone 时，操作结束。
            yield return null; //暂停协程等待下一帧继续执行(yield return null后面的代码会在下一帧运行，并且是在Update执行完之后才开始执行，但是会在LateUpdate之前执行)
        }
    }

    private void LevelLoadCompleted(AsyncOperation obj)
    {
        if (onProgress != null)
            onProgress(1f);
        Debug.Log("LevelLoadCompleted:" + obj.progress);
        if (onSceneLoadDone != null)
            onSceneLoadDone();
    }
}
