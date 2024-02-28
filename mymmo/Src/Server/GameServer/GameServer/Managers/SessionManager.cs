using System.Collections.Generic;
using Common;
using Network;


namespace GameServer.Managers
{
    class SessionManager : Singleton<SessionManager> //Session在线会话管理器，全局存在
    {
        public Dictionary<int, NetConnection<NetSession>> Sessions = new Dictionary<int, NetConnection<NetSession>>();//角色Id 为Key

        public void AddSession(int characterId,NetConnection<NetSession> session)//玩家进入游戏时，添加到在线会话管理器
        {
            this.Sessions[characterId] = session;
        }

        public void RemoveSession(int characterId)//玩家离开游戏时，从 在线会话管理器中删除
        {
            this.Sessions.Remove(characterId);
        }

        public NetConnection<NetSession> GetSession(int characterId)//获取 characterId对应玩家的 session在线会话状态
        {
            NetConnection<NetSession> session = null;
            this.Sessions.TryGetValue(characterId, out session);
            return session;
        }

    }
}
