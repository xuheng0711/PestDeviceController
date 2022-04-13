using BZ10.Common;
using DevComponents.DotNetBar;
using DeviceController.Common;
using DeviceController.DAL;
using DeviceController.Models;
using DeviceController.View.Climate;
using LitJson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.ListViewItem;
using UrlType = DeviceController.View.Climate.UrlType;

namespace DeviceController.View.DevOverview
{

    /// <summary>
    /// 气象数据展示刷新委托
    /// </summary>
    public delegate void ClimateUpdataShow();

    /// <summary>
    /// 气象Led屏测试
    /// </summary>
    public delegate void ClimateLedText();

    /// <summary>
    /// CQ12运行状态刷新
    /// </summary>
    public delegate void CQ12RunStateUpdata(string cq12RunState, string cq12CurrState);

    /// <summary>
    /// CQ12刷新工作模式
    /// </summary>
    /// <param name="workMode">当前的工作模式</param>
    public delegate void CQ12WorkModeUpdata(CQ12WorkMode workMode);

    /// <summary>
    /// 智能孢子刷新运行状态显示
    /// </summary>
    /// <param name="index">图片索引</param>
    public delegate void SporeRunStateUpdata(int index);

    /// <summary>
    /// 孢子行为过程刷新
    /// </summary>
    /// <param name="str"></param>
    public delegate void SporeBehaviorUpdata(string str);

    /// <summary>
    /// 设备运行联网异常数量刷新
    /// </summary>
    public delegate void DevRunNetCountUpdata();

    /// <summary>
    /// 设置CQ12开关为不可点击
    /// </summary>
    /// <param name="isEnable"></param>
    public delegate void SetIsEnable(bool isEnable);

