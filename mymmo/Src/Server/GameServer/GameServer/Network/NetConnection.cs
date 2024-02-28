using System;
using System.Net;
using System.Net.Sockets;

namespace Network
{
    //NetConnection<T> 类，表示与服务器的一个连接，泛型参数 T 必须实现 INetSession 接口，表示该连接NetConnection 关联的会话类型
    public class NetConnection<T> where T:INetSession
    {
        // DataReceivedCallback 委托：定义了一个回调，用于通知监听者连接已经接收到数据
        //sender 参数：表示调用回调的对象，即 NetConnection<T> 实例
        //e 参数：表示包含接收到的数据的 DataEventArgs 对象。 在异步 Socket 操作中，这里用它包装接收到的数据
        public delegate void DataReceivedCallback(NetConnection<T> sender, DataEventArgs e);

        //DisconnectedCallback 委托：定义了一个回调，用于通知监听者连接已经断开
        public delegate void DisconnectedCallback(NetConnection<T> sender, SocketAsyncEventArgs e);

        #region Internal Classes
        internal class State //一个内部类，包含了连接状态的信息，包括数据接收回调、断开连接回调和与该连接关联的 Socket
        {
            public DataReceivedCallback dataReceived;
            public DisconnectedCallback disconnectedCallback;
            public Socket socket;
        }
        #endregion

        #region public Property

        //用于获取或设置连接的认证状态。true 表示已认证，false 表示未认证。
        public bool Verified { get; set; }

        private T session;

        // 用于获取连接的会话对象。
        public T Session { get { return session; } }

        #endregion

        #region Fields
        private SocketAsyncEventArgs eventArgs;//用于处理异步 Socket 事件的 SocketAsyncEventArgs 对象

        public PackageHandler<NetConnection<T>> packageHandler;//用于处理接收到的数据包
        #endregion

        #region Events

        // 触发 DataReceivedCallback 事件。这个事件在数据接收时触发，用于通知监听者数据已经到达。
        // 通过callback回调函数将接收到的数据、远程地址等信息传递给监听者
        private void OnDataReceived(Byte[] data, IPEndPoint remoteEndPoint, DataReceivedCallback callback)
        {
            callback(this, new DataEventArgs() { RemoteEndPoint = remoteEndPoint, Data = data, Offset = 0, Length = data.Length });
        }

