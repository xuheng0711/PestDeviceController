/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.Models
/// 文件名称    ：SaveDataModel.cs
/// 命名空间    ：DeviceController.Models
/// =================================
/// 创 建 者    ：ZhaoXinYu
/// 创建日期    ：2019/11/21 10:23:12 
/// 邮    箱    ：zhaoxinyu12580@163.com
/// 功能描述    ：中间类
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
    /// 类 名 称 ：SaveDataModel
    /// 作    者 ：ZhaoXinYu 
    /// 创建时间 ：2019/11/21 10:23:12 
    /// 更新时间 ：2019/11/21 10:23:12
    /// </summary>
    public class SaveDataModel
    {
        /// <summary>
        /// 小气候配置文件数据模型
        /// </summary>
        public static ClimateConfigModel climateConfigModel = new ClimateConfigModel();

       

        /// <summary>
        /// 采集信息数据模型
        /// </summary>
        public static List<ClimateModel> climateModelList = new List<ClimateModel>();

        /// <summary>
        /// 害虫类型Model
        /// </summary>
        public static BugTypeModel bugTypeModel = new BugTypeModel();

        /// <summary>
        /// 环境类型Model
        /// </summary>
        public static SceneTypeModel sceneTypeModel = new SceneTypeModel();

        /// <summary>
        /// 根据IP获取经纬度
        /// </summary>
        public static IPAddressRoot iPAddressRoot = new IPAddressRoot();

        /// <summary>
        /// 获取设备出厂码
        /// </summary>
        public static GetDevFactoryCodeRoot getDevFactoryCodeRoot = new GetDevFactoryCodeRoot();

        /// <summary>
        /// CQ8采集数据的数据模型
        /// </summary>
        public static List<CQ8CollectionRecord> cQ8CollectionRecords = new List<CQ8CollectionRecord>();

        /// <summary>
        /// CQ12配置文件数据模型
        /// </summary>

        public static CQ12ConfigParamModel cQ12ConfigParamModel = new CQ12ConfigParamModel();
    }
}
