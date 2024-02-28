// RayMix Libs - RayMix's .Net Libs
// Copyright 2018 Ray@raymix.net.  All rights reserved.
// https://www.raymix.net
//
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
//
//     * Redistributions of source code must retain the above copyright
// notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above
// copyright notice, this list of conditions and the following disclaimer
// in the documentation and/or other materials provided with the
// distribution.
//     * Neither the name of RayMix.net. nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
using System;
using System.Collections.Generic;
using System.Threading;
using Common;

namespace Network
{
    /// <summary>
    /// MessageDistributer 消息分发器, 负责处理消息的分发、订阅和取消订阅等功能。
    /// 这个消息分发器的设计 是为了在多线程环境下处理消息的分发，确保线程安全。在启动时，可以通过设置 ThreadCount 来指定工作线程数。
    /// </summary>
    public class MessageDistributer : MessageDistributer<object>
    {
        //如果子类没有提供自己的构造函数，将会自动调用基类（父类）的默认构造函数。
        //MessageDistributer 没有定义自己的构造函数，因此它将继承 MessageDistributer<object> 的构造函数
    }

    public class MessageDistributer<T> : Singleton<MessageDistributer<T>>
    {
        class MessageArgs//嵌套类 MessageArgs：用于封装消息的发送者和消息内容。
        {
            public T sender;
            public SkillBridge.Message.NetMessage message;
        }
        private Queue<MessageArgs> messageQueue = new Queue<MessageArgs>();//消息接收队列,用于存储待分发的消息

        public delegate void MessageHandler<Tm>(T sender, Tm message);//消息处理委托，用于处理消息
        //消息处理器字典 messageHandlers,用于管理 消息类型 和 对应的消息处理器。编译器在你使用 delegate 关键字时会生成一个类， 派生自 签名匹配的 System.Delegate
        private Dictionary<string, System.Delegate> messageHandlers = new Dictionary<string, System.Delegate>();

        private bool Running = false;//表示消息分发器是否在运行的标志
        //线程同步：在多线程环境下，使用 AutoResetEvent 来实现线程同步，确保在有消息时唤醒工作线程
        private AutoResetEvent threadEvent = new AutoResetEvent(true);//true：非阻塞状态，设置事件是 "有信号的"，如果有线程调用 `WaitOne` 方法，则当信号处于发送状态时, 该线程会得到信号, 继续向下执行

        //线程计数 
        public int ThreadCount = 0;//记录总线程数
        public int ActiveThreadCount = 0;//当前活跃线程数

        public bool ThrowException = false;//根据 ThrowException 的值决定是否在异常时抛出异常

        public MessageDistributer()
        {
        }

        //Subscribe 方法 将消息处理器与消息类型关联起来，实现了订阅功能。
        public void Subscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;//获取网络协议消息的名称，消息名称即 消息类型
            if (!messageHandlers.ContainsKey(type))//若消息处理器字典中 还不包含 此消息类型
            {
                messageHandlers[type] = null;//添加到消息处理器字典中
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] + messageHandler;//"+" 订阅，将消息处理器 添加到字典的对应消息类型上
            //由于 messageHandlers[type] 的类型是 System.Delegate，我们需要强制转换为实际的消息处理器类型 (MessageHandler<Tm>)，然后使用 + 运算符来订阅消息。
            //这里的 + 运算符实际上是委托的组合操作，将多个处理器组合成一个。这里无法直接使用 += ，因为类型不匹配 需要先转换
        }
        public void Unsubscribe<Tm>(MessageHandler<Tm> messageHandler)
        {
            string type = typeof(Tm).Name;
            if (!messageHandlers.ContainsKey(type))
            {
                messageHandlers[type] = null;
            }
            messageHandlers[type] = (MessageHandler<Tm>)messageHandlers[type] - messageHandler;//"-" 注销订阅
        }

        //消息处理方法: RaiseEvent 方法用于触发事件，实现了消息的分发和异常处理的逻辑，确保消息能够被正确地传递给订阅者
        public void RaiseEvent<Tm>(T sender, Tm msg)
        {
            string key = msg.GetType().Name;// 获取消息的类型名称，即消息的名字
            if (messageHandlers.ContainsKey(key)) // 如果消息处理器字典中 包含该消息类型
            {
                MessageHandler<Tm> handler = (MessageHandler<Tm>)messageHandlers[key];// 获取该消息类型对应的 消息处理器
                if (handler != null)// 如果消息处理器不为空，表示有订阅者订阅了这个消息类型
                {
                    try
                    {
                        handler(sender, msg);// 调用消息处理器，将消息传递给订阅者（Service层）
                    }
                    catch (System.Exception ex)
                    {
                        Log.ErrorFormat("Message handler exception:{0}, {1}, {2}, {3}", ex.InnerException, ex.Message, ex.Source, ex.StackTrace);
                        if (ThrowException) // 如果设置了 ThrowException 为 true，将异常抛出
                            throw ex;
                    }
                }
                else// 如果消息处理器为空
                {
                    Log.Warning("No handler subscribed for {0}" + msg.ToString());// 记录警告，表示没有订阅该类型消息的处理器
                }
            }
        }

