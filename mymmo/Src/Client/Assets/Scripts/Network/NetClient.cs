using System; //包含基本的类和类型
using System.Collections; //非泛型集合,包括数组、链表、队列、堆栈等一系列基本的集合接口和类
using System.Collections.Generic;//泛型集合提供了更好的类型安全性和性能，不需要进行装箱和拆箱操作。
using System.Text; //提供了处理文本的一些基本类,如字符串构建、编码和解码等， 包括：StringBuilder、Encoding 类
using System.Net;//System.Net 主要提供了高级别的网络通信支持，包括 HTTP 请求、URI 操作、Web 客户端等。它是在更高层次上处理网络通信的。
using System.Net.Sockets;//System.Net.Sockets 提供了更底层的套接字编程支持，包括 Socket 类，它提供了基本的套接字功能，允许你在网络上发送和接收数据。
using System.IO; //处理输入和输出操作，即文件和流的操作，包含：File、Directory、FileStream、MemoryStream、StreamReader、BinaryReader 等类
using UnityEngine;// Unity 游戏引擎中的一个主要命名空间，用于图形渲染、物理模拟、用户输入、资源管理等

using SkillBridge.Message; //使用 ProtoBuf生成的 协议库

namespace Network
{
    class NetClient : MonoSingleton<NetClient> //Mono单例类，挂载到Loading启动场景中的 NetClient 游戏物体上
    {

        const int DEF_POLL_INTERVAL_MILLISECONDS = 100; //默认网络线程保持的间隔
        const int DEF_TRY_CONNECT_TIMES = 3;            //默认服务器连接的重试次数
        const int DEF_RECV_BUFFER_SIZE = 64 * 1024;     //默认接收缓冲区的初始大小
        const int DEF_PACKAGE_HEADER_LENGTH = 4;        //默认数据包头部的大小
        const int DEF_SEND_PING_INTERVAL = 30;          //默认发送 ping 数据包的间隔
        const int NetConnectTimeout = 10000;            //默认连接等待时间
        const int DEF_LOAD_WHEEL_MILLISECONDS = 1000;   //默认等待一段时间后显示加载界面
        const int NetReconnectPeriod = 10;              //默认重连间隔时间

        public const int NET_ERROR_UNKNOW_PROTOCOL = 2;         //协议错误
        public const int NET_ERROR_SEND_EXCEPTION = 1000;       //发送异常
        public const int NET_ERROR_ILLEGAL_PACKAGE = 1001;      //接受到错误数据包
        public const int NET_ERROR_ZERO_BYTE = 1002;            //收发0字节
        public const int NET_ERROR_PACKAGE_TIMEOUT = 1003;      //收包超时
        public const int NET_ERROR_PROXY_TIMEOUT = 1004;        //proxy超时
        public const int NET_ERROR_FAIL_TO_CONNECT = 1005;      //3次连接不上
        public const int NET_ERROR_PROXY_ERROR = 1006;          //proxy重启
        public const int NET_ERROR_ON_DESTROY = 1007;           //结束的时候，关闭网络连接
        public const int NET_ERROR_ON_KICKOUT = 25;             //被踢了

        public delegate void ConnectEventHandler(int result, string reason); //定义事件委托，ConnectEventHandler 用于声明连接事件的委托，可以指向一个接受 int ，string 两个参数的方法
        public event ConnectEventHandler OnConnect;              // 连接事件，ConnectEventHandler 委托的一个实例。 事件是委托的一种封装，用于实现观察者设计模式。
        public event ConnectEventHandler OnDisconnect;           // 断开连接事件
        public delegate void ExpectPackageEventHandler();
        public event ExpectPackageEventHandler OnExpectPackageTimeout;  // 预期数据包超时事件
        public event ExpectPackageEventHandler OnExpectPackageResume;    // 预期数据包恢复事件

        public PackageHandler packageHandler = new PackageHandler(null);    // 数据包处理器

        // Socket 实例和数据流
        private IPEndPoint address;     // 网络终结点类，由 IP 地址和端口号组成，用于标识服务器地址
        private Socket clientSocket;    // 客户端 Socket

        private MemoryStream sendBuffer = new MemoryStream();       // 发送缓冲区，不设置初始容量，根据需要动态分配内存
        private MemoryStream receiveBuffer = new MemoryStream(DEF_RECV_BUFFER_SIZE);   // 接收缓冲区，设置初始容量
        private Queue<NetMessage> sendQueue = new Queue<NetMessage>();  // 发送消息队列

        private bool connecting = false;    // true 正在连接，false 非连接状态