        //触发 DisconnectedCallback 事件。这个事件在连接断开时触发，通知监听者连接已断开。
        //通过回调函数将连接对象和相关参数传递给监听者。
        private void OnDisconnected(SocketAsyncEventArgs args, DisconnectedCallback callback)
        {
            callback(this, args);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// NetConnection构造函数：创建了一个到服务器的连接，并使用异步方式始终监听
        /// </summary>
        /// <param name="socket">与服务器的连接的 Socket 对象</param>
        /// <param name="args">异步 Socket 操作的状态和结果的容器</param>
        /// <param name="dataReceived">数据被接收时调用 此回调函数</param>
        /// <param name="disconnectedCallback">当连接断开时调用 此回调函数</param>
        /// <param name="session">一个泛型类型 T，代表网络会话的类型</param>
        public NetConnection(Socket socket, SocketAsyncEventArgs args, DataReceivedCallback dataReceived,
            DisconnectedCallback disconnectedCallback, T session)
        {
            lock (this)
            {   //创建了一个 PackageHandler<NetConnection<T>>，这是一个用于处理数据包的对象
                this.packageHandler = new PackageHandler<NetConnection<T>>(this);
                State state = new State()//创建了一个 State 对象，用于保存连接的状态信息
                {
                    socket = socket,
                    dataReceived = dataReceived,
                    disconnectedCallback = disconnectedCallback
                };
                eventArgs = new SocketAsyncEventArgs();//初始化了 eventArgs
                eventArgs.AcceptSocket = socket;
                eventArgs.Completed += ReceivedCompleted;//配置了它的回调函数 ReceivedCompleted
                eventArgs.UserToken = state;
                eventArgs.SetBuffer(new byte[64 * 1024],0, 64 * 1024);//指定了缓冲区的大小（64KB）

                BeginReceive(eventArgs);//调用 BeginReceive 方法开始接收数据
                this.session = session;
            }
        }
        #endregion


        #region Public Methods

        //断开客户端连接
        public void Disconnect()
        {
            lock (this)
            {
                CloseConnection(eventArgs);//通过 CloseConnection(eventArgs) 关闭连接
            }
        }

        public void SendResponse()
        {
            byte[] data = session.GetResponse();//打包响应消息
            this.SendData(data, 0, data.Length);//发送给客户端
        }

        /// <summary>
        /// 将数据发送给客户端
        /// </summary>
        /// <param name="data">要发送的数据</param>
        /// <param name="offset">数据的偏移量</param>
        /// <param name="count">发送的数据量</param>
        private void SendData(Byte[] data, Int32 offset, Int32 count)
        {
            lock (this)//通过lock (this) 语句块进行了同步，确保在多线程环境下的线程安全性
            {
                State state = eventArgs.UserToken as State;
                Socket socket = state.socket;
                if (socket.Connected)
                    //socket.Send(data, offset, count, SocketFlags.None);
                    //通过 BeginSend 异步开始发送数据，并指定了 SendCallback 作为回调函数
                    socket.BeginSend(data, 0, count, SocketFlags.None, new AsyncCallback(SendCallback), socket);
            }
        }


        //异步发送数据的回调函数
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;//从 IAsyncResult 对象中获取 Socket

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);//调用 EndSend 方法完成发送
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion


        #region Private Methods

        // 开始异步接收数据
        private void BeginReceive(SocketAsyncEventArgs args)
        {
            lock (this)
            {
                Socket socket = (args.UserToken as State).socket;
                if (socket.Connected)
                {
                    args.AcceptSocket.ReceiveAsync(args);//使用 ReceiveAsync 向连接的 Socket 发起异步接收操作
                    /*
                    socket.InvokeAsyncMethod(new SocketAsyncMethod(socket.ReceiveAsync),
                        ReceivedCompleted, args);*/
                }
            }
        }


        // 当异步接收完成时调用
        private void ReceivedCompleted(Object sender, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred == 0)//检查接收的字节数，如果为 0，表示连接已经关闭
            {
                CloseConnection(args); //调用 CloseConnection 进行断开
                return;
            }
            if (args.SocketError != SocketError.Success)//如果发生错误
            {
                CloseConnection(args); //也调用 CloseConnection 进行断开
                return;
            }
            //如果接收正常
            State state = args.UserToken as State;

            Byte[] data = new Byte[args.BytesTransferred];
            Array.Copy(args.Buffer, args.Offset, data, 0, data.Length);
            OnDataReceived(data, args.RemoteEndPoint as IPEndPoint, state.dataReceived);//调用 OnDataReceived 处理接收到的数据

            BeginReceive(args);//调用 BeginReceive 启动下一轮异步接收
        }


        // 用于关闭连接
        private void CloseConnection(SocketAsyncEventArgs args)
        {
            State state = args.UserToken as State;
            Socket socket = state.socket;
            try
            {
                socket.Shutdown(SocketShutdown.Both);//先尝试通过 socket.Shutdown(SocketShutdown.Both) 关闭连接
            }
            catch { } //如果连接的另一端已经关闭，则会引发异常，可以忽略
            socket.Close();//关闭 socket 并置为 null
            socket = null;

            args.Completed -= ReceivedCompleted; //一定要移除 args.Completed 事件处理器，避免内存泄漏
            OnDisconnected(args, state.disconnectedCallback);//调用 OnDisconnected 处理连接断开的回调
        }
        #endregion


    }
}
