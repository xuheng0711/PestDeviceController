/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.DAL.Climate
/// 文件名称    ：CollectionInfo.cs
/// 命名空间    ：DeviceController.DAL.Climate
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/19 12:10:46 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：小气候采集
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
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace DeviceController.DAL.Climate
{
    /// <summary>
    /// 项目名称 ：DeviceController.DAL.Climate
    /// 命名空间 ：DeviceController.DAL.Climate
    /// 类 名 称 ：CollectionInfo
    /// 作    者 ：ZhaoXinYu
    /// 创建时间 ：2019/11/19 12:10:46
    /// 更新时间 ：2019/11/19 12:10:46
    /// </summary>
    public class ClimateCollectionInfo
    {
        //代表33个指令是否有回复 1有回复  0无回复
        public static int[] isExist = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
        public static int index = 0;
        //是否检测传感器
        public static bool isTesting = true;
        /// <summary>
        /// 获取采集信息
        /// </summary>
        public static void GetCollertionInfo(SerialPort serialPort)
        {
            try
            {
                #region 协议格式 1
                ClimateMainForm.whichAgreement = 1;
                index = 0;
                if (isExist[index] == 1)
                {
                    string msg1 = "AA 00 64 01 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg1);
                }

                index = 1;
                if (isExist[index] == 1)
                {
                    string msg2 = "AA 00 64 02 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg2);
                }

                index = 2;
                if (isExist[index] == 1)
                {
                    string msg3 = "AA 00 64 03 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg3);
                }

                index = 3;
                if (isExist[index] == 1)
                {
                    string msg4 = "AA 00 64 04 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg4);
                }

                index = 4;
                if (isExist[index] == 1)
                {
                    string msg5 = "AA 00 64 05 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg5);
                }

                //旧的风速风向
                index = 5;
                if (isExist[index] == 1)
                {
                    isNew = 0;
                    string msg6 = "AA 00 64 06 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg6);
                }

                index = 6;
                if (isExist[index] == 1)
                {
                    isNew = 0;
                    string msg7 = "AA 00 64 07 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg7);
                }

                index = 7;
                if (isExist[index] == 1)
                {
                    string msg8 = "AA 00 64 08 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg8);
                }

                index = 8;
                if (isExist[index] == 1)
                {
                    string msg9 = "AA 00 64 0A A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg9);
                }

                index = 9;
                if (isExist[index] == 1)
                {
                    string msg10 = "AA 00 64 0B A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg10);
                }

                index = 10;
                if (isExist[index] == 1)
                {
                    string msg11 = "AA 00 64 0C A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg11);
                }

                index = 11;
                if (isExist[index] == 1)
                {
                    string msg12 = "AA 00 64 0D A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg12);
                }

                index = 12;
                if (isExist[index] == 1)
                {
                    string msg13 = "AA 00 64 0E A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg13);
                }

                index = 13;
                if (isExist[index] == 1)
                {
                    string msg14 = "AA 00 64 0F A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg14);
                }


                index = 14;
                if (isExist[index] == 1)
                {
                    string msg15 = "AA 00 64 31 A5";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg15);
                }

                #endregion 协议格式 1

                #region 协议格式 3
                ClimateMainForm.whichAgreement = 3;

                index = 15;
                if (isExist[index] == 1)
                {
                    string msg16 = "09 03 00 00 00 01 85 42";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg16);
                }


                index = 16;
                if (isExist[index] == 1)
                {
                    string msg17 = "15 03 00 00 00 01 87 1E";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg17);
                }


                index = 17;
                if (isExist[index] == 1)
                {
                    string msg18 = "1D 03 00 00 00 01 86 56";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg18);
                }


                index = 18;
                if (isExist[index] == 1)
                {
                    string msg19 = "20 03 00 00 00 01 82 BB";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg19);
                }


                index = 19;
                if (isExist[index] == 1)
                {
                    string msg20 = "21 03 00 00 00 01 83 6A";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg20);
                }

                #endregion  协议格式 3

                #region 协议格式 5
                ClimateMainForm.whichAgreement = 5;

                index = 20;
                if (isExist[index] == 1)
                {

                }
                string msg21 = "1E 03 00 00 00 01 86 65";
                SerialPortCtrl.DataSend_Climate(serialPort, msg21);

                #endregion 协议格式 5

                #region 协议格式 4
                ClimateMainForm.whichAgreement = 4;

                index = 21;
                if (isExist[index] == 1)
                {
                    string msg22 = "14 03 00 00 00 02 C6 CE";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg22);
                }

                index = 22;
                if (isExist[index] == 1)
                {
                    string msg23 = "10 03 00 00 00 02 C7 4A";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg23);
                }
                #endregion 协议格式 4

                #region 协议格式 6
                ClimateMainForm.whichAgreement = 6;

                index = 23;
                if (isExist[index] == 1)
                {
                    string msg24 = "16 03 01 00 00 02 D0 C6";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg24);
                }

                #endregion 协议格式 6

                #region 协议格式 7
                ClimateMainForm.whichAgreement = 7;

                index = 24;
                if (isExist[index] == 1)
                {
                    string msg25 = "17 03 00 2A 00 01 A7 34";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg25);

                }

                index = 25;
                if (isExist[index] == 1)
                {
                    string msg26 = "18 03 00 2A 00 01 A7 34";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg26);
                }

                #endregion 协议格式 7

                #region 协议格式8
                ClimateMainForm.whichAgreement = 8;
                //新的风向
                index = 26;
                if (isExist[index] == 1)
                {
                    isNew = 1;
                    string msg29 = "06 03 00 00 00 01 85 BD";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg29);
                }

                //新的风速
                index = 27;
                if (isExist[index] == 1)
                {
                    isNew = 1;
                    string msg30 = "07 03 00 00 00 01 84 6C";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg30);
                }

                //新的雨量
                index = 28;
                if (isExist[index] == 1)
                {
                    string msg31 = "0A 03 00 00 00 01 85 71";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg31);
                }

                #endregion

                #region 协议格式9
                ClimateMainForm.whichAgreement = 9;
                //新的温度
                index = 29;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement9Add = "01";
                    string msg32 = "01 03 00 01 00 01 D5 CA";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg32);
                }

                //新的湿度
                index = 30;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement9Add = "02";
                    string msg33 = "01 03 00 02 00 01 25 CA";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg33);
                }

                //新的光照度
                index = 31;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement9Add = "05";
                    string msg34 = "01 03 00 05 00 01 94 0B";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg34);
                }

                //新的大气压
                index = 32;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement9Add = "04";
                    string msg35 = "01 03 00 04 00 02 85 CA";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg35);
                }
                #endregion

                #region 协议格式10
                ClimateMainForm.whichAgreement = 10;
                //土壤湿度
                index = 33;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement10Add = "12";
                    string msg36 = "03 03 00 12 00 01 25 ED";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg36);
                }
                //土壤温度
                index = 34;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement10Add = "13";
                    string msg37 = "03 03 00 13 00 01 74 2D";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg37);
                }
                //土壤PH
                index = 35;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement10Add = "06";
                    string msg38 = "03 03 00 06 00 01 65 E9";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg38);
                }
                //土壤EC(电导率)
                index = 36;
                if (isExist[index] == 1)
                {
                    ClimateMainForm.agreement10Add = "15";
                    string msg39 = "03 03 00 15 00 01 94 2C";
                    SerialPortCtrl.DataSend_Climate(serialPort, msg39);
                }

                #endregion

                isTesting = false;
                //数据输出
                //if (!serialPort.IsOpen) { return; }
                //ClimateMessage climateMessage = ClimateMainForm.climateMessage;
                //for (int i = 0; climateMessage != null && climateMessage.environments != null && i < climateMessage.environments.Count; i++)
                //{
                //    DebOutPut.DebLog(climateMessage.environments[i].name + "=" + climateMessage.environments[i].value);

                //}
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        public static int isNew = 0;//是否是新的风速协议  0代表旧的  1代表新的

        /// <summary>
        /// 斜率查询，不同的值类型，斜率不同
        /// </summary>
        /// <param name="valueType">值类型</param>
        /// <returns>返回当前值类型的斜率</returns>
        public static float ValueTypeHandle(string valueType)
        {
            float Slope = -1;
            try
            {
                switch (valueType)
                {
                    case "01":
                        Slope = 0.1f;
                        break;
                    case "02":
                        Slope = 0.1f;
                        break;
                    case "03":
                        Slope = 0.1f;
                        break;
                    case "04":
                        Slope = 1f;
                        break;
                    case "05":
                        Slope = 0.1f;
                        break;
                    case "06":
                        Slope = 1f;
                        break;
                    case "07":
                        if (isNew == 0)
                        {
                            Slope = 0.001f;
                        }
                        else if (isNew == 1)
                        {
                            Slope = 0.1f;
                        }
                        break;
                    case "08":
                        Slope = 1f;
                        break;
                    case "0A":
                        Slope = 0.1f;
                        break;
                    case "0B":
                        Slope = 0.1f;
                        break;
                    case "0C":
                        Slope = 0.1f;
                        break;
                    case "0D":
                        Slope = 0.01f;
                        break;
                    case "0E":
                        Slope = 1f;
                        break;
                    case "0F":
                        Slope = 0.01f;
                        break;
                    case "31":
                        Slope = 1f;
                        break;
                    case "09":
                        Slope = 1f;
                        break;
                    case "15":
                        Slope = 1f;
                        break;
                    case "1D":
                        Slope = 1f;
                        break;
                    case "20":
                        Slope = 0.01f;
                        break;
                    case "21":
                        Slope = 1f;
                        break;
                    case "14":
                        Slope = 0.01f;
                        break;
                    case "10":
                        Slope = 1f;
                        break;
                    case "1E":
                        Slope = 0.1f;
                        break;
                    case "16":
                        Slope = 1f;
                        break;
                    case "17":
                        Slope = 0.1f;
                        break;
                    case "18":
                        Slope = 0.1f;
                        break;
                    case "19":
                        Slope = 0.01f;
                        break;
                    case "99":
                        Slope = 1f;
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

            return Slope;
        }
    }
}
