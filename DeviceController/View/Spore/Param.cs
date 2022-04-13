using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using ADOX;
using System.IO.Ports;
using System.Drawing;
using System.Threading;
using System.Net;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using cn.bmob.io;
using BZ10.Common;
using DeviceController.Common;

namespace BZ10
{
    class Param
    {
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        //配置文件路径 Application.StartupPath
        //public static string BasePath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
        public static string BasePath = System.Windows.Forms.Application.StartupPath;
        //服务器地址
        public static string UploadIP = "";
        public static string UploadPort = "";

        //设备编号
        public static string DeviceID = "";
        //采集时间
        public static string CollectHour = "";
        public static string CollectMinute = "";
        //设备运行标记
        public static string RunFlag = "";
        //串口端口号
        public static string SerialPortName = "";
        public static String SerialPortGpsName = "";
        public static String SerialPortHjName = "";
        public static string SerialPortCamera = "";

        //玻片矫正
        public static string slideCorrection = "";
        //采集模式  0恒定  1双值
        public static string FanMode = "0";
        //风扇开启时间
        public static String FanMinutes = "";
        //风扇开启强度
        public static String FanStrength = "";
        //风扇开启最大强度  1双值  2递减
        public static string FanStrengthMax = "";
        //风扇开启最小强度  1双值  2递减
        public static string FanStrengthMin = "";

        //培养液数量
        public static String peiyangye = "";
        //粘附液滴液数量
        public static String fanshilin = "";

        //培养时间
        public static String peiyangtime = "";

        //原点对焦起始步数
        public static String MinSteps = "";
        //原点对焦结束步数
        public static String MaxSteps = "";
        //原点选图
        public static string clearCount = "1";

        //正向补偿
        public static string tranStepsMin = "";
        //负向补偿
        public static string tranStepsMax = "";
        //多位选图
        public static string tranClearCount = "";



        //移动轴一电机拍照右侧最大步数
        public static string rightMaxSteps = "";
        //移动轴一电机拍照左侧最大步数
        public static string leftMaxSteps = "";
        //移动轴一电机拍照移动间隔
        public static string moveInterval = "";
        //多位复选
        public static string liftRightClearCount = "1";



        /*工作时间段*/
        public static String work1 = "";
        public static String work2 = "";
        public static String work3 = "";
        public static String work4 = "";
        public static String work5 = "";

        /*剩余载玻片数量*/
        public static string remain = "0";

        public static List<WorkTime> worllist = new List<WorkTime>();
        public static string version = "1";//成为为标准版还是定制版  1标准版  2定制版
        public static string dataType = "";//时间格式
        //相机版本    1：老版相机U口相机    2：新版海康相机
        public static string cameraVersion = "";
        //采集新数据查询间隔，单位分钟
        public static string collectNewDataQueryInterval = "1";



        //阈值补偿  适应阈值T(x, y)。通过计算每个像素周围bxb大小像素块的加权均值并减去常量threshold得到
        public static string compensate = "-1";
        //选图方案  0：连续法 连续法是以包围状面积最大处向上选和向下选    1：间断法 间断法是只选出包围状面积最大的图像
        public static string mapSelectionScheme = "1";

        public static string YJustRange = "";//纵向正距
        public static string YNegaRange = "";//纵向负距
        public static string YInterval = "";//纵向间隔
        public static string YJustCom = "";//纵向正补
        public static string YNageCom = "";//纵向负补
        public static string YFirst = "";//纵向首选
        public static string YCheck = "";//纵向复选
        public static string XCorrecting = "";//横向矫正
        public static string YCorrecting = "";//纵向矫正

