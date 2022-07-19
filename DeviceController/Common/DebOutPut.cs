/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：BZ10.Common
/// 文件名称    ：DbgOutPut.cs
/// 命名空间    ：BZ10.Common
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/28 17:32:49 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：BZ10设备的日志输出
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2019. All rights reserved.
/// ***********************************************************************
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BZ10.Common
{
    /// <summary>
    /// 项目名称 ：BZ10.Common
    /// 命名空间 ：BZ10.Common
    /// 类 名 称 ：DbgOutPut
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/28 17:32:49 
    /// 更新时间 ：2019/11/28 17:32:49
    /// </summary>
    public class DebOutPut
    {
#if DEBUG
        /// <summary>
        /// DebView模式
        /// </summary>
        public static bool isDebView = true;
#else
        /// <summary>
        /// DebView模式
        /// </summary>
        public static bool isDebView = false;
#endif
        /// <summary>
        /// 锁
        /// </summary>
        static readonly object SequenceLock = new object();

        /// <summary>
        /// 输出错误日志
        /// </summary>
        /// <param name="message">错误信息</param>
        public static void DebErr(string message)
        {
            if (isDebView)
            {
                Debugger.Log(0, null, "Debug:" + message + "\n");
            }
        }

        /// <summary>
        /// 输出正常信息
        /// </summary>
        /// <param name="message">信息</param>
        public static void DebLog(string message)
        {
            if (isDebView)
            {
                Debugger.Log(0, null, "Debug:" + message + "\n");
            }
        }

        /// <summary>
        /// 写入日志
        /// </summary>
        /// <param name="sInfo">日志信息</param>
        /// <param name="isDebug">是否开启debug,如果为true表示不是异常日志</param>
        public static void WriteLog(LogType logType, string sInfo, bool isDebug = false)
        {
            lock (SequenceLock)
            {
                try
                {
                    if (!isDebug)
                    {
                        string sDir = "";
                        switch (logType)
                        {
                            case LogType.Normal:
                                sDir = SavePath(LogType.Normal);
                                break;
                            case LogType.Error:
                                sDir = SavePath(LogType.Error);
                                break;
                            default:
                                break;
                        }
                        if (sDir != "")
                        {
                            //文件流
                            FileStream fileStream = new FileStream(sDir, FileMode.Append, FileAccess.Write);
                            //获取当前时间
                            string sTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            //将当前时间以及日志信息写入文件
                            string data = "\r\n" + "[" + sTime + "]" + sInfo + "\r\n";
                            //拿到长度 
                            int tlen = Encoding.ASCII.GetCharCount(Encoding.UTF8.GetBytes(data.ToCharArray(), 0, data.Length));
                            byte[] writeByte = Encoding.UTF8.GetBytes(data.ToCharArray(), 0, data.Length);
                            fileStream.Write(writeByte, 0, tlen);
                            //刷新流
                            fileStream.Flush();
                            //关闭流
                            fileStream.Close();
                            fileStream.Dispose();
                        }
                    }
                }
                catch (Exception ex)
                {
                    DebErr(ex.ToString());
                }
            }
        }


        /// <summary>
        /// 读取日志函数
        /// </summary>
        /// <returns></returns>
        public static string ReadLog(LogType logType)
        {
            lock (SequenceLock)
            {
                try
                {
                    string sDir = "";
                    switch (logType)
                    {
                        case LogType.Normal:
                            sDir = SavePath(LogType.Normal);
                            break;
                        case LogType.Error:
                            sDir = SavePath(LogType.Error);
                            break;
                        default:
                            break;
                    }
                    //路径
                    if (sDir != "")
                    {
                        //文件流
                        FileStream fileReadStream = new FileStream(sDir, FileMode.Open, FileAccess.Read);
                        if (fileReadStream == null || !fileReadStream.CanRead)
                        {
                            return "";
                        }
                        byte[] bReadBufer = new byte[2048];
                        int len = 0;
                        string ret = "";
                        while ((len = fileReadStream.Read(bReadBufer, 0, 2048)) != 0)
                        {
                            ret += UTF8Encoding.UTF8.GetString(bReadBufer);
                            Thread.Sleep(100);
                        }
                        fileReadStream.Close();
                        fileReadStream.Dispose();
                        return ret;
                    }
                    else
                    {
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    DebErr(ex.ToString());
                    return "";
                }
            }
        }

        /// <summary>
        /// 保存路径
        /// </summary>
        private static string SavePath(LogType logType)
        {
            try
            {
                //路径
                string sDir = PubField.basePath;
                //拼接当前程序文件夹和日志文件夹
                sDir = Path.Combine(sDir, "DeviceControllerLog");
                //判断这个目录是否存在，如果不存在，则创建
                if (!Directory.Exists(sDir))
                {
                    Directory.CreateDirectory(sDir);
                }
                string logName = DateTime.Now.ToString("yyyyMMdd") + ".log";
                switch (logType)
                {
                    case LogType.Normal:
                        sDir = Path.Combine(sDir, "NormalLog");
                        if (!Directory.Exists(sDir))
                            Directory.CreateDirectory(sDir);
                        //将两个字符串和成一个路径，创建日志文本
                        sDir = Path.Combine(sDir, logName);
                        break;
                    case LogType.Error:
                        sDir = Path.Combine(sDir, "ErrorLog");
                        if (!Directory.Exists(sDir))
                            Directory.CreateDirectory(sDir);
                        //将两个字符串和成一个路径，创建日志文本
                        sDir = Path.Combine(sDir, logName);
                        break;
                    default:
                        break;
                }
                string size = Tools.GetFileSize(sDir);
                if (size != "" && size != null)
                {
                    //如果文件大于10兆 ，则直接删除
                    if (double.Parse(size) > 3f)
                    {
                        if (File.Exists(sDir))
                        {
                            File.Delete(sDir);
                        }
                    }
                }
                return sDir;
            }
            catch (Exception ex)
            {
                DebErr(ex.ToString());
                return "";
            }

        }
    }
}