        //接收消息：ReceiveMessage 方法用于接收消息，将消息加入消息队列，并通知工作线程有消息需要处理。
        public void ReceiveMessage(T sender, SkillBridge.Message.NetMessage message)
        {
            this.messageQueue.Enqueue(new MessageArgs() { sender = sender, message = message });//消息入队
            threadEvent.Set();//发信号
        }

        public void Clear()
        {
            this.messageQueue.Clear();
        }

        //消息分发：Distribute 方法一次性将消息队列中的所有消息都分发出去，确保消息能够被处理。
        public void Distribute()
        {
            if (this.messageQueue.Count == 0)// 如果消息队列为空，直接返回
            {
                return;
            }

            while (this.messageQueue.Count > 0)//只要还有消息
            {
                MessageArgs package = this.messageQueue.Dequeue();// 从消息队列中取出一个消息包
                if (package.message.Request != null) // 如果消息包中有请求消息（客户端发送给服务器的消息）
                    MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Request);// 调用消息分发器，将请求消息分发给相应的模块
                if (package.message.Response != null) // 如果消息包中有响应消息（服务器发送给客户端的消息）
                    MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Response);// 调用消息分发器，将响应消息分发给相应的模块
            }
        }


        //Start 方法用于启动消息处理器，创建工作线程。 [多线程模式]
        public void Start(int ThreadNum)
        {
            this.ThreadCount = ThreadNum;//设置工作线程数
            if (this.ThreadCount < 1) this.ThreadCount = 1;//如果工作线程数小于 1，将其设置为 1
            if (this.ThreadCount > 1000) this.ThreadCount = 1000;//如果工作线程数大于 1000，将其设置为 1000
            Running = true;//设置消息处理器为运行状态
            for (int i = 0; i < this.ThreadCount; i++) //循环创建工作线程，ThreadPool 通过使用工作队列来管理等待执行的任务。任务被添加到队列，一旦有可用的线程，它们就会被调度执行
            {//使用线程池创建工作线程，并将 MessageDistribute 方法排入线程池工作队列。而且线程池会自动管理线程的生命周期。线程一旦完成任务，就会返回线程池供其他任务使用。
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageDistribute));
            }
            while (ActiveThreadCount < this.ThreadCount)//等待所有工作线程启动
            {
                Thread.Sleep(100);//休眠一段时间，观察异步行为，确保 MessageDistribute 有足够的时间执行 ；然后再次检查
            }
        }

        //停止消息处理器 [多线程模式]
        public void Stop()
        {
            Running = false;// 设置消息处理器为停止状态
            this.messageQueue.Clear();// 清空消息队列
            while (ActiveThreadCount > 0) //只要当前还有活跃的线程
            {
                threadEvent.Set();//发信号，唤醒工作线程，有消息需要处理。
            }
            Thread.Sleep(100);// 等待一段时间确保工作线程停止
        }


        // 消息处理线程：MessageDistribute 方法是一个使用线程池的工作线程函数，从消息队列中取出消息并进行处理分发。[多线程模式]
        /*
         这个方法的主要目的是在有新消息时从队列中取出并处理，如果没有消息则等待。
        同时，通过 Interlocked 保证了对活跃线程计数的安全操作，而 try-catch 块则是为了捕获异常并记录日志，确保线程不会因为异常而崩溃。
         */
        private void MessageDistribute(Object stateInfo)
        {
            Log.Warning("MessageDistribute thread start");//记录日志，服务器启动时，工作线程开始执行。 启动 8 个线程打印 8 条日志
            try
            {
                //通过 Interlocked.Increment 方法增加活跃线程的计数。Interlocked 类提供原子操作，确保线程安全地增加计数。
                ActiveThreadCount = Interlocked.Increment(ref ActiveThreadCount);
                while (Running) //只要 Running 标志为 true，消息处理器为运行状态，线程就会一直运行
                {
                    if (this.messageQueue.Count == 0)//检查消息队列是否为空
                    {
                        threadEvent.WaitOne();//WaitOne阻止当前线程，直到收到信号，此处直到有新的消息到达时才被唤醒（因为信息到达的ReceiveMessage方法中调用了 threadEvent.Set()发信号）
                        //Log.WarningFormat("[{0}]MessageDistribute Thread[{1}] Continue:", DateTime.Now, Thread.CurrentThread.ManagedThreadId);
                        continue;
                    }
                    MessageArgs package = this.messageQueue.Dequeue();//从消息队列中取出一个消息
                    //检查消息是否包含请求或响应，后通过 MessageDispatch<T>.Instance.Dispatch 方法将消息分发给相应的处理程序。
                    if (package.message.Request != null) // 如果消息包中有请求消息（客户端发送给服务器的消息）
                        MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Request);// 调用消息分发器，将请求消息分发给相应的模块（Service层）
                    if (package.message.Response != null)
                        MessageDispatch<T>.Instance.Dispatch(package.sender, package.message.Response);
                }
            }
            catch
            {
            }
            finally
            {   //线程完成工作后，通过 Interlocked.Decrement 减少活跃线程的计数
                ActiveThreadCount = Interlocked.Decrement(ref ActiveThreadCount);
                Log.Warning("MessageDistribute thread end");//记录日志，表示工作线程结束执行
            }
        }

    }
}