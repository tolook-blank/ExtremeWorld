using System;

namespace Common
{
    public class GameDefine
    {//游戏中数值设定
        public const int BagMaxItemPerPage = 30;
        public const int GuildMaxMemberCount = 40;//公会容纳成员上限
        public const int MaxChatRecordNums = 20;  //每次进入客户端，拉取最近的聊天记录，最多20条
        public const int MaxChatRecordTimes = 600; //聊天记录保存时间，10分钟内
    }
}