        public static string isSoftKeyBoard = "0";//屏幕键盘  0开启  1关闭
        public static string DripDevice = "0";//滴液装置  0蠕动泵  1注射器
        public static string recoveryDevice = "0";//回收装置 0代表50mm长的轴四轴长 1代表70mm长的轴四轴长
        public static string SingleAspiration = "";//粘附液单次吸液推液量（用于一键吸液推液）
        public static string AspirationCount = "";//粘附液吸液推液次数（用于一键吸液推液）
        public static string AspirationIntervalMs = "";//粘附液吸液推液间隔（用于一键吸液推液）
        /// <summary>
        /// 读取配置文件参数
        /// </summary>
        /// <param name="configfileName">配置文件名称</param>
        /// <param name="key"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string Read_ConfigParam(string configfileName, string key, string name)
        {
            try
            {
                string configPath = BasePath + "\\BZ10Config\\" + configfileName;
                StringBuilder stringBuilder = new StringBuilder(255);
                GetPrivateProfileString(key, name, "", stringBuilder, 255, configPath);
                return stringBuilder.ToString();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }

        }
        //设置配置文件参数
        public static void Set_ConfigParm(string configfileName, string key, string name, string value)
        {
            try
            {
                string configPath = BasePath + "\\BZ10Config\\" + configfileName;
                WritePrivateProfileString(key, name, value, configPath);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        /// <summary>
        /// 初始化读取程序参数
        /// </summary>
        /// <param name="configfileName">配置文件名称</param>
        public static void Init_Param(string configfileName)
        {
            try
            {
                DebOutPut.DebLog("初始化参数");
                UploadIP = Read_ConfigParam(configfileName, "Config", "UploadIP");//服务器IP
                UploadPort = Read_ConfigParam(configfileName, "Config", "UploadPort");//服务器端口
                DeviceID = Read_ConfigParam(configfileName, "Config", "DeviceID");//设备编号
                CollectHour = Read_ConfigParam(configfileName, "Config", "CollectHour");//定时 时
                CollectMinute = Read_ConfigParam(configfileName, "Config", "CollectMinute");//定时 分
                SerialPortName = Read_ConfigParam(configfileName, "Config", "SerialPort");//主控串口
                SerialPortGpsName = Read_ConfigParam(configfileName, "Config", "SerialPortGps");//GPS串口
                SerialPortHjName = Read_ConfigParam(configfileName, "Config", "SerialPortHj");//环境串口
                SerialPortCamera = Read_ConfigParam(configfileName, "Config", "SerialPortCamera");//副控串口
                if (SerialPortCamera == "")
                {
                    Random rd = new Random();
                    string ComValue = "COM" + rd.Next(1, 20).ToString();
                    while (ComValue == SerialPortName || ComValue == SerialPortGpsName || ComValue == SerialPortCamera)
                        ComValue = "COM" + rd.Next(1, 20).ToString();
                    SerialPortCamera = ComValue;
                }
                RunFlag = Read_ConfigParam(configfileName, "Config", "RunFlag");//运行模式
                FanMinutes = Read_ConfigParam(configfileName, "Config", "FanMinutes");//采集时间
                FanStrength = Read_ConfigParam(configfileName, "Config", "FanStrength");//采集强度
                FanMode = Read_ConfigParam(configfileName, "Config", "FanMode");//采集模式
                if (FanMode == "")
                    FanMode = "0";
                FanStrengthMax = Read_ConfigParam(configfileName, "Config", "FanStrengthMax");//最大采强
                if (FanStrengthMax == "")
                    FanStrengthMax = "0";
                FanStrengthMin = Read_ConfigParam(configfileName, "Config", "FanStrengthMin");//最小采强
                if (FanStrengthMin == "")
                    FanStrengthMin = "0";
                peiyangye = Read_ConfigParam(configfileName, "Config", "peiyangye");//培养液量
                fanshilin = Read_ConfigParam(configfileName, "Config", "fanshilin");//粘附液量
                peiyangtime = Read_ConfigParam(configfileName, "Config", "peiyangtime");//培养时间
                work1 = Read_ConfigParam(configfileName, "Config", "work1");//工作时段1
                work2 = Read_ConfigParam(configfileName, "Config", "work2");//工作时段2
                work3 = Read_ConfigParam(configfileName, "Config", "work3");//工作时段2
                work4 = Read_ConfigParam(configfileName, "Config", "work4");//工作时段3
                work5 = Read_ConfigParam(configfileName, "Config", "work5");//工作时段5
                remain = Read_ConfigParam(configfileName, "Config", "remain");//载玻片量
                if (int.Parse(remain) < 0)
                    remain = "0";
                version = Read_ConfigParam(configfileName, "Config", "version");//系统版本
                dataType = Read_ConfigParam(configfileName, "Config", "dataType");//日期格式
                if (dataType == "0")//时间格式为：yyyy-MM-dd HH:mm:ss
                    dataType = "yyyy-MM-dd HH:mm:ss";
                else if (dataType == "1")//时间格式为：yyyy/MM/dd HH:mm:ss
                    dataType = "yyyy/MM/dd HH:mm:ss";
                else
                {
                    if (UploadIP == "testfood.cn" && UploadPort == "9126")
                        dataType = "yyyy-MM-dd HH:mm:ss";
                    else
                        dataType = "yyyy/MM/dd HH:mm:ss";
                }
                cameraVersion = Read_ConfigParam(configfileName, "Config", "CameraVersion");//相机版本
                collectNewDataQueryInterval = Read_ConfigParam(configfileName, "Config", "collectNewDataQueryInterval");//上传间隔
                if (collectNewDataQueryInterval == "")
                    collectNewDataQueryInterval = "1";
                compensate = Read_ConfigParam(configfileName, "Config", "Compensate");//阈值补偿
                if (compensate == "")
                    compensate = "-1";
                mapSelectionScheme = Read_ConfigParam(configfileName, "Config", "MapSelectionScheme");//选图方案
                if (mapSelectionScheme == "")
                    mapSelectionScheme = "1";
                MaxSteps = Read_ConfigParam(configfileName, "Config", "MaxSteps");//原位终止
                MinSteps = Read_ConfigParam(configfileName, "Config", "MinSteps");//原位起始
                clearCount = Read_ConfigParam(configfileName, "Config", "ClearCount");//原位选图
                if (clearCount == "")
                    clearCount = "5";
                leftMaxSteps = Read_ConfigParam(configfileName, "Config", "LeftMaxSteps");//横向正距
                if (leftMaxSteps == "")
                    leftMaxSteps = "0";
                rightMaxSteps = Read_ConfigParam(configfileName, "Config", "RightMaxSteps");//横向负距
                if (rightMaxSteps == "")
                    rightMaxSteps = "0";
                liftRightClearCount = Read_ConfigParam(configfileName, "Config", "LiftRightClearCount");//横向复选
                if (liftRightClearCount == "")
                    liftRightClearCount = "0";
                moveInterval = Read_ConfigParam(configfileName, "Config", "LiftRightMoveInterval");//横向间隔
                if (moveInterval == "")
                    moveInterval = "0";

                tranStepsMin = Read_ConfigParam(configfileName, "Config", "tranStepsMin");//横向正补
                if (tranStepsMin == "")
                    tranStepsMin = "0";
                tranStepsMax = Read_ConfigParam(configfileName, "Config", "tranStepsMax");//横向负补
                if (tranStepsMax == "")
                    tranStepsMax = "0";
                tranClearCount = Read_ConfigParam(configfileName, "Config", "tranClearCount");//横向首选
                if (tranClearCount == "")
                    tranClearCount = "0";

                slideCorrection = Read_ConfigParam(configfileName, "Config", "slideCorrection");//玻片矫正
                if (slideCorrection == "")
                    slideCorrection = "0";
                YJustRange = Read_ConfigParam(configfileName, "Config", "YJustRange");//纵向正距
                if (YJustRange == "")
                    YJustRange = "0";
                YNegaRange = Read_ConfigParam(configfileName, "Config", "YNegaRange");//纵向负距
                if (YNegaRange == "")
                    YNegaRange = "0";
                YInterval = Read_ConfigParam(configfileName, "Config", "YInterval");//纵向间隔
                if (YInterval == "")
                    YInterval = "0";
                YJustCom = Read_ConfigParam(configfileName, "Config", "YJustCom");//纵向正补
                if (YJustCom == "")
                    YJustCom = "0";
                YNageCom = Read_ConfigParam(configfileName, "Config", "YNageCom");//纵向负补
                if (YNageCom == "")
                    YNageCom = "0";
                YFirst = Read_ConfigParam(configfileName, "Config", "YFirst");//纵向首选
                if (YFirst == "")
                    YFirst = "0";
                YCheck = Read_ConfigParam(configfileName, "Config", "YCheck");//纵向复选
                if (YCheck == "")
                    YCheck = "0";
                XCorrecting = Read_ConfigParam(configfileName, "Config", "XCorrecting");//横向矫正ss
                if (XCorrecting == "")
                    XCorrecting = "0";
                YCorrecting = Read_ConfigParam(configfileName, "Config", "YCorrecting");//纵向矫正
                if (YCorrecting == "")
                    YCorrecting = "0";
                isSoftKeyBoard = Read_ConfigParam(configfileName, "Config", "IsSoftKeyBoadrd");//屏幕键盘
                if (isSoftKeyBoard == "")
                    isSoftKeyBoard = "1";
                DripDevice = Read_ConfigParam(configfileName, "Config", "DripDevice");//滴液装置
                if (DripDevice == "")
                    DripDevice = "0";
                recoveryDevice = Read_ConfigParam(configfileName, "Config", "RecoveryDevice");//回收装置
                if (recoveryDevice == "")
                    recoveryDevice = "0";

                SingleAspiration = Read_ConfigParam(configfileName, "Config", "SingleAspiration");//一键吸液推液量
                if (SingleAspiration == "")
                    SingleAspiration = "200";

                AspirationCount = Read_ConfigParam(configfileName, "Config", "AspirationCount");//一键吸液推液次数
                if (AspirationCount == "")
                    AspirationCount = "100";

                AspirationIntervalMs = Read_ConfigParam(configfileName, "Config", "AspirationIntervalMs");//一键吸液推液间隔毫秒
                if (AspirationIntervalMs == "")
                    AspirationIntervalMs = "500";




                initWorkTimeArray();
                MainForm.updataConfigShow();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        private static void initWorkTimeArray()
        {
            try
            {
                worllist.Clear();
                WorkTime time1 = new WorkTime(Param.work1);
                worllist.Add(time1);
                WorkTime time2 = new WorkTime(Param.work2);
                worllist.Add(time2);
                WorkTime time3 = new WorkTime(Param.work3);
                worllist.Add(time3);
                WorkTime time4 = new WorkTime(Param.work4);
                worllist.Add(time4);
                WorkTime time5 = new WorkTime(Param.work5);
                worllist.Add(time5);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(List<byte> bytes)
        {
            try
            {
                string returnStr = "";
                if (bytes != null)
                {
                    for (int i = 0; i < bytes.Count; i++)
                    {
                        returnStr += bytes[i].ToString("X2") + " ";
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
        public static string byteToHexStr(byte[] bytes)
        {
            try
            {
                string returnStr = "";
                if (bytes != null)
                {
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        returnStr += bytes[i].ToString("X2") + " ";
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

        public static bool SaveImage(Image img, string imgName)
        {
            try
            {
                string path = BasePath + "\\BZ10Config\\BZ10GrabImg\\";
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
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, "保存图片失败！" + ex.ToString());
                return false;
            }

        }
    }

    class DB_BZ10
    {
        public static OleDbConnection SqlConn;
        //初始化数据库
        public static bool DBInit()
        {
            string DataBasePath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\BZ10Config\\BZ10data.mdb";
            try
            {
                string strconn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + DataBasePath;

                if (File.Exists(DataBasePath))
                {
                    SqlConn = new OleDbConnection(strconn);
                    SqlConn.Open();
                    return true;
                }
                else
                {
                    //新建数据库
                    CreateDatabase(DataBasePath);
                    //新建表
                    SqlConn = new OleDbConnection(strconn);
                    SqlConn.Open();
                    string sql = "CREATE TABLE Record(ID AUTOINCREMENT,Flag TEXT(10),CollectTime TEXT(50))";
                    int a = updateDatabase(sql);
                    sql = "CREATE TABLE Param(ID AUTOINCREMENT,Type TEXT(10),Name TEXT(50))";
                    a += updateDatabase(sql);
                    if (a == 2)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }
        }
        /// <summary>
        /// 在access数据库中创建表
        /// </summary>
        public static void CreateDatabase(string filePath)
        {
            try
            {
                //新建数据库
                ADOX.Catalog catalog = new Catalog();
                string conn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + filePath + ";Jet OLEDB:Engine Type=5";
                catalog.Create(conn);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        //更新数据库
        public static int updateDatabase(string sql)
        {
            int x = -1;
            try
            {
                OleDbCommand oc = new OleDbCommand();
                oc.CommandText = sql;
                oc.CommandType = CommandType.Text;
                oc.Connection = SqlConn;
                x = oc.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                if (sql.Contains("insert"))
                {
                    DebOutPut.WriteLog(LogType.Error, "数据库insert操作出错：" + ex.Message);
                }
                else if (sql.Contains("delete"))
                {
                    DebOutPut.WriteLog(LogType.Error, "数据库delete操作出错：" + ex.Message);
                }
                else if (sql.Contains("update"))
                {
                    DebOutPut.WriteLog(LogType.Error, "数据库update操作出错：" + ex.Message);
                }
            }
            return x;
        }
        //查询
        public static DataSet QueryDatabase(string sql)
        {

            DataSet ds = new DataSet();
            try
            {
                OleDbDataAdapter da = new OleDbDataAdapter(sql, SqlConn);
                da.Fill(ds);

            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                if (ds.Tables.Count == 0)
                {
                    DebOutPut.WriteLog(LogType.Error, "数据库查询失败：" + ex.Message);
                }
                else
                {
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                }
            }

            return ds;
        }
        //关闭
        public static void CloseDatabaseConnection()
        {
            SqlConn.Close();
        }
    }

 
    class ImageItem
    {

        public ImageItem(int id, string path, string time)
        {
            this.ID = id;
            this.path = path;

            this.CollectTime = time;

            ImageSource imgtemp = new BitmapImage(new Uri(path, UriKind.Relative));
            this.Image1 = imgtemp;

        }
        public int ID { get; set; }
        public string path { get; set; }
        public string CollectTime { get; set; }
        public ImageSource Image1 { get; set; }

    }
  
    public class TimeRoot
    {
        public List<string> timecontrol = new List<string>();
    }
    //public class SofeVersion : BmobTable
    //{
    //    public string version_i { get; set; }
    //    public string version { get; set; }
    //    public string update_log { get; set; }
    //    public string target_size { get; set; }
    //    public BmobBoolean isforced { get; set; }
    //    public BmobFile filePath { get; set; }


    //    public string UpdatedInstruction { get; set; }
    //    public override void readFields(BmobInput input)
    //    {
    //        base.readFields(input);
    //        this.version_i = input.getString("version_i");
    //        this.version = input.getString("version");
    //        this.update_log = input.getString("update_log");
    //        this.target_size = input.getString("target_size");
    //        this.isforced = input.getBoolean("isforced");
    //        this.filePath = input.getFile("filePath");
    //        this.UpdatedInstruction = input.getString("UpdatedInstruction");
    //    }


    //}

}
