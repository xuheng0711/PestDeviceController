using BZ10;
using BZ10.Common;
using cn.bmob.api;
using cn.bmob.io;
using cn.bmob.tools;
using DevComponents.DotNetBar;
using DeviceController.Common;
using DeviceController.View;
using DeviceController.View.Climate;
using DeviceController.View.CQ12;
using DeviceController.View.DevOverview;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using Environment = System.Environment;
using NewVersion = DeviceController.Common.NewVersion;
using Tools = DeviceController.Common.Tools;

namespace DeviceController
{
    /// <summary>
    /// 气象数据展示刷新委托
    /// </summary>
    public delegate void GPSUpdata(string str);
    public partial class Main : Office2007Form
    {
        public static GPSUpdata gpsUpdata;
        public static Main instance;
        int inTimer1 = 0;
        System.Timers.Timer timer1 = new System.Timers.Timer();//MESSAGEBOX窗口定时关闭
        int inTimer2 = 0;
        System.Timers.Timer timer2 = new System.Timers.Timer();//时间展示刷新、定时重启
        public Main()
        {

            //hintForm.Show();
            InitializeComponent();
#if DEBUG
            this.WindowState = FormWindowState.Normal;
#else
            this.WindowState = FormWindowState.Maximized;
#endif
            gpsUpdata = GPSUpdata_;
            this.LabVersion.Text = "V_" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            this.label61.Text = "系统当前登录用户: " + Tools.GetCurrentUser();
            //时间矫正
            TimeManager.SetSysTime();
            timer1.Elapsed += new ElapsedEventHandler(timer1_Tick);//MESSAGEBOX窗口定时关闭
            timer1.Interval = 1000;//设置时间间隔
            timer2.Elapsed += new ElapsedEventHandler(timer2_Tick);//时间展示刷新、定时重启
            timer2.Interval = 1000;//设置时间间隔
            instance = this;
            x = this.Width;
            y = this.Height;
            SetTag(this);
        }
        private void Timer2Stop()
        {
            timer2.Stop();
            Interlocked.Exchange(ref inTimer2, 0);
        }

        private void GPSUpdata_(string str)
        {
            this.labelX3.Text = str;
        }