        private int retryTimes = 0;         // 当前重试次数
        private int retryTimesTotal = DEF_TRY_CONNECT_TIMES;    // 最大重试次数

        private float lastSendTime = 0;     // 上次发送时间，用于控制发送频率。
        private int sendOffset = 0;         // 发送偏移量

        public bool running { get; set; }   // 网络是否运行中

        // 判断当前是否处于连接状态
        public bool Connected
        {
            get
            {
                return (clientSocket != default(Socket)) ? clientSocket.Connected : false;
            }
        }


        //void Awake() 父类MonoSingleton中使用了，子类避免覆盖，不能再使用
        //{
        //    running = true;
        //}

        protected override void OnStart()
        {
            running = true;
            MessageDistributer.Instance.ThrowException = true;
        }

        // 触发连接事件
        protected virtual void RaiseConnected(int result, string reason) //result为0，代表成功 ；1，代表失败
        {
            /* 线程安全性:
            在 C# 事件的实现中，事件的调用列表是在一个临时的副本上进行的， 如果 OnConnect事件 在调用时被修改（有其他线程在订阅或取消订阅事件），
            在一些情况下可能导致 NullReferenceException， 这是因为多线程环境下，OnConnect 可能在检查不为 null 后被另一个线程置为 null。
            而将事件调用赋值给一个本地变量（handler），是为了确保在检查和调用之间不会有其他线程修改委托，所以更安全。
             */
            ConnectEventHandler handler = OnConnect;
            if (handler != null) //首先检查是否有事件的订阅者
            {
                handler(result, reason); //逐个调用订阅的方法
            }
            //if (OnConnect != null) //可行但不够安全
            //{
            //    OnConnect(result, reason);
            //}
        }

        // 触发断开连接事件
        public virtual void RaiseDisonnected(int result, string reason = "")
        {
            ConnectEventHandler handler = OnDisconnect;
            if (handler != null)
            {
                handler(result, reason);
            }
        }

        // 触发期望接收数据包超时事件
        protected virtual void RaiseExpectPackageTimeout()
        {
            ExpectPackageEventHandler handler = OnExpectPackageTimeout;
            if (handler != null)
            {
                handler();
            }
        }

        // 触发恢复期望接收数据包事件
        protected virtual void RaiseExpectPackageResume()
        {
            ExpectPackageEventHandler handler = OnExpectPackageResume;
            if (handler != null)
            {
                handler();
            }
        }

        public NetClient()
        {
        }

        // 重置客户端状态，清空事件订阅者，清空发送队列等
        public void Reset()
        {
            MessageDistributer.Instance.Clear();
            this.sendQueue.Clear();

            this.sendOffset = 0;

            this.connecting = false; //非连接状态

            this.retryTimes = 0;
            this.lastSendTime = 0;

            this.OnConnect = null;
            this.OnDisconnect = null;
            this.OnExpectPackageTimeout = null;
            this.OnExpectPackageResume = null;
        }

        // 初始化客户端，设置服务器 IP 和端口
        public void Init(string serverIP, int port)
        {
            this.address = new IPEndPoint(IPAddress.Parse(serverIP), port);//IPAddress.Parse 将string类 IP 地址 转换为 IPAddress 类的实例
        }

        //异步连接服务器
        public void Connect(int times = DEF_TRY_CONNECT_TIMES)//times 是最大重连次数，默认为3次
        {
            if (this.connecting)// 如果正在连接中，直接返回，不进行新的连接尝试
            {
                return;
            }
            if (this.clientSocket != null) // 如果之前已经连接过，则先关闭
            {
                this.clientSocket.Close();
            }
            if (this.address == default(IPEndPoint)) // 如果服务器地址未初始化，则抛出异常，提示需要先初始化服务器地址
            {
                throw new Exception("Please Init first.");
            }
            //开始连接
            Debug.Log("DoConnect"); //打印日志
            this.connecting = true; //修改为 正在连接状态
            this.lastSendTime = 0;

            this.DoConnect(); // 真正连接服务器的操作
        }


