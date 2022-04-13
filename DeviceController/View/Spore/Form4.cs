using BZ10.Common;
using DeviceController.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BZ10
{
    public partial class Login : Form
    {
        private void Login_Load(object sender, EventArgs e)
        {

        }
        public Login()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text == "admin")
                {
                    DebOutPut.DebLog(  "进入调试模式");
                    DebOutPut.WriteLog(  LogType.Error, "进入调试模式");
                    Form2 form = new Form2();
                    form.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show("密码错误!");
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog( ex.ToString());
                DebOutPut.WriteLog( LogType.Error, ex.ToString());
            }

        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SoftKeyboardCtrl.CloseWindow();

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
          
        }

        private void textBox1_Click(object sender, EventArgs e)
        {
            try
            {
                if (Param.isSoftKeyBoard == "0")
                    SoftKeyboardCtrl.OpenAndSetWindow();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
          
        }
    }
}
