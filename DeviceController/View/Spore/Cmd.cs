using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace BZ10
{
    public class Cmd
    {
        public static RichTextBox richTex = null;
        public static SerialPort serialPort1=null;
        public static void InitComm(SerialPort serial)
        {
            serialPort1 = serial;
        }
        public static byte GetCheckByte(byte[] by)
        {
            try
            {
                int size = by.Length;
                int crc = 0;
                for (int i = 1; i < size - 1; i++)
                {
                    crc += by[i];
                }
                //   crc = ~crc + 1;
                return (byte)(crc & 0xFF);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error,ex.ToString());
                return 0;
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
                        returnStr += bytes[i].ToString("X2");
                    }
                }
                return returnStr;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error,ex.ToString());
                return "";
            }
        }
        public static byte[] CommunicateDp(byte func, int steps)
        {
            try
            {
                if (serialPort1==null||!serialPort1.IsOpen)
                {
                    return null;
                }
                int index = 0;
                serialPort1.DiscardInBuffer();
                byte[] rec = new byte[10];
                byte[] by = { 0xBB, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
                by[2] = func;
                by[5] = (byte)((steps >> 24) & 0xFF);
                by[6] = (byte)((steps >> 16) & 0xFF);
                by[7] = (byte)((steps >> 8) & 0xFF);
                by[8] = (byte)((steps >> 0) & 0xFF);
                by[9] = GetCheckByte(by);

                for (int i = 0; i < 3; i++)
                {
                    serialPort1.Write(by, 0, by.Length);
                    DebOutPut.DebLog( "发送:" + byteToHexStr(by));
                    Thread.Sleep(50);
                    if (serialPort1.BytesToRead <= 0)
                    {
                        DebOutPut.DebLog( "未收到终端回应！请重试!");
                        index++;
                    }
                    else
                        break;
                }
                if (index == 3)
                {
                    //   shutdown_PC();
                    return null;
                }
                serialPort1.Read(rec, 0, 10);
                DebOutPut.DebLog( "接收:" + byteToHexStr(rec));
                int m = GetCheckByte(rec);
                if (rec[2] != 0xA0 && rec[0] == 0xFF && m == rec[9])
                {
                    if (rec[8] == 0)
                        DebOutPut.DebLog( "收到并执行!");
                    else
                        DebOutPut.DebLog( "执行失败!");
                }
                return rec;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error,ex.ToString());
                return null;
            }
           

        }
        private static void shutdown_PC()
        {
            try
            {
                System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
                myProcess.StartInfo.FileName = "cmd.exe";//启动cmd命令
                myProcess.StartInfo.UseShellExecute = false;//是否使用系统外壳程序启动进程
                myProcess.StartInfo.RedirectStandardInput = true;//是否从流中读取
                myProcess.StartInfo.RedirectStandardOutput = true;//是否写入流
                myProcess.StartInfo.RedirectStandardError = true;//是否将错误信息写入流
                myProcess.StartInfo.CreateNoWindow = true;//是否在新窗口中启动进程
                myProcess.Start();//启动进程
                myProcess.StandardInput.WriteLine("shutdown -s -t 0");//执行关机命令
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error,ex.ToString());
            }
           
        }
        
    }
}
