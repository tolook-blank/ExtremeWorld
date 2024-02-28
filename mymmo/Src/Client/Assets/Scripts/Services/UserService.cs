using System;
using Common;
using Network;
using UnityEngine;

using SkillBridge.Message;
using Models;
using Managers;

namespace Services
{
    //客户端Client有的Service，Server服务器中也要添加，保持做相同的事
    //Services层的主要逻辑： 监听协议响应、发送消息请求
    //UserService服务端负责业务逻辑，***通过添加事件UnityAction，将收到的消息发送给UI层***

    class UserService : Singleton<UserService>, IDisposable //逻辑层的类都设为单例类，不宜 使用UI层的引用 来发送消息
    {
        //发布器是一个包含事件和委托的对象，发布器类的对象可以触发事件，并使用委托通知其他的对象

        //UnityEngine.Events.UnityAction 是 Unity 中的一种委托（delegate）类型，通常用于处理事件或回调函数。
        //这些UnityAction 用于存储事件处理函数，这些函数带有两个签名： Result 枚举类型，string 类型。
        public UnityEngine.Events.UnityAction<Result, string> OnLogin;//登录事件发布器，触发登录事件
        public UnityEngine.Events.UnityAction<Result, string> OnRegister;
        public UnityEngine.Events.UnityAction<Result, string> OnCharacterCreate; 

        NetMessage pendingMessage = null;//当断线时，保存待发消息，等到连接上服务器，一起发送。

        bool connected = false;//是否处于连接状态

        bool IsQuitGame = false;//UserService是单例，它的实例唯一，IsQuitGame代表当前游戏是否退出

        public UserService()//构造函数，在对象创建时执行。执行一些初始化工作，包括订阅了一些事件处理函数
        {
            NetClient.Instance.OnConnect += OnGameServerConnect;//服务器连接，订阅网络连接事件，会在 OnConnect 事件触发时被调用
            NetClient.Instance.OnDisconnect += OnGameServerDisconnect;//服务器断开，订阅网络断开事件

            //当写完函数方法【如：this.OnUserLogin】时，需要在此处注册【MessageDistributer.Instance.Subscribe<协议类型>(this.OnUserLogin)】，使方法生效
            //客户端的服务 接收服务器返回的响应 ,所以应注意 Subscribe<协议类型T是XXXResponse>，协议类型T 可通过SkillBridge.Message.，在VS的提示中查找到。
            MessageDistributer.Instance.Subscribe<UserLoginResponse>(this.OnUserLogin);//客户端接收服务器返回的 <UserLoginResponse>（用户登录响应）
            MessageDistributer.Instance.Subscribe<UserRegisterResponse>(this.OnUserRegister);//接收 UserRegisterResponse（用户注册响应)，回馈UI层
            MessageDistributer.Instance.Subscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);

            MessageDistributer.Instance.Subscribe<UserGameEnterResponse>(this.OnGameEnter); //进入游戏， 与角色进入地图不同 
            MessageDistributer.Instance.Subscribe<UserGameLeaveResponse>(this.OnGameLeave);
            //目前 MapCharacterEnter 在 MapService中订阅、处理
            //MessageDistributer.Instance.Subscribe<MapCharacterEnterResponse>(this.OnCharacterEnter);
        }


        public void Dispose()//实现了 IDisposable 接口，通常用于释放非托管资源。在这里，它取消了之前订阅的事件处理函数，以确保在对象不再需要时释放资源。
        {
            NetClient.Instance.OnConnect -= OnGameServerConnect;
            NetClient.Instance.OnDisconnect -= OnGameServerDisconnect;

            MessageDistributer.Instance.Unsubscribe<UserLoginResponse>(this.OnUserLogin);
            MessageDistributer.Instance.Unsubscribe<UserRegisterResponse>(this.OnUserRegister);
            MessageDistributer.Instance.Unsubscribe<UserCreateCharacterResponse>(this.OnUserCreateCharacter);

            MessageDistributer.Instance.Unsubscribe<UserGameEnterResponse>(this.OnGameEnter);
            MessageDistributer.Instance.Unsubscribe<UserGameLeaveResponse>(this.OnGameLeave);
            //MessageDistributer.Instance.Unsubscribe<MapCharacterEnterResponse>(this.OnCharacterEnter);
        }

        public void Init()
        {

        }

        public void ConnectToServer()//连接到游戏服务器
        {
            Debug.Log("UserService ConnectToServer() Start ");
            //NetClient.Instance.CryptKey = this.SessionId;
            NetClient.Instance.Init("127.0.0.1", 8000);
            NetClient.Instance.Connect();
        }


