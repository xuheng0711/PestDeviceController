/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：BZ10
/// 文件名称    ：TimeManager.cs
/// 命名空间    ：BZ10
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2020-05-08  10:03:31 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：设置系统本地时间
/// 使用说明    ：使用此功能必须为管理员权限，使用者调用SetSysTime即可
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2020. All rights reserved.
/// ***********************************************************************
using BZ10.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace DeviceController.Common
{
    /// <summary>
    /// 项目名称 ：BZ10
    /// 命名空间 ：BZ10
    /// 类 名 称 ：TimeManager
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2020-05-08  10:03:31 
    /// 更新时间 ：2020-05-08  10:03:31
    /// </summary>
    public class TimeManager
    {
        /// <summary>
        /// 设置系统时间
        /// </summary>
        public static void SetSysTime()
        {
            Thread thread = new Thread(SetSysTime_T);
            thread.IsBackground = true;
            thread.Start();
        }

        /// <summary>
        /// 设置系统时间线程函数
        /// </summary>
        public static void SetSysTime_T()
        {
            while (true)
            {
                try
                {
                    DateTime newdatetime = GetNetworkTime();
                    //设置系统当前日期时间
                    SystemTime MySystemTime = new SystemTime();
                    MySystemTime.vYear = (ushort)newdatetime.Year;
                    MySystemTime.vMonth = (ushort)newdatetime.Month;
                    MySystemTime.vDay = (ushort)newdatetime.Day;
                    MySystemTime.vHour = (ushort)newdatetime.Hour;
                    MySystemTime.vMinute = (ushort)newdatetime.Minute;
                    MySystemTime.vSecond = (ushort)newdatetime.Second;
                    long l = Win32API.SetLocalTime(MySystemTime);
                    if (l != 0)
                    {
                        DebOutPut.DebLog("日期矫正成功！");
                    }
                    else
                    {
                        DebOutPut.DebLog("日期矫正失败，使用此功能必须以管理员权限运行！");
                    }
                }
                catch (Exception ex)
                {
                    DebOutPut.DebLog(ex.ToString());
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                }
                Thread.Sleep(180000);
            }
        }

        /// <summary>
        /// 从time.windows.com获取当前日期
        /// </summary>
        /// <returns></returns>
        private static DateTime GetNetworkTime()
        {
            return GetNetworkTime("time.windows.com"); // time-a.nist.gov
        }

        /// <summary>
        /// 获取当前日期时间
        /// </summary>
        /// <param name="ntpServer"></param>
        /// <returns></returns>
        private static DateTime GetNetworkTime(string ntpServer)
        {
            IPAddress[] address = Dns.GetHostEntry(ntpServer).AddressList;
            if (address == null || address.Length == 0)
            {
                DebOutPut.DebLog("无法从解析ip地址:" + ntpServer + " 将返回本地日期");
                return DateTime.Now;
            }
            IPEndPoint ep = new IPEndPoint(address[0], 123);
            return GetNetworkTime(ep);
        }



        /// <summary>
        /// 获取当前日期时间
        /// </summary>
        /// <param name="ep"></param>
        /// <returns></returns>
        private static DateTime GetNetworkTime(IPEndPoint ep)
        {
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

            s.Connect(ep);

            byte[] ntpData = new byte[48]; // RFC 2030 
            ntpData[0] = 0x1B;
            for (int i = 1; i < 48; i++)
                ntpData[i] = 0;

            s.Send(ntpData);
            s.Receive(ntpData);

            byte offsetTransmitTime = 40;
            ulong intpart = 0;
            ulong fractpart = 0;

            for (int i = 0; i <= 3; i++)
                intpart = 256 * intpart + ntpData[offsetTransmitTime + i];

            for (int i = 4; i <= 7; i++)
                fractpart = 256 * fractpart + ntpData[offsetTransmitTime + i];

            ulong milliseconds = (intpart * 1000 + (fractpart * 1000) / 0x100000000L);
            s.Close();

            TimeSpan timeSpan = TimeSpan.FromTicks((long)milliseconds * TimeSpan.TicksPerMillisecond);

            DateTime dateTime = new DateTime(1900, 1, 1);
            dateTime += timeSpan;

            TimeSpan offsetAmount = TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
            DateTime networkDateTime = (dateTime + offsetAmount);

            return networkDateTime;
        }


    }
}
