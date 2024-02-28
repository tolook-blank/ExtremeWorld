using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    /// <summary>
    /// 使用给定的服务端监听的地址和端口，创建TCP服务端的监听器，主要用于监听和处理客户端的连接请求。
    /// </summary>
    public class TcpSocketListener : IDisposable
    {
        #region Fields
        private Int32 connectionBacklog; //表示 可处理客户端连接请求 的上限（即为服务端可监听的最大用户数）
        private IPEndPoint endPoint; //服务端监听的地址和端口

        private Socket listenerSocket; //服务端的网络连接（套接字)
        private SocketAsyncEventArgs args;//异步socket连接请求的参数，args.AcceptSocket 是服务端接受的客户端连接
        #endregion

        #region Properties
        /// <summary>
        /// 获取或设置连接请求队列的最大长度
        /// </summary>
        public Int32 ConnectionBacklog
        {
            get { return connectionBacklog; }
            set
            {
                //lock 语句是一种用于确保在多线程环境中对共享资源进行互斥访问的机制
                /*
                 * 当一个线程进入这个 lock 语句块时，它就获得了 this 对象的锁。
                 * 当代码块执行完毕时（if 和 else 中的逻辑只有一个被执行），锁将被释放，允许其他等待的线程进入。
                 * 这确保了在一个时刻只有一个线程可以修改 connectionBacklog 的值。
                 */
                lock (this)
                {
                    if (IsRunning) //在设置时，如果服务器正在运行，则抛出异常。
                        throw new InvalidOperationException("Property cannot be changed while server running.");
                    else
                        connectionBacklog = value; //设置服务端可监听的最大用户数
                }
            }
        }
        /// <summary>
        /// 获取或设置服务端监听的地址和端口
        /// </summary>
        public IPEndPoint EndPoint
        {
            get { return endPoint; }
            set
            {
                lock (this)
                {
                    if (IsRunning)
                        throw new InvalidOperationException("Property cannot be changed while server running.");
                    else
                        endPoint = value;
                }
            }
        }
        /// <summary>
        /// 表示当前监听器是否正在运行，通过检查 listenerSocket 是否为 null 来判断。
        /// </summary>
        public Boolean IsRunning
        {
            get { return listenerSocket != null; }
        }
        #endregion

        #region Constructors

        //有三个重载的构造函数，它们允许你以不同的方式创建 TcpSocketListener 实例
        public TcpSocketListener(String address, Int32 port, Int32 connectionBacklog)
            : this(IPAddress.Parse(address), port, connectionBacklog)
        { }

        public TcpSocketListener(IPAddress address, Int32 port, Int32 connectionBacklog)
            : this(new IPEndPoint(address, port), connectionBacklog)
        { }

        public TcpSocketListener(IPEndPoint endPoint, Int32 connectionBacklog)
        {
            this.endPoint = endPoint;

            args = new SocketAsyncEventArgs();
            args.Completed += OnSocketAccepted;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 开始监听连接
        /// (1.创建用于通信的套接字 new Socket()、
        /// 2.套接字绑定端口号和IP Bind(endPoint()、
        /// 3.监听连接 Listen()、
        /// 4.接受客户端的连接Accept)
        /// </summary>
        public void Start()
        {
            lock (this)//使用 lock 语句确保线程安全性
            {
                if (!IsRunning)
                {
                    //创建一个新的 Socket 实例，
                    listenerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                    listenerSocket.Bind(endPoint); //给创建的Socket 绑定指定的 endPoint，即服务端的IP+端口
                    listenerSocket.Listen(connectionBacklog);//设定服务端可监听的 最大用户数
                    BeginAccept(args); //BeginAccept 执行异步接受客户端的socket连接
                }
                else
                    throw new InvalidOperationException("The Server is already running.");
            }

        }

        /// <summary>
        /// 停止监听连接
        /// </summary>
        public void Stop()
        {
            lock (this)
            {
                if (listenerSocket == null)//检查 listenerSocket 是否为 null
                    return;
                listenerSocket.Close();//如果listenerSocket不为null，则关闭 listenerSocket
                listenerSocket = null;//置为空
            }
        }
        #endregion


        #region Private Methods
        /// <summary>
        /// 异步监听新的连接
        /// </summary>
        /// <param name="args"></param>
        private void BeginAccept(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            listenerSocket.AcceptAsync(args);//开始异步接受连接
            /*listenerSocket.InvokeAsyncMethod(new SocketAsyncMethod(listenerSocket.AcceptAsync)
                , OnSocketAccepted, args);*/ //在接受连接完成后，会触发 OnSocketAccepted 方法。
        }
        /// <summary>
        /// 异步接受客户端连接完成后的回调
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The SocketAsyncEventArgs for the operation.</param>
        private void OnSocketAccepted(object sender, SocketAsyncEventArgs e)
        {
            SocketError error = e.SocketError;
            if (e.SocketError == SocketError.OperationAborted) //首先检查了连接的状态
                return; //Server was stopped

            if (e.SocketError == SocketError.Success)//如果连接建立成功，服务器和客户端可以互相通讯。e.AcceptSocket 是服务端接受的客户端连接
            {
                Socket handler = e.AcceptSocket;//获取建立连接的客户端，服务器通过 e.AcceptSocket 来发送数据给客户端
                OnSocketConnected(handler);
            }

            lock (this)//然后，在 lock (this) 语句块内，再次调用 BeginAccept(e)，以便继续监听下一个连接
            {
                BeginAccept(e);//这种递归调用方式确保始终保持在监听状态
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// 当新连接被接受时触发 SocketConnected事件
        /// </summary>
        public event EventHandler<Socket> SocketConnected;
        /// <summary>
        /// SocketConnected事件 的处理程序在 OnSocketConnected 方法中被调用。
        /// </summary>
        /// <param name="client">The new client socket.</param>
        private void OnSocketConnected(Socket client)
        {
            if (SocketConnected != null)
                SocketConnected(this, client);
        }
        #endregion

        #region IDisposable Members
        private Boolean disposed = false;

        ~TcpSocketListener() //确保在垃圾回收时同样会释放资源
        {
            Dispose(false);
        }

        public void Dispose()//这是 IDisposable 接口的实现。它用于释放资源，确保在销毁对象时关闭监听器。
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)//Dispose 方法会调用 Stop 方法停止监听，释放 args 对象，并将 disposed 标志设置为 true
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Stop();
                    if (args != null)
                        args.Dispose();
                }

                disposed = true;
            }
        }
        #endregion
    }
}
