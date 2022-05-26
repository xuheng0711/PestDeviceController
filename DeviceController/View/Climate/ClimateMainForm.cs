using BZ10.Common;
using DevComponents.DotNetBar;
using DeviceController.Common;
using DeviceController.DAL.Climate;
using DeviceController.DAL.DataUpload;
using DeviceController.Models;
using DeviceController.View.DevOverview;
using LitJson;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;

namespace DeviceController.View.Climate
{
    public delegate void ClimateInit();
    public delegate void ClimateUpdataDataList();
    public delegate void Climate_ReceivedDataHandle(string recStr);
    public partial class ClimateMainForm : Form
    {
        public static ClimateInit climateInit;
        public static ClimateUpdataDataList climateUpdataDataList;
        public static Climate_ReceivedDataHandle climate_ReceivedDataHandle;
        ScoketClient client;
        private bool isOpen = false;//串口是否打开
        public static int whichAgreement = 0;//判断走哪个协议格式
        public static string agreement9Add = "";//协议9的寄存器地址
        public static DateTime newDataTime = DateTime.Parse("1970-01-01 00:00:00");
        //采集到的数据 数据模型
        public static ClimateModel climateModel = null;
        public static ClimateMessage climateMessage = null;
        public static ClimateKeepLive keepLive = null;

        //自动采集数据
        int inTimer1 = 0;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        //自动上传数据
        int inTimer2 = 0;
        System.Timers.Timer timer2 = new System.Timers.Timer();
        //发送心跳
        int inTimer3 = 0;
        System.Timers.Timer timer3 = new System.Timers.Timer();
        //接收数据
        int inTimer4 = 0;
        System.Timers.Timer timer4 = new System.Timers.Timer();

