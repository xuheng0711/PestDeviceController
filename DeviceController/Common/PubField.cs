/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.Common
/// 文件名称    ：PubField.cs
/// 命名空间    ：DeviceController.Common
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/19 9:31:49 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：公共字段
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2019. All rights reserved.
/// ***********************************************************************
using cn.bmob.io;
using DeviceController.DAL.DataUpload;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceController.Common
{
    /// <summary>
    /// 项目名称 ：DeviceController.Common
    /// 命名空间 ：DeviceController.Common
    /// 类 名 称 ：PubField
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/19 9:31:49 
    /// 更新时间 ：2019/11/19 9:31:49
    /// </summary>
    public class PubField
    {
        
        public static Mutex mutex = null;
        /// <summary>
        /// 打开窗体委托
        /// </summary>
        /// <param name="objForm">要打开的窗体</param>
        /// <param name="objForm">父窗体</param>
        public delegate void EventOpenForm(Form objForm, SplitterPanel spPanel);
        public static EventOpenForm eventOpenForm = Tools.OpenForm;

        /// <summary>
        /// 重新打开窗体委托
        /// </summary>
        /// <param name="objForm"></param>
        /// <param name="sonForm"></param>
        public delegate void AgainOpenMdi(Form objForm, Form sonForm);
        public static AgainOpenMdi againOpenMdi = Tools.AgainOpenMdiForm;

        /// <summary>
        /// 获取启动了应用程序的可执行文件的路径，不包括可执行文件的名称。
        /// </summary>
        public static string basePath = Application.StartupPath;
        

        /// <summary>
        /// 运行中的设备数量
        /// </summary>
        public static List<string> devRunName = new List<string>();
        
        /// <summary>
        /// 联网设备数量
        /// </summary>
        public static List<string> devNetName = new List<string>();

        /// <summary>
        /// 异常数量
        /// </summary>
        public static int abnormalCount = 0;


        #region 小气候
        
        /// <summary>
        /// 小气候配置文件名称
        /// </summary>
        public static string climateConfigName = "ClimateConfig.ini";


        /// <summary>
        /// 小气候json文件名称
        /// </summary>
        public static string climateJsonFileName = "ClimateDataJson.txt";


        /// <summary>
        /// 小气候的环境类型
        /// </summary>
        public static Dictionary<string, string> ClimateType = new Dictionary<string, string>();

        #endregion 小气候
        
        #region CQ12

        /// <summary>
        /// CQ12配置文件名称
        /// </summary>
        public static string cQ12ConfigName = "CQ12Config.ini";

        #endregion CQ12
    }

    /// <summary>
    /// 上传数据的数据类型
    /// </summary>
    public enum UploadDataType
    {
        Nothing,//无
        Climate,//小气候
        ClimateKeepLive,//小气候保活
        CQ12,//CQ12
        CQ12KeepLive,//CQ12保活
    }

    /// <summary>
    /// Url类型（根据Url的类型，选择要请求的接口）
    /// </summary>
    public enum UrlType
    {
        AllBugTypeInfo,//所有害虫类型信息
        GetFactoryCode,//获取设备出厂码
    }

    /// <summary>
    /// 保存的Json数据类型
    /// </summary>
    public enum JsonDataType
    {
        ClimateData,
    }

    /// <summary>
    /// 请求接口根地址                     
    /// </summary>
    public class UrlRoots
    {
         public static string UrlRootInterface = "http://www.clever-argi.com:4050";
        //public static string UrlRootInterface = "http://39.98.155.166";
        
    }
    /// <summary>
    /// 日志类型
    /// </summary>
    public enum LogType
    {
        /// <summary>
        /// 正常日志
        /// </summary>
        Normal,
        /// <summary>
        /// 错误日志
        /// </summary>
        Error,
    }

    /// <summary>
    /// 工作模式
    /// </summary>
    public enum CQ12WorkMode
    {
        /// <summary>
        /// 正常
        /// </summary>
        Normal,
        /// <summary>
        /// 调试
        /// </summary>
        Debug,
        /// <summary>
        /// 时间段
        /// </summary>
        TimeSolt,
        /// <summary>
        /// 强制
        /// </summary>
        Force,
    }

    public class NewVersion : BmobTable
    {
        public string exeName { get; set; }
        public string exeUrl { get; set; }
        public string currVersion { get; set; }
        public BmobBoolean isForced { get; set; }
        public string channel { get; set; }
        public string md5 { get; set; }
        public override void readFields(BmobInput input)
        {
            base.readFields(input);
            this.exeName = input.getString("exeName");
            this.exeUrl = input.getString("exeUrl");
            this.currVersion = input.getString("currVersion");
            this.isForced = input.getBoolean("isForced");
            this.channel = input.getString("channel");
            this.md5 = input.getString("md5");
        }
    }
}
