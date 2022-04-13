using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BZ10
{
    class DevStatus
    {
        public string status = "";   //查询状态
        public byte[] bits = new byte[14];//状态指示位
        public bool bReady = true;//false 未就位，true 已就位

        public void clear()
        {
            try
            {
                bReady = true;
                for (int i = 0; i < 14; i++)
                {
                    bits[i] = 0;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        List<byte> dele = new List<byte>();
        public void isReady(List<byte> by)
        {
            try
            {
                dele.Clear();
                for (int i = 0; i < by.Count; i++)
                {
                    if (bits[by[i]] != 1)
                        bReady = false;
                    else if (bits[by[i]] == 1)
                        dele.Add(by[i]);
                }
                for (int i = 0; i < dele.Count; i++)
                {
                    if (by.Contains(dele[i]))
                        by.Remove(dele[i]);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
    }
}
