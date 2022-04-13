using BZ10.Common;
using DeviceController.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace BZ10
{
    public class Tools
    {
        

        /// <summary>
        /// 求指定时间和当前时间相差的秒数
        /// </summary>
        /// <param name="time">指定时间</param>
        /// <param name="currTime">当前时间</param>
        /// <returns></returns>
        public static string GetNowTimeSpanSec(DateTime time, DateTime currTime)
        {
            //指定时间减去当前时间
            TimeSpan ts = time.Subtract(currTime);
            int sec = (int)ts.TotalSeconds;
            return sec.ToString();
        }

        /// <summary>
        /// 判断字符串是否是数字
        /// </summary>
        public static bool IsNumber(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            const string pattern = "^[0-9]*$";
            Regex rx = new Regex(pattern);
            return rx.IsMatch(s);
        }

        /// <summary>
        /// 验证是否为大写
        /// </summary>
        /// <param name="strValue"></param>
        /// <returns></returns>
        public static bool IsAllUpChar(string strValue)
        {
            bool result = Regex.IsMatch(strValue, @"^[A-Z]+$");
            return result;
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
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error, ex.ToString());
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
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 串口检测
        /// </summary>
        public static List<string> SerialPortTesting()
        {
            try
            {
                //HKEY_LOCAL_MACHINE\HARDWARE\DEVICEMAP\SERIALCOMM
                RegistryKey hklm = Registry.LocalMachine;
                RegistryKey software11 = hklm.OpenSubKey("HARDWARE");
                RegistryKey software = software11.OpenSubKey("DEVICEMAP");
                RegistryKey sitekey = software.OpenSubKey("SERIALCOMM");
                if (sitekey == null)
                {
                    return null;
                }
                //获取当前子键下所有项名字
                string[] termName = sitekey.GetValueNames();
                List<string> termValue = new List<string>();
                //获得当前子键下项的值
                for (int i = 0; termName != null && i < termName.Length; i++)
                {
                    termValue.Add((string)sitekey.GetValue(termName[i]));
                }
                if (termValue.Count > 0)
                {
                    return termValue;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error, ex.ToString());
                return null;
            }
        }

        /// <summary>
        /// 软件重新启动
        /// </summary>
        public static void RestStart()
        {
            DebOutPut.WriteLog( LogType.Normal,"程序被重启！");
            PubField.mutex.Close();
            string path = System.Windows.Forms.Application.ExecutablePath;
            Win32API.ShellExecute(IntPtr.Zero, "open", path, "/S", "", ShellExecute_ShowCommands.SW_SHOWNORMAL);
            System.Environment.Exit(0);
        }

        /// <summary>
        /// 计算机重启
        /// </summary>
        public static void WinRestart()
        {
            DebOutPut.DebLog( "计算机自动重启！");
            DebOutPut.WriteLog( LogType.Normal, "计算机自动重启");
            System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
            myProcess.StartInfo.FileName = "cmd.exe";//启动cmd命令
            myProcess.StartInfo.UseShellExecute = false;//是否使用系统外壳程序启动进程
            myProcess.StartInfo.RedirectStandardInput = true;//是否从流中读取
            myProcess.StartInfo.RedirectStandardOutput = true;//是否写入流
            myProcess.StartInfo.RedirectStandardError = true;//是否将错误信息写入流
            myProcess.StartInfo.CreateNoWindow = true;//是否在新窗口中启动进程
            myProcess.Start();//启动进程
            myProcess.StandardInput.WriteLine("shutdown -r -t 0");//执行重启计算机命令
        }

        /// <summary>
        /// 进程使用的内存交换到虚拟内存,将加载过程不需要的代码放到虚拟内存，这样，程序加载完毕后，保持较大的可用内存。
        /// 在程序刚刚加载时使用一次
        /// </summary>
        public static void ClearMemory()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (System.Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                Win32API.SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            }
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
                DebOutPut.WriteLog( LogType.Error, ex.ToString());
                return null;
            }
            catch (ArgumentException ex)
            {
                DebOutPut.WriteLog( LogType.Error, ex.ToString());
                return null;
            }
            finally
            {
                stream.Close();
            }
        }

        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="path">文件路径</param>
        public static string GetFileSize(string path)
        {
            try
            {
                if (path != null && path != "")
                {
                    if (File.Exists(path))
                    {
                        FileInfo fileInfo = new FileInfo(path);
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(path);
                        string size = (fileInfo.Length / 1024.0 / 1024.0).ToString();
                        return size;
                    }
                }
                return "";
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                return "";
            }
        }
        
    }

    public class GlobalParam
    {
        public string devid = "okq201812001";
        public string host = "192.168.1.66";
        public int port = 9126;
    }
    //{"devId":"okq20170907003","err":"","func":100,"message":"keep-alive"}
    class KeepLive
    {

        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public string message { set; get; }


        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }


    class LocationMsg
    {

        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public locatin message = new locatin();


        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }

    class WorkTime //工作时间段 
    {
        public int starth;
        public int startm;

        public int endh;
        public int endm;

        public bool bstatus = false;//该时间段是否正在执行
        public WorkTime(String str)
        {
            String start, end;
            start = str.Substring(0, str.IndexOf("-"));
            end = str.Substring(str.IndexOf("-") + 1);
            starth = Convert.ToInt16(start.Substring(0, str.IndexOf(":")));
            startm = Convert.ToInt16(start.Substring(str.IndexOf(":") + 1));

            endh = Convert.ToInt16(end.Substring(0, str.IndexOf(":")));
            endm = Convert.ToInt16(end.Substring(str.IndexOf(":") + 1));
        }
        public String toString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(String.Format("{0:D2}", starth)).Append(":").Append(String.Format("{0:D2}", startm)).Append("-").Append(String.Format("{0:D2}", endh)).Append(":").Append(String.Format("{0:D2}", endm));
            return sb.ToString();
        }
    }
    class WorkModeMsg
    {

        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public string model { set; get; }


        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }
    class locatin
    {
        public double lat { set; get; }
        public double lon { set; get; }

    }



    // {"devId":"okq20170907003","err":"","devtype":1,"func":101,"message":{"collectTime":"2017/10/10 17:28:44","environments":[{"name": "温度","value": 17.5}]}}

    class ReplayMsg
    {
        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public string message { set; get; }

        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }
    class ReplayLocation //回应载物台位置
    {
        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public string location { set; get; }

        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }
    class InfoMsg
    {
        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public Message message = new Message();

        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }

    class Message
    {
        public string collectTime { set; get; }
        public List<Environment> environments = new List<Environment>();
    }


    class Environment
    {
        public Environment(string a, double b)
        {
            name = a;
            value = b;
        }

        public string name { set; get; }
        public double value { set; get; }
    }

    class InfoPicMsg
    {
        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public int devtype { set; get; }

        public picMsg message = new picMsg();
        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            jsonSerialize.MaxJsonLength = int.MaxValue;
            return jsonSerialize.Serialize(this);
        }
    }
    class picMsg
    {

        public string collectTime { set; get; }
        public string picStr { set; get; }
        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }
    class devParam
    {
        public string devId { set; get; }
        public string err { set; get; }
        public int func { set; get; }
        public param message = new param();

        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }
    class param
    {
        public int collectHour { set; get; }
        public int collectTime { set; get; }
        public int sampleMinutes { set; get; }
        public int samplemStrength { set; get; }
        public int cultureCount { set; get; }//培养液量
        public int VaselineCount { set; get; }//粘附液量
        public int cultureTime { set; get; }//培养时间
        public int minSteps { set; get; }//最小步数
        public int maxSteps { set; get; }//最大步数
        public int clearCount { set; get; }//选图数量
        public int leftMaxSteps { set; get; }//左侧步数
        public int rightMaxSteps { set; get; }//右侧步数
        public int liftRightClearCount { set; get; }//多位复选
        public int liftRightMoveInterval { set; get; }//移动间隔
        public int fanStrengthMax { set; get; }//最大采强
        public int fanStrengthMin { set; get; }//最小采强
        public int tranStepsMin { set; get; }//正向补偿
        public int tranStepsMax { set; get; }//负向补偿
        public int tranClearCount { set; get; }//多位首选
        
        public int xCorrecting { set; get; }//横向校正
        public int yCorrecting { set; get; }//纵向校正
        public int yJustRange { set; get; }//纵向正距
        public int yNegaRange { set; get; }//纵向负距
        public int yInterval { set; get; }//纵向间隔
        public int yJustCom { set; get; }//纵向正补
        public int yNageCom { set; get; }//纵向负补
        public int yFirst { set; get; }//纵向首选
        public int yCheck { set; get; }//纵向复选
        public int isBug { set; get; }
        public string ObjectToJson()
        {
            JavaScriptSerializer jsonSerialize = new JavaScriptSerializer();
            return jsonSerialize.Serialize(this);
        }
    }

}
