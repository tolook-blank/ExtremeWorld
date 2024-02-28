using System.Linq;
using Common;
using Network;
using SkillBridge.Message;
using GameServer.Entities;
using GameServer.Managers;

namespace GameServer.Services
{
    class UserService : Singleton<UserService>
    {

        public UserService()
        {
            //服务器的服务 接收客户端发来的请求 Request,所以应注意Subscribe<类型后缀是 Request >
            //网络协议消息订阅，只要服务器上有消息（UserLoginRequest）来，就会调用对应的消息处理方法（this.OnLogin）
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserRegisterRequest>(this.OnRegister);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserLoginRequest>(this.OnLogin);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserCreateCharacterRequest>(this.OnCreateCharacter);
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameEnterRequest>(this.OnGameEnter);//游戏进入，进入主城
            MessageDistributer<NetConnection<NetSession>>.Instance.Subscribe<UserGameLeaveRequest>(this.OnGameLeave);
        }


        public void Init()
        {
            Log.Info("UserService Started");
        }

        //服务器收到角色登录请求，并回发处理登录的响应给客户端
        void OnLogin(NetConnection<NetSession> sender, UserLoginRequest request)//sender代表发送消息的那个客户端，服务器的响应也将返回给这个客户端
        {
            Log.InfoFormat("UserLoginRequest: User:{0}  Pass:{1}", request.User, request.Passward);
            //NetMessage message = new NetMessage();
            //message.Response = new NetMessageResponse();
            sender.Session.Response.userLogin = new UserLoginResponse();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user == null)
            {
                sender.Session.Response.userLogin.Result = Result.Failed;//服务器填充Result字段，返回给客户端，告知结果
                sender.Session.Response.userLogin.Errormsg = "用户不存在";
            }
            else if (user.Password != request.Passward)
            {
                sender.Session.Response.userLogin.Result = Result.Failed;
                sender.Session.Response.userLogin.Errormsg = "密码错误";
            }
            else
            {
                sender.Session.User = user;//登陆成功时，给Session.User当前用户赋值，否则运行时 User会报 空引用异常 

                sender.Session.Response.userLogin.Result = Result.Success;
                sender.Session.Response.userLogin.Errormsg = "None";
                sender.Session.Response.userLogin.Userinfo = new NUserInfo();
                sender.Session.Response.userLogin.Userinfo.Id = (int)user.ID;
                sender.Session.Response.userLogin.Userinfo.Player = new NPlayerInfo();
                sender.Session.Response.userLogin.Userinfo.Player.Id = user.Player.ID;
                foreach (var c in user.Player.Characters)//登录时，把当前数据库中已有的角色信息打包，发送给客户端
                {
                    NCharacterInfo info = new NCharacterInfo();
                    info.Id = c.ID; //c.ID 是 角色表中的主键DB_id 
                    info.Name = c.Name;
                    info.Type = CharacterType.Player;
                    info.Class = (CharacterClass)c.Class;
                    info.ConfigId = c.Class;//CharacterDefine中的TID（对应职业类型Class的枚举值1~3), 
                    //info.EntityId没有赋值 默认为0，因为当前处于登录阶段，还未进入游戏，即还未创建游戏角色Entity
                    sender.Session.Response.userLogin.Userinfo.Player.Characters.Add(info);
                }
            }
            //byte[] data = PackageHandler.PackMessage(message);
            //sender.SendData(data, 0, data.Length);
            sender.SendResponse();
        }


        void OnRegister(NetConnection<NetSession> sender, UserRegisterRequest request)//sender代表发送消息的那个客户端，服务器的响应也将返回给这个客户端
        {
            Log.InfoFormat("UserRegisterRequest: User:{0}  Pass:{1}", request.User, request.Passward);
            //要返回给客户端的消息
            sender.Session.Response.userRegister = new UserRegisterResponse();

            TUser user = DBService.Instance.Entities.Users.Where(u => u.Username == request.User).FirstOrDefault();
            if (user != null)//账户已存在，不可再注册
            {
                sender.Session.Response.userRegister.Result = Result.Failed;
                sender.Session.Response.userRegister.Errormsg = "用户已存在.";
            }
            else//账户不存在，新增此账号
            {
                TPlayer player = DBService.Instance.Entities.Players.Add(new TPlayer());
                //由于一个User 包含一个Player，所以要先创建Player，再创建User
                DBService.Instance.Entities.Users.Add(new TUser() { Username = request.User, Password = request.Passward, Player = player });
                DBService.Instance.Entities.SaveChanges();//必须保存数据库的修改

                sender.Session.Response.userRegister.Result = Result.Success;
                sender.Session.Response.userRegister.Errormsg = "None";
            }
            sender.SendResponse();
        }

