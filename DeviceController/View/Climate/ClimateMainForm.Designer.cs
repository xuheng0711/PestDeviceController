namespace DeviceController.View.Climate
{
    partial class ClimateMainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ClimateMainForm));
            this.panel2 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.BtnSystemSet = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.BtnLookData = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel4 = new System.Windows.Forms.Panel();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.nextpage = new DevComponents.DotNetBar.ButtonX();
            this.prepage = new DevComponents.DotNetBar.ButtonX();
            this.button11 = new DevComponents.DotNetBar.ButtonX();
            this.button10 = new DevComponents.DotNetBar.ButtonX();
            this.PageInfo = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.button2 = new DevComponents.DotNetBar.ButtonX();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label23 = new System.Windows.Forms.Label();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.buttonX3 = new DevComponents.DotNetBar.ButtonX();
            this.buttonX2 = new DevComponents.DotNetBar.ButtonX();
            this.label1 = new System.Windows.Forms.Label();
            this.TxtCollectionInterval = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.buttonX1 = new DevComponents.DotNetBar.ButtonX();
            this.BtnParamSet = new DevComponents.DotNetBar.ButtonX();
            this.label7 = new System.Windows.Forms.Label();
            this.TxtUploadPort = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.TxtUploadAddress = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtSiteName = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.TxtAlarmInterval = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.TxtAlarmNumber = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.TxtUploadInterval = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.panel11 = new System.Windows.Forms.Panel();
            this.label29 = new System.Windows.Forms.Label();
            this.serialPort1 = new System.IO.Ports.SerialPort(this.components);
            this.panel2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel11.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(35)))), ((int)(((byte)(35)))), ((int)(((byte)(35)))));
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.BtnSystemSet);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.BtnLookData);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(188, 741);
            this.panel2.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold);
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(0, 209);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(188, 19);
            this.label5.TabIndex = 5;
            this.label5.Text = "系统设置";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnSystemSet
            // 
            this.BtnSystemSet.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnSystemSet.BackgroundImage")));
            this.BtnSystemSet.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BtnSystemSet.FlatAppearance.BorderSize = 0;
            this.BtnSystemSet.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnSystemSet.Location = new System.Drawing.Point(0, 158);
            this.BtnSystemSet.Name = "BtnSystemSet";
            this.BtnSystemSet.Size = new System.Drawing.Size(188, 43);
            this.BtnSystemSet.TabIndex = 4;
            this.BtnSystemSet.UseVisualStyleBackColor = true;
            this.BtnSystemSet.Click += new System.EventHandler(this.BtnSystemSet_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(0, 103);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(188, 19);
            this.label4.TabIndex = 3;
            this.label4.Text = "数据查看";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // BtnLookData
            // 
            this.BtnLookData.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("BtnLookData.BackgroundImage")));
            this.BtnLookData.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.BtnLookData.FlatAppearance.BorderSize = 0;
            this.BtnLookData.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BtnLookData.Location = new System.Drawing.Point(0, 52);
            this.BtnLookData.Name = "BtnLookData";
            this.BtnLookData.Size = new System.Drawing.Size(188, 43);
            this.BtnLookData.TabIndex = 2;
            this.BtnLookData.UseVisualStyleBackColor = true;
            this.BtnLookData.Click += new System.EventHandler(this.BtnLookData_Click_1);
            // 
            // tabControl1
            // 
            this.tabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.ItemSize = new System.Drawing.Size(0, 1);
            this.tabControl1.Location = new System.Drawing.Point(188, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(953, 741);
            this.tabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl1.TabIndex = 4;
            // 
            // tabPage2
            // 
            this.tabPage2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(234)))), ((int)(((byte)(255)))));
            this.tabPage2.Controls.Add(this.panel4);
            this.tabPage2.Controls.Add(this.panel3);
            this.tabPage2.Location = new System.Drawing.Point(4, 5);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(945, 732);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.White;
            this.panel4.Controls.Add(this.groupBox7);
            this.panel4.Location = new System.Drawing.Point(7, 53);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(932, 673);
            this.panel4.TabIndex = 7;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.nextpage);
            this.groupBox7.Controls.Add(this.prepage);
            this.groupBox7.Controls.Add(this.button11);
            this.groupBox7.Controls.Add(this.button10);
            this.groupBox7.Controls.Add(this.PageInfo);
            this.groupBox7.Controls.Add(this.flowLayoutPanel1);
            this.groupBox7.Controls.Add(this.button2);
            this.groupBox7.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox7.Location = new System.Drawing.Point(18, 3);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(897, 641);
            this.groupBox7.TabIndex = 1;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "数据查看";
            // 
            // nextpage
            // 
            this.nextpage.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.nextpage.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.nextpage.Location = new System.Drawing.Point(426, 606);
            this.nextpage.Name = "nextpage";
            this.nextpage.Size = new System.Drawing.Size(37, 25);
            this.nextpage.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.nextpage.TabIndex = 68;
            this.nextpage.Text = " 》";
            this.nextpage.Click += new System.EventHandler(this.nextpage_Click);
            // 
            // prepage
            // 
            this.prepage.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.prepage.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.prepage.Location = new System.Drawing.Point(345, 606);
            this.prepage.Name = "prepage";
            this.prepage.Size = new System.Drawing.Size(37, 25);
            this.prepage.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.prepage.TabIndex = 67;
            this.prepage.Text = "《";
            this.prepage.Click += new System.EventHandler(this.prepage_Click);
            // 
            // button11
            // 
            this.button11.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.button11.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.button11.Location = new System.Drawing.Point(474, 606);
            this.button11.Name = "button11";
            this.button11.Size = new System.Drawing.Size(50, 25);
            this.button11.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.button11.TabIndex = 66;
            this.button11.Text = "尾页";
            this.button11.Click += new System.EventHandler(this.button11_Click);
            // 
            // button10
            // 
            this.button10.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.button10.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.button10.Location = new System.Drawing.Point(279, 606);
            this.button10.Name = "button10";
            this.button10.Size = new System.Drawing.Size(50, 25);
            this.button10.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.button10.TabIndex = 65;
            this.button10.Text = "首页";
            this.button10.Click += new System.EventHandler(this.button10_Click);
            // 
            // PageInfo
            // 
            this.PageInfo.AutoSize = true;
            this.PageInfo.Location = new System.Drawing.Point(388, 609);
            this.PageInfo.Name = "PageInfo";
            this.PageInfo.Size = new System.Drawing.Size(32, 16);
            this.PageInfo.TabIndex = 64;
            this.PageInfo.Text = "1/0";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.AutoScroll = true;
            this.flowLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(16, 25);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(868, 700);
            this.flowLayoutPanel1.TabIndex = 62;
            // 
            // button2
            // 
            this.button2.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.button2.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.button2.Location = new System.Drawing.Point(530, 606);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(51, 25);
            this.button2.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.button2.TabIndex = 61;
            this.button2.Text = "清空";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(158)))), ((int)(((byte)(227)))));
            this.panel3.Controls.Add(this.label23);
            this.panel3.Location = new System.Drawing.Point(7, 11);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(932, 35);
            this.panel3.TabIndex = 6;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label23.ForeColor = System.Drawing.SystemColors.Control;
            this.label23.Location = new System.Drawing.Point(15, 10);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(255, 16);
            this.label23.TabIndex = 0;
            this.label23.Text = "智能病虫害一体化监测装备-气象";
            // 
            // tabPage1
            // 
            this.tabPage1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(234)))), ((int)(((byte)(255)))));
            this.tabPage1.Controls.Add(this.panel1);
            this.tabPage1.Controls.Add(this.panel11);
            this.tabPage1.Location = new System.Drawing.Point(4, 5);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(945, 732);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Location = new System.Drawing.Point(7, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(932, 673);
            this.panel1.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.buttonX3);
            this.groupBox1.Controls.Add(this.buttonX2);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.TxtCollectionInterval);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.textBox1);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.buttonX1);
            this.groupBox1.Controls.Add(this.BtnParamSet);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.TxtUploadPort);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.TxtUploadAddress);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.TxtSiteName);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.TxtAlarmInterval);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.TxtAlarmNumber);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.TxtUploadInterval);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.groupBox1.Location = new System.Drawing.Point(18, 3);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(898, 643);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "参数设置";
            // 
            // buttonX3
            // 
            this.buttonX3.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX3.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX3.Location = new System.Drawing.Point(214, 518);
            this.buttonX3.Name = "buttonX3";
            this.buttonX3.Size = new System.Drawing.Size(107, 34);
            this.buttonX3.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonX3.TabIndex = 78;
            this.buttonX3.Text = "数据上传";
            this.buttonX3.Click += new System.EventHandler(this.buttonX3_Click);
            // 
            // buttonX2
            // 
            this.buttonX2.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX2.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX2.Location = new System.Drawing.Point(352, 518);
            this.buttonX2.Name = "buttonX2";
            this.buttonX2.Size = new System.Drawing.Size(107, 34);
            this.buttonX2.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonX2.TabIndex = 77;
            this.buttonX2.Text = "LED屏测试";
            this.buttonX2.Click += new System.EventHandler(this.buttonX2_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(596, 139);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(24, 16);
            this.label1.TabIndex = 76;
            this.label1.Text = "分";
            // 
            // TxtCollectionInterval
            // 
            this.TxtCollectionInterval.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtCollectionInterval.Location = new System.Drawing.Point(401, 136);
            this.TxtCollectionInterval.Name = "TxtCollectionInterval";
            this.TxtCollectionInterval.Size = new System.Drawing.Size(189, 26);
            this.TxtCollectionInterval.TabIndex = 75;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label11.Location = new System.Drawing.Point(315, 139);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(80, 16);
            this.label11.TabIndex = 74;
            this.label11.Text = "采集间隔:";
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.textBox1.Location = new System.Drawing.Point(401, 81);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(189, 26);
            this.textBox1.TabIndex = 73;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(315, 84);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(80, 16);
            this.label13.TabIndex = 72;
            this.label13.Text = "设备编号:";
            // 
            // buttonX1
            // 
            this.buttonX1.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.buttonX1.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.buttonX1.Location = new System.Drawing.Point(491, 518);
            this.buttonX1.Name = "buttonX1";
            this.buttonX1.Size = new System.Drawing.Size(107, 34);
            this.buttonX1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.buttonX1.TabIndex = 71;
            this.buttonX1.Text = "修改";
            this.buttonX1.Click += new System.EventHandler(this.buttonX1_Click);
            // 
            // BtnParamSet
            // 
            this.BtnParamSet.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.BtnParamSet.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.BtnParamSet.Location = new System.Drawing.Point(630, 518);
            this.BtnParamSet.Name = "BtnParamSet";
            this.BtnParamSet.Size = new System.Drawing.Size(107, 34);
            this.BtnParamSet.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.BtnParamSet.TabIndex = 69;
            this.BtnParamSet.Text = "确定";
            this.BtnParamSet.Click += new System.EventHandler(this.BtnParamSet_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(596, 454);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 16);
            this.label7.TabIndex = 68;
            this.label7.Text = "分";
            // 
            // TxtUploadPort
            // 
            this.TxtUploadPort.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtUploadPort.Location = new System.Drawing.Point(401, 294);
            this.TxtUploadPort.Name = "TxtUploadPort";
            this.TxtUploadPort.Size = new System.Drawing.Size(189, 26);
            this.TxtUploadPort.TabIndex = 63;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label10.Location = new System.Drawing.Point(315, 297);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(80, 16);
            this.label10.TabIndex = 62;
            this.label10.Text = "上传端口:";
            // 
            // TxtUploadAddress
            // 
            this.TxtUploadAddress.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtUploadAddress.Location = new System.Drawing.Point(401, 243);
            this.TxtUploadAddress.Name = "TxtUploadAddress";
            this.TxtUploadAddress.Size = new System.Drawing.Size(189, 26);
            this.TxtUploadAddress.TabIndex = 61;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(315, 246);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 16);
            this.label2.TabIndex = 60;
            this.label2.Text = "上传地址:";
            // 
            // TxtSiteName
            // 
            this.TxtSiteName.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtSiteName.Location = new System.Drawing.Point(401, 346);
            this.TxtSiteName.Name = "TxtSiteName";
            this.TxtSiteName.Size = new System.Drawing.Size(189, 26);
            this.TxtSiteName.TabIndex = 59;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label9.Location = new System.Drawing.Point(315, 349);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(80, 16);
            this.label9.TabIndex = 58;
            this.label9.Text = "站点名称:";
            // 
            // TxtAlarmInterval
            // 
            this.TxtAlarmInterval.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtAlarmInterval.Location = new System.Drawing.Point(401, 451);
            this.TxtAlarmInterval.Name = "TxtAlarmInterval";
            this.TxtAlarmInterval.Size = new System.Drawing.Size(189, 26);
            this.TxtAlarmInterval.TabIndex = 57;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(315, 454);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 16);
            this.label8.TabIndex = 56;
            this.label8.Text = "报警间隔:";
            // 
            // TxtAlarmNumber
            // 
            this.TxtAlarmNumber.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtAlarmNumber.Location = new System.Drawing.Point(401, 397);
            this.TxtAlarmNumber.Name = "TxtAlarmNumber";
            this.TxtAlarmNumber.Size = new System.Drawing.Size(189, 26);
            this.TxtAlarmNumber.TabIndex = 55;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(315, 400);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(80, 16);
            this.label6.TabIndex = 54;
            this.label6.Text = "报警号码:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(596, 196);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(24, 16);
            this.label3.TabIndex = 53;
            this.label3.Text = "分";
            // 
            // TxtUploadInterval
            // 
            this.TxtUploadInterval.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtUploadInterval.Location = new System.Drawing.Point(401, 193);
            this.TxtUploadInterval.Name = "TxtUploadInterval";
            this.TxtUploadInterval.Size = new System.Drawing.Size(189, 26);
            this.TxtUploadInterval.TabIndex = 52;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label12.Location = new System.Drawing.Point(315, 196);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(80, 16);
            this.label12.TabIndex = 51;
            this.label12.Text = "上传间隔:";
            // 
            // panel11
            // 
            this.panel11.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(79)))), ((int)(((byte)(158)))), ((int)(((byte)(227)))));
            this.panel11.Controls.Add(this.label29);
            this.panel11.Location = new System.Drawing.Point(7, 11);
            this.panel11.Name = "panel11";
            this.panel11.Size = new System.Drawing.Size(932, 35);
            this.panel11.TabIndex = 5;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("宋体", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label29.ForeColor = System.Drawing.SystemColors.Control;
            this.label29.Location = new System.Drawing.Point(15, 10);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(255, 16);
            this.label29.TabIndex = 0;
            this.label29.Text = "智能病虫害一体化监测装备-气象";
            // 
            // ClimateMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1141, 741);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.panel2);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "ClimateMainForm";
            this.Text = "气象";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ClimateMainForm_FormClosing);
            this.Load += new System.EventHandler(this.ClimateMainForm_Load);
            this.Shown += new System.EventHandler(this.ClimateMainForm_Shown);
            this.SizeChanged += new System.EventHandler(this.ClimateMainForm_SizeChanged);
            this.Resize += new System.EventHandler(this.ClimateMainForm_Resize);
            this.panel2.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.panel4.ResumeLayout(false);
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabPage1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.panel11.ResumeLayout(false);
            this.panel11.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button BtnSystemSet;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button BtnLookData;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Panel panel11;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox TxtUploadPort;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox TxtUploadAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox TxtSiteName;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox TxtAlarmInterval;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox TxtAlarmNumber;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TxtUploadInterval;
        private System.Windows.Forms.Label label12;
        private DevComponents.DotNetBar.ButtonX BtnParamSet;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.GroupBox groupBox7;
        private DevComponents.DotNetBar.ButtonX button2;
        private DevComponents.DotNetBar.ButtonX buttonX1;
        private DevComponents.DotNetBar.ButtonX nextpage;
        private DevComponents.DotNetBar.ButtonX prepage;
        private DevComponents.DotNetBar.ButtonX button11;
        private DevComponents.DotNetBar.ButtonX button10;
        private System.Windows.Forms.Label PageInfo;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.IO.Ports.SerialPort serialPort1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox TxtCollectionInterval;
        private System.Windows.Forms.Label label11;
        private DevComponents.DotNetBar.ButtonX buttonX2;
        private DevComponents.DotNetBar.ButtonX buttonX3;
    }
}