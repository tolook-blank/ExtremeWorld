using System;
using System.Threading.Tasks;
using System.Net.Sockets;

namespace GameServer.Network
{
    /// <summary>
    /// SocketAsyncMethod 委托，表示一个在 SocketAsyncEventArgs 上执行的异步操作，并通过返回值指示操作是否完成。
    /// </summary>
    /// <returns>如果操作在异步模式下完成，则返回 true，否则返回 false。</returns>
    public delegate Boolean SocketAsyncMethod(SocketAsyncEventArgs args);

 
    public static class ExtensionMethods
    {
        /// <summary>
        /// 包含一个公共静态方法 InvokeAsyncMethod，作为 Socket 类的扩展方法。在 C# 中，通过扩展方法，你可以向现有类型添加新的方法而无需修改该类型的源代码。
        /// 简化 Socket 类的异步操作模式的使用。在异步模型中，通常你需要调用异步方法，并在操作完成时等待回调。这个扩展方法把这一模式包装起来，使其更易用和清晰。
        /// See http://www.flawlesscode.com/post/2007/12/Extension-Methods-and-SocketAsyncEventArgs.aspx
        /// </summary>
        /// <param name="socket">The socket this method acts on.</param>
        /// <param name="method">要调用的 xxxAsync 方法（通过 SocketAsyncMethod 委托表示）</param>
        /// <param name="callback">如果异步操作已经完成，就会调用回调事件处理程序</param>
        /// <param name="args">The SocketAsyncEventArgs to be used with this call.</param>
        public static void InvokeAsyncMethod(this Socket socket, SocketAsyncMethod method, EventHandler<SocketAsyncEventArgs> callback, SocketAsyncEventArgs args)
        {
            if (!method(args))//检查 method 委托执行的异步操作是否已经完成，method 是一个代表异步操作的委托，它会在给定的 SocketAsyncEventArgs 上执行。
            {
                callback(socket, args);//如果异步操作未完成，就调用传入的回调方法 callback
            }
        }
    }
}