using BZ10;
using DeviceController.Common;
using DeviceController.View.CQ12;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceController
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //互斥量
            bool isUnique;
            using (PubField.mutex = new Mutex(true, System.Diagnostics.Process.GetCurrentProcess().ProcessName, out isUnique))
            {
                if (isUnique)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
#if DEBUG
                    Application.Run(new Main());
#else
                    /*
                    * 当前用户是管理员的时候，直接启动应用程序
                    * 如果不是管理员，则使用启动对象启动程序，以确保使用管理员身份运行
                    */
                    //获得当前登录的Windows用户标示
                    System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                    System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
                    //判断当前登录用户是否为管理员
                    if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
                    {
                        //如果是管理员，则直接运行
                        Application.Run(new Main());
                    }
                    else
                    {
                        //创建启动对象
                        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                        startInfo.UseShellExecute = true;
                        startInfo.WorkingDirectory = System.Environment.CurrentDirectory;
                        startInfo.FileName = Application.ExecutablePath;
                        //设置启动动作,确保以管理员身份运行
                        startInfo.Verb = "runas";
                        try
                        {
                            System.Diagnostics.Process.Start(startInfo);
                        }
                        catch
                        {
                            return;
                        }
                        //退出
                        Application.Exit();
                    }
#endif
                }
                else
                {
                    MessageBox.Show("应用程序已经在运行中...", "提示");
                    Thread.Sleep(1000);
                    System.Environment.Exit(0);
                }
            }
        }
    }
}
