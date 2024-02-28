using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using GameServer.Network;
using System.Configuration;

using System.Threading;

using Network;
using GameServer.Services;
using GameServer.Managers;
namespace GameServer
{
    class GameServer
    {
        Thread thread;//用于运行服务器Update的线程
        bool running = false;//用于控制服务器是否在运行中
        NetService network;//处理网络通信的服务实例

        public bool Init()//当完成相关服务功能时，还要记得在GameServer.Init()中启动服务，如UserService.Instance.Init();
        {
            //Properties.Settings.具体配置方式：右键资源管理器中GameSever项目->选择属性->设置，添加完配置后保存，即可自动生成
            int Port = Properties.Settings.Default.ServerPort;//从配置中读取服务器监听的端口号 Properties.Settings.Default是C#配置，在App.config中
            //初始化服务
            network = new NetService();//启动网络服务
            network.Init(Port);//创建 TcpSocketListener 对象，用于监听连接请求

            DBService.Instance.Init();   //启动数据库服务（需要先启动SSMS数据库）
			DataManager.Instance.Load();//manager初始化
            MapService.Instance.Init(); //启动地图服务
            UserService.Instance.Init(); //启动User服务
            ItemService.Instance.Init(); //初始化道具服务，涉及商店购买道具
            BagService.Instance.Init();
            QuestService.Instance.Init();
            FriendService.Instance.Init();
            TeamService.Instance.Init();
            GuildService.Instance.Init();
            ChatService.Instance.Init();

            //创建了一个新的线程 thread 用于运行 Update 方法
            thread = new Thread(new ThreadStart(this.Update));

            return true;
        }

        public void Start()
        {
            network.Start();//启动网络服务
            running = true;//服务器标记为 运行中
            //在创建一个线程实例后，通过 Start() 方法启动该线程。启动后，线程的执行将从 Thread 类的构造函数传入的 ThreadStart 委托所指定的方法开始。
            thread.Start();//启动 thread 线程，用于运行 this.Update 方法
        }


        public void Stop()
        {
            running = false;
            //Join() 方法会阻塞主线程，直到调用它的线程（即被等待的线程）执行完成。
            thread.Join();//在主线程中等待 thread 线程结束
            network.Stop();
        }

        public void Update()
        {
            var mapManager = MapManager.Instance;
            while (running)
            {
                Time.Tick();
                Thread.Sleep(100);//这里设定服务端，每100毫秒跑一帧，执行一次Update
                //Console.WriteLine("{0} {1} {2} {3} {4}", Time.deltaTime, Time.frameCount, Time.ticks, Time.time, Time.realtimeSinceStartup);
                mapManager.Update();
            }
        }
    }
}
