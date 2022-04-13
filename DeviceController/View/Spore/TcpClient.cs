using BZ10.Common;
using DeviceController.Common;
using DeviceController.View.DevOverview;
using SporeDetectionEquipment;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace BZ10
{
    public class TcpClient
    {

        public class StateObject
        {
            public Socket workSocket = null;
            public const int BUFFER_SIZE = 1024;
            public byte[] buffer = new byte[BUFFER_SIZE];
            public StringBuilder sb = new StringBuilder();
        }

        //全局参数
        private System.Timers.Timer myTimer = null;
        public static DateTime newDateTime = DateTime.Parse("1970-01-01 00:00:00");//记录最新的从服务器接受到的数据时间
        private static int inteval = 30000; //设置定时器执行间隔
        private static MainForm form;    //主界面对话框
        public Socket clientSocket = null; //客户端Socket
        public GlobalParam global = null; //全局参数
        public static string response;
        //string PostParam = "", GpsJson = "";


        public TcpClient(MainForm form1, GlobalParam param)
        {
            form = (MainForm)form1;
            global = param;
            InitTimer();

        }
        public void InitTimer()
        {
            try
            {
                if (myTimer == null)
                {
                    myTimer = new System.Timers.Timer(inteval);
                    myTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnTimedEvent);
                    myTimer.Interval = inteval;
                    myTimer.Enabled = true;
                }
            }
            catch (Exception ex)
            {

                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }


        }
        /* 停止服务*/
        public void stopConnect()
        {

            closeSocket();
            //if (myTimer != null)
            //{
            //    myTimer.Enabled = false;
            //    myTimer = null;
            //}
            //InitTimer();
        }
        /*
         * 定时发送心跳帧
         */
        private void OnTimedEvent(object source, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (clientSocket == null || newDateTime.AddMinutes(3) < DateTime.Now) //判断Socket是否为空，或者最后一条数据到现在是否大于3分钟
                {
                    if (PubField.devNetName.Contains("病情"))
                    {
                        PubField.devNetName.Remove("病情");
                        DevOverviewMain.devRunNetCountUpdata();
                    }
                    //  form.SetNetStatus("断开");
                    myTimer.Enabled = false;
                    Debug.WriteLine("定时器中止");
                    stopConnect();
                    //Thread.Sleep(10000);
                    Thread.Sleep(10000);

                    connectToSever(); //重连
                }
                else if (clientSocket != null)
                {
                    sendKeepLive(); //发送心跳帧
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        /*
         * 发送位置信息
         */
        public void sendLocation(double lat, double lon)
        {
            try
            {
                LocationMsg location = new LocationMsg();

                location.devId = global.devid;
                location.func = 102;
                location.err = "";
                location.message.lat = lat;
                location.message.lon = lon;
                SendMsg(location.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        /* 发送工作模式
         */
        public void sendWorkMode(String runflag)
        {
            try
            {
                WorkModeMsg location = new WorkModeMsg();

                location.devId = global.devid;
                location.func = 136;
                location.err = "";
                location.model = runflag;
                SendMsg(location.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /*
         * 发送连接保持帧
         */
        public void sendKeepLive()
        {
            try
            {
                KeepLive lc = new KeepLive();
                lc.message = "keep-alive";
                lc.devId = global.devid;
                lc.func = 100;
                lc.err = "";
                SendMsg(lc.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        /*连接服务器*/
        public bool connectToSever()
        {
            bool isConnected = false;
            try
            {
                /*连接服务器*/
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                clientSocket.Connect(global.host, global.port);
                if (clientSocket.Connected)
                {
                    newDateTime = DateTime.Now;
                    DebOutPut.WriteLog(LogType.Normal, "与服务器连接成功！");
                    Receive(clientSocket);
                    isConnected = true;
                    if (!PubField.devNetName.Contains("病情"))
                    {
                        PubField.devNetName.Add("病情");
                    }
                    DevOverviewMain.devRunNetCountUpdata();
                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, "与服务器连接失败——" + ex.ToString());
                isConnected = false;
            }
            finally
            {
                ///*开启定时器，定时发送心跳帧 */
                //if (myTimer != null)
                //{
                //    myTimer.Enabled = false;
                //    myTimer = null;
                //}
                if (myTimer == null)
                {
                    InitTimer();
                    myTimer.Enabled = true;

                }
                else if (myTimer.Enabled == false)
                {

                    myTimer.Enabled = true;
                    Debug.WriteLine("定时器启用");

                }


            }

            return isConnected;

        }
        

        private static void Receive(Socket client)
        {
            try
            {
                if (client != null && client.Connected)
                {
                    StateObject state = new StateObject();
                    state.workSocket = client;
                    client.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), state);

                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket socket = state.workSocket;
            try
            {

                if (socket != null && socket.Connected)
                {
                    int length = socket.EndReceive(ar);
                    string message = Encoding.GetEncoding("gb2312").GetString(state.buffer, 0, length);
                    response = message;
                    if (message != "")
                    {
                        form.dealMsg(response);//处理消息
                        newDateTime = DateTime.Now;
                        socket.BeginReceive(state.buffer, 0, StateObject.BUFFER_SIZE, 0, new AsyncCallback(ReceiveCallback), state);
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
      
        /*
         * 关闭Socket
         */
        public void closeSocket()
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    Thread.Sleep(10);
                    clientSocket.Disconnect(false);
                    DebOutPut.DebLog("断开连接");
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
            finally
            {
                if (clientSocket != null)
                {
                    clientSocket.Close();
                    clientSocket = null;
                }
            }
        }

        /*
         * 发送信息
         */
        public bool SendMsg(String cmd)
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    byte[] sendBytes = Encoding.GetEncoding("gb2312").GetBytes(cmd + "\r\n");
                    int n = clientSocket.Send(sendBytes);
                    if (n != sendBytes.Length)
                    {
                        DebOutPut.WriteLog(LogType.Normal, "发送失败!");
                        return false;
                    }
                    else
                    {
                        DebOutPut.DebLog("发送:" + cmd);
                        return true;
                    }
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, "网络Send通讯异常——" + ex.Message);
                closeSocket();
                return false;
            }
        }

        /// <summary>
        /// 数据发送
        /// </summary>
        /// <param name="cmd">图片信息</param>
        /// <param name="logT">输出日志</param>
        /// <returns></returns>
        public bool SendMsg(String cmd, string logT)
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    byte[] sendBytes = Encoding.GetEncoding("gb2312").GetBytes(cmd + "\r\n");
                    int n = clientSocket.Send(sendBytes);
                    if (n != sendBytes.Length)
                    {
                        DebOutPut.DebLog("发送失败!");
                        return false;
                    }
                    else
                    {
                        DebOutPut.DebLog("发送成功!");
                        return true;
                    }
                }
                else
                    return false;

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, "网络Send通讯异常——" + ex.Message);
                closeSocket();
                return false;
            }
        }

        /// <summary>
        /// 上传图片数据
        /// </summary>
        /// <param name="time">采集时间</param>
        /// <param name="path">图片路径</param>
        public bool SendPicMsg(string time, string path)
        {
            try
            {
                InfoPicMsg infopic = new InfoPicMsg();
                infopic.devId = global.devid;
                infopic.devtype = 2;
                infopic.func = 101;
                infopic.err = "";

                picMsg pic = new picMsg();
                pic.collectTime = time;
                pic.picStr = getBase64FromPic(path);
                infopic.message = pic;

                InfoPicMsg mg = new InfoPicMsg();
                mg.devId = global.devid;
                mg.devtype = 2;
                mg.func = 101;
                mg.err = "";
                picMsg pic1 = new picMsg();
                pic1.collectTime = time;
                pic1.picStr = "图片大小：" + pic.picStr.Length.ToString();
                mg.message = pic1;

                return SendMsg(infopic.ObjectToJson(), mg.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }

        }

        public void SendCurrentImage(string time)
        {
            try
            {
                InfoPicMsg infopic = new InfoPicMsg();
                infopic.devId = global.devid;
                infopic.devtype = 2;
                infopic.func = 130;
                infopic.err = "";

                picMsg pic = new picMsg();
                pic.collectTime = time;
                pic.picStr = getBase64FromPic(Param.BasePath + "\\BZ10Config\\BZ10GrabImg\\tempImg.bmp");
                infopic.message = pic;

                InfoPicMsg mg = new InfoPicMsg();
                mg.devId = global.devid;
                mg.devtype = 2;
                mg.func = 130;
                mg.err = "";
                picMsg pic1 = new picMsg();
                pic1.collectTime = time;
                pic1.picStr = "图片大小：" + pic.picStr.Length.ToString();
                mg.message = pic1;

                SendMsg(infopic.ObjectToJson(), mg.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());

            }

        }

        /*回应状态*/
        public void Replay(int replay, string status, string err)
        {
            try
            {
                ReplayMsg msg = new ReplayMsg();
                msg.func = replay;
                msg.err = err;
                msg.devId = global.devid; ;
                msg.message = status;
                string str = msg.ObjectToJson();
                SendMsg(msg.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }



        /*回应载物台位置*/
        public void ReplayLoc(int replay, string status, string err)
        {
            try
            {
                ReplayLocation msg = new ReplayLocation();
                msg.func = replay;
                msg.err = err;
                msg.devId = global.devid; ;
                msg.location = status;
                SendMsg(msg.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        class SlideGlassCount
        {
            public int func { set; get; }
            public string err { set; get; }
            public string devId { set; get; }
            public Message message { set; get; }
            public string ObjectToJson()
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                return jsonSerialize.Serialize(this);
            }
        }
        class Message
        {
            public int nums { set; get; }
            public int workMode { set; get; }
            public float temp { set; get; }
        }

        class CurrActive
        {
            public int func { set; get; }
            public string err { set; get; }
            public string devId { set; get; }
            public string message { set; get; }//0.原点 1.推片 2.滴加粘附液 3.收集 4.滴加培养液 5.回收 6.复位
            public string ObjectToJson()
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                return jsonSerialize.Serialize(this);
            }
        }

        /// <summary>
        /// 设备当前状态和当前动作
        /// </summary>
        class devStatus
        {
            public string devId { set; get; }
            public string err { set; get; }
            public int func { set; get; }
            public string message { set; get; }
            public string location { set; get; }

            public string ObjectToJson()
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                return jsonSerialize.Serialize(this);
            }
        }

        /// <summary>
        /// 获取设备当前异常状态和当前动作
        /// </summary>
        /// <param name="replay">设备编码</param>
        /// <param name="err">错误信息</param>
        /// <param name="status">当前状态</param>
        /// <param name="location">当前位置</param>
        public void senddevStatus(int replay, String err, String status, string location)
        {
            try
            {
                devStatus msg = new devStatus();
                msg.func = replay;
                msg.err = err;
                msg.devId = global.devid;
                msg.message = status;
                msg.location = location;
                SendMsg(msg.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 发送当前动作
        /// </summary>
        /// <param name="func"></param>
        /// <param name="err"></param>
        /// <param name="active"></param>
        public void SendCurrAction(int func, string err, string active)
        {
            try
            {
                CurrActive currActive = new CurrActive();
                currActive.func = func;
                currActive.err = err;
                currActive.devId = global.devid;
                currActive.message = active;
                SendMsg(currActive.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 发送载玻片数量和当前工作模式
        /// </summary>
        /// <param name="replay">功能码</param>
        /// <param name="err">错误信息</param>
        /// <param name="remain">载玻片数量</param>
        /// <param name="currMode">当前模式</param>
        /// <param name="temp">腔体温度</param>
        public void SendSlideGlassCount(int replay, String err, int remain, int currMode, float temp)
        {
            try
            {
                SlideGlassCount msg = new SlideGlassCount();
                msg.func = replay;
                msg.err = err;
                msg.devId = global.devid;
                Message message = new Message();
                message.nums = remain;
                message.workMode = currMode;
                message.temp = temp;
                msg.message = message;
                SendMsg(msg.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        public class timespan
        {
            public string time1 { set; get; }
            public string time2 { set; get; }
            public string time3 { set; get; }
            public string time4 { set; get; }
            public string time5 { set; get; }

        }
        class worktimes
        {
            public string devId { set; get; }
            public string err { set; get; }
            public int func { set; get; }

            public String timecontrol { set; get; }

            public string ObjectToJson()
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                return jsonSerialize.Serialize(this);
            }
        }

        /* 发送设备时间段*/
        public void sendtimeControl(int replay, String err, String timecontrl)
        {
            try
            {
                worktimes msg = new worktimes();
                msg.func = replay;
                msg.err = err;
                msg.devId = global.devid;
                msg.timecontrol = timecontrl;

                SendMsg(msg.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }


        }
        /// <summary>
        /// 发送设备参数
        /// </summary>
        /// <param name="hour">时</param>
        /// <param name="time">分</param>
        /// <param name="sampleminute">采集时间</param>
        /// <param name="samplemStrength">采集强度</param>
        /// <param name="peiyangyeCount">培养液量</param>
        /// <param name="fanshilinCount">粘附液量</param>
        /// <param name="peiyangTime">培养时间</param>
        /// <param name="minSteps">最小步数</param>
        /// <param name="maxSteps">最大步数</param>
        /// <param name="clearCount">单点选图</param>
        /// <param name="leftMaxSteps">左侧步数</param>
        /// <param name="rightMaxSteps">右侧步数</param>
        /// <param name="liftRightClearCount">多点选图</param>
        /// <param name="liftRightMoveInterval">移动间隔</param>
        /// <param name="isBug">调试、正常</param>
        public void sendDevParam(int hour, int time, int sampleminute, int samplemStrength, int peiyangyeCount, int fanshilinCount, int peiyangTime, int minSteps, int maxSteps, int clearCount, int leftMaxSteps, int rightMaxSteps, int liftRightClearCount, int liftRightMoveInterval, int fanStrengthMax, int fanStrengthMin, int tranStepsMin, int tranStepsMax, int tranClearCount, int xCorrecting, int yCorrecting, int yJustRange, int yNegaRange, int yInterval, int yJustCom, int yNageCom, int yFirst, int yCheck, int isBug)
        {
            try
            {
                devParam dev = new devParam();
                dev.func = 110;
                dev.err = "";
                dev.devId = global.devid; ;
                param parm = new param();
                parm.collectHour = hour;
                parm.collectTime = time;
                parm.sampleMinutes = sampleminute;
                parm.samplemStrength = samplemStrength;
                parm.cultureCount = peiyangyeCount;
                parm.VaselineCount = fanshilinCount;
                parm.cultureTime = peiyangTime;
                parm.minSteps = minSteps;
                parm.maxSteps = maxSteps;
                parm.clearCount = clearCount;
                parm.leftMaxSteps = leftMaxSteps;
                parm.rightMaxSteps = rightMaxSteps;
                parm.liftRightClearCount = liftRightClearCount;
                parm.liftRightMoveInterval = liftRightMoveInterval;
                parm.fanStrengthMax = fanStrengthMax;
                parm.fanStrengthMin = fanStrengthMin;
                parm.tranStepsMin = tranStepsMin;
                parm.tranStepsMax = tranStepsMax;
                parm.tranClearCount = tranClearCount;
                parm.xCorrecting = xCorrecting;
                parm.yCorrecting = yCorrecting;
                parm.yJustRange = yJustRange;
                parm.yNegaRange = yNegaRange;
                parm.yInterval = yInterval;
                parm.yJustCom = yJustCom;
                parm.yNageCom = yNageCom;
                parm.yFirst = yFirst;
                parm.yCheck = yCheck;
                parm.isBug = isBug;//1是调试，0是在正常
                dev.message = parm;
                SendMsg(dev.ObjectToJson());
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        public string getBase64FromPic(string picPath)
        {
            try
            {
                FileStream fs = new FileStream(picPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                long size = fs.Length;
                byte[] array = new byte[size];
                fs.Read(array, 0, array.Length);
                fs.Close();
                Base64Encoder en = new Base64Encoder();
                return en.GetEncoded(array);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }


        }
        public static string byteToHexStr(byte[] bytes, int n)
        {
            try
            {
                string returnStr = "";
                if (bytes != null)
                {
                    for (int i = 0; i < n; i++)
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

    }

    public class GpsParam
    {
        public string address { get; set; }
        public GpsContent content { get; set; }
        public int status { get; set; }
        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }
    public class GpsContent
    {
        public string address { get; set; }
        //public string address_detail { get; set; }
        public CenterPoint point { get; set; }

    }
    public class CenterPoint
    {
        public string x { get; set; }
        public string y { get; set; }

    }

}
