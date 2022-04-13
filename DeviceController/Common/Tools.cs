/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.Common
/// 文件名称    ：Tools.cs
/// 命名空间    ：DeviceController.Common
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/18 17:21:34 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：工具类
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2019. All rights reserved.
/// ***********************************************************************
using BZ10.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Xml;

namespace DeviceController.Common
{
    /// <summary>
    /// 项目名称 ：DeviceController.Common
    /// 命名空间 ：DeviceController.Common
    /// 类 名 称 ：Tools
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/18 17:21:34 
    /// 更新时间 ：2019/11/18 17:21:34
    /// </summary>
    public class Tools
    {
        private static string[] directArr = new String[] { "北", "东北", "东北", "东北", "东", "东南", "东南", "东南", "南",
   "西南", "西南", "西南", "西", "西北", "西北", "西北" };

        public static string WindDirectionSwitch(float degrees)
        {
            int index = 0;
            if (348.75 <= degrees && degrees <= 360)
            {
                index = 0;
            }
            else if (0 <= degrees && degrees <= 11.25)
            {
                index = 0;
            }
            else if (11.25 < degrees && degrees <= 33.75)
            {
                index = 1;
            }
            else if (33.75 < degrees && degrees <= 56.25)
            {
                index = 2;
            }
            else if (56.25 < degrees && degrees <= 78.75)
            {
                index = 3;
            }
            else if (78.75 < degrees && degrees <= 101.25)
            {
                index = 4;
            }
            else if (101.25 < degrees && degrees <= 123.75)
            {
                index = 5;
            }
            else if (123.75 < degrees && degrees <= 146.25)
            {
                index = 6;
            }
            else if (146.25 < degrees && degrees <= 168.75)
            {
                index = 7;
            }
            else if (168.75 < degrees && degrees <= 191.25)
            {
                index = 8;
            }
            else if (191.25 < degrees && degrees <= 213.75)
            {
                index = 9;
            }
            else if (213.75 < degrees && degrees <= 236.25)
            {
                index = 10;
            }
            else if (236.25 < degrees && degrees <= 258.75)
            {
                index = 11;
            }
            else if (258.75 < degrees && degrees <= 281.25)
            {
                index = 12;
            }
            else if (281.25 < degrees && degrees <= 303.75)
            {
                index = 13;
            }
            else if (303.75 < degrees && degrees <= 326.25)
            {
                index = 14;
            }
            else if (326.25 < degrees && degrees < 348.75)
            {
                index = 15;
            }
            else
            {
                DebOutPut.WriteLog(LogType.Normal, "风向大于 360.0");
            }
            return directArr[index];
        }

        /// <summary>
        /// DateTime转时间戳
        /// </summary>
        /// <param name="time">DateTime时间</param>
        /// <param name="type">0为毫秒,1为秒</param>
        /// <returns></returns>
        public static string ConvertTimestamp(DateTime time, int type = 0)
        {
            double intResult = 0;
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            if (type == 0)
            {
                intResult = (time - startTime).TotalMilliseconds;
            }
            else if (type == 1)
            {
                intResult = (time - startTime).TotalSeconds;
            }
            else
            {
                Console.WriteLine("参数错误!");
            }
            return Math.Round(intResult, 0).ToString();
        }

        /// <summary>     
        /// 获取Md5码
        /// </summary>
        /// <param name="value">需要转换成Md5码的原码</param>
        /// <returns></returns>
        public static string Md5(string value)
        {
            var result = string.Empty;
            if (string.IsNullOrEmpty(value)) return result;
            using (var md5 = MD5.Create())
            {
                result = GetMd5Hash(md5, value);
            }
            return result;
        }
        static string GetMd5Hash(MD5 md5Hash, string input)
        {

            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            var sBuilder = new StringBuilder();
            foreach (byte t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }
        static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            var hashOfInput = GetMd5Hash(md5Hash, input);
            var comparer = StringComparer.OrdinalIgnoreCase;
            return 0 == comparer.Compare(hashOfInput, hash);
        }