        void OnGameServerConnect(int result, string reason)//事件处理函数，用于处理与游戏服务器的连接事件。
        {
            Log.InfoFormat("LoadingMesager::OnGameServerConnect :{0} reason:{1}", result, reason);
            if (NetClient.Instance.Connected)//当前处于连接状态
            {
                this.connected = true;
                if(this.pendingMessage!=null)
                {
                    NetClient.Instance.SendMessage(this.pendingMessage);
                    this.pendingMessage = null;
                }
            }
            else
            {
                if (!this.DisconnectNotify(result, reason))
                {
                    MessageBox.Show(string.Format("网络错误，无法连接到服务器！\n RESULT:{0} ERROR:{1}", result, reason), "错误", MessageBoxType.Error);
                }
            }
        }

        public void OnGameServerDisconnect(int result, string reason)//事件处理函数，用于处理与游戏服务器的断开连接事件
        {
            this.DisconnectNotify(result, reason);
            return;
        }

        bool DisconnectNotify(int result,string reason)//通知连接断开事件
        {
            if (this.pendingMessage != null)
            {
                if (this.pendingMessage.Request.userLogin!=null)
                {
                    if (this.OnLogin != null)
                    {
                        //触发登录事件时，使用其委托调用语法，调用此类事件处理程序
                        this.OnLogin(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                else if(this.pendingMessage.Request.userRegister!=null)
                {
                    if (this.OnRegister != null)
                    {
                        this.OnRegister(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                else
                {
                    if (this.OnCharacterCreate != null)
                    {
                        this.OnCharacterCreate(Result.Failed, string.Format("服务器断开！\n RESULT:{0} ERROR:{1}", result, reason));
                    }
                }
                return true;
            }
            return false;
        }

        public void SendLogin(string user, string psw)//发送用户登录请求。函数名为SendXXX，表示发送XXX请求
        {
            Debug.LogFormat("UserLoginRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();//创建一个包含用户登录信息的NetMessage
            message.Request = new NetMessageRequest();//发送请求
            message.Request.userLogin = new UserLoginRequest();//请求登录用户
            message.Request.userLogin.User = user;
            message.Request.userLogin.Passward = psw;

            if (this.connected && NetClient.Instance.Connected)//根据连接状态决定是直接发送还是等待连接成功后再发送
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            }
        }

        void OnUserLogin(object sender, UserLoginResponse response)//事件处理函数，处理用户登录响应消息
        {
            Debug.LogFormat("OnLogin:{0} [{1}]", response.Result, response.Errormsg);

            if (response.Result == Result.Success)
            {//登陆成功逻辑
                Models.User.Instance.SetupUserInfo(response.Userinfo);//首先把服务器返回的info信息记录到本地
            };
            if (this.OnLogin != null)
            {
                this.OnLogin(response.Result, response.Errormsg);//然后把登陆成功的消息 通知订阅者去处理 （转到UILogin中的OnLogin函数）
            }
        }


        public void SendRegister(string user, string psw)//发送用户注册请求，与SendLogin函数几乎一样
        {
            Debug.LogFormat("UserRegisterRequest::user :{0} psw:{1}", user, psw);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();//发送请求
            message.Request.userRegister = new UserRegisterRequest();//请求注册用户
            message.Request.userRegister.User = user;
            message.Request.userRegister.Passward = psw;

            if (this.connected && NetClient.Instance.Connected)
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);
            }
            else
            {
                this.pendingMessage = message;
                this.ConnectToServer();
            } 
        }

        void OnUserRegister(object sender, UserRegisterResponse response) //事件处理函数，用于处理用户注册响应消息
        {
            Debug.LogFormat("OnUserRegister:{0} [{1}]", response.Result, response.Errormsg);

            if (this.OnRegister != null)
            {
                this.OnRegister(response.Result, response.Errormsg);//通知订阅者对象，处理用户注册事件

            }
        }

        public void SendCharacterCreate(string name, CharacterClass cls) //发送创建角色请求消息，参数：角色名称，职业类型
        {
            Debug.LogFormat("UserCreateCharacterRequest::name :{0} class:{1}", name, cls);
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();//发送请求
            message.Request.createChar = new UserCreateCharacterRequest();//请求创建角色
            message.Request.createChar.Name = name;
            message.Request.createChar.Class = cls;//职业

            if (this.connected && NetClient.Instance.Connected)//已连接上服务器
            {
                this.pendingMessage = null;
                NetClient.Instance.SendMessage(message);//消息直接发送给服务器
            }
            else
            {
                this.pendingMessage = message;//当断线时，保存待发消息，等到连接上服务器，一起发送
                this.ConnectToServer();
            }
        }

        void OnUserCreateCharacter(object sender, UserCreateCharacterResponse response) //事件处理函数，用于处理创建角色响应消息
        {
            Debug.LogFormat("OnUserCreateCharacter:{0} [{1}]", response.Result, response.Errormsg);

            if(response.Result == Result.Success)
            {//（客户端本地数据 Models.User）
                Models.User.Instance.Info.Player.Characters.Clear();//先清空
                Models.User.Instance.Info.Player.Characters.AddRange(response.Characters);//再导入角色列表
            }

            if (this.OnCharacterCreate != null)//通知订阅者，角色创建
            {
                this.OnCharacterCreate(response.Result, response.Errormsg);

            }
        }

        //角色选择场景中，选定使用的角色后，可点击进入游戏按钮，发送进入游戏请求
        public void SendGameEnter(int characterIdx)//客户端和服务端的角色列表顺序一致，只要 角色的索引，就能知道具体的角色
        {
            Debug.LogFormat("UserGameEnterRequest::characterId :{0}", characterIdx);

            ChatManager.Instance.Init(); //在进入游戏前，先初始化聊天（玩家可能返回角色选择界面，更换角色再次进入游戏）
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.gameEnter = new UserGameEnterRequest();
            message.Request.gameEnter.characterIdx = characterIdx;
            NetClient.Instance.SendMessage(message);
        }

        //无论是 初次登录，还是返回角色选择后 再次进入游戏，都要点击CharacterSelect的 进入游戏按钮，来SendGameEnter，所以都会有OnGameEnter处理
        void OnGameEnter(object sender, UserGameEnterResponse response)//进入游戏，同时拉取角色数据
        {
            Debug.LogFormat("OnGameEnter:{0} [{1}]", response.Result, response.Errormsg);

            if (response.Result == Result.Success) //游戏成功进入，初始化角色的信息 NCharacterInfo
            {
                if (response.Character != null) //如果角色不为空
                {
                    //response.Character 是 服务端返回的Character.Info（ NCharacterInfo网络协议类型），用来与客户端之间通讯，传递角色身上的数据
                    User.Instance.CurrentCharacter = response.Character; //初始化当前控制的角色（从UserGameEnterResponse.Character中获得了Entity_ID）
                    ItemManager.Instance.Init(response.Character.Items);//初始化 角色的道具管理器
                    BagManager.Instance.Init(response.Character.Bag); //初始化 角色的背包管理器
                    EquipManager.Instance.Init(response.Character.Equips); //初始化 装备管理器
                    QuestManager.Instance.Init(response.Character.Quests); //任务管理器
                    FriendManager.Instance.Init(response.Character.Friends);//好友管理器
                    GuildManager.Instance.Init(response.Character.Guild);//公会管理器
                }
            }
        }


        public void SendGameLeave(bool isQuitGame = false)//点击退出游戏时，isQuitGame = true； 默认为返回角色选择，isQuitGame = false
        {
            this.IsQuitGame = isQuitGame;
            Debug.Log("UserGameLeaveRequest");
            NetMessage message = new NetMessage();
            message.Request = new NetMessageRequest();
            message.Request.gameLeave = new UserGameLeaveRequest();
            NetClient.Instance.SendMessage(message);
        }

        void OnGameLeave(object sender, UserGameLeaveResponse response)
        {//保证，服务端先收到了退出游戏请求，才把该客户端退出去
            MapService.Instance.CurrentMapId = 0;//退出游戏时，重置 CurrentMapId的值
            User.Instance.CurrentCharacter = null; //退出游戏时，清空当前角色
            User.Instance.CurrentCharacterObject = null; //退出游戏时，清空当前角色的游戏对象
            Debug.LogFormat("OnGameLeave:{0} [{1}]", response.Result, response.Errormsg);
            if (this.IsQuitGame)//isQuitGame == true，才执行退出游戏
            {//无论在编辑器下，或是打包发布后，都可以正常停止游戏
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;//UNITY_EDITOR编辑器中，游戏状态从 运行状态 变为 停止状态
#else
                Application.Quit(); //打包发布成.exe后
#endif
            }
        }

    }
}
