using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace BZ10
{
    public partial class Form2 : Form
    {
        int inTimer1 = 0;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        int inTimer2 = 0;
        System.Timers.Timer timer2 = new System.Timers.Timer();

        DateTime startTime;
        DevStatus devstatus = new DevStatus();
        //bool bflag = false;
        List<byte> list = new List<byte>(); //待查询状态
        byte bStep = 0;//工作流程 1:准备阶段
        Label[] ct = null;
        bool bloop = false;
        public Form2()
        {
            InitializeComponent();
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);//绑定Elapsed事件
            timer1.Interval = 100;//设置时间间隔
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);//绑定Elapsed事件
            timer2.Interval = 100;//设置时间间隔


            ct = new Label[] { lb_X1, lb_X2, lb_X3, lb_X4, lb_X5, lb_X6, lb_X7, lb_X8, lb_X9, lb_X10, lb_X11, lb_X12, lb_X13, lb_X14, lb_X15 };//初始化数组
        }
        private void Timer1Stop()
        {
            try
            {
                timer1.Stop();
                Interlocked.Exchange(ref inTimer1, 0);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void Timer2Stop()
        {
            try
            {
                timer2.Stop();
                Interlocked.Exchange(ref inTimer2, 0);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void Form2_Load(object sender, EventArgs e)
        {

            try
            {
                this.Text = "调试模式 " + "V_" + System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
                IsPortOpen();//
                init();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void init()
        {
            try
            {
                enableControl(false);
                enableControl1(false);
                comboBox1.Text = Param.SerialPortName;
                comboBox2.Text = "115200";
                comboBox6.Text = Param.SerialPortCamera;
                comboBox5.Text = "115200";
                for (int i = 0; i < this.Controls.Count; i++)
                {
                    if (this.Controls[i] is GroupBox)
                    {
                        if (this.Controls[i].Name == "groupBox12" || this.Controls[i].Name == "groupBox3")
                            continue;
                        for (int j = 0; j < ((GroupBox)this.Controls[i]).Controls.Count; j++)
                        {
                            if (((GroupBox)this.Controls[i]).Controls[j] is ComboBox)
                                ((ComboBox)(((GroupBox)this.Controls[i]).Controls[j])).SelectedIndex = 0;
                            else if (((GroupBox)this.Controls[i]).Controls[j] is TextBox)
                                ((TextBox)(((GroupBox)this.Controls[i]).Controls[j])).Text = "0";
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

        public bool IsPortOpen()
        {
            try
            {
                //create vars for testing
                bool _available = false;
                SerialPort _tempPort;
                String[] Portname = SerialPort.GetPortNames();

                //create a loop for each string in SerialPort.GetPortNames
                foreach (string str in Portname)
                {
                    try
                    {
                        _tempPort = new SerialPort(str);
                        _tempPort.Open();

                        //if the port exist and we can open it
                        if (_tempPort.IsOpen)
                        {
                            comboBox1.Items.Add(str);
                            comboBox6.Items.Add(str);
                            _tempPort.Close();
                            _available = true;
                        }
                        comboBox1.SelectedIndex = 0;
                        comboBox6.SelectedIndex = 0;
                    }

                    //else we have no ports or can't open them display the 
                    //precise error of why we either don't have ports or can't open them
                    catch (Exception ex)
                    {
                        DebOutPut.WriteLog(LogType.Error, "相机打开失败:"+ex.ToString());
                        _available = false;
                    }
                }

                //return the temp bool
                return _available;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }
        }
        private void enableControl(bool bf)
        {
            try
            {
                foreach (Control ct in this.Controls)
                {
                    if (ct is GroupBox)
                    {
                        if (((GroupBox)ct).Name == "groupBox13" || ((GroupBox)ct).Name == "groupBox15" || ((GroupBox)ct).Name == "groupBox3" || ((GroupBox)ct).Name == "groupBox12" || ((GroupBox)ct).Name == "groupBox2" || ((GroupBox)ct).Name == "groupBox14")
                            continue;
                        if (Param.DripDevice == "1")
                        {
                            if (((GroupBox)ct).Name == "groupBox7")
                                continue;
                        }
                        ct.Enabled = bf;
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (button1.Text == "打开")
                {
                    if (comboBox1.SelectedIndex < 0)
                    {
                        MessageBox.Show("请选择串口");
                        return;
                    }
                    if (comboBox2.SelectedIndex < 0)
                    {
                        MessageBox.Show("请选择波特率");
                        return;
                    }
                    serialPort1.PortName = comboBox1.Text;
                    serialPort1.BaudRate = Convert.ToInt32(comboBox2.Text);
                    serialPort1.Open();
                    if (serialPort1.IsOpen)
                    {
                        button1.Text = "关闭";
                        enableControl(true);

                    }
                }
                else
                {
                    serialPort1.Close();
                    button1.Text = "打开";
                    enableControl(false);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        private byte GetCheckByte(byte[] by)
        {
            try
            {
                int size = by.Length;
                int crc = 0;
                for (int i = 1; i < size - 1; i++)
                {
                    crc += by[i];
                }
                //   crc = ~crc + 1;
                return (byte)(crc & 0xFF);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return 0;
            }
        }
        //字节数组转16进制字符串
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            try
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
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }
        }
        /*
         * 向下位机发送指令并将接收到的指令返回
         */
        private byte[] CommunicateDp(byte func, int steps)
        {
            try
            {
                serialPort1.DiscardInBuffer();
                byte[] rec = new byte[10];
                byte[] by = { 0xBB, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                by[2] = func;
                by[5] = (byte)((steps >> 24) & 0xFF);
                by[6] = (byte)((steps >> 16) & 0xFF);
                by[7] = (byte)((steps >> 8) & 0xFF);
                by[8] = (byte)((steps >> 0) & 0xFF);
                by[9] = GetCheckByte(by);

                serialPort1.Write(by, 0, by.Length);
                AppendLog("发:" + byteToHexStr(by));
                Thread.Sleep(1000);
                if (serialPort1.BytesToRead <= 0)
                {
                    AppendLog("未收到终端回应！请重试!");
                    return rec;
                }

                serialPort1.Read(rec, 0, 10);
                AppendLog("收:" + byteToHexStr(rec));
                int m = GetCheckByte(rec);
                if (rec[2] != 0xA0 && rec[0] == 0xFF && m == rec[9])
                {
                    if (rec[8] == 0)
                        AppendLog("收到并执行!");
                    else
                        AppendLog("执行失败!");
                }
                return rec;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x10, 0);

        }
        private void AppendLog(String info)
        {
            try
            {
                if (richTex.Lines.Length > 800)
                {
                    richTex.Text = "";
                }
                richTex.Text = richTex.Text + DateTime.Now.ToLongTimeString() + info + "\r\n";
                richTex.Select(richTex.TextLength, 0);
                //滚动到控件光标处   
                richTex.ScrollToCaret();
                label33.Text = (richTex.Lines.Length - 1).ToString();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x13, cb_z1.SelectedIndex + 1);
        }



        private void tb_step1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar) || e.KeyChar == (char)8))
            {
                e.Handled = true;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step1.Text == "")
                    return;
                byte dir = (byte)(cb_dir1.SelectedIndex == 0 ? 0x11 : 0x12);
                int step = Convert.ToInt32(tb_step1.Text);
                CommunicateDp(dir, step);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x20, 0);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x23, cb_z2.SelectedIndex + 1);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step2.Text == "")
                    return;
                byte dir = (byte)(cb_dir2.SelectedIndex == 0 ? 0x21 : 0x22);
                int step = Convert.ToInt32(tb_step2.Text);
                CommunicateDp(dir, step);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void button9_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x33, cb_z3.SelectedIndex + 1);
        }

        private void button10_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x30, 0);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x40, 0);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x20, 0);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            CommunicateDp(0x43, cb_z4.SelectedIndex + 1);
        }

        private void button14_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step5.Text == "")
                    return;
                if (Param.DripDevice == "0")
                {
                    byte dir = (byte)(cb_dir5.SelectedIndex == 0 ? 0x51 : 0x52);
                    int step = Convert.ToInt32(tb_step5.Text);
                    CommunicateDp(dir, step);
                }
                else if (Param.DripDevice == "1")
                {
                    bool isNeg = true;
                    if (cb_dir5.SelectedIndex == 0)
                        isNeg = true;
                    else if (cb_dir5.SelectedIndex == 1)
                        isNeg = false;
                    PushingFluidMove(isNeg, tb_step5.Text);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        /// <summary>
        /// 推液电机移动
        /// </summary>
        /// <param name="step">移动步数</param>
        /// <param name="isPosNeg">true正     false反</param>
        private void PushingFluidMove(bool isPosNeg, string step)
        {
            try
            {
                if (serialPort4 == null || !serialPort4.IsOpen)
                {
                    DebOutPut.DebLog("副控串口未打开!");
                    DebOutPut.WriteLog(LogType.Normal, "副控串口未打开!");
                    return;
                }
                DebOutPut.DebLog("推液电机正在移动至指定位置");
                int index = 0;
                serialPort4.DiscardInBuffer();
                string frameHead = "5AA5";
                string func = "34";
                string content = (isPosNeg == true) ? "01" : "00";
                string step1 = Tools.TenToSixteen(step).PadLeft(4, '0');
                string frameTail = "F0";
                string data = frameHead + func + content + step1 + frameTail;
                byte[] bytes = Tools.HexStrTobyte(data);

                for (int i = 0; i < 3; i++)
                {
                    serialPort4.Write(bytes, 0, bytes.Length);
                    AppendLog("发:" + Cmd.byteToHexStr(bytes));
                    Thread.Sleep(1000);
                    if (serialPort4.BytesToRead <= 0)
                    {
                        AppendLog("未收到终端回应！请重试！");
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (index == 3)
                {
                    DebOutPut.DebLog("推液电机移动到合适位置处指令连续三次发送未收到回复，指令为：" + data);
                    return;
                }
                byte[] rec = new byte[5];
                serialPort4.Read(rec, 0, 5);
                string recStr = Cmd.byteToHexStr(rec);
                string recContent = recStr.Substring(6, 2);
                AppendLog("收:" + recStr);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 推液电机位置读取
        /// </summary>
        private string PushingFluidRead()
        {
            try
            {
                if (serialPort4 == null || !serialPort4.IsOpen)
                {
                    DebOutPut.DebLog("副控串口未打开!");
                    DebOutPut.WriteLog(LogType.Normal, "副控串口未打开!");
                    return "";
                }
                DebOutPut.DebLog("推液电机读取位置");
                int index = 0;
                serialPort4.DiscardInBuffer();
                string frameHead = "5AA5";
                string func = "35";
                string content = "00";
                string frameTail = "F0";
                string data = frameHead + func + content + frameTail;
                byte[] bytes = Tools.HexStrTobyte(data);

                for (int i = 0; i < 3; i++)
                {
                    serialPort4.Write(bytes, 0, bytes.Length);
                    AppendLog("发:" + Cmd.byteToHexStr(bytes));
                    Thread.Sleep(1000);
                    if (serialPort4.BytesToRead <= 0)
                    {
                        AppendLog("未收到终端回应！请重试！");
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (index == 3)
                {
                    DebOutPut.DebLog("滴液电机读取位置指令连续三次发送未收到回复，指令为：" + data);
                    return "";
                }
                byte[] rec = new byte[5];
                serialPort4.Read(rec, 0, 5);
                string recStr = Cmd.byteToHexStr(rec);
                string recContent = recStr.Substring(6, 2);
                AppendLog("收:" + recStr);
                return recContent;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }
        }


        private void button8_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step3.Text == "")
                    return;
                byte dir = (byte)(cb_dir3.SelectedIndex == 0 ? 0x31 : 0x32);
                int step = Convert.ToInt32(tb_step3.Text);
                CommunicateDp(dir, step);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step4.Text == "")
                    return;
                byte dir = (byte)(cb_dir4.SelectedIndex == 0 ? 0x41 : 0x42);
                int step = Convert.ToInt32(tb_step4.Text);
                CommunicateDp(dir, step);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void button15_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step6.Text == "")
                    return;
                byte dir = (byte)(cb_dir6.SelectedIndex == 0 ? 0x61 : 0x62);
                int step = Convert.ToInt32(tb_step6.Text);
                CommunicateDp(dir, step);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void button18_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step7.Text == "")
                    return;
                int step = Convert.ToInt32(tb_step7.Text);
                CommunicateDp(0x91, step);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void button19_Click(object sender, EventArgs e)
        {
            try
            {
                if (tb_step8.Text == "")
                    return;
                int step = Convert.ToInt32(tb_step8.Text);
                CommunicateDp(0x92, step);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void button20_Click(object sender, EventArgs e)
        {
            try
            {
                CommunicateDp(0x93, 0);//初始化
                button16_Click_1(sender, e);//读取状态
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }





        }

        private void button16_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (serialPort1 != null && serialPort1.IsOpen)
                {
                    byte[] ret = CommunicateDp(0xA0, 0);
                    if (ret[0] == 0xFF)
                    {
                        int dirs = (ret[7] << 8) | ret[6];
                        for (int i = 0; i < 15; i++)
                        {
                            if (i == 11)
                            {
                                if (Param.DripDevice == "0")
                                {
                                    if (((dirs >> i) & 0x01) == 1)
                                        ct[i].BackColor = Color.Green;
                                    else
                                        ct[i].BackColor = Color.Yellow;
                                }
                            }
                            else
                            {
                                if (((dirs >> i) & 0x01) == 1)
                                    ct[i].BackColor = Color.Green;
                                else
                                    ct[i].BackColor = Color.Yellow;
                            }
                        }
                    }
                }
                if (serialPort4 != null || serialPort4.IsOpen)
                {
                    if (Param.DripDevice == "1")
                    {
                        string stuta = PushingFluidRead();
                        if (stuta == "00")
                            lb_X12.BackColor = Color.Yellow;
                        else if (stuta == "01")
                            lb_X12.BackColor = Color.Green;
                    }
                }
                ReadAxis7Location();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 读取轴7位置信息
        /// </summary>
        private void ReadAxis7Location()
        {
            try
            {
                if (serialPort4 == null || !serialPort4.IsOpen)
                {
                    return;
                }
                serialPort4.DiscardInBuffer();
                string frameHead = "5AA5";
                string func = "31";
                string content = "00";
                string step1 = Tools.TenToSixteen("0").PadLeft(4, '0');
                string frameTail = "F0";
                string data = frameHead + func + content + step1 + frameTail;
                byte[] bytes = Tools.HexStrTobyte(data);
                serialPort4.Write(bytes, 0, bytes.Length);
                AppendLog("发:" + Cmd.byteToHexStr(bytes).ToUpper());
                Thread.Sleep(1000);
                if (serialPort4.BytesToRead <= 0)
                {
                    DebOutPut.DebLog("未收到终端回应！请重试!");
                    AppendLog("未收到终端回应！请重试!");
                    return;
                }
                byte[] rec = new byte[5];
                serialPort4.Read(rec, 0, 5);
                string recStr = Cmd.byteToHexStr(rec);
                string recContent = recStr.Substring(6, 2);
                AppendLog("收:" + recStr.ToUpper());
                if (recContent == "01")//当前在右限位
                {
                    this.label35.BackColor = Color.Green;
                    this.label37.BackColor = Color.Yellow;
                }
                else if (recContent == "02")//以到达左限位
                {
                    this.label35.BackColor = Color.Yellow;
                    this.label37.BackColor = Color.Green;
                }
                else//可自由移动
                {
                    this.label35.BackColor = Color.Yellow;
                    this.label37.BackColor = Color.Yellow;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        //执行流程
        private void doLiucheng()
        {
            try
            {
                initDev();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        private void getDevStatus()
        {
            try
            {
                byte[] ret = CommunicateDp(0xA0, 0);
                if (ret[0] != 0xFF)
                    return;
                int dirs = (ret[7] << 8) | ret[6];
                for (int i = 0; i < 14; i++)
                {
                    if (((dirs >> i) & 0x01) == 1)
                        ct[i].BackColor = Color.Green;
                    else
                        ct[i].BackColor = Color.Yellow;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        private bool getAutoDevStatus(List<byte> bits)
        {
            try
            {
                devstatus.clear();
                StringBuilder sb = new StringBuilder();
                string[] yes = new string[] { "轴1 原点位置", "轴1 粘附液位置", "轴1 吸孢子位置", "轴1： 培养液位置", "轴1：拍照位置", "轴2:原点位置", "轴2:已推片位置", "轴3：接片位置", "轴3：原点位置", "轴4：原点位置", "轴4：推完片位置" };
                string[] no = new string[] { "轴1 未到原点位置", "轴1 未到粘附液位置", "轴1 未到吸孢子位置", "轴1： 未到培养液位置", "轴1：未到拍照位置", "轴2:未到原点位置", "轴2:未到已推片位置", "轴3：未到接片位置", "轴3：未到原点位置", "轴4：未到原点位置", "轴4：未到推完片位置" };

                byte[] ret = CommunicateDp(0xA0, 0);
                if (ret[0] != 0xFF)
                    return false;
                // ret[7]=0x1A;
                //ret[6]=0xA1;
                int dirs = (ret[7] << 8) | ret[6];

                foreach (byte i in bits)
                {
                    if (((dirs >> i) & 0x01) == 1)
                    {
                        sb.Append(yes[i]);
                        devstatus.bits[i] = 1;
                        ct[i].BackColor = Color.Green;
                    }
                    else
                    {
                        sb.Append(no[i]);
                        devstatus.bits[i] = 0;
                        ct[i].BackColor = Color.Yellow;
                    }
                }
                devstatus.status = sb.ToString();
                devstatus.isReady(bits);

                return true;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }

        }

        private bool isReady()
        {
            try
            {
                if (getAutoDevStatus(list))
                {
                    if (!devstatus.bReady)
                        return false;
                    else
                        return true;
                }
                return false;

                // return true;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }

        }

        private void stopStatus()
        {
            try
            {
                Timer2Stop();
                Timer1Stop();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        /*
         * 初始化设备：将所有轴回归原点
         */
        private void initDev()
        {
            try
            {

                //1.设备初始化，全部归原点。
                CommunicateDp(0x10, 0);//轴一找原点
                CommunicateDp(0x20, 0);//轴二找原点
                CommunicateDp(0x30, 0);//轴三找原点
                CommunicateDp(0x40, 0);//轴四找原点
                CommunicateDp(0x93, 0);//关闭风机和补光

                bStep = 0;//
                list.Clear();
                list.Add(0);
                list.Add(5);
                list.Add(7);
                list.Add(9);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        /*
        * 推片准备：轴3 到接片位置 轴4到待推片位置
        */
        private void inituipian()
        {
            try
            {

                //2. 推片准备：轴3 到接片位置 轴4到待推片位置
                CommunicateDp(0x33, 2);
                CommunicateDp(0x43, 2);
                bStep = 1;
                list.Clear();
                list.Add(8);
                list.Add(10);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        /*
         *   //3. 轴2执行推片
         */
        private void tuipian()
        {
            try
            {

                //3. 轴2执行推片执行推片后复位
                CommunicateDp(0x23, 2);//轴二推片
                bStep = 2;
                list.Clear();
                list.Add(6);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /*推片后复位*/
        private void tuipianreset()
        {
            try
            {

                CommunicateDp(0x20, 0);//轴二复位
                bStep = 3;
                list.Clear();
                list.Add(5);
                timer1.Start();

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        private void tuifanshilin()
        {
            try
            {

                //4.推片到粘附液位置
                CommunicateDp(0x13, 2);
                bStep = 4;
                list.Clear();
                list.Add(1);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void jiafanshilin()
        {
            try
            {

                CommunicateDp(0x51, 100);
                //Thread.Sleep(10 * 1000);
                Thread.Sleep(20 * 1000);
                bStep = 5;
                list.Clear();
                list.Add(1);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        private void tuipianFengshan()
        {
            try
            {

                CommunicateDp(0x13, 3);
                bStep = 6;
                list.Clear();
                list.Add(2);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        private void openFengji()
        {
            try
            {
                //7.开风机
                bStep = 7;
                CommunicateDp(0x91, 500);


                startTime = DateTime.Now;
                timer2.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        private void closeFengji()
        {
            try
            {

                //8.关风机
                CommunicateDp(0x93, 0);//初始化
                bStep = 8;
                list.Clear();
                list.Add(2);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        private void tuiPeiyangye()
        {
            try
            {
                //9.推片值培养液位置

                CommunicateDp(0x13, 4);
                bStep = 9;
                list.Clear();
                list.Add(3);
                timer1.Start();

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void peiyang()
        {
            try
            {
                bStep = 10;
                //10.滴加培养液
                CommunicateDp(0x61, 100);
                startTime = DateTime.Now;

                timer2.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void tuipaizhao()
        {
            try
            {
                bStep = 11;
                //11.推片到拍照位置
                CommunicateDp(0x13, 5);
                CommunicateDp(0x92, 500);

                list.Clear();
                list.Add(4);
                startTime = DateTime.Now;
                timer2.Start();

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        private void huishoupian()
        {
            try
            {

                //2. 推片准备：轴3 到接片位置 轴4到待推片位置
                CommunicateDp(0x33, 1);
                Thread.Sleep(2000);
                CommunicateDp(0x43, 1);
                bStep = 12;
                list.Clear();
                list.Add(7);
                list.Add(9);
                timer1.Start();

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void finish()
        {
            try
            {

                //12.设备初始化，全部归原点。
                CommunicateDp(0x10, 0);//轴一找原点
                CommunicateDp(0x20, 0);//轴二找原点
                CommunicateDp(0x30, 0);//轴三找原点
                CommunicateDp(0x40, 0);//轴四找原点
                CommunicateDp(0x93, 0);//关闭风机和补光
                bStep = 13;
                list.Clear();
                list.Add(0);
                list.Add(5);
                list.Add(7);
                list.Add(9);
                timer1.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void tuipian1()
        {

        }

        /*获取设备状态指令*/
        private void timer1_Elapsed(object sender, EventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer1, 1) == 0)
                {
                    if (!isReady())
                    {
                        Interlocked.Exchange(ref inTimer1, 0);
                        return;
                    }
                    Timer1Stop();//查询状态成功，准备就绪
                    switch (bStep)
                    {
                        case 0:
                            inituipian();//推片准备
                            break;
                        case 1:
                            tuipian();//推片
                            break;
                        case 2:
                            tuipianreset();//推片后复位
                            break;
                        case 3:
                            tuifanshilin();//推片到粘附液位置
                            break;
                        case 4:
                            jiafanshilin();//加粘附液
                            break;
                        case 5:
                            tuipianFengshan();//推片到风扇
                            break;
                        case 6:
                            openFengji();//打开风机
                            break;
                        case 7:
                            closeFengji();//关闭风机
                            break;
                        case 8:
                            tuiPeiyangye();//推片到培养液位置
                            break;
                        case 9:
                            peiyang();//滴加培养液
                            break;
                        case 10:
                            tuipaizhao();//推片到拍照位置
                            break;
                        case 11:
                            huishoupian();//回收片
                            break;
                        case 12:
                            finish();//全部回归原点
                            break;
                        case 13:
                            if (bloop)
                                doLiucheng();
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        private void timer2_Elapsed(object sender, EventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer2, 1) == 0)
                {
                    DateTime dt = DateTime.Now;
                    if (dt > (startTime.AddSeconds(60)))
                    {
                        timer1.Start();
                        Timer2Stop();
                    }
                    Interlocked.Exchange(ref inTimer2, 0);
                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        
        private void button35_Click(object sender, EventArgs e)
        {
            try
            {
                if (button35.Text == "打开")
                {
                    if (comboBox6.SelectedIndex < 0)
                    {
                        MessageBox.Show("请选择串口");
                        return;
                    }
                    if (comboBox5.SelectedIndex < 0)
                    {
                        MessageBox.Show("请选择波特率");
                        return;
                    }
                    serialPort4.PortName = comboBox6.Text;
                    serialPort4.BaudRate = Convert.ToInt32(comboBox5.Text);
                    serialPort4.Open();
                    if (serialPort4.IsOpen)
                    {
                        button35.Text = "关闭";
                        enableControl1(true);

                    }
                }
                else
                {
                    serialPort4.Close();
                    button35.Text = "打开";
                    enableControl1(false);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void enableControl1(bool bf)
        {
            try
            {
                foreach (Control ct in this.Controls)
                {
                    if (ct is GroupBox)
                    {
                        if (((GroupBox)ct).Name == "groupBox13" || ((GroupBox)ct).Name == "groupBox15")
                            ct.Enabled = bf;
                        if (Param.DripDevice == "1")
                        {
                            if (((GroupBox)ct).Name == "groupBox7")
                                ct.Enabled = bf;
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

        private void button36_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.Txt1.Text == "")
                    return;
                if (this.comboBox3.SelectedIndex == 0)//PC8
                {
                    CameraRightLeftMove(true, this.Txt1.Text.Trim());
                }
                else if (this.comboBox3.SelectedIndex == 1)//PC9
                {
                    CameraRightLeftMove(false, this.Txt1.Text.Trim());
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 相机移动到合适位置
        /// </summary>
        /// <param name="isLeftRegit">true：移动至PC8，false：移动至PC9</param>
        /// <param name="step">步数</param>
        /// <summary>
        /// 相机移动到合适位置
        /// </summary>
        /// <param name="isLeftRegit">true：移动至PC8，false：移动至PC9</param>
        /// <param name="step">步数</param>
        private void CameraRightLeftMove(bool isLeftRegit, string step)
        {
            try
            {
                if (serialPort4 == null && !serialPort4.IsOpen)
                {
                    DebOutPut.DebLog("副控串口未打开!");
                    DebOutPut.WriteLog(LogType.Normal, "串口未打开!");
                    return;
                }
                DebOutPut.DebLog("相机正在移动至指定位置");
                int index = 0;
                serialPort4.DiscardInBuffer();
                string frameHead = "5AA5";
                string func = "31";
                string content = (isLeftRegit == true) ? "01" : "00";
                string step1 = Tools.TenToSixteen(step).PadLeft(4, '0');
                string frameTail = "F0";
                string data = frameHead + func + content + step1 + frameTail;
                byte[] bytes = Tools.HexStrTobyte(data);

                for (int i = 0; i < 3; i++)
                {
                    serialPort4.Write(bytes, 0, bytes.Length);
                    AppendLog("发:" + Cmd.byteToHexStr(bytes));
                    Thread.Sleep(1000);
                    if (serialPort4.BytesToRead <= 0)
                    {
                        AppendLog("未收到终端回应！请重试!");
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (index == 3)
                {
                    DebOutPut.DebLog("轴七电机移动到合适位置处指令连续三次发送未收到回复，指令为：" + data);
                    return;
                }
                byte[] rec = new byte[5];
                serialPort4.Read(rec, 0, 5);
                string recStr = Cmd.byteToHexStr(rec);
                string recContent = recStr.Substring(6, 2);
                AppendLog("收:" + recStr);
                DebOutPut.DebLog("相机已移动到指定位置");
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void button34_Click(object sender, EventArgs e)
        {
            try
            {
                this.richTex.Text = "";
                this.label33.Text = "0";
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 履带控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button37_Click(object sender, EventArgs e)
        {
            try
            {
                MoveTrack();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        /// <summary>
        /// 移动履带
        /// </summary>
        private void MoveTrack()
        {
            try
            {
                if (serialPort4 == null && !serialPort4.IsOpen)
                {
                    DebOutPut.DebLog("副控串口未打开!");
                    DebOutPut.WriteLog(LogType.Normal, "副控串口未打开!");
                    return;
                }
                DebOutPut.DebLog("移动履带");
                int index = 0;
                serialPort4.DiscardInBuffer();
                string frameHead = "5AA5";
                string func = "33";
                string content = "00";
                string frameTail = "F0";
                string data = frameHead + func + content + frameTail;
                byte[] bytes = Tools.HexStrTobyte(data);
                for (int i = 0; i < 3; i++)
                {
                    serialPort4.Write(bytes, 0, bytes.Length);
                    AppendLog("发:" + Cmd.byteToHexStr(bytes));
                    Thread.Sleep(1000);
                    if (serialPort4.BytesToRead <= 0)
                    {
                        AppendLog("未收到终端回应！请重试!");
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
                if (index == 3)
                {
                    DebOutPut.DebLog("履带移动指令发送三次未收到回应，指令为：" + data);
                    return;
                }
                byte[] rec = new byte[5];
                serialPort4.Read(rec, 0, 5);
                string recStr = Cmd.byteToHexStr(rec);
                AppendLog("收:" + recStr);
                DebOutPut.DebLog("履带移动完成！");
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void tb_step1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Param.isSoftKeyBoard == "0")
                    SoftKeyboardCtrl.OpenAndSetWindow();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SoftKeyboardCtrl.CloseWindow();

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private void groupBox15_Enter(object sender, EventArgs e)
        {

        }
    }
}
