/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：StartEXE
/// 文件名称    ：Tools.cs
/// 命名空间    ：StartEXE
/// =================================
/// 创 建 者    ：赵新雨
/// 创建日期    ：2020/09/21 13:49:10 
/// 邮    箱    ：zhao2271154036@163.com
/// 功能描述    ：
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2020. All rights reserved.
/// ***********************************************************************
using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BZ10
{
    /// <summary>
    /// 项目名称 ：StartEXE
    /// 命名空间 ：StartEXE
    /// 类 名 称 ：Tools
    /// 作    者 ：赵新雨 
    /// 创建时间 ：2020/09/21 13:49:10 
    /// 更新时间 ：2020/09/21 13:49:10
    /// </summary>
    class SoftKeyboardCtrl
    {

        public static string windowNames = "郑州欧柯奇仪器制造有限公司-屏幕键盘";
        public static string softKeyBoard = Application.StartupPath + "\\" + "SoftKeyboard.exe";
        [DllImport("user32.dll", EntryPoint = "MoveWindow")]
        public static extern bool MoveWindow(System.IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, out Rect lpRect);
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32", EntryPoint = "GetWindowThreadProcessId")]
        public static extern int GetWindowThreadProcessId(IntPtr hwnd, out int pid);


        //打开窗体方法，fileName是的窗体名称，包含路径
        public static void OpenAndSetWindow()
        {
            try
            {
                Process p = new Process();
                p.StartInfo.FileName = softKeyBoard;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                p.Start();
                //设置窗体位置
                new Thread(() =>
                {
                    IntPtr handle = IntPtr.Zero;
                    while (handle == IntPtr.Zero)
                        handle = FindWindow(null, windowNames);
                    int x = 0;
                    int y = 0;
                    int tag = -1;// 0 上    1下
                    Point mosePoint = Control.MousePosition;
                    if (mosePoint.Y > Screen.PrimaryScreen.Bounds.Height / 2)
                        tag = 0;
                    else if (mosePoint.Y <= Screen.PrimaryScreen.Bounds.Height / 2)
                        tag = 1;

                    //获取外部窗口大小
                    Rect rect = new Rect();
                    GetWindowRect(handle, out rect);
                    int width = rect.Right - rect.Left;
                    int height = rect.Bottom - rect.Top;

                    //弹窗在上
                    if (tag == 0)
                        y = 0;
                    //弹窗在下
                    else if (tag == 1)
                        y = Screen.PrimaryScreen.Bounds.Height - height;
                    x = Screen.PrimaryScreen.Bounds.Width / 2 - width / 2;
                    MoveWindow(handle, x, y, width, height, true);
                }).Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
            }
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        public static void CloseWindow()
        {
            try
            {
                IntPtr handle = FindWindow(null, SoftKeyboardCtrl.windowNames);
                if (handle != IntPtr.Zero)
                {
                    int pid = 0;
                    GetWindowThreadProcessId(handle, out pid);
                    if (pid != 0)
                    {
                        Process proc = Process.GetProcessById(pid);
                        proc.Kill();
                        proc.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
           
        }
    }
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

}
