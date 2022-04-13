using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BZ10
{
    public partial class PicturePreview : Form
    {
        public PicturePreview()
        {
            InitializeComponent();
        }

        private void PicturePreview_Load(object sender, EventArgs e)
        {

        }
        public void setImageSource(String  path)
        {
            try
            {
                // img
                Bitmap bitmap = new Bitmap(path);
                pictureBox1.Image = bitmap;
                pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
                pictureBox1.Width = this.Width;
                pictureBox1.Height = this.Height;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error,ex.ToString());
            }
           
        }

        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
