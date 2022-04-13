/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.View.Climate
/// 文件名称    ：LEDModel.cs
/// 命名空间    ：DeviceController.View.Climate
/// =================================
/// 创 建 者    ：赵新雨
/// 创建日期    ：2021-04-29  21:53:23 
/// 邮    箱    ：zhao2271154036@163.com
/// 功能描述    ：
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2021. All rights reserved.
/// ***********************************************************************
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceController.View.Climate
{
    public class LedClear
    {
        public Cmd cmd = new Cmd();
        public string ids_dev { get; set; }
        public int sno { get; set; }
    }
    public class Cmd
    {
        public Delete delete = new Delete();
    }
    public class Delete
    {
        public int del_all { get; set; }
    }



    public enum UrlType
    {
        Login,
        ClearScene,
        ShowData,
    }
    public class LedLogn
    {
        public string user { get; set; }
        public string pwd { get; set; }
    }


    /// <summary>
    /// 项目名称 ：DeviceController.View.Climate
    /// 命名空间 ：DeviceController.View.Climate
    /// 类 名 称 ：LEDModel
    /// 作    者 ：赵新雨 
    /// 创建时间 ：2021-04-29  21:53:23 
    /// 更新时间 ：2021-04-29  21:53:23
    /// </summary>
    public class LEDModel
    {
        public Pkts_program pkts_program = new Pkts_program();
        public string ids_dev { get; set; }//设备编号
        public int sno { get; set; }
    }
    public class Pkts_program
    {
        public int id_pro { get; set; }//节目号
        public Property_pro property_pro = new Property_pro();//设备宽高
        public List<List_regionItem> list_region = new List<List_regionItem>();//分区
    }
    public class List_regionItem
    {
        public Info_pos info_pos = new Info_pos();//内容显示位置
        public List<List_itemItem> list_item = new List<List_itemItem>();//动画信息列表
    }
    public class List_itemItem
    {
        public Info_animate info_animate = new Info_animate();//动画信息
        public string type_item { get; set; }//信息类型
        public string text { get; set; }//信息内容
    }
    public class Info_animate
    {
        public string model_continue { get; set; }//从左到右或者从右到左滚动
        public string speed { get; set; }//滚动速度
    }
    public class Info_pos
    {
        public int x { get; set; }//x轴
        public int y { get; set; }//y轴
        public int w { get; set; }//区域宽
        public int h { get; set; }//区域高
    }
    public class Property_pro
    {
        public int width { get; set; }//设备宽
        public int height { get; set; }//设备高
    }
}
