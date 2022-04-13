/// ***********************************************************************
///
/// =================================
/// CLR 版本    ：4.0.30319.42000
/// 项目名称    ：DeviceController.View.Climate
/// 文件名称    ：Database.cs
/// 命名空间    ：DeviceController.View.Climate
/// =================================
/// 创 建 者    ：赵新雨
/// 创建日期    ：2020/11/12 10:07:08 
/// 邮    箱    ：zhao2271154036@163.com
/// 功能描述    ：
/// 使用说明    ：
/// =================================
/// 修 改 者    ：
/// 修改日期    ：
/// 修改内容    ：
/// =================================
/// * Copyright @ OuKeQi 2020. All rights reserved.
/// ***********************************************************************
using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeviceController.View.Climate
{
    /// <summary>
    /// 项目名称 ：DeviceController.View.Climate
    /// 命名空间 ：DeviceController.View.Climate
    /// 类 名 称 ：Database
    /// 作    者 ：赵新雨 
    /// 创建时间 ：2020/11/12 10:07:08 
    /// 更新时间 ：2020/11/12 10:07:08
    /// </summary>
    class Database
    {
    }
    class DB_Climate
    {
        public static OleDbConnection SqlConn;
        //初始化数据库
        public static bool DBInit()
        {
            string DataBasePath = System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\ClimateConfig\\ClimateData.mdb";
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
}