    /// <summary>
    /// 孢子错误信息刷新
    /// </summary>
    /// <param name="str"></param>
    public delegate void SporeErrInfo(string str, Color color);
    public partial class DevOverviewMain : Form
    {
        public static ClimateLedText climateLedText;
        public static ClimateUpdataShow climateUpdataShow;
        public static CQ12RunStateUpdata cQ12RunStateUpdata;
        public static CQ12WorkModeUpdata cQ12WorkModeUpdata;
        public static SporeRunStateUpdata sporeRunStateUpdata;
        public static SporeBehaviorUpdata sporeBehaviorUpdata;
        public static SporeErrInfo sporeErrInfo;
        public static SetIsEnable setIsEnable;
        public static DevRunNetCountUpdata devRunNetCountUpdata;
        PictureBox[] ctrls = null;//智能孢子
        private static readonly object Lock = new object();
        public DevOverviewMain()
        {
            InitializeComponent();
            System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
            climateLedText = climateUpdataFunc;
            climateUpdataShow = climateUpdataFunc;
            cQ12RunStateUpdata = cQ12RunStateUpdataFunc;
            cQ12WorkModeUpdata = cQ12WorkModeUpdataFunc;
            sporeRunStateUpdata = setLocation;
            sporeBehaviorUpdata = SporeBehaviorUpdata_;
            sporeErrInfo = SporeErrInfo_;
            setIsEnable = SetControlsEnabled;
            devRunNetCountUpdata = devRunNetCountUpdataFunc;

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
        private void DevOverviewMain_Load(object sender, EventArgs e)
        {
            try
            {
                Init();
                //Thread thread = new Thread(Init);
                //thread.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        string url = "http://api.popled.cn/";
        static string LedCom = "";
        static string LedUser = "";
        static string Ledpwd = "";
        static string LeddevID = "";
        static string LedTitle = "";
        static string LedShowType = "";//0代表HTTP 5*5控制 以开封祥符区为例编写、1代表串口 6*6控制 
        private void Init()
        {
            try
            {
                devRunNetCountUpdata();
                //设置控件是否可点击
                setIsEnable(false);
                ctrls = new PictureBox[13] { pictureBox6, pictureBox7, pictureBox8, pictureBox9, pictureBox10, pictureBox11, pictureBox12, pictureBox13, pictureBox14, pictureBox15, pictureBox16, pictureBox17, pictureBox18 };
                hideLocations();

                string climateConfigSavePath = Path.Combine(PubField.basePath, "ClimateConfig");
                climateConfigSavePath = Path.Combine(climateConfigSavePath, PubField.climateConfigName);

                LedUser = Tools.ConfigParmRead("Basic Parameters", "LedUser", climateConfigSavePath);
                Ledpwd = Tools.ConfigParmRead("Basic Parameters", "Ledpwd", climateConfigSavePath);
                LeddevID = Tools.ConfigParmRead("Basic Parameters", "LeddevID", climateConfigSavePath);
                LedTitle = Tools.ConfigParmRead("Basic Parameters", "LedTitle", climateConfigSavePath);
                LedShowType = Tools.ConfigParmRead("Basic Parameters", "LedShowType", climateConfigSavePath);
                LedCom = Tools.ConfigParmRead("Basic Parameters", "SerialPortLed", climateConfigSavePath);
                bool isOpen = SerialPortCtrl.SetSerialPortAttribute(serialPort1, LedCom, 115200);
                if (!isOpen)
                {
                    DebOutPut.DebLog("led屏串口打开失败！");
                    DebOutPut.WriteLog(LogType.Normal, "led屏串口打开失败！");
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }


        }

        /// <summary>
        /// 设置控件是否可点击
        /// </summary>
        /// <param name="isEnabled">是否可点击</param>
        private void SetControlsEnabled(bool isEnabled)
        {
            try
            {
                Tools.SetControlEnabled(this.listView1, isEnabled);
                Tools.SetControlEnabled(this.Buguangdeng, isEnabled);
                Tools.SetControlEnabled(this.Paishui, isEnabled);
                Tools.SetControlEnabled(this.Jiareguan, isEnabled);
                Tools.SetControlEnabled(this.Shangluochong, isEnabled);
                Tools.SetControlEnabled(this.Lvdai, isEnabled);
                Tools.SetControlEnabled(this.Youchong, isEnabled);
                Tools.SetControlEnabled(this.Xialuochong, isEnabled);
                Tools.SetControlEnabled(this.Zhendong, isEnabled);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }



        #region 气象

        /// <summary>
        /// 数据刷新
        /// </summary>
        private void climateUpdataFunc()
        {
            try
            {
                Thread climateThread = new Thread(ClimateUpDataShow);
                climateThread.IsBackground = true;
                climateThread.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 数据展示刷新
        /// </summary>
        private void ClimateUpDataShow()
        {
            try
            {
                string sql = "select * from Record";
                DataTable dt = DB_Climate.QueryDatabase(sql).Tables[0];
                SaveDataModel.climateModelList.Clear();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ClimateModel myModel = JsonMapper.ToObject<ClimateModel>(dt.Rows[i]["Data"].ToString());
                    SaveDataModel.climateModelList.Add(myModel);
                }
                SaveDataModel.climateModelList.Reverse();
                if (SaveDataModel.climateModelList.Count > 0 && SaveDataModel.climateModelList[0].message.environments != null)
                {
                    this.LabCollectionTime.Text = SaveDataModel.climateModelList[0].message.collectTime;
                    ClimateCreateListControl();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }


        }

        /// <summary>
        /// 生成
        /// </summary>
        public void ClimateCreateListControl()
        {
            try
            {
                if (SaveDataModel.climateModelList.Count == 0 || SaveDataModel.climateModelList[0].message.environments == null)
                {
                    return;
                }
                List<ClimateEnvironmentsItem> environments = SaveDataModel.climateModelList[0].message.environments;

                listView1.Clear();
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

                listView1.Columns.Add(columnHeader);
                listView1.Columns.Add(columnHeader1);
                listView1.Columns.Add(columnHeader2);
                listView1.Columns.Add(columnHeader3);

                Dictionary<string, string> showsDic = new Dictionary<string, string>();
                for (int i = 0; i < environments.Count; i += 2)
                {
                    ListViewItem listViewItem = new ListViewItem();
                    listViewItem.Text = environments[i].name;
                    string company = GetCompany(environments[i].name);
                    ListViewSubItem listViewSubItem = new ListViewSubItem();
                    listViewSubItem.Text = environments[i].value + " " + company;

                    if (listViewItem.Text == "风向")
                    {
                        string fengxiang = Tools.WindDirectionSwitch(float.Parse(environments[i].value));
                        showsDic.Add(listViewItem.Text, fengxiang);
                    }
                    else if (listViewItem.Text == "二氧化碳")
                    {
                        showsDic.Add(listViewItem.Text, environments[i].value + company);
                    }
                    else if (listViewItem.Text == "湿度")
                    {
                        showsDic.Add("湿度", float.Parse(environments[i].value).ToString("F0") + "%");
                    }
                    else if (listViewItem.Text == "风速")
                    {
                        showsDic.Add("风速", float.Parse(environments[i].value).ToString("F0") + company);
                    }
                    else
                    {
                        showsDic.Add(listViewItem.Text, environments[i].value + company);
                    }
                    listViewItem.SubItems.Add(listViewSubItem);
                    if (i + 1 < environments.Count)
                    {
                        ListViewSubItem listViewSubItem1 = new ListViewSubItem();
                        listViewSubItem1.Text = environments[i + 1].name;
                        string company1 = GetCompany(environments[i + 1].name);
                        ListViewSubItem listViewSubItem2 = new ListViewSubItem();
                        listViewSubItem2.Text = environments[i + 1].value + " " + company1;
                        if (listViewSubItem1.Text == "风向")
                        {
                            string fengxiang = Tools.WindDirectionSwitch(float.Parse(environments[i + 1].value));
                            showsDic.Add(listViewSubItem1.Text, fengxiang);
                        }
                        else if (listViewSubItem1.Text == "二氧化碳")
                        {
                            showsDic.Add(listViewSubItem1.Text, environments[i + 1].value + company1);
                        }
                        else if (listViewSubItem1.Text == "湿度")
                        {
                            showsDic.Add("湿度", float.Parse(environments[i + 1].value).ToString("F0") + "%");
                        }
                        else if (listViewSubItem1.Text == "风速")
                        {
                            showsDic.Add("风速", float.Parse(environments[i + 1].value).ToString("F0") + company1);
                        }
                        else
                        {
                            showsDic.Add(listViewSubItem1.Text, environments[i + 1].value + company1);
                        }

                        listViewItem.SubItems.Add(listViewSubItem1);
                        listViewItem.SubItems.Add(listViewSubItem2);
                    }
                    this.listView1.Items.Add(listViewItem);
                }
                if (LedShowType == "0")
                    LedHttp(showsDic);
                else if (LedShowType == "1")
                    LedComTxt(showsDic);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        int ledindex = -1;
        int ledtitle = -1;
        byte[] ledChear;
        byte[] title;
        byte[] ledChar;
        private void LedComTxt(Dictionary<string, string> showsDic)
        {
            if (serialPort1 == null || !serialPort1.IsOpen)
                return;
            //清屏
            ledChear = new byte[16];
            ledChear[0] = 0xAA;
            ledChear[1] = 0xA5;
            ledChear[2] = 0x08;
            ledChear[3] = 0x00;
            ledChear[4] = 0xFF;
            ledChear[5] = 0xFF;
            ledChear[6] = 0x00;
            ledChear[7] = 0x00;
            ledChear[8] = 0xB0;
            ledChear[9] = 0xA1;
            ledChear[10] = 0x00;
            ledChear[11] = 0x02;
            ledChear[12] = 0x00;
            ledChear[13] = 0x00;
            ledChear[14] = 0x5A;
            ledChear[15] = 0x55;
            LedDataSend(ledChear);
            //标题
            ledtitle = 0;
            title = new byte[1280];
            title[ledtitle++] = 0xAA;// 帧头
            title[ledtitle++] = 0xA5;

            title[ledtitle++] = 0X00;//长度
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0xFF;// DES 控制卡地址
            title[ledtitle++] = 0xFF;

            title[ledtitle++] = 0x01;// SRC地址
            title[ledtitle++] = 0x01;

            title[ledtitle++] = 0xB0;// 会话ID
            title[ledtitle++] = 0xA1;

            title[ledtitle++] = 0x10;// CMD:请求命令
            title[ledtitle++] = 0x03;

            title[ledtitle++] = 0x01;// 分区编号: 0x01:站点名称 0x02: 实时时间 0x03:滚动信息
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x00; // 保留
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x02;// 节目样式： 2：永久节目
            title[ledtitle++] = 0x00;
            title[ledtitle++] = 0x00;
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x40;  // 文本显示格式
            title[ledtitle++] = 0x00;
            title[ledtitle++] = 0x00;
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x01;// 进入效果 静止显示
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x02;// 进入效果速度
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x00; // 进入如效果停留时间
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x00;// 强调：保留值
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x00;// 强调 效果速度：保留值
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x01;// 强调效果停留时间
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x00;// 退出效果
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0x00;// 退出效果速度
            title[ledtitle++] = 0x00;

            title[ledtitle++] = 0xFF;// 重复次数
            title[ledtitle++] = 0xFF;

            byte[] bts;
            bts = StrToHex(LedTitle);
            title[ledtitle++] = (byte)bts.Length;//长度
            title[ledtitle++] = 0x00;

            for (int i = 0; i < bts.Length; i++)
            {
                title[ledtitle++] = bts[i];
            }

            title[ledtitle++] = 0x00;
            title[ledtitle++] = 0x00;
            title[ledtitle++] = 0x5A;
            title[ledtitle++] = 0x55;
            title[2] = (byte)(ledtitle - 8);
            LedDataSend(title);

            //滚动信息
            ledindex = 0;
            ledChar = new byte[1280];
            ledChar[ledindex++] = (byte)0xAA;// 帧头
            ledChar[ledindex++] = (byte)0xA5;
            ledChar[ledindex++] = 0x00; // 长度
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = (byte)0xFF; // DES 控制卡地址
            ledChar[ledindex++] = (byte)0xFF;
            ledChar[ledindex++] = 0x01; // SRC地址
            ledChar[ledindex++] = 0x01;
            ledChar[ledindex++] = (byte)0xB0; // 会话ID
            ledChar[ledindex++] = (byte)0xA1;
            ledChar[ledindex++] = 0x10; // CMD:请求命令
            ledChar[ledindex++] = 0x03;
            ledChar[ledindex++] = 0x02; // 分区编号: 0x01:站点名称 0x02: 实时时间 0x03:滚动信息
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00; // 保留
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x02; // 节目样式： 2：永久节目
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00; // 文本显示格式
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x02; // 进入效果
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x05;// 进入效果速度
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = (byte)0x2C;// 进入如效果停留时间
            ledChar[ledindex++] = 0x01;
            ledChar[ledindex++] = 0x00;// 强调：保留值
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00;// 强调 效果速度：保留值
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00;// 强调效果停留时间
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x02;// 退出效果
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x05;// 退出效果速度
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = (byte)0xFF;// 重复次数
            ledChar[ledindex++] = (byte)0xFF;
            byte[] bt;
            StringBuilder curMsg = new StringBuilder();
            foreach (var key in showsDic.Keys)
            {
                if (key == "温度")
                    curMsg.Append("空气温度:" + showsDic["温度"] + "\n\r");
                else if (key == "湿度")
                    curMsg.Append("空气湿度:" + showsDic["湿度"] + "\n\r");
                else if (key == "露点")
                    curMsg.Append("空气露点:" + showsDic["露点"] + "\n\r");
                else if (key == "光照度")
                    curMsg.Append("光照强度:" + showsDic["光照度"] + "\n\r");
                else if (key == "大气压")
                    curMsg.Append("大气压力:" + showsDic["大气压"] + "\n\r");
                else if (key == "二氧化碳")
                    curMsg.Append("二氧化碳:" + showsDic["二氧化碳"] + "\n\r");
                else if (key == "风向")
                    curMsg.Append("风    向:" + showsDic["风向"] + "\n\r");
                else if (key == "风速")
                    curMsg.Append("风    速:" + showsDic["风速"] + "\n\r");
                else if (key == "雨量")
                    curMsg.Append("雨    量:" + showsDic["雨量"] + "\n\r");
            }
            DebOutPut.DebLog("中文数据:" + curMsg.ToString());
            bt = StrToHex(curMsg.ToString());
            ledChar[ledindex++] = (byte)bt.Length;// 字符串长度
            ledChar[ledindex++] = 0x00;
            for (int i = 0; i < bt.Length; i++)
            {
                ledChar[ledindex++] = bt[i];
            }
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x00;
            ledChar[ledindex++] = 0x5A;
            ledChar[ledindex++] = (byte)0x55;
            ledChar[2] = (byte)(ledindex - 8);
            LedDataSend(ledChar);
        }
        public void LedDataSend(byte[] ledChar)
        {
            try
            {
                if (serialPort1 == null || !serialPort1.IsOpen)
                    return;
                int index = 0;
                serialPort1.DiscardInBuffer();
                DebOutPut.DebLog("发送:" + byteToHexStr(ledChar));
                DebOutPut.WriteLog(LogType.Normal, "发送:" + byteToHexStr(ledChar));
                for (int i = 0; i < 3; i++)
                {
                    serialPort1.Write(ledChar, 0, ledChar.Length);
                    Thread.Sleep(1000);
                    if (serialPort1.BytesToRead <= 0)
                    {
                        DebOutPut.DebLog("未收到终端回应！请重试!");
                        DebOutPut.WriteLog(LogType.Normal, "未收到终端回应！请重试!");
                        index++;
                    }
                    else
                        break;
                }
                if (index == 3)
                {
                    return;
                }
                byte[] rec = new byte[1280];
                serialPort1.Read(rec, 0, 10);
                DebOutPut.DebLog("接收:" + byteToHexStr(rec));
                DebOutPut.WriteLog(LogType.Normal, "接收:" + byteToHexStr(rec));
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }



        private byte[] StrToHex(string str)
        {
            if ((str.Length % 2) != 0)
                str = str + " ";
            byte[] bytes = Encoding.GetEncoding("gb2312").GetBytes(str);
            return bytes;
        }

        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2");
                }
            }
            return returnStr;
        }



        /// <summary>
        /// Http控制
        /// </summary>
        /// <param name="showsDic"></param>
        private void LedHttp(Dictionary<string, string> showsDic)
        {
            if (LeddevID == "")
                return;
            //string strNull = " ";
            //while (showsDic.Count < 6)
            //{
            //    strNull += " ";
            //    showsDic.Add(strNull, "");
            //}
            List<string> re = new List<string>();
            foreach (var item in showsDic.Keys)
                if (item != "温度" && item != "湿度" && item != "大气压" && item != "二氧化碳" && item != "风向" && item != "风速")
                    re.Add(item);
            for (int i = 0; i < re.Count; i++)
                showsDic.Remove(re[i]);
            List<string> ledShow = new List<string>();
            if (showsDic.Count > 4)
            {
                string str = "";
                if (showsDic.ContainsKey("温度"))
                    str = "温度:" + showsDic["温度"];
                if (str != "")
                {
                    if (showsDic.ContainsKey("湿度"))
                        str += " 湿度:" + showsDic["湿度"];
                    ledShow.Add(str);
                }
                else
                {
                    if (showsDic.ContainsKey("湿度"))
                        str = "湿度:" + showsDic["湿度"];
                    if (str != "")
                        ledShow.Add(str);
                }
                str = "";
                if (showsDic.ContainsKey("大气压"))
                    str = "大气压:" + showsDic["大气压"];
                if (str != "")
                    ledShow.Add(str);
                str = "";
                if (showsDic.ContainsKey("二氧化碳"))
                    str = "二氧化碳:" + showsDic["二氧化碳"];
                if (str != "")
                    ledShow.Add(str);
                str = "";
                if (showsDic.ContainsKey("风向"))
                    str = "风向:" + showsDic["风向"];
                if (str != "")
                {
                    if (showsDic.ContainsKey("风速"))
                        str += " 风速:" + showsDic["风速"];
                    ledShow.Add(str);
                }
                else
                {
                    if (showsDic.ContainsKey("风速"))
                        str = "风速:" + showsDic["风速"];
                    if (str != "")
                        ledShow.Add(str);
                }
            }
            else if (showsDic.Count <= 4)
            {
                string str = "";
                if (showsDic.ContainsKey("温度"))
                    str = "温度:" + showsDic["温度"];
                if (str != "")
                    ledShow.Add(str);
                str = "";
                if (showsDic.ContainsKey("湿度"))
                    str = "湿度:" + showsDic["湿度"];
                if (str != "")
                    ledShow.Add(str);
                str = "";
                if (showsDic.ContainsKey("大气压"))
                    str = "大气压:" + showsDic["大气压"];
                if (str != "")
                    ledShow.Add(str);
                str = "";
                if (showsDic.ContainsKey("二氧化碳"))
                    str = "二氧化碳:" + showsDic["二氧化碳"];
                if (str != "")
                    ledShow.Add(str);
                str = "";
                if (showsDic.ContainsKey("风向"))
                    str = "风向:" + showsDic["风向"];
                if (str != "")
                    ledShow.Add(str);
                str = "";
                if (showsDic.ContainsKey("风速"))
                    str = "风速:" + showsDic["风速"];
                if (str != "")
                    ledShow.Add(str);

            }
            while (ledShow.Count < 4)
                ledShow.Add("");
            //发送要展示的数据
            #region 数据模型
            LEDModel lEDModel = new LEDModel();
            lEDModel.ids_dev = LeddevID;
            lEDModel.sno = 1;
            lEDModel.pkts_program.id_pro = 1;
            lEDModel.pkts_program.property_pro.width = 80;
            lEDModel.pkts_program.property_pro.height = 160;
            //区域1
            List_regionItem list_RegionItem1 = new List_regionItem();
            list_RegionItem1.info_pos.x = 0;
            list_RegionItem1.info_pos.y = 0;
            list_RegionItem1.info_pos.w = 160;
            list_RegionItem1.info_pos.h = 16;
            List_itemItem list_ItemItem1 = new List_itemItem();
            //list_ItemItem1.info_animate.model_continue = "left";
            //list_ItemItem1.info_animate.speed = "10";
            list_ItemItem1.type_item = "text";
            list_ItemItem1.text = LedTitle;
            list_RegionItem1.list_item.Add(list_ItemItem1);
            lEDModel.pkts_program.list_region.Add(list_RegionItem1);
            DebOutPut.DebLog("区域1 标题：" + LedTitle);
            DebOutPut.WriteLog(LogType.Normal, "区域1 标题：" + LedTitle);
            //区域2
            List_regionItem list_RegionItem2 = new List_regionItem();
            list_RegionItem2.info_pos.x = 0;
            list_RegionItem2.info_pos.y = 16;
            list_RegionItem2.info_pos.w = 160;
            list_RegionItem2.info_pos.h = 16;
            List_itemItem list_ItemItem2 = new List_itemItem();
            //list_ItemItem2.info_animate.model_continue = "left";
            //list_ItemItem2.info_animate.speed = "10";
            list_ItemItem2.type_item = "text";
            list_ItemItem2.text = ledShow[0];
            list_RegionItem2.list_item.Add(list_ItemItem2);
            lEDModel.pkts_program.list_region.Add(list_RegionItem2);
            DebOutPut.DebLog("区域2 内容：" + ledShow[0]);
            DebOutPut.WriteLog(LogType.Normal, "区域2 内容：" + ledShow[0]);
            //区域3
            List_regionItem list_RegionItem3 = new List_regionItem();
            list_RegionItem3.info_pos.x = 0;
            list_RegionItem3.info_pos.y = 32;
            list_RegionItem3.info_pos.w = 160;
            list_RegionItem3.info_pos.h = 16;
            List_itemItem list_ItemItem3 = new List_itemItem();
            //list_ItemItem3.info_animate.model_continue = "left";
            //list_ItemItem3.info_animate.speed = "10";
            list_ItemItem3.type_item = "text";
            list_ItemItem3.text = ledShow[1];
            list_RegionItem3.list_item.Add(list_ItemItem3);
            lEDModel.pkts_program.list_region.Add(list_RegionItem3);
            DebOutPut.DebLog("区域3 内容：" + ledShow[1]);
            DebOutPut.WriteLog(LogType.Normal, "区域3 内容：" + ledShow[1]);
            //区域4
            List_regionItem list_RegionItem4 = new List_regionItem();
            list_RegionItem4.info_pos.x = 0;
            list_RegionItem4.info_pos.y = 48;
            list_RegionItem4.info_pos.w = 160;
            list_RegionItem4.info_pos.h = 16;
            List_itemItem list_ItemItem4 = new List_itemItem();
            //list_ItemItem4.info_animate.model_continue = "left";
            //list_ItemItem4.info_animate.speed = "10";
            list_ItemItem4.type_item = "text";
            list_ItemItem4.text = ledShow[2];
            list_RegionItem4.list_item.Add(list_ItemItem4);
            lEDModel.pkts_program.list_region.Add(list_RegionItem4);
            DebOutPut.DebLog("区域4 内容：" + ledShow[2]);
            DebOutPut.WriteLog(LogType.Normal, "区域4 内容：" + ledShow[2]);
            //区域5
            List_regionItem list_RegionItem5 = new List_regionItem();
            list_RegionItem5.info_pos.x = 0;
            list_RegionItem5.info_pos.y = 64;
            list_RegionItem5.info_pos.w = 160;
            list_RegionItem5.info_pos.h = 16;
            List_itemItem list_ItemItem5 = new List_itemItem();
            //list_ItemItem5.info_animate.model_continue = "left";
            //list_ItemItem5.info_animate.speed = "10";
            list_ItemItem5.type_item = "text";
            list_ItemItem5.text = ledShow[3];
            list_RegionItem5.list_item.Add(list_ItemItem5);
            lEDModel.pkts_program.list_region.Add(list_RegionItem5);
            DebOutPut.DebLog("区域5 内容：" + ledShow[3]);
            DebOutPut.WriteLog(LogType.Normal, "区域5 内容：" + ledShow[3]);
            #endregion

            //发送数据
            bool isSuccess = SendDataLed(lEDModel);
            if (!isSuccess)
            {
                int count = 0;
                while (count < 3)
                {
                    isSuccess = SendDataLed(lEDModel);
                    if (isSuccess)
                        break;
                    count++;
                }
            }
        }


        private bool SendDataLed(LEDModel lEDModel)
        {
            try
            {
                Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                //登录
                LedLogn ledLogn = new LedLogn();
                ledLogn.user = LedUser;
                ledLogn.pwd = Tools.Md5(Ledpwd);
                string logJson = JsonConvert.SerializeObject(ledLogn);
                string result = PostUrl(UrlType.Login, url + "api/login", logJson);
                if (result == "")
                {
                    DebOutPut.DebLog("登录失败！");
                    DebOutPut.WriteLog(LogType.Normal, "登录失败！");
                    return false;
                }
                JObject jo = (JObject)JsonConvert.DeserializeObject(result);
                string key = jo["key"].ToString();
                if (key == "")
                {
                    DebOutPut.DebLog("登录异常，key为空!");
                    DebOutPut.WriteLog(LogType.Normal, "登录异常，key为空!");
                    return false;
                }
                //清屏
                LedClear ledClear = new LedClear();
                ledClear.cmd.delete.del_all = 1;
                ledClear.ids_dev = LeddevID;
                ledClear.sno = 1;
                string clearJson = JsonConvert.SerializeObject(ledClear);
                string time = Tools.ConvertTimestamp(DateTime.Now, 1);
                keyValuePairs.Add("user", "zzokq");
                keyValuePairs.Add("ts", time);
                keyValuePairs.Add("sendmsg", clearJson);
                keyValuePairs.Add("type", "pkts_program");
                keyValuePairs.Add("token", GetToken(clearJson, time, key).ToUpper());
                string clearjson = JsonConvert.SerializeObject(keyValuePairs);
                DebOutPut.DebLog("发送清屏指令：" + clearjson);
                DebOutPut.WriteLog(LogType.Normal, "发送清屏指令：" + clearjson);
                result = PostUrl(UrlType.ClearScene, url + "api/send", clearjson);
                DebOutPut.DebLog("收到：" + result);
                DebOutPut.WriteLog(LogType.Normal, "收到：" + result);
                if (result.Contains("err"))
                {
                    DebOutPut.DebLog("清屏指令返回token错误!");
                    DebOutPut.WriteLog(LogType.Normal, "清屏指令返回token错误!");
                    return false;
                }

                string showJson = JsonConvert.SerializeObject(lEDModel);
                string showtime = Tools.ConvertTimestamp(DateTime.Now, 1);
                keyValuePairs.Remove("sendmsg");
                keyValuePairs.Add("sendmsg", showJson);
                keyValuePairs.Remove("token");
                keyValuePairs.Add("token", GetToken(showJson, showtime, key).ToUpper());
                string Contentjson = JsonConvert.SerializeObject(keyValuePairs);
                DebOutPut.DebLog("发送展示数据指令：" + Contentjson);
                DebOutPut.WriteLog(LogType.Normal, "发送展示数据指令：" + Contentjson);
                result = PostUrl(UrlType.ClearScene, url + "api/send", Contentjson);
                //{"err":"token_error"}
                DebOutPut.DebLog("收到：" + result);
                DebOutPut.WriteLog(LogType.Normal, "收到：" + result);
                if (result.Contains("err"))
                {
                    DebOutPut.DebLog("发送展示数据指令返回token错误!");
                    DebOutPut.WriteLog(LogType.Normal, "发送展示数据指令返回token错误!");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 获取taken
        /// </summary>
        /// <returns></returns>
        private string GetToken(string content, string time, string key)
        {
            string str = "sendmsg=" + content + "&ts=" + time + "&type=pkts_program&user=zzokq&key=" + key;
            DebOutPut.DebLog("Get token md5 Str：" + str);
            DebOutPut.WriteLog(LogType.Normal, "Get token md5 Str：" + str);
            return Tools.Md5(str);
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="urlType"></param>
        /// <param name="url"></param>
        /// <param name="postData"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private string PostUrl(UrlType urlType, string url, string postData, string token = "")
        {
            try
            {
                HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create(url);
                webrequest.Method = "POST";
                webrequest.ContentType = "application/json;charset=utf-8";
                byte[] postdatabyte = Encoding.UTF8.GetBytes(postData);
                webrequest.ContentLength = postdatabyte.Length;
                Stream stream;
                stream = webrequest.GetRequestStream();
                stream.Write(postdatabyte, 0, postdatabyte.Length);
                stream.Close();
                using (var httpWebResponse = webrequest.GetResponse())
                using (StreamReader responseStream = new StreamReader(httpWebResponse.GetResponseStream()))
                {
                    String ret = responseStream.ReadToEnd();
                    string result = ret.ToString();
                    return result;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }
        }
        #endregion

        #region 虫情

        /// <summary>
        /// CQ12运行状态刷新
        /// </summary>
        private void cQ12RunStateUpdataFunc(string cq12RunState, string cq12CurrState)
        {
            try
            {
                if (cq12RunState != "")
                {
                    string runState = cq12RunState;//运行状态  0关   1开
                    string youchongdeng = runState.Substring(runState.Length - 1, 1);//诱虫灯
                    if (youchongdeng == "0")
                        this.Youchong.Value = false;
                    else if (youchongdeng == "1")
                        this.Youchong.Value = true;
                    string paishui = runState.Substring(runState.Length - 2, 1);//排水
                    if (paishui == "0")
                        this.Paishui.Value = false;
                    else if (paishui == "1")
                        this.Paishui.Value = true;
                    string shangfanban = runState.Substring(runState.Length - 3, 1);//上翻板
                    if (shangfanban == "0")
                        this.Shangluochong.Value = false;
                    else if (shangfanban == "1")
                        this.Shangluochong.Value = true;
                    string xiafanban = runState.Substring(runState.Length - 4, 1);//下翻板
                    if (xiafanban == "0")
                        this.Xialuochong.Value = false;
                    else if (xiafanban == "1")
                        this.Xialuochong.Value = true;
                    string zhendong = runState.Substring(runState.Length - 5, 1);//震动
                    if (zhendong == "0")
                        this.Zhendong.Value = false;
                    else if (zhendong == "1")
                        this.Zhendong.Value = true;
                    string buguangdeng = runState.Substring(runState.Length - 6, 1);//补光灯
                    if (buguangdeng == "0")
                        this.Buguangdeng.Value = false;
                    else if (buguangdeng == "1")
                        this.Buguangdeng.Value = true;
                    string jiare = runState.Substring(runState.Length - 7, 1);//加热
                    if (jiare == "0")
                        this.Jiareguan.Value = false;
                    else if (jiare == "1")
                        this.Jiareguan.Value = true;
                    string lvdai = runState.Substring(runState.Length - 8, 1);//履带
                    if (lvdai == "0")
                        this.Lvdai.Value = false;
                    else if (lvdai == "1")
                        this.Lvdai.Value = true;

                }
                if (cq12CurrState != "")
                {
                    this.LabCurrState.Text = cq12CurrState;
                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 刷新工作模式
        /// </summary>
        /// <param name="workMode"></param>
        private void cQ12WorkModeUpdataFunc(CQ12WorkMode workMode)
        {
            try
            {
                switch (workMode)
                {
                    case CQ12WorkMode.Normal:
                        this.RBNormal.Checked = true;
                        break;
                    case CQ12WorkMode.Debug:
                        this.RBDebug.Checked = true;
                        break;
                    case CQ12WorkMode.TimeSolt:
                        this.RBTimeSolt.Checked = true;
                        break;
                    case CQ12WorkMode.Force:
                        this.RBForce.Checked = true;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 智能孢子

        private void hideLocations()
        {
            try
            {
                foreach (PictureBox pic in ctrls)
                {
                    pic.Visible = false;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void setLocation(int index)
        {
            try
            {
                hideLocations();
                ctrls[index].Visible = true;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 行为过程
        /// </summary>
        /// <param name="str">行为过程</param>
        private void SporeBehaviorUpdata_(string str)
        {
            this.label30.Text = str;
        }

        /// <summary>
        /// 错误信息
        /// </summary>
        /// <param name="str">错误信息</param>
        private void SporeErrInfo_(string str, Color color)
        {
            this.label9.Text = str;
            this.label9.ForeColor = color;
        }

        #endregion

        #region 设备运行联网异常数量刷新
        /// <summary>
        /// 设备运行联网异常数量刷新
        /// </summary>
        private void devRunNetCountUpdataFunc()
        {
            lock (Lock)
            {
                try
                {
                    this.listBox1.Items.Clear();
                    this.listBox2.Items.Clear();
                    for (int i = 0; i < PubField.devRunName.Count; i++)
                    {
                        if (!this.listBox1.Items.Contains(PubField.devRunName[i]))
                        {
                            this.listBox1.Items.Add(PubField.devRunName[i]);
                        }

                    }
                    for (int i = 0; i < PubField.devNetName.Count; i++)
                    {
                        if (!this.listBox2.Items.Contains(PubField.devNetName[i]))
                        {
                            this.listBox2.Items.Add(PubField.devNetName[i]);
                        }
                    }

                    this.LabDevRun.Text = this.listBox1.Items.Count.ToString();
                    this.LabDevNet.Text = this.listBox2.Items.Count.ToString();
                    this.LabAbnormalCount.Text = (6 - (this.listBox1.Items.Count + this.listBox2.Items.Count)).ToString();

                }
                catch (Exception ex)
                {
                    DebOutPut.DebErr(ex.ToString());
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                }

            }

        }
        #endregion

        /// <summary>
        /// 获取环境单位
        /// </summary>
        /// <param name="sceName">环境名字</param>
        public static string GetCompany(string sceName)
        {
            string result = "";
            try
            {
                switch (sceName)
                {
                    case "温度":
                        result = "℃";
                        break;
                    case "湿度":
                        result = "%RH";
                        break;
                    case "露点":
                        result = "℃";
                        break;
                    case "光照度":
                        result = "LUX";
                        break;
                    case "蒸发量":
                        result = "mm";
                        break;
                    case "风向":
                        result = "°";
                        break;
                    case "风速":
                        result = "m/s";
                        break;
                    case "光合辐射":
                        result = "umo";
                        break;
                    case "雨量":
                        result = "mm/min";
                        break;
                    case "土壤温度":
                        result = "℃";
                        break;
                    case "土壤水分":
                        result = "%";
                        break;
                    case "大气压":
                        result = "Kpa";
                        break;
                    case "二氧化碳":
                        result = "ppm";
                        break;
                    case "土壤盐分":
                        result = "ms/cm";
                        break;
                    case "PM2.5":
                        result = "ppm";
                        break;
                    case "总辐射":
                        result = "w/㎡";
                        break;
                    case "紫外辐射":
                        result = "W/㎡";
                        break;
                    case "溶解氧":
                        result = "Mg/L";
                        break;
                    case "氨气":
                        result = "ppm";
                        break;
                    case "日照时数":
                        result = "H";
                        break;
                    case "页面湿度":
                        result = "%";
                        break;
                    case "净辐射":
                        result = "W/㎡";
                        break;
                    case "So2":
                        result = "ppm";
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

            return result;
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
                        if (con.Name == "listView1" || con.Name == "panel5")
                        {
                            //根据窗体缩放的比例确定控件的值
                            con.Height = Convert.ToInt32(System.Convert.ToSingle(mytag[1]) * newy);//高度
                            con.Left = (con.Parent.Width - con.Width) / 2;
                            con.Top = Convert.ToInt32(System.Convert.ToSingle(mytag[3]) * newy);//顶边距
                            Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                            con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
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
        private void DevOverviewMain_Shown(object sender, EventArgs e)
        {
            isChangeSize = false;
        }
        private void DevOverviewMain_Resize(object sender, EventArgs e)
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

        private void RBNormal_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            climateUpdataFunc();
        }
    }
}