        //收到sender的创建角色请求，处理后 返回响应给sender
        private void OnCreateCharacter(NetConnection<NetSession> sender, UserCreateCharacterRequest request)
        {
            Log.InfoFormat("UserCreateCharacterRequest: Name:{0}  Class:{1}", request.Name, request.Class);
            //创建一条 DB角色表的数据
            TCharacter character = new TCharacter() //创建角色表TCharacter的对象。设定T开头（Table）的类，代表着EF框架使用的Entity
            {
                Name = request.Name,  //角色名称
                Class = (int)request.Class,//职业，
                TID = (int)request.Class,//CharacterDefine配置表中的TID（对应职业类型Class的枚举值1~3)
                Level = 1,
                MapID = 1,//地图序号，默认在主城出生
                MapPosX = 5000, //出生点坐标（逻辑单位为厘米，而Unity中世界坐标单位为米，使用时需转换）
                MapPosY = 4000,//40米
                MapPosZ = 820,//8.2米
                Gold = 100000,//新建角色有10万初始金币
                Equips = new byte[28],//7个装备槽，每个占4个字节(存储int装备ID）
            };
            //创建背包 同时初始化
            var bag = new TCharacterBag();
            bag.Owner = character; //当前角色为背包拥有者
            bag.Items = new byte[0];//初始道具为空
            bag.Unlocked = 20; //初始设定 解锁20个格子
            //在数据库生成时，设定了背包和角色 是一对一的实体关系，所以先 角色背包、再角色
            character.Bag = DBService.Instance.Entities.TCharacterBags.Add(bag); //先将创建的背包 Add添加到数据库，再绑定给角色
            character = DBService.Instance.Entities.Characters.Add(character);//添加新角色 到数据库的角色表

            character.Items.Add(new TCharacterItem() //赠送新手礼包： 20瓶红、蓝药水 ,这里不是直接添加到背包中
            {
                Owner = character,
                ItemID = 1,
                ItemCount = 20,
            });

            character.Items.Add(new TCharacterItem()
            {
                Owner = character,
                ItemID = 2,
                ItemCount = 20,
            });



            //session“会话控制”，是服务器为了保存用户状态而创建的一个特殊的对象，session中保存当前用户登录信息。服务器通过session来区分客户端
            sender.Session.User.Player.Characters.Add(character);//每次创建完新角色，把最新的玩家的角色列表返回给客户端
            DBService.Instance.Entities.SaveChanges();//然后修改保存到数据库

            sender.Session.Response.createChar = new UserCreateCharacterResponse();
            sender.Session.Response.createChar.Result = Result.Success;
            sender.Session.Response.createChar.Errormsg = "None";

            foreach (var c in sender.Session.User.Player.Characters)//把当前所有角色添加到createChar.Characters列表中
            {
                NCharacterInfo info = new NCharacterInfo();
                info.Id = c.ID; //DB_id
                info.Name = c.Name;
                info.Type = CharacterType.Player;
                info.Class = (CharacterClass)c.Class;
                info.ConfigId = c.TID;//CharacterDefine中的TID（对应职业类型Class的枚举值1~3)
                //info.EntityId没有赋值 默认为0，因为当前处于登录阶段，还未进入游戏，即还未创建出游戏角色Entity
                sender.Session.Response.createChar.Characters.Add(info);
            }
            sender.SendResponse();//返回响应给客户端sender
        }

        //游戏进入（目前，设定所有角色进入游戏时，默认先进入主城）
        void OnGameEnter(NetConnection<NetSession> sender, UserGameEnterRequest request)//sender代表发送消息的那个客户端，服务器的响应也将返回给这个客户端
        {
            //数据库中，1个用户User-> 1个玩家Player -> 多个角色Characters ,
            TCharacter dbchar = sender.Session.User.Player.Characters.ElementAt(request.characterIdx);//首先从DB中读取 选择的角色数据
            Log.InfoFormat("UserGameEnterRequest: characterID:{0}:{1} Map:{2}", dbchar.ID, dbchar.Name, dbchar.MapID);
            Character character = CharacterManager.Instance.AddCharacter(dbchar);//1、游戏进入时，使用读取的DBchar创建角色（同时初始化角色的各种管理器），添加到玩家管理器，并获取Entity_id

            SessionManager.Instance.AddSession(character.Id, sender);//进入游戏时，添加到在线会话管理器(玩家Id,对应Session)
            sender.Session.Response.gameEnter = new UserGameEnterResponse();
            sender.Session.Response.gameEnter.Result = Result.Success;
            sender.Session.Response.gameEnter.Errormsg = "None";

            sender.Session.Character = character; //当进入游戏时，Session会话关联角色character
            sender.Session.PostResponser = character;//游戏进入时初始化 后处理器，后处理逻辑由角色执行，（Character继承了IPostResponser接口）

            sender.Session.Response.gameEnter.Character = character.Info;//进入游戏时，返回角色信息（包含数据库中的存储信息）

            sender.SendResponse(); //让后处理的赋值，在SendResponse发送响应之前

            MapManager.Instance[dbchar.MapID].CharacterEnter(sender, character);//2、通知该地图其他玩家，有角色进入
        }

        //客户端退出游戏，首先触发服务器中UserService.OnGameLeave()函数, 来处理UserGameLeaveRequest请求 并返回UserGameLeaveResponse响应
        void OnGameLeave(NetConnection<NetSession> sender, UserGameLeaveRequest request)
        {
            Character character = sender.Session.Character;
            Log.InfoFormat("UserGameLeaveRequest: characterID:{0}:{1} MapID:{2}", character.Id, character.Info.Name, character.Info.mapId);
            DoGameLeave(character);//玩家下线，执行角色离开逻辑

            sender.Session.Response.gameLeave = new UserGameLeaveResponse();
            sender.Session.Response.gameLeave.Result = Result.Success;
            sender.Session.Response.gameLeave.Errormsg = "None";
            sender.SendResponse(); //返回退出游戏响应
        }

        public void DoGameLeave(Character character)//(触发时机：网络断开 或 客户端退出游戏)，执行玩家离线，及角色离开逻辑
        {
            Log.InfoFormat("CharacterLeave: characterID:{0}:{1}", character.Id, character.Info.Name);
            SessionManager.Instance.RemoveSession(character.Id);//离开游戏时，在线会话管理器 删除该玩家的Session

            CharacterManager.Instance.RemoveCharacter(character.Id); //1、在线角色管理器中 删除此角色
            character.Offline();//玩家角色下线通知，在地图中删除角色时，一起打包 发送消息
            MapManager.Instance[character.Info.mapId].CharacterLeave(character);//2、离开地图，对应地图中 删除此角色
        }
    }
}