        public ClimateMainForm()
        {
            InitializeComponent();
            climateInit = Init;
            climateUpdataDataList = UpdataDataShowList;
            climate_ReceivedDataHandle = ReceivedDataHandle;
            label4.ForeColor = System.Drawing.Color.Green;
            x = this.Width;
            y = this.Height;
            SetTag(this);
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
        /// 窗体加载
        /// </summary>
        private void ClimateMainForm_Load(object sender, EventArgs e)
        {
            //读取配置文件
            ReadConfigFile();
            //自动采集信息
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
            timer1.Interval = int.Parse(SaveDataModel.climateConfigModel.collectionInterval) * 1000 * 60;
            //自动上传数据
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);
            timer2.Interval = int.Parse(SaveDataModel.climateConfigModel.uploadInterval) * 1000 * 60;
            //心跳
            timer3.Elapsed += new ElapsedEventHandler(timer3_Elapsed);
            timer3.Interval = 30 * 1000;
            //接收数据
            timer4.Elapsed += new ElapsedEventHandler(timer4_Elapsed);
            timer4.Interval = 200;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            try
            {
                System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;

                if (DB_Climate.DBInit())
                {
                    //打开串口并创建长连接
                    CreateSerialPort();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
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
                        if (con.Name == "flowLayoutPanel1")
                        {
                            con.Left = (con.Parent.Width - con.Width) / 2;
                        }
                        else
                        {
                            //根据窗体缩放的比例确定控件的值
                            con.Width = Convert.ToInt32(System.Convert.ToSingle(mytag[0]) * newx);//宽度
                            con.Height = Convert.ToInt32(System.Convert.ToSingle(mytag[1]) * newy);//高度
                            con.Left = Convert.ToInt32(System.Convert.ToSingle(mytag[2]) * newx);//左边距
                            con.Top = Convert.ToInt32(System.Convert.ToSingle(mytag[3]) * newy);//顶边距
                            Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                            con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                        }
                        Application.DoEvents();
                        if (con.Controls.Count > 0)
                            SetControls(newx, newy, con);
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
        private void ClimateMainForm_Shown(object sender, EventArgs e)
        {
            isChangeSize = false;
        }

        private void ClimateMainForm_Resize(object sender, EventArgs e)
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

        /// <summary>
        /// 读取配置文件
        /// </summary>
        private void ReadConfigFile()
        {
            try
            {
                string climateConfigSavePath = Path.Combine(PubField.basePath, "ClimateConfig");
                climateConfigSavePath = Path.Combine(climateConfigSavePath, PubField.climateConfigName);

                SaveDataModel.climateConfigModel.collectionInterval = Tools.ConfigParmRead("Basic Parameters", "CollectionInterval", climateConfigSavePath);//采集间隔
                SaveDataModel.climateConfigModel.uploadInterval = Tools.ConfigParmRead("Basic Parameters", "UploadInterval", climateConfigSavePath);//上传间隔
                SaveDataModel.climateConfigModel.uploadAddress = Tools.ConfigParmRead("Basic Parameters", "UploadAddress", climateConfigSavePath);//上传地址
                SaveDataModel.climateConfigModel.uploadPort = Tools.ConfigParmRead("Basic Parameters", "UploadPort", climateConfigSavePath);//上传端口
                SaveDataModel.climateConfigModel.siteName = Tools.ConfigParmRead("Basic Parameters", "SiteName", climateConfigSavePath);//站点名称
                SaveDataModel.climateConfigModel.modifyTime = Tools.ConfigParmRead("Basic Parameters", "ModifyTime", climateConfigSavePath);//修改时间
                SaveDataModel.climateConfigModel.alarmNum = Tools.ConfigParmRead("Basic Parameters", "AlarmNumber", climateConfigSavePath);//报警号码
                SaveDataModel.climateConfigModel.alarmInterval = Tools.ConfigParmRead("Basic Parameters", "AlarmInterval", climateConfigSavePath);//报警间隔
                SaveDataModel.climateConfigModel.serialPortName = Tools.ConfigParmRead("Basic Parameters", "SerialPortName", climateConfigSavePath);//串口号

                SaveDataModel.climateConfigModel.devId = Tools.ConfigParmRead("Basic Parameters", "DevId", climateConfigSavePath);//设备ID

                this.TxtCollectionInterval.Text = SaveDataModel.climateConfigModel.collectionInterval;
                this.TxtUploadInterval.Text = SaveDataModel.climateConfigModel.uploadInterval;
                this.TxtUploadAddress.Text = SaveDataModel.climateConfigModel.uploadAddress;
                this.TxtUploadPort.Text = SaveDataModel.climateConfigModel.uploadPort;
                this.TxtSiteName.Text = SaveDataModel.climateConfigModel.siteName;
                this.TxtAlarmNumber.Text = SaveDataModel.climateConfigModel.alarmNum;
                this.TxtAlarmInterval.Text = SaveDataModel.climateConfigModel.alarmInterval;
                this.textBox1.Text = SaveDataModel.climateConfigModel.devId;
                ParamSetEn(false);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 创建串口和Scoket连接
        /// </summary>
        /// <returns></returns>
        private void CreateSerialPort()
        {
            try
            {
                //打开串口
                isOpen = SerialPortCtrl.SetSerialPortAttribute(serialPort1, SaveDataModel.climateConfigModel.serialPortName, 9600);
                if (isOpen)
                {
                    if (!PubField.devRunName.Contains("气象"))
                    {
                        PubField.devRunName.Add("气象");
                        DevOverviewMain.devRunNetCountUpdata();
                    }
                    DebOutPut.DebLog("小气候串口打开成功！");
                    DebOutPut.WriteLog(LogType.Normal, "小气候串口打开成功！");
                    timer1.Start();
                }
                //创建长连接
                CreateScoketConnect();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        /// <summary>
        /// 系统设置
        /// </summary>
        private void BtnSystemSet_Click(object sender, EventArgs e)
        {
            try
            {
                label4.ForeColor = System.Drawing.Color.White;
                label5.ForeColor = System.Drawing.Color.Green;
                this.tabControl1.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 数据查看
        /// </summary>
        private void BtnLookData_Click_1(object sender, EventArgs e)
        {
            try
            {
                UpdataDataShowList();
                label4.ForeColor = System.Drawing.Color.Green;
                label5.ForeColor = System.Drawing.Color.White;
                this.tabControl1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        /// <summary>
        /// 自动采集信息
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (isOpen)
            {
                try
                {
                    if (Interlocked.Exchange(ref inTimer1, 1) == 0)
                    {
                        //采集信息
                        climateModel = new ClimateModel();
                        climateModel.func = 101;
                        climateModel.err = "";
                        climateModel.devId = SaveDataModel.climateConfigModel.devId;
                        climateModel.devtype = 1;
                        climateMessage = new ClimateMessage();
                        climateMessage.collectTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        DebOutPut.DebLog("采集信息");
                        ClimateCollectionInfo.GetCollertionInfo(serialPort1);
                        if (climateModel == null || climateModel.message == null || climateMessage == null)
                            return;
                        climateModel.message = climateMessage;
                        if (climateModel != null)
                        {
                            DetectMemory();
                            string JsonStr = JsonMapper.ToJson(climateModel);
                            //入库
                            String sql = "insert into Record (Flag,CollectTime,Data) values ('0','" + climateModel.message.collectTime + "',+'" + JsonStr + "')";
                            if (DB_Climate.updateDatabase(sql) != 1)
                                DebOutPut.DebLog("小气候采集时间为：" + climateModel.message.collectTime + "  数据插入数据库失败");
                            else
                                DebOutPut.DebLog("小气候采集时间为：" + climateModel.message.collectTime + "  数据插入数据库成功");
                            climateMessage = null;
                            climateModel = null;
                        }
                        DevOverviewMain.climateUpdataShow();
                        Interlocked.Exchange(ref inTimer1, 0);
                    }
                }
                catch (Exception ex)
                {
                    DebOutPut.DebErr(ex.ToString());
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                    Interlocked.Exchange(ref inTimer1, 0);
                }
            }

        }

        /// <summary>
        /// 自动上传数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer2, 1) == 0)
                {
                    if (client != null && client.clientSocket != null && client.clientSocket.Connected && newDataTime.AddMinutes(3) > DateTime.Now)
                    {
                        DebOutPut.DebLog("小气候检测是否有未发送的数据");
                        string sql = "select * from Record where Flag='0'";
                        DataTable UploadTable = DB_Climate.QueryDatabase(sql).Tables[0];
                        DebOutPut.DebLog("未传共  " + UploadTable.Rows.Count + "  个");
                        for (int i = 0; i < UploadTable.Rows.Count; i++)
                        {
                            if (client != null && client.clientSocket != null && client.clientSocket.Connected && newDataTime.AddMinutes(3) > DateTime.Now)
                            {
                                client.SendData(UploadTable.Rows[i]["Data"] + "");//上传数据
                                DebOutPut.DebLog("当前发送第  " + (i + 1) + "  个，数据为:  " + UploadTable.Rows[i]["Data"].ToString());
                                Thread.Sleep(2000);
                            }
                        }
                        UploadTable.Dispose();
                    }
                    Interlocked.Exchange(ref inTimer2, 0);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (client != null && client.clientSocket != null && client.clientSocket.Connected)
                {
                    DebOutPut.DebLog("上传异常，断开连接");
                    DebOutPut.WriteLog(LogType.Error, "上传异常，断开连接");
                    client.SocketClose();
                }
                Interlocked.Exchange(ref inTimer2, 0);
            }
        }

        /// <summary>
        /// 检测内存
        /// </summary>
        private void DetectMemory()
        {
            try
            {
                float remainingDiskSpace = GetHardDiskSpace(PubField.basePath.Substring(0, PubField.basePath.IndexOf(':')));
                DebOutPut.DebLog("程序运行磁盘剩余空间：" + remainingDiskSpace + " GB");
                DebOutPut.WriteLog(LogType.Normal, "程序运行磁盘剩余空间：" + remainingDiskSpace + " GB");
                if (remainingDiskSpace < 1f)
                {
                    string sql = "Delete * FROM Record";
                    int ret = DB_Climate.updateDatabase(sql);
                    if (ret == -1)
                    {
                        DebOutPut.DebLog("程序运行磁盘剩余空间不足，清空数据失败！");
                        DebOutPut.WriteLog(LogType.Normal, "程序运行磁盘剩余空间不足，清空数据失败！");
                    }
                    else
                    {
                        float remainingDiskSpace_ = GetHardDiskSpace(PubField.basePath.Substring(0, PubField.basePath.IndexOf(':')));
                        DebOutPut.DebLog("程序运行磁盘剩余空间不足，清空数据成功！目前剩余存储空间：" + remainingDiskSpace_.ToString("F4") + " GB");
                        DebOutPut.WriteLog(LogType.Normal, "程序运行磁盘剩余空间不足，清空数据成功！目前剩余存储空间：" + remainingDiskSpace_.ToString("F4") + " GB");
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 获取程序运行所在磁盘，可使用存储空间，单位GB
        /// </summary>
        /// <param name="str_HardDiskName"></param>
        /// <returns></returns>
        public float GetHardDiskSpace(string str_HardDiskName)
        {
            try
            {
                float totalSize = 0;
                str_HardDiskName = str_HardDiskName + ":\\";
                System.IO.DriveInfo[] drives = System.IO.DriveInfo.GetDrives();
                foreach (System.IO.DriveInfo drive in drives)
                    if (drive.Name == str_HardDiskName)
                        totalSize = (float)drive.TotalFreeSpace / 1073741824;
                return totalSize;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return -1;
            }
        }

        int inReceivedData = 0;
        /// <summary>
        /// 收到数据处理--不同的协议格式，数据处理方式不同
        /// </summary>
        /// <param name="msg">收到的数据</param>
        private void ReceivedDataHandle(string recStr)
        {
            try
            {
                if (Interlocked.Exchange(ref inReceivedData, 1) == 0)
                {
                    DebOutPut.DebLog("协议:" + whichAgreement + "  收到:" + Regex.Replace(recStr, @"(\w{2})", "$1 ").Trim(' ').ToUpper());
                    DebOutPut.WriteLog(LogType.Normal, "协议:" + whichAgreement + "  收到:" + Regex.Replace(recStr, @"(\w{2})", "$1 ").Trim(' ').ToUpper());
                    if (recStr.Length == 0)
                    {
                        return;
                    }
                    //int a = whichAgreement;
                    switch (whichAgreement)
                    {
                        case 1:
                            //string valueType1 = recStr.Substring(4, 2);//值类型
                            //string content = recStr.Substring(6, 4);
                            //string content10 = Tools.SixteenToTen(content);
                            //string content2 = Convert.ToString(int.Parse(content10), 2);
                            //string value1 = "";//测试值
                            //byte[] readData = Tools.HexStrTobyte(recStr);
                            //int Current_Zhi = -1;
                            //if ((readData[0] == (byte)0xAA) && (readData[1] == 0x64))
                            //{
                            //    if (readData[3]>=0 || readData[2] == 0x04)
                            //    {
                            //        int i3 = readData[3] << 8 & 0xFF00;
                            //        int i4 = readData[4] & 0xFF;
                            //        Current_Zhi = (i3 | i4);
                            //        if (readData[2] == 0x04 && Current_Zhi >= 0x8000)
                            //            Current_Zhi = (Current_Zhi - 0x8000) * 0x10;
                            //    }
                            //    else
                            //    {
                            //        readData[3] = (byte)((byte)readData[3] ^ 0xFFFFFFFF);
                            //        readData[4] = (byte)((byte)readData[4] ^ 0xFFFFFFFF);
                            //        int i3 = (readData[3] << 8) & 0xFF00;
                            //        int i4 = readData[4] & 0xFF;
                            //        Current_Zhi = -((i3 | i4));
                            //    }
                            //}
                            //value1 = Current_Zhi.ToString();
                            //协议1 以下计算方式也可以用
                            string value1 = "";
                            string valueType1 = recStr.Substring(4, 2);//值类型
                            if (valueType1 == "04")
                            {
                                int Current_Zhi = -1;
                                byte[] readData = Tools.HexStrTobyte(recStr);
                                int i3 = readData[3] << 8 & 0xFF00;
                                int i4 = readData[4] & 0xFF;
                                Current_Zhi = (i3 | i4);
                                if (readData[2] == 0x04 && Current_Zhi >= 0x8000)
                                    Current_Zhi = (Current_Zhi - 0x8000) * 0x10;
                                value1 = Current_Zhi.ToString();
                            }
                            else
                                value1 = Tools.NegComCodeToHex(recStr.Substring(6, 4)).ToString();
                            string valueName1 = "";//值名称
                            PubField.ClimateType.TryGetValue(valueType1.ToUpper(), out valueName1);
                            if (valueName1 == null || valueName1 == "")
                                return;
                            float Slope1 = ClimateCollectionInfo.ValueTypeHandle(valueType1.ToUpper());//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem1 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem1.name = valueName1;
                            climateEnvironmentsItem1.value = (float.Parse(value1) * Slope1).ToString();
                            DebOutPut.DebLog("接收：" + climateEnvironmentsItem1.name);

                            DebOutPut.DebLog("接收：" + climateEnvironmentsItem1.value);
                            DebOutPut.WriteLog(LogType.Normal, "接收：" + climateEnvironmentsItem1.name + ":" + climateEnvironmentsItem1.value);
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem1 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem1);
                            break;
                        case 3://协议3
                            string valueType2 = recStr.Substring(0, 2);//值类型
                            string valueName2 = "";//值名称
                            PubField.ClimateType.TryGetValue(valueType2.ToUpper(), out valueName2);
                            if (valueName2 == null || valueName2 == "")
                                return;
                            string dataByteCount = Tools.SixteenToTen(recStr.Substring(4, 2));//读取字节数
                            string value2 = Tools.NegComCodeToHex(recStr.Substring(6, int.Parse(dataByteCount) * 2));
                            float Slope2 = ClimateCollectionInfo.ValueTypeHandle(valueType2.ToUpper());//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem2 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem2.name = valueName2;
                            climateEnvironmentsItem2.value = (float.Parse(value2) * Slope2).ToString();
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem2 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem2);
                            break;
                        case 4://协议4
                            string valueType4 = recStr.Substring(0, 2);//值类型
                            string valueName4 = "";//值名称
                            PubField.ClimateType.TryGetValue(valueType4.ToUpper(), out valueName4);
                            if (valueName4 == null || valueName4 == "")
                                return;
                            string dataByteCount4 = Tools.SixteenToTen(recStr.Substring(4, 2));//读取字节数
                            string value4 = Tools.NegComCodeToHex(recStr.Substring(6, int.Parse(dataByteCount4)));
                            float Slope4 = ClimateCollectionInfo.ValueTypeHandle(valueType4.ToUpper());//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem4 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem4.name = valueName4;
                            climateEnvironmentsItem4.value = (float.Parse(value4) * Slope4).ToString();
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem4 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem4);
                            break;
                        case 5://协议5
                            string valueType5 = recStr.Substring(0, 2);//值类型
                            string valueName5 = "";//值名称
                            PubField.ClimateType.TryGetValue(valueType5.ToUpper(), out valueName5);
                            if (valueName5 == null || valueName5 == "")
                                return;
                            string dataByteCount5 = Tools.SixteenToTen(recStr.Substring(4, 2));//读取字节数
                            string value5 = Tools.NegComCodeToHex(recStr.Substring(6, int.Parse(dataByteCount5) * 2));
                            float Slope5 = ClimateCollectionInfo.ValueTypeHandle(valueType5.ToUpper());//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem5 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem5.name = valueName5;
                            climateEnvironmentsItem5.value = (float.Parse(value5) * Slope5).ToString();
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem5 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem5);
                            break;
                        case 6://协议6
                            string valueType6 = recStr.Substring(0, 2);//值类型
                            string valueName6 = "";//值名称
                            PubField.ClimateType.TryGetValue(valueType6.ToUpper(), out valueName6);
                            if (valueName6 == null || valueName6 == "")
                                return;
                            string dataByteCount6 = Tools.SixteenToTen(recStr.Substring(4, 2));//读取字节数
                            string value6 = Tools.NegComCodeToHex(recStr.Substring(6, int.Parse(dataByteCount6) * 2));
                            float Slope6 = ClimateCollectionInfo.ValueTypeHandle(valueType6.ToUpper());//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem6 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem6.name = valueName6;
                            climateEnvironmentsItem6.value = (float.Parse(value6) * Slope6).ToString();
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem6 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem6);
                            break;
                        case 7://协议7
                            string valueType7 = recStr.Substring(0, 2);//值类型
                            string valueName7 = "";//值名称
                            PubField.ClimateType.TryGetValue(valueType7.ToUpper(), out valueName7);
                            if (valueName7 == null || valueName7 == "")
                                return;
                            string dataByteCount7 = Tools.SixteenToTen(recStr.Substring(4, 2));//读取字节数
                            string value7 = Tools.NegComCodeToHex(recStr.Substring(6, int.Parse(dataByteCount7) * 2));
                            float Slope7 = ClimateCollectionInfo.ValueTypeHandle(valueType7.ToUpper());//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem7 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem7.name = valueName7;
                            climateEnvironmentsItem7.value = (float.Parse(value7) * Slope7).ToString();
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem7 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem7);
                            break;
                        case 8://协议8
                            string valueType8 = recStr.Substring(0, 2);//值类型
                            string valueName8 = "";//值名称
                            PubField.ClimateType.TryGetValue(valueType8.ToUpper(), out valueName8);
                            if (valueName8 == null || valueName8 == "")
                                return;
                            string value8 = Tools.NegComCodeToHex(recStr.Substring(6, 4));
                            float Slope8 = ClimateCollectionInfo.ValueTypeHandle(valueType8.ToUpper());//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem8 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem8.name = valueName8;
                            climateEnvironmentsItem8.value = (float.Parse(value8) * Slope8).ToString();
                            if (climateEnvironmentsItem8.name == "风速" && float.Parse(climateEnvironmentsItem8.value) > 5)
                                climateEnvironmentsItem8.value = (float.Parse(climateEnvironmentsItem8.value) / 20).ToString("F2");
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem8 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem8);
                            break;
                        case 9://协议9
                            string valueName9 = "";
                            float xielv = 1;
                            switch (agreement9Add)
                            {
                                case "01"://温度
                                    valueName9 = "温度";
                                    xielv = 0.01f;
                                    break;
                                case "02"://湿度
                                    valueName9 = "湿度";
                                    xielv = 0.01f;
                                    break;
                                case "05"://光照度
                                    valueName9 = "光照度";
                                    xielv = 1f;
                                    break;
                                case "04"://大气压
                                    valueName9 = "大气压";
                                    xielv = 0.01f;
                                    break;
                                default:
                                    break;
                            }
                            if (valueName9 == null || valueName9 == "")
                                return;
                            string dataByteCount9 = Tools.SixteenToTen(recStr.Substring(4, 2));//读取字节数
                            string value9 = "";
                            value9 = Tools.NegComCodeToHex(recStr.Substring(6, int.Parse(dataByteCount9) * 2));
                            float Slope9 = xielv;//斜率
                            ClimateEnvironmentsItem climateEnvironmentsItem9 = new ClimateEnvironmentsItem();
                            climateEnvironmentsItem9.name = valueName9;
                            climateEnvironmentsItem9.value = (float.Parse(value9) * Slope9).ToString();
                            if (climateMessage == null || climateMessage.environments == null || climateEnvironmentsItem9 == null)
                                return;
                            climateMessage.environments.Add(climateEnvironmentsItem9);
                            break;
                        case 10://协议10
                            break;
                        default:
                            break;
                    }
                }
                Interlocked.Exchange(ref inReceivedData, 0);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                Interlocked.Exchange(ref inReceivedData, 0);
            }

        }

        /// <summary>
        /// 创建长连接
        /// </summary>
        private void CreateScoketConnect()
        {
            try
            {
                string serverIp = SaveDataModel.climateConfigModel.uploadAddress;
                string serverPort = SaveDataModel.climateConfigModel.uploadPort;
                if (client != null && client.clientSocket != null && client.clientSocket.Connected)
                {
                    DebOutPut.DebLog("客户端未连接，采集数据无法上传，请关闭串口，确认参数设置无误后，重新打开!");
                    DebOutPut.WriteLog(LogType.Error, "客户端未连接，采集数据无法上传，请关闭串口，确认参数设置无误后，重新打开!");
                    return;
                }
                else if (!Tools.IsItAIP(serverIp))
                {
                    serverIp = Tools.GetIP(serverIp);
                }
                if (serverIp == "" || !Tools.IsItAIP(serverIp))
                {
                    DebOutPut.DebLog("域名/IP:" + serverIp + " 无效，请关闭串口，确认参数设置无误后，重新打开!");
                    DebOutPut.WriteLog(LogType.Error, "域名/IP:" + serverIp + " 无效，请关闭串口，确认参数设置无误后，重新打开!");
                    return;
                }
                client = new ScoketClient(serverIp, serverPort);
                client.SocketConnect();
                if (client.clientSocket.Connected)
                {
                    newDataTime = DateTime.Now;
                    if (!PubField.devNetName.Contains("气象"))
                    {
                        PubField.devNetName.Add("气象");
                        DevOverviewMain.devRunNetCountUpdata();
                    }
                    DebOutPut.DebLog("小气候客户端连接成功!");
                    timer2.Start();
                    timer3.Start();
                    timer4.Start();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        bool isConnect = true;
        /// <summary>
        /// 心跳保持
        /// </summary>
        private void timer3_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer3, 1) == 0)
                {
                    if (client == null || client.clientSocket == null || !client.clientSocket.Connected || newDataTime.AddMinutes(3) < DateTime.Now)
                    {
                        if (PubField.devNetName.Contains("气象"))
                        {
                            PubField.devNetName.Remove("气象");
                            DevOverviewMain.devRunNetCountUpdata();
                        }
                        if (isConnect)
                        {
                            client.SocketClose();
                            DebOutPut.DebLog("连接已断开，正在重新连接...");
                            DebOutPut.WriteLog(LogType.Normal, "连接已断开，正在重新连接...");
                            Thread.Sleep(1000 * 60);
                            CreateScoketConnect();
                        }
                    }
                    else
                    {
                        //保活包体
                        keepLive = new ClimateKeepLive();
                        keepLive.message = "keep-alive";//数据
                        keepLive.devId = SaveDataModel.climateConfigModel == null ? "" : SaveDataModel.climateConfigModel.devId;//设备id
                        keepLive.func = 100;//功能码
                        keepLive.err = "";//错误
                        string data = Tools.ObjectToJson(ClimateMainForm.keepLive);
                        client.SendData(data);
                        keepLive = null;
                    }
                    Interlocked.Exchange(ref inTimer3, 0);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (client != null && client.clientSocket != null && client.clientSocket.Connected)
                {
                    DebOutPut.DebLog("心跳异常，断开连接");
                    DebOutPut.WriteLog(LogType.Error, "心跳异常，断开连接");
                    client.SocketClose();
                }
                Interlocked.Exchange(ref inTimer3, 0);
            }

        }


        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timer4_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer4, 1) == 0)
                {
                    if (client.clientSocket != null && client.clientSocket.Connected)
                    {
                        newDataTime = DateTime.Now;
                        byte[] receive = new byte[1024];
                        client.clientSocket.Receive(receive);
                        string receiceMsg = Encoding.Default.GetString(receive);
                        receiceMsg = receiceMsg.Substring(0, receiceMsg.LastIndexOf("}") + 1);
                        DebOutPut.DebLog("Climate:收到数据:" + receiceMsg);
                        JObject obj = JObject.Parse(receiceMsg);
                        string func = obj["func"] + "";
                        string devId = obj["devId"] + "";

                        if (devId == SaveDataModel.climateConfigModel.devId)
                        {
                            if (func == "101")
                            {
                                string collectTime = obj["collectTime"] + "";
                                string sql = "update Record Set Flag = '1' where CollectTime='" + collectTime + "'";
                                DB_Climate.updateDatabase(sql);
                            }
                        }
                    }
                    Interlocked.Exchange(ref inTimer4, 0);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (client.clientSocket != null && client.clientSocket.Connected)
                {
                    DebOutPut.DebLog("接收异常，断开连接");
                    client.SocketClose();
                }
                Interlocked.Exchange(ref inTimer4, 0);
            }

        }

        /// <summary>
        /// 窗口即将关闭时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClimateMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SerialPortScoketClose();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 关闭串口和Scoket
        /// </summary>
        private void SerialPortScoketClose()
        {
            try
            {
                if (client != null && client.clientSocket != null && client.clientSocket.Connected)
                {
                    isConnect = false;
                    client.SocketClose();
                    client = null;
                    timer2.Stop();
                    timer3.Stop();
                    timer4.Stop();
                    DebOutPut.DebLog("小气候连接已读断开!");
                }
                if (isOpen)
                {
                    if (SerialPortCtrl.CloseSerialPort(serialPort1))
                    {
                        isOpen = false;
                        timer1.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 参数设置确定
        /// </summary>
        private void BtnParamSet_Click(object sender, EventArgs e)
        {
            try
            {
                string climateConfigSavePath = Path.Combine(PubField.basePath, "ClimateConfig");
                if (!Directory.Exists(climateConfigSavePath))
                    Directory.CreateDirectory(climateConfigSavePath);
                climateConfigSavePath = Path.Combine(climateConfigSavePath, PubField.climateConfigName);

                if (SaveDataModel.climateConfigModel.collectionInterval != this.TxtCollectionInterval.Text.Trim() || SaveDataModel.climateConfigModel.uploadInterval != this.TxtUploadInterval.Text.Trim() || SaveDataModel.climateConfigModel.uploadAddress != this.TxtUploadAddress.Text.Trim() || SaveDataModel.climateConfigModel.uploadPort != this.TxtUploadPort.Text.Trim() || SaveDataModel.climateConfigModel.siteName != this.TxtSiteName.Text.Trim() || SaveDataModel.climateConfigModel.alarmNum != this.TxtAlarmNumber.Text.Trim() || SaveDataModel.climateConfigModel.alarmInterval != this.TxtAlarmInterval.Text.Trim() || SaveDataModel.climateConfigModel.devId != this.textBox1.Text.Trim())
                {
                    Tools.ConfigParmSet("Basic Parameters", "CollectionInterval", this.TxtCollectionInterval.Text.Trim(), climateConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "UploadInterval", this.TxtUploadInterval.Text.Trim(), climateConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "UploadAddress", this.TxtUploadAddress.Text.Trim(), climateConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "UploadPort", this.TxtUploadPort.Text.Trim(), climateConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "SiteName", this.TxtSiteName.Text.Trim(), climateConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "AlarmNumber", this.TxtAlarmNumber.Text.Trim(), climateConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "AlarmInterval", this.TxtAlarmInterval.Text.Trim(), climateConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "DevId", this.textBox1.Text.Trim(), climateConfigSavePath);
                    DialogResult dialogResult = MessageBox.Show("检测到您更改了系统关键性配置，将在系统重启之后生效。点击“确定”将立即重启本程序，点击“取消”请稍后手动重启！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    if (dialogResult == DialogResult.OK)
                    {
                        Tools.RestStart();
                    }
                }
                ParamSetEn(false);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 参数设置中控件是否可点击
        /// </summary>
        private void ParamSetEn(bool isEn)
        {
            try
            {
                this.textBox1.Enabled = isEn;
                this.TxtCollectionInterval.Enabled = isEn;
                this.TxtUploadInterval.Enabled = isEn;
                this.TxtUploadAddress.Enabled = isEn;
                this.TxtUploadPort.Enabled = isEn;
                this.TxtSiteName.Enabled = isEn;
                this.TxtAlarmNumber.Enabled = isEn;
                this.TxtAlarmInterval.Enabled = isEn;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 修改
        /// </summary>
        private void buttonX1_Click(object sender, EventArgs e)
        {
            try
            {
                ParamSetEn(true);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        private int currentPage = 1;//当前页
        private int pageCount = 1;//总页数
        private const int perPageCount = 4;//每页显示数量

        /// <summary>
        /// 刷新列表
        /// </summary>
        private void UpdataDataShowList()
        {
            try
            {
                Thread thread = new Thread(UpDataShow);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 刷新数据展示
        /// </summary>
        private void UpDataShow()
        {
            try
            {
                string sql = "select * from Record order by CollectTime desc ";
                DataTable dt = DB_Climate.QueryDatabase(sql).Tables[0];
                SaveDataModel.climateModelList.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ClimateModel myModel = JsonMapper.ToObject<ClimateModel>(dt.Rows[i]["Data"].ToString());
                    SaveDataModel.climateModelList.Add(myModel);
                }
                //SaveDataModel.climateModelList.Reverse();
                this.Invoke(new Action(() =>
                {
                    CreateListControl();
                }));
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        int countpage = 4;
        /// <summary>
        /// 锁
        /// </summary>
        int inDataList = 0;
        /// <summary>
        /// 生成
        /// </summary>
        public void CreateListControl()
        {

            try
            {
                if (Interlocked.Exchange(ref inDataList, 1) == 0)
                {
                    flowLayoutPanel1.Controls.Clear();
                    //Size panelSize = new Size(840, 142);
                    Size panelSize = new Size(flowLayoutPanel1.Size.Width - 20, flowLayoutPanel1.Size.Height / 5 + 15);
                    Color color = Color.FromArgb(166, 197, 254);
                    Font font = new Font("宋体", 12F);
                    if (SaveDataModel.climateModelList == null)
                    {
                        return;
                    }
                    UpdataPage(SaveDataModel.climateModelList);
                    for (int i = currentPage * countpage - countpage; i < (((SaveDataModel.climateModelList.Count - (currentPage * countpage - countpage)) > countpage) ? currentPage * countpage : SaveDataModel.climateModelList.Count); i++)
                    {
                        if (i > SaveDataModel.climateModelList.Count - 1 || i < 0)
                        {
                            return;
                        }

                        Panel panel = new Panel();
                        panel.Name = "FLPanel";
                        panel.Size = panelSize;
                        panel.BackColor = color;
                        panel.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);//设置边距
                        flowLayoutPanel1.Controls.Add(panel);
                        panel.Show();

                        LabelX serialLabel = new LabelX();//序号
                        serialLabel.Name = "LabSerialNumber";
                        serialLabel.Text = "采集时间";
                        serialLabel.Font = font;
                        serialLabel.Location = new Point(65, 38);
                        serialLabel.Size = new Size(120, 20);
                        serialLabel.BackColor = color;
                        serialLabel.AutoSize = false;
                        serialLabel.TextAlignment = StringAlignment.Center;
                        serialLabel.Parent = panel;

                        LabelX collectionTimeLabel = new LabelX();//采集时间
                        collectionTimeLabel.Name = "collectionTimeLabel";
                        collectionTimeLabel.Text = DateTime.Parse(SaveDataModel.climateModelList[i].message.collectTime).ToString("yyyy-MM-dd HH:mm:ss");
                        collectionTimeLabel.Font = font;
                        collectionTimeLabel.Location = new Point(20, 78);
                        collectionTimeLabel.Size = new Size(200, 20);
                        collectionTimeLabel.BackColor = color;
                        collectionTimeLabel.AutoSize = false;
                        collectionTimeLabel.TextAlignment = StringAlignment.Center;
                        collectionTimeLabel.Parent = panel;

                        ListView listDataShow = new ListView();
                        listDataShow.Font = font;
                        listDataShow.Size = new Size(550, 125);
                        listDataShow.Location = new Point(230, 15);
                        listDataShow.Parent = panel;
                        listDataShow.View = System.Windows.Forms.View.Details;//列表展示
                        listDataShow.HeaderStyle = ColumnHeaderStyle.None;//不显示标题
                        listDataShow.HideSelection = false;
                        listDataShow.BorderStyle = BorderStyle.None;
                        listDataShow.BackColor = Color.FromArgb(166, 197, 254);
                        ColumnHeader columnHeader = new ColumnHeader();
                        columnHeader.Text = "环境类型";
                        columnHeader.Width = 115;
                        ColumnHeader columnHeader1 = new ColumnHeader();
                        columnHeader1.Text = "环境值";
                        columnHeader1.Width = 115;
                        ColumnHeader columnHeader2 = new ColumnHeader();
                        columnHeader2.Text = "环境类型";
                        columnHeader2.Width = 115;
                        ColumnHeader columnHeader3 = new ColumnHeader();
                        columnHeader3.Text = "环境值";
                        columnHeader3.Width = 115;
                        listDataShow.Columns.Add(columnHeader);
                        listDataShow.Columns.Add(columnHeader1);
                        listDataShow.Columns.Add(columnHeader2);
                        listDataShow.Columns.Add(columnHeader3);
                        List<ClimateEnvironmentsItem> environments = SaveDataModel.climateModelList[i].message.environments;
                        for (int j = 0; j < environments.Count; j += 2)
                        {
                            ListViewItem listViewItem = new ListViewItem();
                            listViewItem.Text = environments[j].name;
                            string company = DevOverviewMain.GetCompany(environments[j].name);
                            ListViewSubItem listViewSubItem = new ListViewSubItem();
                            listViewSubItem.Text = environments[j].value + " " + company;
                            if (environments[j].name == "风向")
                            {
                                listViewSubItem.Text = Tools.WindDirectionSwitch(float.Parse(environments[j].value));
                            }

                            listViewItem.SubItems.Add(listViewSubItem);
                            if (j + 1 < environments.Count)
                            {
                                ListViewSubItem listViewSubItem1 = new ListViewSubItem();
                                listViewSubItem1.Text = environments[j + 1].name;
                                string company1 = DevOverviewMain.GetCompany(environments[j + 1].name);
                                ListViewSubItem listViewSubItem2 = new ListViewSubItem();
                                listViewSubItem2.Text = environments[j + 1].value + " " + company1;
                                if (environments[j + 1].name == "风向")
                                {
                                    listViewSubItem2.Text = Tools.WindDirectionSwitch(float.Parse(environments[j + 1].value));
                                }

                                listViewItem.SubItems.Add(listViewSubItem1);
                                listViewItem.SubItems.Add(listViewSubItem2);
                            }

                            listDataShow.Items.Add(listViewItem);
                        }
                    }
                    Interlocked.Exchange(ref inDataList, 0);

                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                Interlocked.Exchange(ref inDataList, 0);
            }
        }

        /// <summary>
        /// 刷新当前页和总页数
        /// </summary>
        /// <param name="climateModelList">数据链表</param>
        private void UpdataPage(List<ClimateModel> climateModelList)
        {
            try
            {
                if (climateModelList == null)
                {
                    return;
                }

                int recordnum = climateModelList.Count;
                if ((recordnum % perPageCount) == 0)
                {
                    pageCount = recordnum / perPageCount;
                }
                else
                {
                    pageCount = recordnum / perPageCount + 1;
                }
                this.PageInfo.Text = ((currentPage == 0) ? 1 : currentPage) + "/" + ((pageCount == 0) ? 0 : pageCount);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 删除全部
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                string str = "Delete * FROM Record";

                int ret = DB_Climate.updateDatabase(str);
                if (ret == -1)
                {
                    DebOutPut.DebLog("清空数据失败！");
                    DebOutPut.WriteLog(LogType.Normal, "清空数据失败！");
                    MessageBox.Show("清空数据失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                this.PageInfo.Text = 1 + "/" + 0;
                this.flowLayoutPanel1.Controls.Clear();
                SaveDataModel.climateModelList.Clear();
                currentPage = 1;
                pageCount = 0;
                DevOverviewMain.climateUpdataShow();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {

            try
            {
                if (pageCount > 1)
                {
                    currentPage = 1;
                    CreateListControl();

                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 尾页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                if (pageCount > 1)
                {
                    currentPage = pageCount;
                    CreateListControl();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void prepage_Click(object sender, EventArgs e)
        {

            try
            {
                if (currentPage > 1)
                {
                    currentPage -= 1;
                    CreateListControl();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextpage_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPage < pageCount)
                {
                    currentPage += 1;
                    CreateListControl();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void ClimateMainForm_SizeChanged(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// LED屏测试
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX2_Click(object sender, EventArgs e)
        {
            DevOverviewMain.climateLedText();
        }

        /// <summary>
        /// 数据上传
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX3_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(ManualSendData);
            thread.IsBackground = true;
            thread.Start();
        }
        /// <summary>
        /// 锁
        /// </summary>
        int inSendData = 0;

        private void ManualSendData()
        {
            try
            {
                if (Interlocked.Exchange(ref inSendData, 1) == 0)
                {
                    if (client != null && client.clientSocket != null && client.clientSocket.Connected && newDataTime.AddMinutes(3) > DateTime.Now)
                    {
                        DebOutPut.DebLog("小气候检测是否有未发送的数据");
                        string sql = "select * from Record where Flag='0'";
                        DataTable UploadTable = DB_Climate.QueryDatabase(sql).Tables[0];
                        DebOutPut.DebLog("未传共  " + UploadTable.Rows.Count + "  个");
                        for (int i = 0; i < UploadTable.Rows.Count; i++)
                        {
                            if (client != null && client.clientSocket != null && client.clientSocket.Connected && newDataTime.AddMinutes(3) > DateTime.Now)
                            {
                                client.SendData(UploadTable.Rows[i]["Data"] + "");//上传数据
                                DebOutPut.DebLog("当前发送第  " + (i + 1) + "  个，数据为:  " + UploadTable.Rows[i]["Data"].ToString());
                                Thread.Sleep(2000);
                            }
                        }
                        UploadTable.Dispose();
                    }
                    Interlocked.Exchange(ref inSendData, 0);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (client != null && client.clientSocket != null && client.clientSocket.Connected)
                {
                    DebOutPut.DebLog("上传异常，断开连接");
                    DebOutPut.WriteLog(LogType.Error, "上传异常，断开连接");
                    if (client != null)
                        client.SocketClose();
                }
            }

        }
    }
}