        // 连接服务器
        void DoConnect()
        {
            Debug.Log("NetClient.DoConnect on " + this.address.ToString());
            try
            {
                if (this.clientSocket != null) // 首先检查并关闭之前的网络连接（即套接字）
                {
                    this.clientSocket.Close();
                }
                // 创建一个基于 TCP 的 IPv4 网络连接 （套接字），用于连接服务器  ，AddressFamily.InterNetworkV6（IPv6）
                this.clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.clientSocket.Blocking = true; //先将网络连接 设置为阻塞模式， 阻塞 客户端连接服务器，便于实时等待连接结果
                //在阻塞模式下，当网络连接执行一个操作（比如发送或接收数据）时，该操作会一直阻塞（暂停）程序的执行，直到操作完成或发生错误
                Debug.Log(string.Format("Connect[{0}] to server {1}", this.retryTimes, this.address) + "\n");
                //异步连接服务器
                IAsyncResult result = this.clientSocket.BeginConnect(this.address, null, null);
                //AsyncWaitHandle.WaitOne 方法用于阻塞主线程，等到异步操作完成或超时
                bool success = result.AsyncWaitHandle.WaitOne(NetConnectTimeout);
                if (success)//如果连接成功，则调用 EndConnect 完成连接操作。
                {
                    this.clientSocket.EndConnect(result);//结束异步连接请求，将完成连接操作并将控制权还给主线程
                }
                /*
                BeginConnect 的异步操作：
                在 BeginConnect 方法中，IAsyncResult 对象表示了异步操作的状态。当调用 BeginConnect 启动异步连接操作后，该方法立即返回，而不会等待连接完成。
                BeginConnect 在主线程之外启动了一个新线程（IO线程）执行连接尝试，不阻塞主线程的执行。

                AsyncWaitHandle.WaitOne 的阻塞：
                AsyncWaitHandle.WaitOne 方法是用于阻塞当前线程，等待异步操作完成或超时。
                WaitOne 是在主线程中调用的，它会阻塞主线程，等待异步连接线程的结果，这个等待是在主线程中进行的。
                当 WaitOne 返回时，说明异步连接操作已经完成，此时主线程会继续往下执行。
                如果连接在规定时间内成功建立，WaitOne 返回 true，主线程会执行 if (success) EndConnect。
                EndConnect 方法会结束异步连接请求，将控制权还给主线程。因为在异步操作完成后，EndConnect 方法会把异步操作的结果（连接成功或失败）传递给主线程。
                否则，WaitOne 返回 false，表示连接超时或失败，主线程执行执行 catch捕获异常

                注意：主线程在调用 WaitOne 时会被阻塞，但仅限于等待异步连接线程完成或超时。
                异步操作的完成状态并不是通过 WaitOne 返回的布尔值来决定的，而是通过EndConnect 返回 IAsyncResult异步操作的结果。
                WaitOne 只是为了主线程能够等待异步操作的完成，确保在连接完成之前主线程不会继续执行。
                */

            }
            catch (SocketException ex)// 如果连接失败，处理套接字异常，例如连接被服务器拒绝
            {
                if (ex.SocketErrorCode == SocketError.ConnectionRefused)
                {
                    this.CloseConnection(NET_ERROR_FAIL_TO_CONNECT);
                }
                Debug.LogErrorFormat("DoConnect SocketException:[{0},{1},{2}]{3} ", ex.ErrorCode, ex.SocketErrorCode, ex.NativeErrorCode, ex.ToString());
            }
            catch (Exception e)// 处理其他异常
            {
                Debug.Log("DoConnect Exception:" + e.ToString() + "\n");
            }

            if (this.clientSocket.Connected) // 如果成功连接服务器
            {
                this.clientSocket.Blocking = false;// 客户端和服务器之间的通讯 设为非阻塞模式
                this.RaiseConnected(0, "Success");// 触发连接成功事件，通知其他部分连接已建立
            }
            else // 连接失败，增加重试次数
            {
                this.retryTimes++;
                if (this.retryTimes >= this.retryTimesTotal) // 如果重试次数达到上限，触发连接失败事件
                {
                    this.RaiseConnected(1, "Cannot connect to server");
                }
            }
            this.connecting = false; // 修改为 非连接状态
        }


        // 保证网络连接（有断线重连功能）
        bool KeepConnect()
        {
            if (this.connecting) //正在连接中，返回false ; 非连接状态才向下执行，重连
            {
                return false;
            }
            if (this.address == null)
                return false;

            if (this.Connected) //连接成功
            {
                return true;
            }

            if (this.retryTimes < this.retryTimesTotal)//重连
            {
                this.Connect();
            }
            return false;
        }

        // 当直接关闭客户端时，调用CloseConnection关闭连接
        public void OnDestroy()
        {
            Debug.Log("OnDestroy NetworkManager.");
            this.CloseConnection(NET_ERROR_ON_DESTROY);
        }

