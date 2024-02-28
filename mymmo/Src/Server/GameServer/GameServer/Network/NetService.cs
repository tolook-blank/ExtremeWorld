using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using GameServer;
using Common;

namespace Network
{//服务器端网络服务
    class NetService
    {
        static TcpSocketListener ServerListener;
        public bool Init(int port)//初始化网络服务，接受一个端口参数
        {  //创建了一个 TcpSocketListener 对象（用于监听连接请求），服务端可监听的最大用户数为 10
            ServerListener = new TcpSocketListener("127.0.0.1", port, 10);
            ServerListener.SocketConnected += OnSocketConnected;//并订阅了 SocketConnected 事件，该事件在新的连接建立时触发
            return true;
        }

        //启动网络服务
        public void Start()
        {
            Log.Info("Server Starting Listener...");
            ServerListener.Start();//开始监听连接请求
            
            //启动消息分发器，指定 8 个工作线程
            MessageDistributer<NetConnection<NetSession>>.Instance.Start(8);
            Log.Info("NetService Started");
        }

        //停止网络服务
        public void Stop()
        {
            Log.Warning("Stop NetService...");

            ServerListener.Stop();//停止了 TcpSocketListener 对象的监听

            Log.Warning("Stoping Message Handler...");
            MessageDistributer<NetConnection<NetSession>>.Instance.Stop();//停止消息分发器
        }

        //当有新的连接建立时调用的回调方法
        private void OnSocketConnected(object sender, Socket e)
        {
            IPEndPoint clientIP = (IPEndPoint)e.RemoteEndPoint;
            //可以在这里对IP做一级验证,比如黑名单

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            NetSession session = new NetSession();
            //用这个NetSession对象创建了一个 NetConnection 对象（表示一个网络连接），这个NetConnection对象被关联到一个 Socket 对象，用于与客户端进行通信。
            NetConnection<NetSession> connection = new NetConnection<NetSession>(e, args,
                new NetConnection<NetSession>.DataReceivedCallback(DataReceived),
                new NetConnection<NetSession>.DisconnectedCallback(Disconnected), session);

            Log.WarningFormat("Client[{0}]] Connected", clientIP);//记录客户端连接的日志信息
        }


        // 当连接断开时调用的回调方法
        static void Disconnected(NetConnection<NetSession> sender, SocketAsyncEventArgs e)
        {
            //Performance.ServerConnect = Interlocked.Decrement(ref Performance.ServerConnect);
            sender.Session.Disconnected();//调用断开连接方法
            Log.WarningFormat("Client[{0}] Disconnected", e.RemoteEndPoint);//日志记录 客户端断开连接
        }


        // 当接收到数据时调用的回调方法
        static void DataReceived(NetConnection<NetSession> sender, DataEventArgs e)
        {
            Log.WarningFormat("Client[{0}] DataReceived Len:{1}", e.RemoteEndPoint, e.Length);//记录了客户端接收到数据的日志信息
            //由包处理器处理封包
            lock (sender.packageHandler)
            {
                sender.packageHandler.ReceiveData(e.Data, 0, e.Data.Length);//将接收到的数据传递给连接对象的包处理器（sender.packageHandler）进行处理
            }
            //PacketsPerSec = Interlocked.Increment(ref PacketsPerSec);
            //RecvBytesPerSec = Interlocked.Add(ref RecvBytesPerSec, e.Data.Length);
        }
    }
}
