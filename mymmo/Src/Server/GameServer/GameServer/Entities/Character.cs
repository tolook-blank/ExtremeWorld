using Common;
using GameServer.Managers;
using SkillBridge.Message;
using GameServer.Models;
using Network;


namespace GameServer.Entities
{
    // Character包括三个CharacterType：Player、NPC、 Monster
    class Character : CharacterBase, IPostResponse
    {
        public TCharacter Data;//角色数据，用来同步 数据库中角色数据

        //客户端进入游戏时，获得从服务器返回的角色
        //初始化属于角色的管理器，包括 道具管理器、状态管理器、任务管理器
        public ItemManager ItemManager;
        public StatusManager StatusManager;
        public QuestManager QuestManager;
        public FriendManager FriendManager;

        public Team Team; //Team不用数据库存储，不作持久化存储
        public double TeamUpdateTS;//当前角色 的队伍信息变更的时间戳，每个玩家各自保存，方便收到队伍变动信息

        public Guild Guild; //玩家所在的公会，使用数据库存储
        public double GuildUpdateTS; //公会信息变更时间戳，每个玩家各自保存一份，方便收到公会变动信息

        public Chat Chat;//聊天记录 不用数据库存储

        //在客户端进入游戏，创建玩家Player的角色时 调用此构造函数
        public Character(CharacterType type, TCharacter dbCha) :
            base(new Core.Vector3Int(dbCha.MapPosX, dbCha.MapPosY, dbCha.MapPosZ), new Core.Vector3Int(100, 0, 0))
        {
            this.Data = dbCha; //读取数据库，每当在DB的TCharacter表中增加字段时，就要在Character构造函数中增加该字段的初始化

            this.Id = dbCha.ID; //TCharacter_ID是DB_id,  而npc、怪物没在数据库存储，所以没有DB_id，即只有玩家有DB_id
            this.Info = new NCharacterInfo(); //this.Info是服务器在和客户端通讯中，传递角色数据的载体NCharacterInfo
            this.Info.Type = type;
            this.Info.Id = dbCha.ID;//DB_id
            this.Info.EntityId = this.entityId; //为了移动同步，应该使用Entity_id
            this.Info.ConfigId = dbCha.TID; //CharacterDefine中的TID（对应职业类型Class的枚举值1~3)
            this.Info.Name = dbCha.Name;
            this.Info.Level = 10;//dbCha.Level;
            this.Info.Class = (CharacterClass)dbCha.Class;//角色职业
            this.Info.mapId = dbCha.MapID;  //进入的地图标号
            this.Info.Gold = dbCha.Gold; //从DB数据库中读出金币
            this.Info.Ride = 0;
            this.Info.Entity = this.EntityData; //角色的实体信息（Pos、Dir、Speed）
            this.Define = DataManager.Instance.Characters[this.Info.ConfigId]; //角色职业的配置表信息

            //登陆时初始化道具管理器
            this.ItemManager = new ItemManager(this);//指定 道具管理器的拥有者，Character类型
            this.ItemManager.GetItemInfos(this.Info.Items);//将 道具管理器中的DB道具信息，填充到 网络道具协议 Info.Items，包括赠送的20瓶红、蓝药水

            //登陆时取获取DB中背包数据，发送给客户端  (也可专门创建BagManager，都要将DB中背包中 道具信息 取出来打包成 网络背包协议)
            this.Info.Bag = new NBagInfo();
            this.Info.Bag.Unlocked = this.Data.Bag.Unlocked;//从数据库中，把背包信息取出，打包成网络背包数据，发送给客户端来做初始化
            this.Info.Bag.Items = this.Data.Bag.Items;

            //登陆时获取DB中的装备数据，发送给客户端
            this.Info.Equips = this.Data.Equips; 

            //登陆时初始化任务管理器
            this.QuestManager = new QuestManager(this);
            this.QuestManager.GetQuestInfos(this.Info.Quests);//获得DB中任务信息 填充到网络Info.Quests
            //初始化状态管理器
            this.StatusManager = new StatusManager(this);//指定 状态管理器的所有者

            //登陆时初始化好友管理器
            this.FriendManager = new FriendManager(this);
            this.FriendManager.GetFriendInfos(this.Info.Friends);

            this.Guild = GuildManager.Instance.GetGuild(this.Data.GuildId);//登陆时，由玩家身上的公会ID 获取公会信息

            this.Chat = new Chat(this);
        }