        // 关闭连接，根据错误码处理不同情况
        public void CloseConnection(int errCode)
        {
            Debug.LogWarning("CloseConnection(), errorCode: " + errCode.ToString());
            this.connecting = false; //改为 非连接状态
            if (this.clientSocket != null)
            {
                this.clientSocket.Close(); //关闭当前网络连接
            }

            //清空缓冲区
            MessageDistributer.Instance.Clear();
            this.sendQueue.Clear();

            this.receiveBuffer.Position = 0;
            this.sendBuffer.Position = sendOffset = 0;

            switch (errCode)
            {
                case NET_ERROR_UNKNOW_PROTOCOL:
                    {
                        //协议错误，停止网络服务
                        this.running = false;
                    }
                    break;
                case NET_ERROR_FAIL_TO_CONNECT: //3次连接失败
                case NET_ERROR_PROXY_TIMEOUT: //proxy超时
                case NET_ERROR_PROXY_ERROR: //proxy重启
                    //NetworkManager.Instance.dropCurMessage();
                    //NetworkManager.Instance.Connect();
                    break;
                //离线处理
                case NET_ERROR_ON_KICKOUT:
                case NET_ERROR_ZERO_BYTE:
                case NET_ERROR_ILLEGAL_PACKAGE:
                case NET_ERROR_SEND_EXCEPTION:
                case NET_ERROR_PACKAGE_TIMEOUT:
                default:
                    this.lastSendTime = 0;
                    this.RaiseDisonnected(errCode); //触发断开连接事件
                    break;
            }

        }

        // 将网络协议消息添加到 发送队列中, ProcessSend中真正发送
        public void SendMessage(NetMessage message)
        {
            if (!running) //（突发网络波动）网络断开时，无法发送消息，直接返回
            {
                return;
            }
            if (!this.Connected)//若网络处于未连接状态，先进行连接后，再发送失败返回
            {
                this.receiveBuffer.Position = 0;
                this.sendBuffer.Position = sendOffset = 0;

                this.Connect(); //先进行连接
                Debug.Log("Connect Server before Send Message!");
                return; 
            }
            //网络连接正常
            sendQueue.Enqueue(message);//发送队列，将消息添加到 发送队列中

            if (this.lastSendTime == 0)
            {
                this.lastSendTime = Time.time;//更新上次发送时间
            }
        }

        // 分发 服务器返回的响应消息
        void ProceeMessage()
        {
            MessageDistributer.Instance.Distribute(); //主要分发 服务器返回的响应到 对应的模块（Service层）去处理
        }

        // 处理接收的协议消息，接收从服务器发送来的数据并进行解析
        bool ProcessRecv()
        {
            bool ret = false;
            try
            {   //1.阻塞模式检查
                if (this.clientSocket.Blocking)//阻塞模式下，当没有数据可读取时，程序会被阻塞，一直等待数据到达。
                {
                    Debug.Log("this.clientSocket.Blocking = true\n");
                }

                //2.错误状态检查：
                //Poll 轮询套接字的状态，参数是微秒级别的超时时间如 microseconds 和轮询模式SelectMode 。
                //microseconds是Poll程序中断运行时间。如microseconds=1000,Poll阻塞1000毫秒 ; microseconds<0 将无限等待响应
                //microseconds = 0 表示立即检查，SelectMode.SelectError 检查网络连接是否发生错误

                //Poll 方法返回 true 表示套接字处于指定的状态，返回 false 表示不在指定的状态。
                bool error = this.clientSocket.Poll(0, SelectMode.SelectError);
                if (error)//若出错
                {
                    Debug.Log("ProcessRecv Poll SelectError\n");
                    this.CloseConnection(NET_ERROR_SEND_EXCEPTION);//关闭网络连接，并传递错误代码
                    return false;
                }

                //3.可读性检查：
                //Poll轮询，SelectMode.SelectRead 检查是否有可读数据，若有，则将 ret 设置为 true。
                ret = this.clientSocket.Poll(0, SelectMode.SelectRead);
                if (ret) //ret == true,有可读数据
                {/*Receive 方法：  public int Receive(byte[] buffer, int offset, int size, SocketFlags flags);
                    buffer: 用于存储接收数据的缓冲区。 offset: 缓冲区中存储数据的起始偏移量。
                    size: 要接收的数据的最大字节数。 flags: 指定套接字接收操作的标志，如是否要阻止操作、是否处理带外数据等。
                    功能：该方法用于从连接的远程主机接收数据，数据将被存储在提供的缓冲区中，从指定的偏移位置开始。
                  */
                    //4.接收数据：
                    //Receive返回值：返回实际接收的字节数。如果连接已关闭，则返回 0。如果发生错误，则返回 -1。
                    int n = this.clientSocket.Receive(this.receiveBuffer.GetBuffer(), 0, this.receiveBuffer.Capacity, SocketFlags.None);
                    if (n <= 0)//n 为接收到的字节数，如果 n <= 0，表示 未从连接的远程主机 接收到有效数据
                    {
                        this.CloseConnection(NET_ERROR_ZERO_BYTE);//可能是连接关闭或发生错误，调用 CloseConnection关闭网络连接
                        return false;
                    }
                    //5.处理接收到的数据：
                    //接收有效数据，解析成网络协议消息类型   GetBuffer()返回存储数据的字节数组
                    this.packageHandler.ReceiveData(this.receiveBuffer.GetBuffer(), 0, n);
                }
            }
            catch (Exception e)//6.异常处理：
            {
                Debug.Log("ProcessReceive exception:" + e.ToString() + "\n");
                this.CloseConnection(NET_ERROR_ILLEGAL_PACKAGE);
                return false;
            }
            //最后，如果没有发生异常且成功接收数据，函数返回 true，表示数据接收和处理成功。
            return true;
        }

