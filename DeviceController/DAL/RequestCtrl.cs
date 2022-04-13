/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.DAL
/// 文件名称    ：RequestCtrl.cs
/// 命名空间    ：DeviceController.DAL
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/23 13:38:19 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：接口请求
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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeviceController.DAL
{
    /// <summary>
    /// 项目名称 ：DeviceController.DAL
    /// 命名空间 ：DeviceController.DAL
    /// 类 名 称 ：RequestCtrl
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/23 13:38:19 
    /// 更新时间 ：2019/11/23 13:38:19
    /// </summary>
    public class RequestCtrl
    {
        /// <summary>
        /// 请求接口控制器
        /// </summary>
        /// <param name="urlType">请求接口的类型</param>
        /// <param name="urlRoots">请求接口的根路径</param>
        /// <param name="saveDataModel">保存接口返回数据的模型</param>
        /// <param name="Param">传给接口的参数</param>
        /// <returns></returns>
        public static string RequestSelect(UrlType urlType, string urlRoots, params object[] Param)
        {
            string result = "";
            try
            {
                //判断接口类型
                switch (urlType)
                {
                    case UrlType.AllBugTypeInfo://害虫
                        result = Request(urlType, urlRoots + "/agro_monitor/TCP_equipment/getPestList.action?deviceCoding=" + Param[0]);
                        break;
                    case UrlType.GetFactoryCode://获取设备出厂码
                        result = Request(urlType, urlRoots + "/agro_monitor/TCP_equipment/getEqNum.action?deviceCoding=" + Param[0]);
                        break;
                    default:
                        break;
                }
                return result;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }
        }


        /// <summary>
        /// 发起请求
        /// </summary>
        /// <param name="url">请求的路径</param>
        /// <param name="saveDataModel">保存返回数据的数据模型</param>
        /// <returns></returns>
        public static string Request(UrlType urlType, string url)
        {
            string result = "";
            try
            {
                IntPtr size = new IntPtr(0);
                //请求数据
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                //请求方式
                req.Method = "POST";
                //提交数据方式
                req.ContentType = "application/x-www-form-urlencoded";
                //获取响应
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                //获取流
                Stream stream = resp.GetResponseStream();
                //获取响应内容
                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                {
                    result = reader.ReadToEnd();
                    //判断类型保存进对应的模型中
                    switch (urlType)
                    {
                        case UrlType.AllBugTypeInfo://害虫类型
                            SaveDataModel.bugTypeModel = JsonConvert.DeserializeObject<BugTypeModel>(result);
                            break;
                        case UrlType.GetFactoryCode:
                            SaveDataModel.getDevFactoryCodeRoot= JsonConvert.DeserializeObject<GetDevFactoryCodeRoot>(result);
                            break;
                        default:
                            break;
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }
        }

        /// <summary>
        /// 获取本机经纬度根据IPv4
        /// </summary>
        public static void SearchIPAddress()
        {
            try
            {
                //省市、经纬度查询
                WebClient client = new WebClient();
                string url = @"http://api.map.baidu.com/location/ip?ak=CRGeKEM99NlIQM3uO6K6vG9H66sHrzPS&coor=bd09ll";
                string jsonData = client.DownloadString(url);
                SaveDataModel.iPAddressRoot = JsonConvert.DeserializeObject<IPAddressRoot>(jsonData);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
           
        }

    }
}
