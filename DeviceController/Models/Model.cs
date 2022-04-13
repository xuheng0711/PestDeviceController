/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.Models
/// 文件名称    ：Model.cs
/// 命名空间    ：DeviceController.Models
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/19 16:19:35 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2019. All rights reserved.
/// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceController.Models
{
    /// <summary>
    /// 项目名称 ：DeviceController.Models
    /// 命名空间 ：DeviceController.Models
    /// 类 名 称 ：Model
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/19 16:19:35 
    /// 更新时间 ：2019/11/19 16:19:35
    /// </summary>
    class Model
    {
    }

    /// <summary>
    /// CQ12配置文件参数
    /// </summary>
    public class CQ12ConfigParamModel
    {
        public string deviceID { set; get; }//设备ID
        public string uploadAddress { set; get; }//数据上传地址
        public string uploadPort { set; get; }//数据上传端口
        public string counterAddress { set; get; }//计数器地址
        public string alarmNumber { set; get; }//报警号码
        public string alarmSwitch { set; get; }//是否报警
        public string serialPortName { set; get; }//串口名字
        public string bugLimit { set; get; }//害虫限值
    }

    /// <summary>
    /// 小气候配置文件参数
    /// </summary>
    public class ClimateConfigModel
    {
        public string collectionInterval { get; set; }//采集间隔
        public string uploadInterval { get; set; }//上传间隔
        public string uploadAddress { get; set; }//上传地址
        public string uploadPort { get; set; }//上传端口
        public string siteName { get; set; }//站点名称
        public string modifyTime { get; set; }//修改时间
        public string alarmNum { get; set; }//报警号码
        public string alarmInterval { get; set; }//报警间隔
        public string devId { get; set; }//设备ID
        public string serialPortName { get; set; }//串口号
        public string runFlag { get; set; }//运行模式
    }


    /// <summary>
    /// CQ8设置终端参数
    /// </summary>
    public class SetParam
    {
        //帧头
        public string startChar { set; get; }//起始字符
        public string leght { set; get; }//长度
        //内容
        public string func { set; get; }//功能码
        public string address { set; get; }//设备编号
        public string param { set; get; }//参数
        //帧尾
        public string checkData { set; get; }//校验数据
        public string shiliuH { set; get; }//16H

    }


    #region 小气候设备采集信息
    /// <summary>
    /// 小气候设备采集信息
    /// </summary>
    public class ClimateModel
    {
        public int func { get; set; }//功能码
        public string err { get; set; }//错误信息
        public string devId { get; set; }//设备id
        public int devtype { get; set; }//设备类型
        public ClimateMessage message = new ClimateMessage();
    }

    public class ClimateMessage
    {
        public string collectTime { get; set; }//采集时间
        public List<ClimateEnvironmentsItem> environments = new List<ClimateEnvironmentsItem>();//环境信息
    }
    public class ClimateEnvironmentsItem
    {
        public string name { get; set; }//名字
        public string value { get; set; }//值
    }
    #endregion 小气候设备采集信息

    #region 小气候心跳
    /// <summary>
    /// 心跳包
    /// </summary>
    public class ClimateKeepLive
    {
        public string devId { set; get; }// 设备Id
        public string err { set; get; }// 错误信息
        public int func { set; get; }// 功能
        public string message { set; get; }// 消息


    }
    #endregion 小气候心跳

    /// <summary>
    /// 端口4562心跳包
    /// </summary>
    public class CQ8KeepLive
    {
        //帧头
        public string startChar { set; get; }//起始字符
        public string leght { set; get; }//长度
        //内容
        public string func { set; get; }//功能码
        public string address { set; get; }//设备编号
        public string electricQuantity { set; get; }//状态
        //帧尾
        public string checkData { set; get; }//校验数据
        public string shiliuH { set; get; }//16H
    }

    /// <summary>
    /// CQ12心跳包
    /// </summary>
    public class CQ12KeepLive
    {
        public string devId { set; get; }// 设备Id
        public string err { set; get; }// 错误信息
        public int func { set; get; }// 功能
        public string message { set; get; }// 消息

    }

    #region CQ12发送采集信息

    /// <summary>
    /// 智能虫情测报灯-CQ12虫情测报灯   采集信息-发送
    /// </summary>
    public class CollectInfoModel
    {
        public int func { get; set; }//功能码
        public string err { get; set; }//错误信息
        public string devId { get; set; }//设备Id
        public string devtype { get; set; }//设备类型
        public CollectInfoMessage message { get; set; }//消息
    }
    public class CollectInfoMessage
    {
        public string collectTime { get; set; }//采集时间
        public List<EnvironmentsItem> environments { get; set; }//环境  温度、湿度
        public int bugNum { get; set; }//害虫数量
        public string picStr { get; set; }//图片 base64编码
    }
    public class EnvironmentsItem
    {
        public string name { get; set; }//环境名
        public double value { get; set; }//值
    }


    #endregion 

    #region   CQ12发送位置信息
    /// <summary>
    /// 位置信息
    /// </summary>
    public class GPSInfoModel
    {
        public int func { get; set; }//功能码
        public string err { get; set; }//错误信息
        public string devId { get; set; }//设备id
        public GPSInfoMessage message { get; set; }
    }

    public class GPSInfoMessage
    {
        public double lat { get; set; }//经度
        public double lon { get; set; }//纬度
    }

    /// <summary>
    /// 上传工作模式
    /// </summary>
    public class SendWorkModeModel
    {
        public int func { get; set; }//功能码
        public string err { get; set; }//错误信息
        public string devId { get; set; }//设备id
        public string message { get; set; }//数据
    }

    #region 上传设备参数

    /// <summary>
    /// 设备参数model
    /// </summary>
    public class UpLoadDevParamRoot
    {
        public int func { get; set; }//功能码
        public string err { get; set; }
        public string devId { get; set; }//设备ID
        public UpLoadDevParamMessage message { get; set; }
    }

    public class UpLoadDevParamMessage
    {
        public int electricity { get; set; }//电流
        public int voltage { get; set; }//电压
        public string autoRollOverTime { get; set; }//自动转仓时间
        public int squence_time { get; set; }//震动时间（秒）
        public int squence_strength { get; set; }//震动强度
        public int inbug_time { get; set; }//进虫时间（分）
        public int killTime { get; set; }//落虫时间（秒）
        public int hotTime { get; set; }//加热时间（分）
    }

    #endregion



    #region CQ12远程设置
    /// <summary>
    /// CQ12远程设置报警号码
    /// </summary>
    public class LongRangeSetAlarmNum
    {
        public int func { get; set; }//功能码
        public string err { get; set; }//错误信息
        public string devId { get; set; }//设备id
        public List<Int64> message = new List<Int64>();//号码
    }


    /// <summary>
    /// 远程设置害虫限值
    /// </summary>
    public class LongRangeSetBugLimitRoot
    {
        public int func { get; set; }
        public string err { get; set; }
        public string devId { get; set; }
        public LongRangeSetBugLimitMessage message { get; set; }
    }
    public class LongRangeSetBugLimitMessage
    {
        public int bugMax { get; set; }
    }


    /// <summary>
    /// CQ12远程设置设备参数
    /// </summary>
    public class LongRangeSetDevParamRoot
    {
        public int func { get; set; }//功能码
        public string err { get; set; }//错误信息
        public string devId { get; set; }//设备ID
        public LongRangeSetDevParamMessage message { get; set; }
    }
    public class LongRangeSetDevParamMessage
    {
        public string autoRollOverTime { get; set; }//自动转仓时间
        public int squence_time { get; set; }//震动时间
        public int squence_strength { get; set; }//震动强度
        public int inbug_time { get; set; }//进虫时间
        public int killTime { get; set; }//落虫时间
        public int hotTime { get; set; }//加热时间
    }


    /// <summary>
    /// 远程拍照
    /// </summary>
    public class LongRangeSetPhotographRoot
    {
        public int func { get; set; }
        public string err { get; set; }
        public string devId { get; set; }
        public string message { get; set; }
        // public LongRangeSetPhotographMessage message { get; set; }
    }
    public class LongRangeSetPhotographMessage
    {
        public string type { get; set; }
    }


    /// <summary>
    /// 远程设置开关灯
    /// </summary>
    public class LongRangeSetYouChongRoot
    {
        public int func { get; set; }
        public string err { get; set; }
        public string devId { get; set; }
        public string message { get; set; }
    }


    /// <summary>
    /// 远程设置服务器IP和端口
    /// </summary>
    public class LongRangeSetIPRoot
    {
        public int func { get; set; }
        public string err { get; set; }
        public string devId { get; set; }
        public LongRangeSetIPMessage message { get; set; }
    }
    public class LongRangeSetIPMessage
    {
        public string ip { get; set; }
        public string port { get; set; }
    }

   


    /// <summary>
    /// CQ12远程设置回复
    /// </summary>
    public class LongRangeReceive
    {
        public int func { get; set; }//功能码
        public string err { get; set; }//错误信息
        public string devId { get; set; }//设备id
        public string message { get; set; }
    }




    #endregion
    #endregion

    #region CQ12发送害虫限值

    public class BugLimitRoot
    {
        public int func { get; set; }
        public string err { get; set; }
        public string devId { get; set; }
        public BugLimitMessage message { get; set; }
    }
    public class BugLimitMessage
    {
        public int bugMax { get; set; }
    }

    #endregion

    #region CQ12发送报警号码
    public class AlarmNumRoot
    {
        public int func { get; set; }
        public string err { get; set; }
        public string devId { get; set; }
        public List<string> message = new List<string>();
    }

    #endregion

    #region CQ12设置工作时间段
    public class SetTimeLoatRoot
    {
        public int func { get; set; }
        public string err { get; set; }
        public string devId { get; set; }
        public List<SetTimeLoatMessageItem> message = new List<SetTimeLoatMessageItem>();
    }
    public class SetTimeLoatMessageItem
    {
        public string startTime { get; set; }
        public string endTime { get; set; }
    }

    

    #endregion

    #region 害虫、环境类型数据

    /// <summary>
    /// 害虫类型
    /// </summary>
    public class BugTypeModel
    {

        public List<BugTypeItem> aaData { get; set; }
    }
    public class BugTypeItem
    {
        public string id { get; set; }//害虫ID
        public string name { get; set; }//害虫名字
        public string code { get; set; }//害虫编码
    }


    /// <summary>
    /// 环境类型
    /// </summary>
    public class SceneTypeModel
    {
        public List<SceneTypeItem> aaData { get; set; }
    }
    public class SceneTypeItem
    {
        public string id { get; set; }//环境ID
        public string name { get; set; }//环境名字
        public string code { get; set; }//环境编码
    }

    #endregion 害虫、环境类型数据

    #region 根据IP获取GPS

    public class IPAddressRoot
    {
        public string address { get; set; }
        public Content content { get; set; }
        public int status { get; set; }
    }

    public class Content
    {
        /// <summary>
        /// 河南省郑州市
        /// </summary>
        public string address { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Address_detail address_detail { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public MyPoint point { get; set; }
    }

    public class Address_detail
    {
        public string city { get; set; }
        public int city_code { get; set; }
        public string district { get; set; }
        public string province { get; set; }
        public string street { get; set; }
        public string street_number { get; set; }
    }

    public class MyPoint
    {
        public string x { get; set; }//lon 经度
        public string y { get; set; }//lat 纬度
    }

    #endregion 根据IP获取GPS


    #region CQ8获取设备出厂码


    public class GetDevFactoryCodeRoot
    {
        public GetDevFactoryCodeMessage message { get; set; }
        public string result { get; set; }
        public string code { get; set; }
    }
    public class GetDevFactoryCodeMessage
    {
        public string num { get; set; }//设备出厂码
        public string deviceCoding { get; set; }//MAC地址
    }

    #endregion CQ8获取设备出厂码


    #region CQ8JSON

    /// <summary>
    /// CQ8采集信息
    /// </summary>
    public class CQ8CollectionRecord
    {
        public string collectionTime { get; set; }//采集时间
        public string lureCore { get; set; }//诱芯类型
        public string info { get; set; }//采集信息
    }

    #endregion CQ8JSON
}
