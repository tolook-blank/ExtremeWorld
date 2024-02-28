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
using System.Runtime.InteropServices;
class Time
{
    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceCounter([In, Out] ref long lpPerformanceCount);
    [DllImport("kernel32.dll")]
    static extern bool QueryPerformanceFrequency([In, Out] ref long lpFrequency);

    static Time()
    {
        startupTicks = ticks;
    }

    private static long _frameCount = 0;

    /// <summary>
    /// The total number of frames that have passed (Read Only).
    /// </summary>
    public static long frameCount { get { return _frameCount; } }

    static long startupTicks = 0;

    static long freq = 0;

    /// <summary>
    /// Tick count
    /// </summary>
    static public long ticks
    {
        get
        {
            long f = freq;

            if (f == 0)
            {
                if (QueryPerformanceFrequency(ref f))
                {
                    freq = f;
                }
                else
                {
                    freq = -1;
                }
            }
            if (f == -1)
            {
                return Environment.TickCount * 10000;
            }
            long c = 0;
            QueryPerformanceCounter(ref c);
            return (long)(((double)c) * 1000 * 10000 / ((double)f));
        }
    }

    private static long lastTick = 0;
    private static float _deltaTime = 0;

    /// <summary>
    /// The time in seconds it took to complete the last frame (Read Only).
    /// </summary>
    public static float deltaTime
    {
        get
        {
            return _deltaTime;
        }
    }


    private static float _time = 0;
    /// <summary>
    ///  The time at the beginning of this frame (Read Only). This is the time in seconds
    ///  since the start of the game.
    /// </summary> 
    public static float time
    {
        get
        {
            return _time;
        }
    }


    /// <summary>
    /// The real time in seconds since the started (Read Only).
    /// </summary>
    public static float realtimeSinceStartup
    {
        get
        {
            long _ticks = ticks;
            return (_ticks - startupTicks) / 10000000f;
        }
    }

    public static void Tick()
    {
        long _ticks = ticks;


        _frameCount++;
        if (_frameCount == long.MaxValue)
            _frameCount = 0;

        if (lastTick == 0) lastTick = _ticks;
        _deltaTime = (_ticks - lastTick) / 10000000f;
        _time = (_ticks - startupTicks) / 10000000f;
        lastTick = _ticks;
    }

    //此处逻辑改为放在TimeUtil中
    //public static int timestamp
    //{
    //    get { return GetTimestamp(DateTime.Now); } //根据当前时间，取时间戳
    //}

    //public static DateTime GetTime(long timeStamp) //将时间戳转换为具体的日期和时间。
    //{
    //    DateTime dateTimeStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));//dateTimeStart 是一个 DateTime 对象，表示1970年1月1日的本地时间。
    //    long lTime = timeStamp * 10000000; // 将Unix时间戳（以秒为单位）转换为刻度数（Tick），一秒钟内有1千万个刻度,即每个刻度对应100纳秒（刻度是.NET中最小的时间单位）
    //    TimeSpan toNow = new TimeSpan(lTime);//toNow 是一个 TimeSpan 对象，表示从1970年1月1日到 给定时间戳的时间间隔。
    //    return dateTimeStart.Add(toNow); //将这个时间间隔加到 dateTimeStart 上，得到对应时间戳的 DateTime 对象。
    //}

    //public static int GetTimestamp(System.DateTime time)
    //{
    //    DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));//1970年1月1日的本地时间
    //    return (int)(time - startTime).TotalSeconds; 
    //}
}
