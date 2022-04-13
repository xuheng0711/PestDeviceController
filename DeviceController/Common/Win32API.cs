/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.Common
/// 文件名称    ：Win32API.cs
/// 命名空间    ：DeviceController.Common
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/19 11:20:26 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：Win32API导入
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2019. All rights reserved.
/// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DeviceController.Common
{
    /// <summary>
    /// 项目名称 ：DeviceController.Common
    /// 命名空间 ：DeviceController.Common
    /// 类 名 称 ：Win32API
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/19 11:20:26 
    /// 更新时间 ：2019/11/19 11:20:26
    /// </summary>
    public class Win32API
    {
        [DllImport("Wtsapi32.dll")]
        public static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTSInfoClass wtsInfoClass, out IntPtr ppBuffer, out uint pBytesReturned);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
        public const int WM_CLOSE = 0x10;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern int SetWindowText(IntPtr hWnd, string text);
        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern bool ShowWindow(IntPtr hWnd, uint nCmdShow);
        /// <summary>
        /// 启动一个外部程序
        /// </summary>
        /// <returns></returns>
        [DllImport("shell32.dll")]
        public static extern IntPtr ShellExecute(IntPtr hwnd, string lpOperation, string lpFile, string lpParameters, string lpDirectory, ShellExecute_ShowCommands nShowCmd);


        /// <summary>
        /// 转换虚拟内存
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        public static extern int SetProcessWorkingSetSize(IntPtr process, int minSize, int maxSize);

        /// <summary>
        /// 在窗口结构中为指定的窗口设置信息
        /// </summary>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int wndproc);

        /// <summary>
        /// 从指定窗口的结构中取得信息
        /// </summary>
        /// <returns></returns>
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        /// <summary>
        /// 查找窗口
        /// </summary>
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 窗体放置在最前端
        /// </summary>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        /// <summary>
        /// 为初始化文件中指定的条目取得字串
        /// </summary>
        [DllImport("kernel32")]
        public static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        /// <summary>
        /// 在初始化文件指定小节内设置一个字串
        /// </summary>
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        /// <summary>
        /// 获取指定小节所有项名和值的一个列表
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll", EntryPoint = "GetPrivateProfileSection")]
        public static extern uint GetPrivateProfileSection(string lpAppName,
         sbyte[] lpReturnedString, uint nSize, string lpFileName);

        /// <summary>
        /// 用于设置系统时间
        /// </summary>
        /// <param name="sysTime">时间</param>
        /// <returns></returns>
        [DllImport("Kernel32.dll")]
        public static extern long SetLocalTime(SystemTime sysTime);

        /// <summary>
        /// 用于获得系统时间
        /// </summary>
        /// <param name="sysTime">时间</param>
        [DllImport("Kernel32.dll")]
        public static extern void GetLocalTime(ref SystemTime sysTime);

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns></returns>
        [DllImport("kernel32.dll")]
        public static extern uint GetLastError();
    }

    /// <summary>
    ///系统时间类
    /// </summary>
    [StructLayoutAttribute(LayoutKind.Sequential)]
    public class SystemTime
    {
        public ushort vYear;
        public ushort vMonth;
        public ushort vDayOfWeek;
        public ushort vDay;
        public ushort vHour;
        public ushort vMinute;
        public ushort vSecond;
    }

    /// <summary>
    /// ShellExecute_ShowCommands
    /// </summary>
    public enum ShellExecute_ShowCommands : int
    {
        SW_HIDE = 0,
        SW_SHOWNORMAL = 1,
        SW_NORMAL = 1,
        SW_SHOWMINIMIZED = 2,
        SW_SHOWMAXIMIZED = 3,
        SW_MAXIMIZE = 3,
        SW_SHOWNOACTIVATE = 4,
        SW_SHOW = 5,
        SW_MINIMIZE = 6,
        SW_SHOWMINNOACTIVE = 7,
        SW_SHOWNA = 8,
        SW_RESTORE = 9,
        SW_SHOWDEFAULT = 10,
        SW_FORCEMINIMIZE = 11,
        SW_MAX = 11
    }
    public enum WTSInfoClass
    {
        WTSInitialProgram,
        WTSApplicationName,
        WTSWorkingDirectory,
        WTSOEMId,
        WTSSessionId,
        WTSUserName,
        WTSWinStationName,
        WTSDomainName,
        WTSConnectState,
        WTSClientBuildNumber,
        WTSClientName,
        WTSClientDirectory,
        WTSClientProductId,
        WTSClientHardwareId,
        WTSClientAddress,
        WTSClientDisplay,
        WTSClientProtocolType,
        WTSIdleTime,
        WTSLogonTime,
        WTSIncomingBytes,
        WTSOutgoingBytes,
        WTSIncomingFrames,
        WTSOutgoingFrames,
        WTSClientInfo,
        WTSSessionInfo
    }
}