        // 处理待发送的网络协议消息，将其打包成字节数组 发送给服务器
        bool ProcessSend()
        {
            bool ret = false;
            try
            {   //1.阻塞模式检查
                if (this.clientSocket.Blocking)
                {
                    Debug.Log("this.clientSocket.Blocking = true\n");//如果是阻塞模式，打印一条日志
                }
                //2.错误状态检查：
                bool error = this.clientSocket.Poll(0, SelectMode.SelectError); //检查网络连接是否发生错误
                if (error)
                {
                    Debug.Log("ProcessSend Poll SelectError\n");
                    this.CloseConnection(NET_ERROR_SEND_EXCEPTION);//如果有错误，会调用 CloseConnection 关闭网络连接
                    return false;
                }
                //3.可写性检查：
                ret = this.clientSocket.Poll(0, SelectMode.SelectWrite);//SelectMode.SelectWrite 表示 检查是否有写入数据的权限
                if (ret)//如果有写入权限
                {
                    if (this.sendBuffer.Position > this.sendOffset)//如果 sendBuffer 中有未发送的留存数据
                    {
                        int bufsize = (int)(this.sendBuffer.Position - this.sendOffset);//计算待发送的数据大小 bufsize
                        //使用 Send 方法将sendBuffer.GetBuffer()留存数据发送出去。Send 方法返回实际发送的字节数。
                        int n = this.clientSocket.Send(this.sendBuffer.GetBuffer(), this.sendOffset, bufsize, SocketFlags.None);
                        if (n <= 0) //未能发送出留存数据
                        {
                            this.CloseConnection(NET_ERROR_ZERO_BYTE);
                            return false;
                        }
                        this.sendOffset += n;//更新 sendOffset 的位置，表示已发送的偏移
                        if (this.sendOffset >= this.sendBuffer.Position)//如果发送完 sendBuffer 中的留存数据
                        {
                            this.sendOffset = 0; // 重置为 0
                            this.sendBuffer.Position = 0;
                            this.sendQueue.Dequeue();//并从发送队列中移除已发送的消息。
                        }
                    }
                    else//如果 sendBuffer 中没有未发送的留存数据了：
                    {
                        //fetch package from sendQueue
                        if (this.sendQueue.Count > 0)//如果发送队列中有待发送的消息
                        {
                            NetMessage message = this.sendQueue.Peek();//从队列中取出消息
                            byte[] package = PackageHandler.PackMessage(message);//将网络协议消息 打包成字节数组
                            this.sendBuffer.Write(package, 0, package.Length);//写入到 sendBuffer 中，等待发送
                        }
                    }
                }
            }
            catch (Exception e)//异常处理：捕获异常并调用 CloseConnection 关闭网络连接
            {
                Debug.Log("ProcessSend exception:" + e.ToString() + "\n");
                this.CloseConnection(NET_ERROR_SEND_EXCEPTION);
                return false;
            }
            //最后，如果没有发生异常且成功发送数据，函数返回 true
            return true;
        }

        //每一帧中更新网络模块的状态，确保连接正常，接收和发送数据
        public void Update()
        {
            if (!running)//若网络断开，直接返回
            {
                return;
            }

            if (this.KeepConnect()) //保证网络连接
            {
                if (this.ProcessRecv())//接收从服务器发送到网卡的数据，提取保存到内存中
                {
                    if (this.Connected)//当前是连接状态
                    {
                        this.ProcessSend();// 处理待发送的网络协议消息，将其打包成字节数组 发送给服务器
                        this.ProceeMessage();// 分发 服务器返回的响应消息
                    }
                }
            }
        }

    }
}