        /// <summary>
        /// 图片读取
        /// </summary>
        /// <param name="fileName">路径</param>
        /// <returns></returns>
        public static Bitmap FileToBitmap(string fileName)
        {
            // 打开文件    
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            // 读取文件的 byte[]    
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();
            // 把 byte[] 转换成 Stream    
            Stream stream = new MemoryStream(bytes);

            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始    
            stream.Seek(0, SeekOrigin.Begin);

            MemoryStream mstream = null;
            try
            {
                mstream = new MemoryStream(bytes);
                Bitmap bmp = new Bitmap(stream);
                //return new Bitmap((Image)new Bitmap(stream));
                return bmp;
            }
            catch (ArgumentNullException ex)
            {
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }
            catch (ArgumentException ex)
            {
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// 获取当前登录用户(可用于管理员身份运行)
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentUser()
        {
            IntPtr buffer;
            uint strLen;
            int cur_session = -1;
            var username = "SYSTEM";
            if (Win32API.WTSQuerySessionInformation(IntPtr.Zero, cur_session, WTSInfoClass.WTSUserName, out buffer, out strLen) && strLen > 1)
            {
                username = Marshal.PtrToStringAnsi(buffer);
            }
            return username;
        }

        /// <summary>  
        /// 修改程序在注册表中的键值  
        /// </summary>  
        /// <param name="isAuto">true:开机启动,false:不开机自启</param> 
        public static void AutoStart(bool isAuto)
        {
            try
            {
                string ShortFileName = Application.ProductName;
                if (isAuto == true)
                {
                    //获取本地计算机的注册表
                    RegistryKey R_local = Registry.CurrentUser;
                    //注册表里面创建一个新子项或打开一个现有子项进行访问
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\");
                    string[] keys = R_run.GetValueNames();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        if (keys[i] == ShortFileName)
                        {
                            return;
                        }
                    }
                    //要执行文件的名称及路径
                    R_run.SetValue(ShortFileName, Application.ExecutablePath);
                    //关闭
                    R_run.Close();
                    R_local.Close();
                }
                else
                {
                    RegistryKey R_local = Registry.LocalMachine;
                    RegistryKey R_run = R_local.CreateSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run\");
                    R_run.DeleteValue(ShortFileName, false);//删除指定值
                    R_run.Close();
                    R_local.Close();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        
        /// <summary>
        /// 软件重新启动
        /// </summary>
        public static void RestStart()
        {
            try
            {
                PubField.mutex.Close();
                string path = System.Windows.Forms.Application.ExecutablePath;
                Win32API.ShellExecute(IntPtr.Zero, "open", path, "", "", ShellExecute_ShowCommands.SW_SHOWNORMAL);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                DebOutPut.DebErr(ex.ToString());
            }
           
        }

        /// <summary>
        /// 图片转Base64
        /// </summary>
        /// <param name="ImageFileName">图片</param>
        /// <returns></returns>
        public static string ImgToBase64(Image image)
        {
            try
            {

                using (Bitmap bmp = new Bitmap(image))
                {
                    //bmp = KiResizeImage(bmp, 1920, 1440);
                    MemoryStream ms = new MemoryStream();
                    bmp.Save(ms, ImageFormat.Jpeg);
                    byte[] arr = new byte[ms.Length];
                    ms.Position = 0;
                    ms.Read(arr, 0, (int)ms.Length);
                    ms.Close();
                    ms.Dispose();
                    return Convert.ToBase64String(arr);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr( ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
        static readonly object SequenceLock4 = new object();
        /// <summary>
        ///直接删除指定目录下的所有文件及文件夹(保留目录)
        /// </summary>
        /// <param name="strPath">文件夹路径</param>
        /// <returns>执行结果</returns>
        public static void DeleteAllDir(string file)
        {
            lock (SequenceLock4)
            {
                try
                {
                    //去除文件夹和子文件的只读属性
                    //去除文件夹的只读属性
                    System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
                    fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;
                    //去除文件的只读属性
                    System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);
                    //判断文件夹是否还存在
                    if (Directory.Exists(file))
                    {
                        foreach (string f in Directory.GetFileSystemEntries(file))
                        {
                            if (File.Exists(f))
                            {
                                //如果有子文件删除文件
                                File.Delete(f);
                                DebOutPut.DebLog(f);
                            }
                            else
                            {
                                //循环递归删除子文件夹
                                DeleteAllDir(f);
                            }
                        }
                        //删除空文件夹
                        Directory.Delete(file);
                    }

                }
                catch (Exception ex)
                {
                    DebOutPut.DebErr( ex.ToString());
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                }
            }


        }
        
        /// <summary>
        /// 保存图片到本地
        /// </summary>
        /// <param name="img">图片</param>
        /// <param name="imgName">图片名称</param>
        /// <returns></returns>
        public static bool SaveImage(Image img, string imgName)
        {
            try
            {
                string path = PubField.basePath + "\\CQ12Config\\CQ12GrabImg\\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (Directory.Exists(path))
                {
                    path += imgName;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    img.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr( ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }
        }
        public const int GWL_STYLE = -16;
        public const int WS_DISABLED = 0x8000000;

        /// <summary>
        /// 设置控件是否可点击状态，且不改变颜色
        /// </summary>
        /// <param name="c"></param>
        /// <param name="enabled"></param>
        public static void SetControlEnabled(Control c, bool enabled)
        {
            try
            {
                //c.Enabled = enabled;
                if (enabled)
                {
                    Win32API.SetWindowLong(c.Handle, GWL_STYLE, (~WS_DISABLED) & Win32API.GetWindowLong(c.Handle, GWL_STYLE));
                }
                else
                {
                    Win32API.SetWindowLong(c.Handle, GWL_STYLE, WS_DISABLED | Win32API.GetWindowLong(c.Handle, GWL_STYLE));
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 16进制转2进制
        /// </summary>
        /// <param name="sixStr">16进制字符串</param>
        /// <returns></returns>
        public static string SixteenToTwo(string sixStr)
        {
            try
            {
                if (sixStr == "")
                {
                    return "";
                }
                int s1 = Convert.ToInt32(sixStr, 16);//16进制转10进制
                string s2 = Convert.ToString(s1, 2);//10进制转2进制
                return s2;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 2进制转16进制
        /// </summary>
        /// <param name="twoStr">2进制字符串</param>
        /// <returns></returns>
        public static string TwoToSixteen(string twoStr)
        {
            try
            {
                if (twoStr == "")
                {
                    return "";
                }
                string bin = twoStr;
                return string.Format("{0:x}", Convert.ToInt32(bin, 2));
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error,  ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 判断当前时间是否在工作时间段内
        /// </summary>
        /// <param name="timeStr">当前时间</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <returns></returns>
        public static bool GetTimeSpan(string timeStr, string startTime, string endTime)
        {
            try
            {
                string _strWorkingDayAM = startTime;//工作时间上午08:30
                string _strWorkingDayPM = endTime;
                TimeSpan dspWorkingDayAM = DateTime.Parse(_strWorkingDayAM).TimeOfDay;
                TimeSpan dspWorkingDayPM = DateTime.Parse(_strWorkingDayPM).TimeOfDay;
                //string time1 = "2017-2-17 8:10:00";
                DateTime t1 = Convert.ToDateTime(timeStr);

                TimeSpan dspNow = t1.TimeOfDay;
                if (dspNow > dspWorkingDayAM && dspNow < dspWorkingDayPM)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }

        }

        /// <summary>
        /// 通过NetworkInterface获取MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMacByNetworkInterface()
        {
            try
            {
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in interfaces)
                {
                    string macAdd = BitConverter.ToString(ni.GetPhysicalAddress().GetAddressBytes()).Replace("-", ":");

                    return macAdd;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error,  ex.ToString());
            }
            return "00-00-00-00-00-00";
        }

        /// <summary>
        /// 获取本地主机的IPv4地址
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIp()
        {
            try
            {
                //获取本地的IP地址
                string AddressIP = string.Empty;
                foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                    {
                        AddressIP = _IPAddress.ToString();
                    }
                }
                return AddressIP;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error,  ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// Ascii转Hex
        /// </summary>
        /// <param name="str">Ascii字符串</param>
        /// <param name="charset">编码格式</param>
        /// <param name="fenge">是否已空格分离转换后的值</param>
        /// <returns></returns>
        public static string StrToHex(string str, string charset, bool fenge)
        {
            try
            {
                string result = "";
                Encoding encoding = Encoding.GetEncoding(charset);
                byte[] bytes = encoding.GetBytes(str);
                for (int i = 0; i < bytes.Length; i++)
                {
                    //将数据以16进制格式输出
                    result += string.Format("{0:X}", bytes[i]);
                    if (fenge && (i != bytes.Length - 1))
                    {
                        result += string.Format("{0}", " ");
                    }
                }
                return result.ToUpper();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error,  ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 数据模型转Json串
        /// </summary>
        /// <param name="obj">数据模型</param>
        /// <returns></returns>
        public static string ObjectToJson(object obj)
        {
            try
            {
                JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
                return jsonSerialize.Serialize(obj);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }
        }

        ///<summary>
        /// 传入域名返回对应的IP
        ///</summary>
        ///<param name="domain">域名</param>
        ///<returns></returns>
        public static string GetIP(string domain)
        {
            try
            {
                domain = domain.Replace("http://", "").Replace("https://", "");
                IPHostEntry hostEntry = Dns.GetHostEntry(domain);
                IPEndPoint ipEndPoint = new IPEndPoint(hostEntry.AddressList[0], 0);
                return ipEndPoint.Address.ToString();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error,  ex.ToString());
                return "";
            }

        }

        /// <summary>
        /// 检查输入字符串是否是IP
        /// </summary>
        /// <param name="strJudgeString">所输入字符串</param>
        /// <returns>是返回true，不是返回false</returns>
        public static bool IsItAIP(string str)
        {
            bool blnTest = false;
            bool _Result = true;
            try
            {

                Regex regex = new Regex("^[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}.[0-9]{1,3}$");
                blnTest = regex.IsMatch(str);
                if (blnTest == true)
                {
                    string[] strTemp = str.Split(new char[] { '.' });
                    int nDotCount = strTemp.Length - 1;
                    if (3 == nDotCount)//判断字符串中.的数量
                    {
                        for (int i = 0; i < strTemp.Length; i++)
                        {
                            if (Convert.ToInt32(strTemp[i]) > 255)
                            {
                                _Result = false;
                            }
                        }
                    }
                    else
                    {
                        _Result = false;
                    }
                }
                else
                {
                    _Result = false;
                }
            }
            catch (Exception ex)
            {
                _Result = false;
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
            return _Result;
        }

        /// <summary>
        /// 十进制负数转十六进制补码
        /// </summary>
        /// <param name="NegStr">十进制的负数</param>
        /// <returns></returns>
        public static string HexComCodeToNeg(string NegStr)
        {
            try
            {
                MemoryStream io = new MemoryStream();
                BinaryWriter bw = new BinaryWriter(io);
                bw.Write(int.Parse(NegStr));
                bw.Flush();
                BinaryReader br = new BinaryReader(io);
                io.Seek(0, SeekOrigin.Begin);
                byte[] b = br.ReadBytes(2);
                string str = HighLowInversion(BitConverter.ToString(b).Replace("-", ""));
                return str;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }


        /// <summary> 
        /// 十六进制转为负数
        ///</summary>
        ///<param name="strNumber">HEXSTRING</param>
        ///<returns></returns>
        public static int HexStringToNegative(string strNumber)
        {
            int iNegate = 0;
            int iNumber = Convert.ToInt32(strNumber, 16);
            if (iNumber > 127)
            {
                int iComplement = iNumber - 1;
                string strNegate = string.Empty;
                char[] binChar = Convert.ToString(iComplement, 2).PadLeft(8, '0').ToArray();
                foreach (char ch in binChar)
                {
                    if (Convert.ToInt32(ch) == 48)
                        strNegate += "1";
                    else
                        strNegate += "0";
                }
                iNegate = -Convert.ToInt32(strNegate, 2);
            }
            return iNegate;
        }

        /// <summary>
        /// 十六进制正负数转十进制负数
        /// </summary>
        /// <param name="NegStr">十六进制的负数</param>
        /// <returns>十六进制的负数</returns>
        public static string NegComCodeToHex(string hexStr)
        {
            try
            {
                hexStr = hexStr.Replace(" ", "");
                byte[] b = StrToHexByte(HighLowInversion(hexStr));
                System.IO.MemoryStream io = new System.IO.MemoryStream(b);
                System.IO.BinaryReader br = new System.IO.BinaryReader(io);
                return br.ReadInt16().ToString();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error,  ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 16进制字符串转16进制byte[]  例 string str="FF65";-->byte[]bytes={ 0xFF , 0x65 }
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] StrToHexByte(string hexString)
        {
            try
            {
                hexString = hexString.Replace(" ", "");
                if ((hexString.Length % 2) != 0)
                {
                    hexString += " ";
                }
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }
                return returnBytes;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 十进制转十六进制
        /// </summary>
        /// <param name="TenString">十进制字符串</param>
        /// <returns></returns>
        public static string TenToSixteen(string TenString)
        {
            try
            {
                if (TenString == null)
                {
                    return "";
                }
                string sixteenStr = Convert.ToString(ushort.Parse(TenString), 16);
                return sixteenStr.ToUpper();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 十六进制转十进制
        /// </summary>
        /// <param name="SixteenString">十六进制字符串</param>
        /// <returns></returns>
        public static string SixteenToTen(string SixteenString)
        {
            try
            {
                string TenString = Convert.ToInt32(SixteenString, 16).ToString();
                return TenString.ToUpper();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// hexString转byte[]
        /// </summary>
        /// <param name="hexString">hexString</param>
        /// <returns></returns>
        public static byte[] HexStrTobyte(string hexString)
        {
            try
            {
                hexString = hexString.Replace(" ", "");
                byte[] returnBytes = new byte[hexString.Length / 2];
                for (int i = 0; i < returnBytes.Length; i++)
                {
                    returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2).Trim(), 16);
                }
                return returnBytes;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// byte[]转hexString
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteToHexStr(byte[] bytes)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                if (bytes != null || bytes.Length > 0)
                {
                    foreach (var item in bytes)
                    {
                        sb.Append(item.ToString("x2"));
                    }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 高低反转
        /// </summary>
        /// <param name="str">16进制数据</param>
        /// <returns></returns>
        public static string HighLowInversion(string str16)
        {
            try
            {
                List<string> strList = new List<string>();
                for (int i = 0; i < str16.Length; i += 2)
                {
                    strList.Add(str16.Substring(i, 2));
                }
                string[] strArr = new string[strList.Count];
                strList.CopyTo(strArr);
                strList.Clear();
                Array.Reverse(strArr);
                string retStr = "";
                for (int i = 0; i < strArr.Length; i++)
                {
                    retStr += strArr[i];
                }
                return retStr;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 读取配置文件参数
        /// </summary>
        /// <param name="key">小节名称</param>
        /// <param name="name">项名称</param>
        /// <param name="path">配置文件路径</param>
        /// <returns></returns>
        public static string ConfigParmRead(string key, string name, string path)
        {
            try
            {
                string configPath = path;
                StringBuilder stringBuilder = new StringBuilder(255);
                Win32API.GetPrivateProfileString(key, name, "", stringBuilder, 255, configPath);
                string strb = stringBuilder.ToString();
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 设置配置文件参数
        /// </summary>
        /// <param name="key">小节名称</param>
        /// <param name="name">项名称</param>
        /// <param name="value">值</param>
        /// <param name="path">配置文件路径</param>
        public static void ConfigParmSet(string key, string name, string value, string path)
        {
            try
            {
                string configPath = path;
                Win32API.WritePrivateProfileString(key, name, value, configPath);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private const uint StringBufferSize = 32768;

        /// <summary>
        /// 获取选中小节中所有的项和值
        /// </summary>
        /// <param name="section">小节名字</param>
        /// <param name="path">配置文件路径</param>
        /// <returns></returns>
        public static List<string> GetSection(string section, string path)
        {
            try
            {
                sbyte[] lpReturnedString = new sbyte[StringBufferSize];
                uint len = Win32API.GetPrivateProfileSection(section, lpReturnedString, (uint)lpReturnedString.Length, path);

                List<string> items = new List<string>();
                unsafe
                {
                    fixed (sbyte* pBuf = lpReturnedString)
                    {
                        uint i = 0;
                        while (i < len)
                        {
                            uint start = i;
                            uint length = 0;
                            while (i < len && lpReturnedString[i] != 0)
                            {
                                ++i;
                            }
                            length = i - start;
                            items.Add(new string(pBuf,
                                 (int)start, (int)length, Encoding.Default));
                            ++i;
                            if (i < len && lpReturnedString[i] == 0)
                            {
                                break;
                            }
                        }
                    }
                }
                return items;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }
        }

        [System.Runtime.InteropServices.DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string lpApplicationName, string lpKeyName, string lpString, string lpFileName);
        /// <summary>
        /// 删除指定小节中的键
        /// </summary>
        /// <param name="section">小节的名称</param>
        /// <param name="key">键的名称</param>
        /// <param name="path">文件路径</param>
        /// <returns>执行成功为True，失败为False。</returns>
        public static long DeleteIniKey(string section, string key, string path)
        {
            try
            {
                if (section.Trim().Length <= 0 || key.Trim().Length <= 0)
                {
                    return 0;
                }
                return WritePrivateProfileString(section, key, null, path);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return 0;
            }
           
        }

        /// <summary>
        /// 打开窗体
        /// </summary>
        /// <param name="objForm">要打开的窗体</param>
        /// <param name="objForm">父窗体</param>
        public static void OpenForm(Form objForm, SplitterPanel spPanel)
        {
            try
            {
                ClosePreForm(spPanel);
                OpenNewForm(objForm, spPanel);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 打开新的窗体
        /// </summary>
        /// <param name="objForm">要打开的窗体</param>
        /// <param name="parent">父窗体</param>
        public static void OpenNewForm(Form objForm, Control parent)
        {
            try
            {
                if (objForm != null && parent != null)
                {
                    objForm.TopLevel = false;
                    objForm.FormBorderStyle = FormBorderStyle.None;
                    objForm.Parent = parent;
                    objForm.Dock = DockStyle.Fill;
                    objForm.Show();
                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 关闭所有嵌入的窗体
        /// </summary>
        /// <param name="parent">指定父物体</param>
        public static void ClosePreForm(Control parent)
        {
            try
            {
                if (parent != null)
                {
                    foreach (Control item in parent.Controls)
                    {
                        if (item is Form)
                        {
                            Form objControl = (Form)item;
                            objControl.Close();
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

        /// <summary>
        /// 打开Mid子窗体
        /// </summary>
        /// <param name="parentForm">父窗体</param>
        /// <param name="sonForm">子窗体</param>
        public static void OpenNewMdiForm(Form parentForm, Form sonForm)
        {
            try
            {
                sonForm.MdiParent = parentForm;
                sonForm.WindowState = FormWindowState.Normal;
                sonForm.MaximizeBox = false;
                sonForm.Dock = DockStyle.Fill;
                sonForm.Show();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());

            }

        }

        /// <summary>
        /// 重新打开指定的Mdi窗体
        /// </summary>
        /// <param name="sonForm">子窗体</param>
        public static void AgainOpenMdiForm(Form parentForm, Form sonForm)
        {
            try
            {
                for (int i = 0; i < parentForm.MdiChildren.Length; i++)
                {
                    if (parentForm.MdiChildren[i] is Form)
                    {
                        if (parentForm.MdiChildren[i].Text == sonForm.Text)
                        {
                            parentForm.MdiChildren[i].Close();
                        }
                    }
                }
                sonForm.MdiParent = parentForm;
                sonForm.WindowState = FormWindowState.Normal;
                sonForm.MaximizeBox = false;
                sonForm.Dock = DockStyle.Fill;
                sonForm.Show();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());

            }

        }

        /// <summary>
        /// 查找窗口是否存在,如果存在，则显示该窗口
        /// </summary>
        /// <param name="formName">窗口标题</param>
        /// <returns></returns>
        public static bool SearchMDIFormIsExist(Form[] mdiParentForm, string formName)
        {
            for (int i = 0; i < mdiParentForm.Length; i++)
            {
                if (mdiParentForm[i].Text == formName)
                {
                    for (int j = 0; j < mdiParentForm.Length; j++)
                    {
                        if (mdiParentForm[j].Text != formName)
                        {
                            //mdiParentForm[j].Visible = false;
                            // mdiParentForm[j].Hide();
                            //mdiParentForm[j].SendToBack();
                        }
                        else
                        {
                            //mdiParentForm[j].Show();
                            // mdiParentForm[j].Visible = true;
                            mdiParentForm[j].BringToFront();
                            // mdiParentForm[j].Activate();
                        }
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 时间转16进制 
        /// </summary>
        public static string TimeToSixteen(string timeStr)
        {
            try
            {
                string result = "";
                string[] arrTemp1 = timeStr.Split(' ');
                List<string> arrTemp = new List<string>();
                for (int i = 0; i < arrTemp1.Length; i++)
                {
                    if (arrTemp1[i] != "")
                    {
                        arrTemp.Add(arrTemp1[i]);
                    }
                }
                if (arrTemp.Count == 2)
                {
                    string[] arrDate = null;
                    if (arrTemp[0].Contains('/'))
                    {
                        arrDate = arrTemp[0].Split('/'); //日期
                    }
                    else if (arrTemp[0].Contains('-'))
                    {
                        arrDate = arrTemp[0].Split('-'); //日期
                    }
                    string[] arrTime = arrTemp[1].Split(':');//时间
                    for (int i = 0; i < arrDate.Length; i++)
                    {
                        string str = Convert.ToString(int.Parse(arrDate[i]), 16);
                        for (int j = str.Length; j < 4; j++)
                        {
                            str = "0" + str;
                        }
                        result += str;
                    }
                    for (int i = 0; i < arrTime.Length; i++)
                    {
                        string str = Convert.ToString(int.Parse(arrTime[i]), 16);
                        for (int j = str.Length; j < 4; j++)
                        {
                            str = "0" + str;
                        }
                        result += str;
                    }
                }
                return result.ToUpper();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// 将进程使用的内存交换到虚拟内存
        /// </summary> 
        public static void ExchangeMemory()
        {
            try
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    Win32API.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
             
            }
        }
    }
}
