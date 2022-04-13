/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.Common
/// 文件名称    ：SerialPortCtrl.cs
/// 命名空间    ：DeviceController.Common
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/19 14:23:25 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：串口控制
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2019. All rights reserved.
/// ***********************************************************************
using BZ10.Common;
using DeviceController.DAL.Climate;
using DeviceController.Models;
using DeviceController.View.Climate;
using DeviceController.View.CQ12;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceController.Common
{
    /// <summary>
    /// 项目名称 ：DeviceController.Common
    /// 命名空间 ：DeviceController.Common
    /// 类 名 称 ：SerialPortCtrl
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/19 14:23:25 
    /// 更新时间 ：2019/11/19 14:23:25
    /// </summary>
    public class SerialPortCtrl
    {
        /// <summary>
        /// 打开串口
        /// </summary>
        /// <param name="serialPort">串口</param>
        /// <param name="serialPortName">串口名</param>
        /// <returns></returns>
        public static bool SetSerialPortAttribute(SerialPort serialPort, string serialPortName,int baudRate)
        {
            try
            {
                String[] Portname = SerialPort.GetPortNames();
                int serialCount = 0;
                bool SerialInitOk = false;
                do
                {
                    try
                    {
                        if (Portname.Contains(serialPortName))
                        {
                            if (serialPort.IsOpen)
                                serialPort.Close();
                            serialPort.PortName = serialPortName;
                            serialPort.BaudRate = Convert.ToInt32(baudRate);
                            serialPort.ReceivedBytesThreshold = 1;
                            serialPort.Open();
                            SerialInitOk = true;
                        }
                        else
                        {
                            DebOutPut.DebLog(serialPortName + "打开故障");
                            DebOutPut.WriteLog(LogType.Normal, "串口:" + serialPortName + "打开故障");
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        serialCount++;
                        DebOutPut.DebLog("串口:" + serialPortName + " 初始化失败(第" + serialCount.ToString() + "次)——" + ex.Message);
                        DebOutPut.WriteLog(LogType.Error, "串口:" + serialPortName + " 初始化失败(第" + serialCount.ToString() + "次)——" + ex.Message);
                        SerialInitOk = false;
                    }
                    Thread.Sleep(100);
                } while (!SerialInitOk && serialCount != 4);
                return SerialInitOk;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }
        }

        /// <summary>
        /// 数据发送 小气候
        /// </summary>
        /// <param name="serialPort">serialPort</param>
        /// <param name="Msg">数据</param>
        public static void DataSend_Climate(SerialPort serialPort, string Msg)
        {
            try
            {
                if (serialPort == null && !serialPort.IsOpen)
                {
                    DebOutPut.DebLog("串口未打开!");
                    DebOutPut.WriteLog(LogType.Normal, "串口未打开!");
                    return;
                }
                serialPort.DiscardOutBuffer();
                //string recStr = "";
                //int ilen;
                //byte[] readBytes = null;
                int index = 0;
                byte[] bytes = Tools.HexStrTobyte(Msg);
                DebOutPut.DebLog("发:" + Msg.ToUpper());
                DebOutPut.WriteLog(LogType.Normal, "发:" + Msg.ToUpper());
                for (int i = 0; i < 5; i++)
                {
                    serialPort.Write(bytes, 0, bytes.Length);
                    Thread.Sleep(250);
                    if (serialPort.BytesToRead <= 0)
                        index++;
                    else
                        break;
                }
                if (index == 5)
                {
                    DebOutPut.DebLog("小气候数据读取指令连续5次发送未收到回复，指令为：" + Msg.ToUpper());
                    DebOutPut.WriteLog(LogType.Normal, "小气候数据读取指令连续5次发送未收到回复，指令为：" + Msg.ToUpper());
                    if (ClimateCollectionInfo.isTesting)
                        ClimateCollectionInfo.isExist[ClimateCollectionInfo.index] = 0;
                    return;
                }
                int ilen = serialPort.BytesToRead;
                byte[] readBytes = new byte[ilen];
                serialPort.Read(readBytes, 0, ilen);
                if (readBytes.Length <= 0) return;
                string recStr = Tools.ByteToHexStr(readBytes);//接收到的
                ClimateMainForm.climate_ReceivedDataHandle(recStr);
                serialPort.DiscardInBuffer();
                readBytes = null;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        /// <summary>
        /// 数据发送
        /// </summary>
        /// <param name = "serialPort" > serialPort </ param >
        /// < param name="Msg">数据</param>
        public static void DataSend_CQ12(SerialPort serialPort, string Msg)
        {
            try
            {
                if (serialPort == null && !serialPort.IsOpen)
                {
                    DebOutPut.DebLog("串口未打开!");
                    DebOutPut.WriteLog(LogType.Normal, "串口未打开!");
                    return;
                }
                serialPort.DiscardOutBuffer();
                byte[] bytes = Tools.HexStrTobyte(Msg);
                serialPort.Write(bytes, 0, bytes.Length);
                DebOutPut.DebLog("发送:" + Msg.ToUpper());
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return;
            }
        }

        /// <summary>
        /// 关闭串口
        /// </summary>
        /// <param name="serialPort">串口</param>
        /// <param name="devType">指示当前是CQ12还是OKTQ</param>
        /// <returns></returns>
        public static bool CloseSerialPort(SerialPort serialPort)
        {
            try
            {
                serialPort.Close();
                return true;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }
        }
    }
}