        /// <summary>
        /// 窗体加载
        /// </summary>
        private void Main_Load(object sender, EventArgs e)
        {
            try
            {
                Init();
                //自动更新
                Thread updataSetup = new Thread(UpdataSetup_T);
                updataSetup.IsBackground = true;
                updataSetup.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void Init()
        {
            try
            {
                Tools.ExchangeMemory();//进程内存交换到虚拟内存
                BtnOverview_Click(null, null);//设备总览
                BtnClimate_Click(null, null);//气象
                BtnSpore_Click(null, null);//病情
                BtnCQ12_Click(null, null);//虫情
                BtnOverview_Click(null, null);//设备总览
                ClimateTypeValue();
                ClimateMainForm.climateInit();
                Application.DoEvents();
                MainForm.bz10Init();
                Application.DoEvents();
                CQ12MainForm.cQ12Init();
                Application.DoEvents();
                this.LabVersion.Text = "V_" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                timer2.Start();
                Tools.AutoStart(false);
                if (DebOutPut.isDebView)
                {
                    this.label27.Text = "测试版";
                    this.label27.ForeColor = System.Drawing.Color.Red;

                }
                else if (!DebOutPut.isDebView)
                {
                    this.label27.Text = "发布版";
                    this.label27.ForeColor = System.Drawing.Color.White;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;
        //        return cp;
        //    }
        //}
        /// <summary>
        /// 采集总览
        /// </summary>
        private void BtnOverview_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Tools.SearchMDIFormIsExist(this.MdiChildren, "采集总览"))
                {
                    DevOverviewMain devOverviewMain = new DevOverviewMain();
                    Tools.OpenNewMdiForm(this, devOverviewMain);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// CQ12
        /// </summary>
        private void BtnCQ12_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Tools.SearchMDIFormIsExist(this.MdiChildren, "虫情"))
                {
                    CQ12MainForm cQ12MainForm = new CQ12MainForm();
                    Tools.OpenNewMdiForm(this, cQ12MainForm);
                }
                if (sender != null && e != null)
                {
                    CQ12MainForm.cQ12UpdataDataList();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 气象
        /// </summary>
        private void BtnClimate_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Tools.SearchMDIFormIsExist(this.MdiChildren, "气象"))
                {
                    ClimateMainForm climateMainForm = new ClimateMainForm();
                    Tools.OpenNewMdiForm(this, climateMainForm);
                }
                if (sender != null && e != null)
                {
                    ClimateMainForm.climateUpdataDataList();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 智能孢子监测站
        /// </summary>
        private void BtnSpore_Click(object sender, EventArgs e)
        {
            try
            {
                if (!Tools.SearchMDIFormIsExist(this.MdiChildren, "病情"))
                {
                    MainForm bz10 = new MainForm();
                    Tools.OpenNewMdiForm(this, bz10);
                }
                if (sender != null && e != null)
                {
                    //BZ10数据刷新
                    MainForm.bz10UpdataDataShwo();
                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 小气候的环境类型复制
        /// </summary>
        private void ClimateTypeValue()
        {
            try
            {
                PubField.ClimateType.Add("01", "温度");
                PubField.ClimateType.Add("02", "湿度");
                PubField.ClimateType.Add("03", "露点");
                PubField.ClimateType.Add("04", "光照度");
                PubField.ClimateType.Add("05", "蒸发量");
                PubField.ClimateType.Add("06", "风向");
                PubField.ClimateType.Add("07", "风速");
                PubField.ClimateType.Add("08", "光合辐射");
                PubField.ClimateType.Add("0A", "雨量");
                PubField.ClimateType.Add("0B", "土壤温度");
                PubField.ClimateType.Add("0C", "土壤水分");
                PubField.ClimateType.Add("0D", "大气压");
                PubField.ClimateType.Add("0E", "二氧化碳");
                PubField.ClimateType.Add("0F", "土壤盐分");
                PubField.ClimateType.Add("31", "PH(玻璃电极)");
                PubField.ClimateType.Add("09", "PM2.5");
                PubField.ClimateType.Add("15", "总辐射");
                PubField.ClimateType.Add("1D", "紫外辐射");
                PubField.ClimateType.Add("20", "溶解氧");
                PubField.ClimateType.Add("21", "水温");
                PubField.ClimateType.Add("14", "PH(非玻璃)");
                PubField.ClimateType.Add("10", "氨气");
                PubField.ClimateType.Add("1E", "日照时数");
                PubField.ClimateType.Add("16", "雨雪");
                PubField.ClimateType.Add("17", "页面湿度");
                PubField.ClimateType.Add("18", "净辐射");
                PubField.ClimateType.Add("19", "So2");
                PubField.ClimateType.Add("99", "液位");
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 程序关闭时
        /// </summary>
        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {

        }



        Point mPoint;
        private void panelEx1_MouseDown(object sender, MouseEventArgs e)
        {
            mPoint = new Point(e.X, e.Y);
        }

        private void panelEx1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                this.Location = new Point(this.Location.X + e.X - mPoint.X, this.Location.Y + e.Y - mPoint.Y);
            }
        }

        private void BtnQuit_Click(object sender, EventArgs e)
        {
            Dispose();
            this.Close();
            //System.Diagnostics.Process.GetCurrentProcess().Kill();
            System.Environment.Exit(0);
        }


        private void timer2_Tick(object sender, EventArgs e)
        {
            if (Interlocked.Exchange(ref inTimer2, 1) == 0)
            {
                //刷新时间展示
                this.LabTimeShow.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                string currDate = DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                //程序定点重启一次
                string restDate = "00:00:00";
                if (restDate == currDate)
                {
                    DebOutPut.DebLog("程序重启！");
                    DebOutPut.WriteLog(LogType.Normal, "程序重启！");
                    Tools.RestStart();
                }
                Interlocked.Exchange(ref inTimer2,0);
            }
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("http://www.zzokq.com/");
            }
            catch (Exception)
            {
                Process.Start("iexplore.exe", "http://www.zzokq.com/");
            }

        }


        #region 控件等比例缩放
        private float x;//定义当前窗体的宽度
        private float y;//定义当前窗体的高度
        private void SetTag(Control cons)
        {
            try
            {
                foreach (Control con in cons.Controls)
                {
                    con.Tag = con.Width + ";" + con.Height + ";" + con.Left + ";" + con.Top + ";" + con.Font.Size;
                    if (con.Controls.Count > 0)
                    {
                        SetTag(con);
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void SetControls(float newx, float newy, Control cons)
        {
            try
            {
                //遍历窗体中的控件，重新设置控件的值
                foreach (Control con in cons.Controls)
                {
                    //获取控件的Tag属性值，并分割后存储字符串数组
                    if (con.Tag != null)
                    {
                        string[] mytag = con.Tag.ToString().Split(new char[] { ';' });
                        //根据窗体缩放的比例确定控件的值
                        con.Width = Convert.ToInt32(System.Convert.ToSingle(mytag[0]) * newx);//宽度
                        con.Height = Convert.ToInt32(System.Convert.ToSingle(mytag[1]) * newy);//高度
                        con.Left = Convert.ToInt32(System.Convert.ToSingle(mytag[2]) * newx);//左边距
                        con.Top = Convert.ToInt32(System.Convert.ToSingle(mytag[3]) * newy);//顶边距
                        Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                        con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                        Application.DoEvents();
                        if (con.Controls.Count > 0)
                        {
                            SetControls(newx, newy, con);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        bool isChangeSize = true;
        private void Main_Shown(object sender, EventArgs e)
        {
            isChangeSize = false;

        }
        private void Main_Resize(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("是否执行SizeChanged----------->" + isChangeSize);
                if (isChangeSize)
                {
                    float newx = (this.Width) / x;
                    float newy = (this.Height) / y;
                    SetControls(newx, newy, this);
                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }




        #endregion

        private void panelEx1_Click(object sender, EventArgs e)
        {

        }

        #region 检测更新

        /// <summary>
        /// 检测更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdataSetup_T()
        {
            //程序刚开机时自动检测一次
            VersionCheck();
            string date = DateTime.Parse("05:00:00").ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            string date1 = DateTime.Parse("22:00:00").ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            string date2 = DateTime.Parse("12:00:00").ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            while (true)
            {
                try
                {
                    if (date == DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo) || date1 == DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo) || date2 == DateTime.Now.ToString("HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo))
                    {
                        DebOutPut.DebLog("版本检测");
                        DebOutPut.WriteLog(LogType.Error, "版本检测");
                        //版本检测
                        VersionCheck();
                    }
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    DebOutPut.DebErr(ex.ToString());
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                }
            }
        }
        static readonly object UpdateLock = new object();
        //最新版本
        NewVersion newVersion = null;
        int timeDown;
        /// <summary>
        /// 版本检测
        /// </summary>
        private void VersionCheck()
        {
            lock (UpdateLock)
            {
                try
                {
                    //int a1 = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Major;//主版本号
                    //int a3 = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Minor;//次版本号
                    //int a5 = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Build;//生成版本号
                    //int a6 = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Revision;//修正版本号
                    //表名
                    string tableName = "DeviceController";
                    string channelName = "设备房一代";
                    //当前程序集版本号
                    string currVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.Revision.ToString();
                    //新版本
                    newVersion = new NewVersion();
                    //查找
                    BmobWindows bmob = new BmobWindows();
                    bmob.initialize("5c31b12cb34012459fc94587a95521f1", "19fe4a17fe69a82c4b41b181c06fe93f");
                    BmobDebug.Register(msg => { Debug.WriteLine(msg); });
                    BmobQuery query = new BmobQuery();
                    query.WhereEqualTo("channel", channelName);
                    //**解决Win7中问题：cn.bmob.exception.BmobException:基础连接已关闭：无法为SSL/TLS安全通道建立信任关系，响应内容为--->System.Net.WebException:基础连接已关闭：无法为SSL/TLS安全通道建立信任关系。
                    ServicePointManager.ServerCertificateValidationCallback = Callback;
                    bmob.Find<NewVersion>(tableName, query, (resp, exception) =>
                    {
                        if (exception != null)
                        {
                            DebOutPut.DebLog("远程服务器证书无效:" + exception.ToString());
                            DebOutPut.WriteLog(LogType.Normal, exception.ToString());
                            timeDown = 15;
                            timer1.Start();
                            MessageBox.Show("更新失败，服务器证书无效！" + exception.ToString(), "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return;
                        }
                        List<NewVersion> list = resp.results;
                        for (int i = 0; i < list.Count; i++)
                        {
                            if (list[i].channel == channelName)
                            {
                                newVersion = list[i];
                            }
                        }
                        if (newVersion == null || int.Parse(newVersion.currVersion) == int.Parse(currVersion) || int.Parse(newVersion.currVersion) < int.Parse(currVersion))
                        {
                            timeDown = 15;
                            timer1.Start();
                            MessageBox.Show("已是最新版本！", "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                            return;
                        }
                        //强更新、弱更新判断
                        if (int.Parse(newVersion.currVersion) > int.Parse(currVersion))
                        {
                            if (newVersion.isForced.Get())
                            {
                                timeDown = 15;
                                timer1.Start();
                                DialogResult dialogResult = MessageBox.Show("软件有重大版本更新！点击“确定”开始下载更新！倒计时结束系统将自动完成本次更新！\r\n", "倒计时:（" + timeDown + "）", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                                Download();
                            }
                            else if (!newVersion.isForced.Get())
                            {
                                timeDown = 15;
                                timer1.Start();
                                DialogResult dialogResult = MessageBox.Show("软件已有新版本！点击“确定”开始下载更新,点击“取消”放弃本次更新！倒计时结束系统将视为放弃本次更新！\r\n", "倒计时:（" + timeDown + "）", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                                if (dialogResult == DialogResult.OK)
                                {
                                    Download();
                                }
                            }
                        }
                    });
                }
                catch (Exception ex)
                {
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                    DebOutPut.DebLog(ex.ToString());
                    timeDown = 15;
                    timer1.Start();
                    MessageBox.Show("更新失败！" + ex.ToString(), "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                }
            }
        }
        private static bool Callback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true;
        }

        /// <summary>
        /// 下载
        /// </summary>
        private void Download()
        {
            try
            {
                if (newVersion.exeUrl == null || newVersion.exeUrl == "")
                {
                    DebOutPut.DebLog("更新地址为空");
                    DebOutPut.WriteLog(LogType.Normal, "更新地址为空");
                    timeDown = 15;
                    timer1.Start();
                    MessageBox.Show("更新失败，更新地址为空！", "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return;
                }
                string address = newVersion.exeUrl;
                WebClient webClient = new WebClient();
                //下载安装包
                webClient.DownloadFile(address, newVersion.exeName);
                //静默安装
                Install();

            }
            catch (Exception ex)
            {
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                DebOutPut.DebLog(ex.ToString());
                timeDown = 15;
                timer1.Start();
                MessageBox.Show("更新失败！" + ex.ToString(), "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            }

        }

        /// <summary>
        /// 安装
        /// </summary>
        private void Install()
        {
            try
            {
                string path = Application.StartupPath + "\\" + newVersion.exeName;
                if (!File.Exists(path))
                {
                    DebOutPut.DebLog("安装包不存在：" + "安装包下载地址：" + newVersion.exeUrl);
                    DebOutPut.WriteLog(LogType.Normal, "安装包不存在：" + "安装包下载地址：" + newVersion.exeUrl);
                    timeDown = 15;
                    timer1.Start();
                    MessageBox.Show("安装包不存在！" + "安装包下载地址：" + newVersion.exeUrl, "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    return;
                }
                string Md5Hash = GetMD5HashFromFile(path);
                //效验MD5值
                if (Md5Hash.ToLower() == newVersion.md5.ToLower())
                {
                    IntPtr retint = Win32API.ShellExecute(IntPtr.Zero, "open", path, " /S", "", ShellExecute_ShowCommands.SW_SHOWNORMAL);//@/S  静默安装
                    Thread.Sleep(1000);
                    System.Environment.Exit(0);
                }
                else
                {
                    timeDown = 15;
                    timer1.Start();
                    MessageBox.Show("更新失败，该软件可能被劫持，请联系管理员确认！\r\n", "倒计时:（" + timeDown + "）", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1);
                    DebOutPut.DebLog("更新失败，请检查网络连接！");
                }
            }
            catch (Exception ex)
            {
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                DebOutPut.DebLog(ex.ToString());
                timeDown = 15;
                timer1.Start();
                MessageBox.Show("更新失败！" + ex.ToString(), "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            }
        }

        /// <summary>
        /// 获取文件的MD5码
        /// </summary>
        /// <param name="fileName">传入的文件名（含路径及后缀名）</param>
        /// <returns></returns>
        private string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream file = new FileStream(fileName, System.IO.FileMode.Open);
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] retVal = md5.ComputeHash(file);
                file.Close();
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < retVal.Length; i++)
                {
                    sb.Append(retVal[i].ToString("x2"));
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                timeDown = 15;
                timer1.Start();
                MessageBox.Show("MD5校验失败！" + ex.ToString() + " 安装包下载地址：" + newVersion.exeUrl, "倒计时:（" + timeDown + "）", MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                return "";
            }
        }

        /// <summary>
        /// MESSAGEBOX窗口定时关闭
        /// </summary>
        private void timer1_Tick(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.Exchange(ref inTimer1, 1) == 0)
            {
                KillMessageBox(sender);
                Interlocked.Exchange(ref inTimer1, 0);
            }
        }
        private void KillMessageBox(object sender)
        {
            try
            {
                //按照MessageBox的标题，找到MessageBox的窗口 
                IntPtr ptr = (IntPtr)Win32API.FindWindow(null, "倒计时:（" + timeDown + "）");
                timeDown--;
                Win32API.SetWindowText(ptr, "倒计时:（" + timeDown + "）");
                if (timeDown == 0)
                {
                    if (ptr != IntPtr.Zero)
                    {
                        //找到则关闭MessageBox窗口
                        Win32API.PostMessage(ptr, Win32API.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        ((System.Timers.Timer)sender).Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        #endregion

        private void label27_DoubleClick(object sender, EventArgs e)
        {
            if (this.label27.Text.Trim() == "发布版")
            {
                this.label27.Text = "测试版";
                this.label27.ForeColor = System.Drawing.Color.Red;
                DebOutPut.isDebView = true;
            }
            else if (this.label27.Text.Trim() == "测试版")
            {
                this.label27.Text = "发布版";
                this.label27.ForeColor = System.Drawing.Color.White;
                DebOutPut.isDebView = false;
            }

        }

        private void label27_Click(object sender, EventArgs e)
        {

        }

        private void labelX2_Click(object sender, EventArgs e)
        {

        }

        private void labelX3_Click(object sender, EventArgs e)
        {

        }
    }
}
