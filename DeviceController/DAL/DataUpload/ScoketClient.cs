/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.DAL.DataUpload
/// 文件名称    ：ScoketClient.cs
/// 命名空间    ：DeviceController.DAL.DataUpload
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/20 17:08:54 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：Scoket客户端
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2019. All rights reserved.
/// ***********************************************************************
using BZ10.Common;
using DeviceController.Common;
using DeviceController.Models;
using DeviceController.View.Climate;
using DeviceController.View.CQ12;
using DeviceController.View.DevOverview;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceController.DAL.DataUpload
{
    /// <summary>
    /// 项目名称 ：DeviceController.DAL.DataUpload
    /// 命名空间 ：DeviceController.DAL.DataUpload
    /// 类 名 称 ：ScoketClient
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/20 17:08:54 
    /// 更新时间 ：2019/11/20 17:08:54
    /// </summary>
    public class ScoketClient
    {

        public Socket clientSocket;
        public string ip = "";
        public string port = "";
        public string devId = "";//设备ID
     
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ip">要连接的服务器IP，必传</param>
        /// <param name="port">要连接的服务器端口号，必传</param>
        public ScoketClient(string ip, string port)
        {
            try
            {
                this.ip = ip;
                this.port = port;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// Socket连接
        /// </summary>
        public void SocketConnect()
        {
            try
            {
                if (port == "")
                {
                    MessageBox.Show("上传端口不能为空!", "提示");
                    return;
                }
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));
                //连接服务器
                clientSocket.Connect(ipEndPoint);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString()+ "域名/IP:" + ip + " 无效，请关闭串口，确认参数设置无误后，重新打开!");
                DebOutPut.WriteLog(LogType.Error, ex.ToString() + "域名/IP:" + ip + " 无效，请关闭串口，确认参数设置无误后，重新打开!");
                //关闭连接
                SocketClose();
            }
        }

        /// <summary>
        /// 关闭连接
        /// </summary>
        public void SocketClose()
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    Thread.Sleep(10);
                    clientSocket.Disconnect(false);
                    DebOutPut.DebLog("断开连接");
                    DebOutPut.WriteLog(LogType.Normal, "CQ12断开连接");
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
        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="uploadDataType">上传数据的数据类型</param>
        public void UploadData(UploadDataType uploadDataType, string data,string collectTime=null)
        {
            byte[] sendBytesClimate = null;
            try
            {
                if (clientSocket == null || !clientSocket.Connected)
                {
                    DebOutPut.DebLog("上传失败");
                    return;
                }
                Encoding encoding = Encoding.GetEncoding("gb2312");
                switch (uploadDataType)
                {
                    case UploadDataType.Climate://小气候上传单条数据
                                                //发送字节
                        sendBytesClimate = encoding.GetBytes(data + "\r\n");
                        clientSocket.Send(sendBytesClimate);
                        DebOutPut.DebLog("Climate:发送:" + data);
                        ReceiveData(UploadDataType.Climate, collectTime);
                        break;
                    case UploadDataType.ClimateKeepLive://小气候的保活
                        //发送字节
                        sendBytesClimate = encoding.GetBytes(data + "\r\n");
                        clientSocket.Send(sendBytesClimate);
                        DebOutPut.DebLog("ClimateKeepLive:发送:" + data);
                        ReceiveData(UploadDataType.ClimateKeepLive);
                        break;

                    default:
                        break;
                }
                data = "";
                sendBytesClimate = null;
            }
            catch (Exception ex)
            {
                data = "";
                sendBytesClimate = null;
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                SocketClose();
            }
        }

        private static readonly object Lock = new object();

        /// <summary>
        /// 接收数据
        /// </summary>
        /// <param name="uploadDataType">接收数据的数据类型</param>
        public void ReceiveData(UploadDataType uploadDataType,string collectTime = null)
        {
            lock (Lock)
            {
                byte[] receive = null;
                string receiceMsg = "";
                try
                {
                    switch (uploadDataType)
                    {
                        case UploadDataType.Climate:
                           ClimateMainForm. newDataTime = DateTime.Now;
                            //接收服务器信息
                            receive = new byte[1024];
                            clientSocket.Receive(receive);
                            receiceMsg = Encoding.Default.GetString(receive);
                            DebOutPut.DebLog("Climate:收到数据:" + receiceMsg);
                            collectTime = "update Record Set Flag = '1' where CollectTime='" + collectTime + "'";
                            DB_Climate.updateDatabase(collectTime);
                            break;
                        case UploadDataType.ClimateKeepLive:
                            ClimateMainForm.newDataTime = DateTime.Now;
                            //接收服务器信息
                            receive = new byte[1024];
                            clientSocket.Receive(receive);
                            receiceMsg = Encoding.Default.GetString(receive);
                            DebOutPut.DebLog("ClimateKeepLive:收到数据:" + receiceMsg);
                            break;
                        default:
                            break;
                    }
                    receive = null;
                    receiceMsg = "";
                }
                catch (Exception ex)
                {
                    receive = null;
                    receiceMsg = "";
                    DebOutPut.DebErr(ex.ToString());
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                    SocketClose();
                }
            }
        }

    }
}
