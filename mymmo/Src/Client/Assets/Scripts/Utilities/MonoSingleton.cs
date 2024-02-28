using UnityEngine;


public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoBehaviour
{//继承MonoBehaviour的脚本 必须绑定在游戏对象上。当游戏运行时，一创建游戏对象，就会创建一个Mono脚本实例，但如果每次场景加载，都创建一个实例，那就天生无法保证单例。 所以如果非要做成Mono单例，就要保证场景中只能存在一个实例，不能再额外创建，且不会销毁

    public bool global = true; //global = true 表示全局单例，即在游戏中全局存在，切换场景不销毁；global = false 表示场景单例，只在该场景中存在，切换场景就立刻销毁；
    static T instance; //T是脚本类名称
    public static T Instance //static类型， 在第一次被其他脚本请求时 才会生成此Instance
    {
        get
        {
            if (instance == null)
            {
                instance = (T)FindObjectOfType<T>();//返回第一个类型为 T 的已加载的激活对象，否则返回 null。
            }
            return instance;
        }

    }

    /*
     * 报错：再次进入主城小地图出现MinmapManager. Instance. minmap 为null
     问题原因：UIMinMap的Start() 先于UIMain的Mono单例Start()执行，导致UIMinMap执行Start()之后被删掉所致
     解决方法：Mono单例的Start() 改为Awake() ，同时删除NetClient的Awake
     */

    void Awake() //单例类被继承时，子类中不能有Awake()，否则会重载覆盖掉父类的 Awake()，导致单例失效
    {
        Debug.LogWarningFormat("{0}[{1}] Awake", typeof(T), this.GetInstanceID());
        if (global)//如果是全局单例
        { //Mono单例，就要保证场景中只能存在一个实例（不会销毁），不能再额外创建
            if (instance != null && instance != this.gameObject.GetComponent<T>())//如果instance != null 说明已经存在一个单例了，当前脚本是多余的副本
            {
                Destroy(this.gameObject);//删除 多余的实例
                return; //返回
            }
            //否则 instance == null 
            DontDestroyOnLoad(this.gameObject);//保证全局单例脚本 绑定的游戏对象不销毁，永久保存
            instance = this.gameObject.GetComponent<T>();//初始化，把当前脚本设为单例， public Component GetComponent (Type type)：如果游戏对象附加了类型为 type 的组件，则将其返回，否则返回 null 
        }
        this.OnStart(); //调用 OnStart 初始化
    }

    protected virtual void OnStart()//将OnStart()设为virtual虚函数，方便给子类重写;避免覆盖Mono单例的Start()
    {

    }
}