        public long Gold //这里做成属性的好处是：当对金币进行赋值，就会触发状态管理器
        {
            get { return this.Data.Gold; }//角色身上的金币 this.Data.Gold
            set
            {
                if (this.Data.Gold == value) { return; }
                this.StatusManager.AddGoldChange((int)(value - this.Data.Gold)); //金币数改变（新的 - 旧的），传给StatusManager，通知客户端来同步金币变化
                this.Data.Gold = value; //修改金币数量
            }
        }

        public int Ride //简单的封装，使用 Ride 代替 this.Info.Ride
        {
            get { return this.Info.Ride; }
            set
            {
                if (this.Info.Ride == value)
                    return;
                this.Info.Ride = value;
            }
        }

        //继承IPostResponse实现PostProcess方法，每次在调用 NetSession的GetResponse()打包消息时触发
        public void PostProcess(NetMessageResponse message)//每个Character都有的 后处理方法，处理各种状态变化
        {
            Log.InfoFormat("PostProcess > Character :characterID:{0}_{1}", this.Id, this.Info.Name);
            this.FriendManager.PostProcess(message); //FriendManager中后处理，同步好友列表中的变化

            if (this.Team != null)//当前角色有队伍
            {
                Log.InfoFormat("PostProcess >Team: characterID:{0}_{1} {2}<{3}", this.Id, this.Info.Name, TeamUpdateTS, this.Team.timestamp);
                if (TeamUpdateTS < this.Team.timestamp) //若队伍信息发生了改变（TeamUpdateTS当前角色的队伍信息变更的时间 < timestamp当前队伍信息变更的时间）
                {
                    TeamUpdateTS = Team.timestamp;//更新变更时间
                    this.Team.PostProcess(message);//每个玩家都会调用，都会收到队伍变化的通知
                }
            }

            if (this.Guild != null)//角色身上的公会存在（本地内存中）
            {
                Log.InfoFormat("PostProcess > Guild: characterID:{0}:{1}  {2}<{3}", this.Id, this.Info.Name, GuildUpdateTS, this.Guild.timestamp);
                if (this.Info.Guild == null) //该角色的 公会信息为空,可能是中途创建的？
                {
                    this.Info.Guild = this.Guild.GuildInfo(this);//获取当前公会信息，填充角色的公会信息
                    if (message.mapCharacterEnter != null) //进入地图响应非空，是在线状态
                        GuildUpdateTS = Guild.timestamp;//更新为公会的时间戳
                }
                if (GuildUpdateTS < this.Guild.timestamp && message.mapCharacterEnter == null)//只要时间戳变了（代表公会信息改变）
                {
                    GuildUpdateTS = Guild.timestamp;//就把公会信息 分别发送给公会中每个成员
                    this.Guild.PostProcess(this, message);
                }
            }

            if (this.StatusManager.HasStatus)
            {
                this.StatusManager.PostProcess(message);//状态管理器 后处理
            }

            this.Chat.PostProcess(message); //角色的聊天 后处理
        }

        /// <summary>
        /// CharacterLeave角色离开 时调用
        /// </summary>
        public void Offline()
        {
            this.FriendManager.OfflineNotify();//更新好友列表中在线状态
        }

        public NCharacterInfo GetBasicInfo() //可用于好友、组队、公会系统中，获取在线的 角色信息
        {
            return new NCharacterInfo()
            {
                Id = this.Id,
                Name = this.Info.Name,
                Class = this.Info.Class,
                Level = this.Info.Level
            };
        }

    }
}
