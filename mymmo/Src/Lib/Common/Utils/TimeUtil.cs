using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common.Utils
{
    //Common中数据，客户端和服务端是通用的
    public class TimeUtil
    {
        public static double timestamp
        {
            get { return GetTimestamp(DateTime.Now); } //根据当前时间，取时间戳
        }

        public static DateTime GetTime(long timeStamp) //将时间戳转换为具体的日期和时间。
        {
            DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));//dateTimeStart 是一个 DateTime 对象，表示1970年1月1日的本地时间。
            long lTime = timeStamp * 10000000; // 将Unix时间戳（以秒为单位）转换为刻度数（Tick），一秒钟内有1千万个刻度,即每个刻度对应100纳秒（刻度是.NET中最小的时间单位）
            TimeSpan toNow = new TimeSpan(lTime);//toNow 是一个 TimeSpan 对象，表示从1970年1月1日到 给定时间戳的时间间隔。
            return dateTimeStart.Add(toNow); //将这个时间间隔加到 dateTimeStart 上，得到对应时间戳的 DateTime 对象。
        }

        public static double GetTimestamp(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (time - startTime).TotalSeconds;
        }
    }
}
