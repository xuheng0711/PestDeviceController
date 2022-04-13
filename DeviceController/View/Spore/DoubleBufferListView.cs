using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BZ10
{
    class DoubleBufferListView : ListView
    {

        public DoubleBufferListView()
        {
            try
            {
                SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.AllPaintingInWmPaint, true);
                UpdateStyles();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error,ex.ToString());
            }


        }
    }
}
