using BZ10.Common;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Controls;
using DeviceController.Common;
using DeviceController.DAL;
using DeviceController.DAL.DataUpload;
using DeviceController.Models;
using DeviceController.View.DevOverview;
using LitJson;
using MvCamCtrl.NET;
using Newtonsoft.Json.Linq;
using SporeDetectionEquipment;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace DeviceController.View.CQ12
{

    public delegate void CQ12UpdataDataList();
    public delegate void CQ12Init();
    public delegate void CQ12_ReceivedDataHandle(string recStr);
    public partial class CQ12MainForm : Form
    {
        //发送数据
        int inTimer1 = 0;
        System.Timers.Timer timer1 = new System.Timers.Timer();
        //接收数据
        int inTimer2 = 0;
        System.Timers.Timer timer2 = new System.Timers.Timer();
        //心跳
        int inTimer3 = 0;
        System.Timers.Timer timer3 = new System.Timers.Timer();
        public static CQ12Init cQ12Init;
        public static CQ12UpdataDataList cQ12UpdataDataList;
        public static CQ12_ReceivedDataHandle cQ12_ReceivedDataHandle;
        Socket clientSocket;
        private List<string> sendDataList = new List<string>();
        public static CollectInfoModel collectInfoModel;
        private SerialPort serialPort = new SerialPort();
        private bool isOpen = false;
        string bugNum = "0";//每次采集到的害虫数目
        public static DateTime newDataTime = DateTime.Parse("1970-01-01 00:00:00");
        private MyCamera m_pMyCamera;//相机对象
        ComboBox cbDeviceList = new ComboBox();
        MyCamera.MV_CC_DEVICE_INFO_LIST m_pDeviceList;
        UInt32 m_nBufSizeForDriver = 3072 * 2048 * 3;
        byte[] m_pBufForDriver = new byte[3072 * 2048 * 3];
        UInt32 m_nBufSizeForSaveImage = 3072 * 2048 * 3 * 3 + 2048;
        byte[] m_pBufForSaveImage = new byte[3072 * 2048 * 3 * 3 + 2048];

        public CQ12MainForm()
        {
            InitializeComponent();
            m_pDeviceList = new MyCamera.MV_CC_DEVICE_INFO_LIST();
            CheckForIllegalCrossThreadCalls = false;
            cQ12Init = Init;
            cQ12UpdataDataList = UpdataDataList;
            cQ12_ReceivedDataHandle = ReceivedDataHandle;
            this.tabControl1.SelectedIndex = 0;
            label3.ForeColor = System.Drawing.Color.Green;
            x = this.Width;
            y = this.Height;
            SetTag(this);
        }
        //双缓冲加载窗口时会出现相机能打开，但是无法正常加载图像的异常
        //protected override CreateParams CreateParams
        //{
        //    get
        //    {
        //        CreateParams cp = base.CreateParams;
        //        cp.ExStyle |= 0x02000000;
        //        return cp;
        //    }
        //}
        private void CQ12MainForm_Load(object sender, EventArgs e)
        {
            //发送数据
            timer1.Elapsed += new ElapsedEventHandler(timer1_Elapsed);
            timer1.Interval = 200;
            //接受数据
            timer2.Elapsed += new ElapsedEventHandler(timer2_Elapsed);
            timer2.Interval = 200;
            //心跳
            timer3.Elapsed += new ElapsedEventHandler(timer3_Elapsed);
            timer3.Interval = 30000;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        private void Init()
        {
            try
            {
                System.Windows.Forms.Control.CheckForIllegalCrossThreadCalls = false;
                //读取配置文件
                ReadConfig();
                //初始化数据库
                if (DB_CQ12.DBInit())
                {
                    //创建串口和Scoket连接
                    CreateSerialPort();
                    if (isOpen)
                    {
                        //刷新设备状态展示
                        this.UpdataStateShow.Interval = 5000;
                        this.UpdataStateShow.Start();
                    }
                }
                //打开摄像头
                StartOldNewCamera();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        #region 相机

        /// <summary>
        /// 启动新旧相机
        /// </summary>
        private void StartOldNewCamera()
        {
            if (m_pMyCamera == null || !m_pMyCamera.MV_CC_IsDeviceConnected_NET())
            {
                //关闭新相机
                CameraClose();
                //启动新版相机
                StartCamera();
            }
        }

        /// <summary>
        /// 启动相机
        /// </summary>
        private void StartCamera()
        {
            try
            {
                int index = 0;
                for (int i = 0; i < 3; i++)
                {
                    Start_GrabImage();
                    if (m_pMyCamera == null)
                    {
                        index++;
                        DebOutPut.DebLog("第 " + (i + 1) + " 次启动相机失败！");
                    }
                    else
                    {
                        DebOutPut.DebLog("启动相机成功！");
                        break;
                    }
                }
                if (index == 3)
                {
                    LabCurrState.Text = "启动相机失败!";
                    //DevOverviewMain.cQ12RunStateUpdata("", this.LabCurrState.Text.Trim());
                    return;
                }
            }
            catch (Exception ex)
            {
                LabCurrState.Text = "相机异常!";
                DevOverviewMain.cQ12RunStateUpdata("", this.LabCurrState.Text.Trim());
                DebOutPut.DebLog("新相机启动异常：" + ex.ToString());
                DebOutPut.WriteLog(LogType.Error, "新相机启动异常：" + ex.ToString());
                CameraClose();
            }
        }
        private void CameraClose()
        {
            if (m_pMyCamera != null)
            {
                m_pMyCamera.MV_CC_CloseDevice_NET();
                m_pMyCamera.MV_CC_DestroyDevice_NET();
                m_pMyCamera = null;
            }
        }
        /// <summary>
        /// 启动相机
        /// </summary>
        private void Start_GrabImage()
        {
            try
            {
                SeacehDev();//搜索设备
                OpenDev();//打开设备
                StartCollection();//开始采集
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }




        /// <summary>
        /// 搜索设备
        /// </summary>
        private void SeacehDev()
        {
            try
            {
                int nRet;
                // ch:创建设备列表 en:Create Device List
                cbDeviceList.Items.Clear();
                nRet = MyCamera.MV_CC_EnumDevices_NET(MyCamera.MV_GIGE_DEVICE | MyCamera.MV_USB_DEVICE, ref m_pDeviceList);
                if (0 != nRet)
                {
                    DebOutPut.WriteLog(LogType.Normal, "枚举设备失败!");
                    return;
                }

                // ch:在窗体列表中显示设备名 | en:Display device name in the form list
                for (int i = 0; i < m_pDeviceList.nDeviceNum; i++)
                {
                    MyCamera.MV_CC_DEVICE_INFO device = (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[i], typeof(MyCamera.MV_CC_DEVICE_INFO));
                    if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                    {
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stGigEInfo, 0);
                        MyCamera.MV_GIGE_DEVICE_INFO gigeInfo = (MyCamera.MV_GIGE_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_GIGE_DEVICE_INFO));
                        if (gigeInfo.chUserDefinedName != "")
                        {
                            cbDeviceList.Items.Add("GigE: " + gigeInfo.chUserDefinedName + " (" + gigeInfo.chSerialNumber + ")");
                        }
                        else
                        {
                            cbDeviceList.Items.Add("GigE: " + gigeInfo.chManufacturerName + " " + gigeInfo.chModelName + " (" + gigeInfo.chSerialNumber + ")");
                        }
                    }
                    else if (device.nTLayerType == MyCamera.MV_USB_DEVICE)
                    {
                        IntPtr buffer = Marshal.UnsafeAddrOfPinnedArrayElement(device.SpecialInfo.stUsb3VInfo, 0);
                        MyCamera.MV_USB3_DEVICE_INFO usbInfo = (MyCamera.MV_USB3_DEVICE_INFO)Marshal.PtrToStructure(buffer, typeof(MyCamera.MV_USB3_DEVICE_INFO));
                        if (usbInfo.chUserDefinedName != "")
                        {
                            cbDeviceList.Items.Add("USB: " + usbInfo.chUserDefinedName + " (" + usbInfo.chSerialNumber + ")");
                        }
                        else
                        {
                            cbDeviceList.Items.Add("USB: " + usbInfo.chManufacturerName + " " + usbInfo.chModelName + " (" + usbInfo.chSerialNumber + ")");
                        }
                    }
                }

                // ch:选择第一项 | en:Select the first item
                if (m_pDeviceList.nDeviceNum != 0)
                {
                    string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                    cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                    string cameraIndex = Tools.ConfigParmRead("Basic Parameters", "CameraIndex", cq12ConfigSavePath);
                    int index = -1;
                    for (int i = 0; i < cbDeviceList.Items.Count; i++)
                    {
                        if ((cbDeviceList.Items[i].ToString()).Contains(cameraIndex))
                        {
                            index = i;
                        }
                    }
                    if (index != -1)
                        cbDeviceList.SelectedIndex = index;
                    else
                    {
                        DebOutPut.DebErr("相机索引错误");
                        DebOutPut.WriteLog(LogType.Normal, "相机索引错误");
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 打开设备
        /// </summary>
        private void OpenDev()
        {
            try
            {
                if (m_pDeviceList.nDeviceNum == 0 || cbDeviceList.SelectedIndex == -1)
                {
                    DebOutPut.WriteLog(LogType.Normal, "没有设备，请选择！");
                    return;
                }
                int nRet = -1;

                // ch:获取选择的设备信息 | en:Get selected device information
                MyCamera.MV_CC_DEVICE_INFO device =
                    (MyCamera.MV_CC_DEVICE_INFO)Marshal.PtrToStructure(m_pDeviceList.pDeviceInfo[cbDeviceList.SelectedIndex],
                                                                  typeof(MyCamera.MV_CC_DEVICE_INFO));

                // ch:打开设备 | en:Open device
                if (null == m_pMyCamera)
                {
                    m_pMyCamera = new MyCamera();
                    if (null == m_pMyCamera)
                    {
                        return;
                    }
                }

                nRet = m_pMyCamera.MV_CC_CreateDevice_NET(ref device);
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }

                nRet = m_pMyCamera.MV_CC_OpenDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    m_pMyCamera.MV_CC_DestroyDevice_NET();
                    DebOutPut.WriteLog(LogType.Normal, "设备打开失败!");
                    return;
                }

                // ch:探测网络最佳包大小(只对GigE相机有效) | en:Detection network optimal package size(It only works for the GigE camera)
                if (device.nTLayerType == MyCamera.MV_GIGE_DEVICE)
                {
                    int nPacketSize = m_pMyCamera.MV_CC_GetOptimalPacketSize_NET();
                    if (nPacketSize > 0)
                    {
                        nRet = m_pMyCamera.MV_CC_SetIntValue_NET("GevSCPSPacketSize", (uint)nPacketSize);
                        if (nRet != MyCamera.MV_OK)
                        {
                            DebOutPut.DebLog("设置数据包大小失败    " + nRet);
                            DebOutPut.WriteLog(LogType.Normal, "设置数据包大小失败    " + nRet);

                        }
                    }
                    else
                    {
                        DebOutPut.DebLog("获取数据包大小失败    " + nPacketSize);
                        DebOutPut.WriteLog(LogType.Normal, "获取数据包大小失败    " + nPacketSize);
                    }
                }

                // ch:设置采集连续模式 | en:Set Continues Aquisition Mode
                m_pMyCamera.MV_CC_SetEnumValue_NET("AcquisitionMode", 1);// ch:工作在连续模式 | en:Acquisition On Continuous Mode
                                                                         // m_pMyCamera.MV_CC_SetEnumValue_NET("TriggerMode", 0);    // ch:连续模式 | en:Continuous
                                                                         //SetParam();//设置参数

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 设置参数
        /// </summary>
        private void SetParam()
        {
            try
            {
                if (m_pMyCamera == null)
                {
                    DebOutPut.WriteLog(LogType.Normal, "m_pMyCamera:null");
                    return;
                }
                int nRet;
                m_pMyCamera.MV_CC_SetEnumValue_NET("ExposureAuto", 0);

                float.Parse("200000.0");
                float.Parse("10.0");
                float.Parse("2.0");

                nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("ExposureTime", float.Parse("200000.0"));
                if (nRet != MyCamera.MV_OK)
                {
                    DebOutPut.WriteLog(LogType.Normal, "设置曝光时间失败!");
                }

                m_pMyCamera.MV_CC_SetEnumValue_NET("GainAuto", 0);
                nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("Gain", float.Parse("10.0"));
                if (nRet != MyCamera.MV_OK)
                {
                    DebOutPut.WriteLog(LogType.Normal, "设置增益失败!");
                }

                nRet = m_pMyCamera.MV_CC_SetFloatValue_NET("AcquisitionFrameRate", float.Parse("2.0"));
                if (nRet != MyCamera.MV_OK)
                {
                    DebOutPut.WriteLog(LogType.Normal, "设置帧速率失败!");
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 开始采集
        /// </summary>
        private void StartCollection()
        {
            try
            {
                if (m_pMyCamera == null)
                {
                    return;
                }
                int nRet;

                // ch:开始采集 | en:Start Grabbing
                nRet = m_pMyCamera.MV_CC_StartGrabbing_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    DebOutPut.WriteLog(LogType.Normal, "触发失败!");
                    return;
                }


                // ch:标志位置位true | en:Set position bit true

                // ch:显示 | en:Display
                nRet = m_pMyCamera.MV_CC_Display_NET(Cv_Main1.Handle);
                // RotateFormCenter(Cv_Main1, 90);
                if (MyCamera.MV_OK != nRet)
                {
                    DebOutPut.WriteLog(LogType.Normal, "显示失败！");
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 呈像旋转
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="angle"></param>
        private void RotateFormCenter(PictureBox pb, float angle)
        {
            try
            {
                Image img = pb.Image;
                int newWidth = Math.Max(img.Height, img.Width);
                Bitmap bmp = new Bitmap(newWidth, newWidth);
                Graphics g = Graphics.FromImage(bmp);
                Matrix x = new Matrix();
                PointF point = new PointF(img.Width / 2f, img.Height / 2f);
                x.RotateAt(angle, point);
                g.Transform = x;
                g.DrawImage(img, 0, 0);
                g.Dispose();
                img = bmp;
                pb.Image = img;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        /// <summary>
        /// 关闭相机
        /// </summary>
        private void CloseCamera()
        {
            try
            {
                // ch:关闭设备 | en:Close Device
                int nRet;
                if (m_pMyCamera == null)
                {
                    return;
                }
                nRet = m_pMyCamera.MV_CC_CloseDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }

                nRet = m_pMyCamera.MV_CC_DestroyDevice_NET();
                if (MyCamera.MV_OK != nRet)
                {
                    return;
                }
                // ch:取流标志位清零 | en:Reset flow flag bit
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }


        }
        static readonly object SequenceLock = new object();
        /// <summary>
        /// 拍照
        /// </summary>
        /// <returns></returns>
        private Image GetPic()
        {
            try
            {
                if (m_pMyCamera == null)
                {
                    DebOutPut.DebLog("相机对象为空");
                    DebOutPut.WriteLog(LogType.Normal, "相机对象为空");
                    return null;
                }
                lock (SequenceLock)
                {
                    int nRet;
                    UInt32 nPayloadSize = 0;
                    MyCamera.MVCC_INTVALUE stParam = new MyCamera.MVCC_INTVALUE();
                    nRet = m_pMyCamera.MV_CC_GetIntValue_NET("PayloadSize", ref stParam);
                    if (MyCamera.MV_OK != nRet)
                    {
                        DebOutPut.DebLog("获取PayloadSize失败，nRet：" + nRet);
                        DebOutPut.WriteLog(LogType.Normal, "获取PayloadSize失败，nRet：" + nRet);
                        return null;
                    }
                    nPayloadSize = stParam.nCurValue;
                    if (nPayloadSize > m_nBufSizeForDriver)
                    {
                        m_nBufSizeForDriver = nPayloadSize;
                        m_pBufForDriver = new byte[m_nBufSizeForDriver];

                        // ch:同时对保存图像的缓存做大小判断处理 | en:Determine the buffer size to save image
                        // ch:BMP图片大小：width * height * 3 + 2048(预留BMP头大小) | en:BMP image size: width * height * 3 + 2048 (Reserved for BMP header)
                        m_nBufSizeForSaveImage = m_nBufSizeForDriver * 3 + 2048;
                        m_pBufForSaveImage = new byte[m_nBufSizeForSaveImage];
                    }

                    IntPtr pData = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForDriver, 0);
                    MyCamera.MV_FRAME_OUT_INFO_EX stFrameInfo = new MyCamera.MV_FRAME_OUT_INFO_EX();
                    // ch:超时获取一帧，超时时间为1秒 | en:Get one frame timeout, timeout is 1 sec
                    nRet = m_pMyCamera.MV_CC_GetOneFrameTimeout_NET(pData, m_nBufSizeForDriver, ref stFrameInfo, 1000);
                    if (MyCamera.MV_OK != nRet)
                    {
                        DebOutPut.DebLog("无数据，nRet：" + nRet);
                        DebOutPut.WriteLog(LogType.Normal, "无数据，nRet：" + nRet);
                        return null;
                    }

                    MyCamera.MvGvspPixelType enDstPixelType;
                    if (IsMonoData(stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8;
                    }
                    else if (IsColorData(stFrameInfo.enPixelType))
                    {
                        enDstPixelType = MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed;
                    }
                    else
                    {
                        DebOutPut.DebLog("没有这种像素类型!");
                        return null;
                    }

                    IntPtr pImage = Marshal.UnsafeAddrOfPinnedArrayElement(m_pBufForSaveImage, 0);
                    MyCamera.MV_PIXEL_CONVERT_PARAM stConverPixelParam = new MyCamera.MV_PIXEL_CONVERT_PARAM();
                    stConverPixelParam.nWidth = stFrameInfo.nWidth;
                    stConverPixelParam.nHeight = stFrameInfo.nHeight;
                    stConverPixelParam.pSrcData = pData;
                    stConverPixelParam.nSrcDataLen = stFrameInfo.nFrameLen;
                    stConverPixelParam.enSrcPixelType = stFrameInfo.enPixelType;
                    stConverPixelParam.enDstPixelType = enDstPixelType;
                    stConverPixelParam.pDstBuffer = pImage;
                    stConverPixelParam.nDstBufferSize = m_nBufSizeForSaveImage;
                    nRet = m_pMyCamera.MV_CC_ConvertPixelType_NET(ref stConverPixelParam);
                    if (MyCamera.MV_OK != nRet)
                    {
                        DebOutPut.DebLog("保存失败，nRet：" + nRet);
                        DebOutPut.WriteLog(LogType.Normal, "保存失败，nRet：" + nRet);
                        return null;
                    }

                    if (enDstPixelType == MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8)
                    {
                        //************************Mono8 转 Bitmap*******************************
                        Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 1, System.Drawing.Imaging.PixelFormat.Format8bppIndexed, pImage);

                        ColorPalette cp = bmp.Palette;
                        // init palette
                        for (int i = 0; i < 256; i++)
                        {
                            cp.Entries[i] = System.Drawing.Color.FromArgb(i, i, i);
                        }
                        // set palette back
                        bmp.Palette = cp;
                        DebOutPut.DebLog("CQ12:拍照成功");
                        return bmp;
                        // bmp.Save("image.bmp", ImageFormat.Bmp);
                    }
                    else
                    {
                        //*********************RGB8 转 Bitmap**************************
                        for (int i = 0; i < stFrameInfo.nHeight; i++)
                        {
                            for (int j = 0; j < stFrameInfo.nWidth; j++)
                            {
                                byte chRed = m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3];
                                m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3] = m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3 + 2];
                                m_pBufForSaveImage[i * stFrameInfo.nWidth * 3 + j * 3 + 2] = chRed;
                            }
                        }
                        try
                        {
                            Bitmap bmp = new Bitmap(stFrameInfo.nWidth, stFrameInfo.nHeight, stFrameInfo.nWidth * 3, System.Drawing.Imaging.PixelFormat.Format24bppRgb, pImage);
                            //bmp.Save("image.bmp", ImageFormat.Bmp);
                            DebOutPut.DebLog("CQ12:拍照成功");
                            return bmp;
                        }
                        catch
                        {
                            return null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }
        }


        private Boolean IsMonoData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            try
            {
                switch (enGvspPixelType)
                {
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono8:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono10_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_Mono12_Packed:
                        return true;

                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }

        }

        /************************************************************************
        *  @fn     IsColorData()
        *  @brief  判断是否是彩色数据
        *  @param  enGvspPixelType         [IN]           像素格式
        *  @return 成功，返回0；错误，返回-1 
        ************************************************************************/
        private Boolean IsColorData(MyCamera.MvGvspPixelType enGvspPixelType)
        {
            try
            {
                switch (enGvspPixelType)
                {
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR8:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG8:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB8:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG8:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR10_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG10_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB10_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG10_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGR12_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerRG12_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerGB12_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_BayerBG12_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_RGB8_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_YUV422_YUYV_Packed:
                    case MyCamera.MvGvspPixelType.PixelType_Gvsp_YCBCR411_8_CBYYCRYY:
                        return true;
                    default:
                        return false;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }
        }



        #endregion

        /// <summary>
        /// 设备总览
        /// </summary>
        private void BtnDevOverview_Click(object sender, EventArgs e)
        {
            try
            {
                label3.ForeColor = System.Drawing.Color.Green;
                label4.ForeColor = System.Drawing.Color.White;
                label5.ForeColor = System.Drawing.Color.White;
                this.tabControl1.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 数据查看
        /// </summary>
        private void BtnLookData_Click(object sender, EventArgs e)
        {
            try
            {
                BtnLookData.Enabled = false;
                label4.ForeColor = System.Drawing.Color.Green;
                label3.ForeColor = System.Drawing.Color.White;
                label5.ForeColor = System.Drawing.Color.White;
                this.tabControl1.SelectedIndex = 1;
                UpdataDataList();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 系统设置
        /// </summary>
        private void BtnSystemSet_Click(object sender, EventArgs e)
        {
            try
            {
                label5.ForeColor = System.Drawing.Color.Green;
                label3.ForeColor = System.Drawing.Color.White;
                label4.ForeColor = System.Drawing.Color.White;
                this.tabControl1.SelectedIndex = 2;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        /// <summary>
        /// 害虫限值
        /// </summary>
        private void BtnBugLimitDefine_Click(object sender, EventArgs e)
        {
            try
            {
                if (TxtLimit.Text.Trim() == "")
                {
                    MessageBox.Show("请填入害虫限值！", "提示");
                    return;
                }
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                Tools.ConfigParmSet("Basic Parameters", "BugLimit", TxtLimit.Text.Trim(), cq12ConfigSavePath);
                //向服务器发送害虫限值
                BugLimitRoot bugLimit = new BugLimitRoot();
                bugLimit.func = 111;
                bugLimit.err = "";
                bugLimit.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                BugLimitMessage bugLimitMessage = new BugLimitMessage();
                bugLimitMessage.bugMax = int.Parse(this.TxtLimit.Text.Trim());
                bugLimit.message = bugLimitMessage;
                string data = Tools.ObjectToJson(bugLimit);
                sendDataList.Add(data);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 报警号码
        /// </summary>
        private void button9_Click(object sender, EventArgs e)
        {
            try
            {
                if (TxtAlarmNum.Text.Trim() == "")
                {
                    MessageBox.Show("请填入报警号码！", "提示");
                    return;
                }
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                Tools.ConfigParmSet("Basic Parameters", "AlarmNumber", TxtAlarmNum.Text.Trim(), cq12ConfigSavePath);
                //向服务器发送报警号码
                AlarmNumRoot alarmNumRoot = new AlarmNumRoot();
                alarmNumRoot.func = 113;
                alarmNumRoot.err = "";
                alarmNumRoot.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                string alarmNum = this.TxtAlarmNum.Text;
                string[] num = null;
                if (alarmNum.Contains(","))
                {
                    num = alarmNum.Split(',');
                }
                else if (alarmNum.Contains("，"))
                {
                    num = alarmNum.Split('，');
                }
                if (num != null && num.Length > 0)
                {
                    for (int i = 0; i < num.Length; i++)
                    {
                        alarmNumRoot.message.Add(num[i]);
                    }
                }
                else
                {
                    alarmNumRoot.message.Add(alarmNum);
                }
                string data1 = Tools.ObjectToJson(alarmNumRoot);
                sendDataList.Add(data1);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 同步工作时间段到远程
        /// </summary>
        private void BtnSync_Click(object sender, EventArgs e)
        {
            try
            {
                SetTimeLoatRoot setTimeLoatRoot = new SetTimeLoatRoot();
                setTimeLoatRoot.func = 114;
                setTimeLoatRoot.err = "";
                setTimeLoatRoot.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                //待修改
                //工作时间段
                string dt1Front = this.TimeStartHour1.Value.ToString() + ":" + this.TimeStartMin1.Value.ToString();
                string dt1FrontBack = this.TimeEndHour1.Value.ToString() + ":" + this.TimeEndMin1.Value.ToString();
                if ((dt1Front != "00:00") || (dt1FrontBack != "00:00"))
                {
                    SetTimeLoatMessageItem setTimeLoatMessageItem = new SetTimeLoatMessageItem();
                    setTimeLoatMessageItem.startTime = dt1Front;
                    setTimeLoatMessageItem.endTime = dt1FrontBack;
                    setTimeLoatRoot.message.Add(setTimeLoatMessageItem);
                }
                string dt2Front = this.TimeStartHour2.Value.ToString() + ":" + this.TimeStartMin2.Value.ToString();
                string dt2FrontBack = this.TimeEndHour2.Value.ToString() + ":" + this.TimeEndMin2.Value.ToString();
                if ((dt2Front != "00:00") || (dt2FrontBack != "00:00"))
                {
                    SetTimeLoatMessageItem setTimeLoatMessageItem = new SetTimeLoatMessageItem();
                    setTimeLoatMessageItem.startTime = dt2Front;
                    setTimeLoatMessageItem.endTime = dt2FrontBack;
                    setTimeLoatRoot.message.Add(setTimeLoatMessageItem);
                }
                string dt3Front = this.TimeStartHour3.Value.ToString() + ":" + this.TimeStartMin3.Value.ToString();
                string dt3FrontBack = this.TimeEndHour3.Value.ToString() + ":" + this.TimeEndMin3.Value.ToString();
                if ((dt3Front != "00:00") || (dt3FrontBack != "00:00"))
                {
                    SetTimeLoatMessageItem setTimeLoatMessageItem = new SetTimeLoatMessageItem();
                    setTimeLoatMessageItem.startTime = dt3Front;
                    setTimeLoatMessageItem.endTime = dt3FrontBack;
                    setTimeLoatRoot.message.Add(setTimeLoatMessageItem);
                }
                string dt4Front = this.TimeStartHour4.Value.ToString() + ":" + this.TimeStartMin4.Value.ToString();
                string dt4FrontBack = this.TimeEndHour4.Value.ToString() + ":" + this.TimeEndMin4.Value.ToString();
                if ((dt4Front != "00:00") || (dt4FrontBack != "00:00"))
                {
                    SetTimeLoatMessageItem setTimeLoatMessageItem = new SetTimeLoatMessageItem();
                    setTimeLoatMessageItem.startTime = dt4Front;
                    setTimeLoatMessageItem.endTime = dt4FrontBack;
                    setTimeLoatRoot.message.Add(setTimeLoatMessageItem);
                }
                string dt5Front = this.TimeStartHour5.Value.ToString() + ":" + this.TimeStartMin5.Value.ToString();
                string dt5FrontBack = this.TimeEndHour5.Value.ToString() + ":" + this.TimeEndMin5.Value.ToString();
                if ((dt5Front != "00:00") || (dt5FrontBack != "00:00"))
                {
                    SetTimeLoatMessageItem setTimeLoatMessageItem = new SetTimeLoatMessageItem();
                    setTimeLoatMessageItem.startTime = dt5Front;
                    setTimeLoatMessageItem.endTime = dt5FrontBack;
                    setTimeLoatRoot.message.Add(setTimeLoatMessageItem);
                }
                string data = Tools.ObjectToJson(setTimeLoatRoot);
                sendDataList.Add(data);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }



        /// <summary>
        /// 读取配置文件
        /// </summary>
        private void ReadConfig()
        {
            try
            {
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                SaveDataModel.cQ12ConfigParamModel.deviceID = Tools.ConfigParmRead("Basic Parameters", "DeviceID", cq12ConfigSavePath);
                SaveDataModel.cQ12ConfigParamModel.uploadAddress = Tools.ConfigParmRead("Basic Parameters", "UploadAddress", cq12ConfigSavePath);
                SaveDataModel.cQ12ConfigParamModel.uploadPort = Tools.ConfigParmRead("Basic Parameters", "UploadPort", cq12ConfigSavePath);
                SaveDataModel.cQ12ConfigParamModel.counterAddress = Tools.ConfigParmRead("Basic Parameters", "CounterAddress", cq12ConfigSavePath);
                SaveDataModel.cQ12ConfigParamModel.alarmNumber = Tools.ConfigParmRead("Basic Parameters", "AlarmNumber", cq12ConfigSavePath);
                SaveDataModel.cQ12ConfigParamModel.alarmSwitch = Tools.ConfigParmRead("Basic Parameters", "AlarmSwitch", cq12ConfigSavePath);
                SaveDataModel.cQ12ConfigParamModel.serialPortName = Tools.ConfigParmRead("Basic Parameters", "SerialPortName", cq12ConfigSavePath);
                SaveDataModel.cQ12ConfigParamModel.bugLimit = Tools.ConfigParmRead("Basic Parameters", "BugLimit", cq12ConfigSavePath);

                if (SaveDataModel.cQ12ConfigParamModel.alarmSwitch == "N0")
                {
                    this.Btnwitch.Value = false;
                }
                else if (SaveDataModel.cQ12ConfigParamModel.alarmSwitch == "OK")
                {
                    this.Btnwitch.Value = true;
                }
                this.TxtAlarmNum.Text = SaveDataModel.cQ12ConfigParamModel.alarmNumber;
                this.LabSerAddress.Text = SaveDataModel.cQ12ConfigParamModel.uploadAddress;
                this.LabSerPort.Text = SaveDataModel.cQ12ConfigParamModel.uploadPort;
                this.TxtCountAddress.Text = SaveDataModel.cQ12ConfigParamModel.counterAddress;
                this.TxtLimit.Text = SaveDataModel.cQ12ConfigParamModel.bugLimit;
                this.textBox1.Text = SaveDataModel.cQ12ConfigParamModel.deviceID;

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 重置服务器地址和端口
        /// </summary>
        private void BtnReset_Click(object sender, EventArgs e)
        {
            try
            {
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                Tools.ConfigParmSet("Basic Parameters", "UploadAddress", this.LabSerAddress.Text.Trim(), cq12ConfigSavePath);
                Tools.ConfigParmSet("Basic Parameters", "UploadPort", this.LabSerPort.Text.Trim(), cq12ConfigSavePath);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 创建串口
        /// </summary>
        /// <returns></returns>
        private void CreateSerialPort()
        {
            try
            {
                //设置串口信息
                serialPort.RtsEnable = true; // 指示本设备准备好可接收数据
                serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceived);
                isOpen = SerialPortCtrl.SetSerialPortAttribute(serialPort,
                    SaveDataModel.cQ12ConfigParamModel.serialPortName, 9600);

                if (isOpen)
                {
                    if (!PubField.devRunName.Contains("虫情"))
                    {
                        PubField.devRunName.Add("虫情");
                        DevOverviewMain.devRunNetCountUpdata();
                    }
                    DebOutPut.DebLog("CQ12串口打开成功！");
                    DebOutPut.WriteLog(LogType.Normal, "CQ12串口打开成功！");
                }
                CreateScoket();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 创建长连接
        /// </summary>
        private void CreateScoket()
        {
            try
            {
                string serverIp = (SaveDataModel.cQ12ConfigParamModel.uploadAddress == null) ? "" : SaveDataModel.cQ12ConfigParamModel.uploadAddress;
                string serverPort = (SaveDataModel.cQ12ConfigParamModel.uploadPort == null) ? "" : SaveDataModel.cQ12ConfigParamModel.uploadPort;
                if (serverIp == "" || serverPort == "")
                {
                    MessageBox.Show("远程服务器参数配置错误!", "提示");
                    return;
                }
                if (!Tools.IsItAIP(serverIp))
                {
                    serverIp = Tools.GetIP(serverIp);
                }
                if (serverIp == "" || !Tools.IsItAIP(serverIp))
                {
                    DebOutPut.DebLog("域名/IP:" + serverIp + " 无效，请关闭串口，确认参数设置无误后，重新打开!");
                    return;
                }
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(serverIp), int.Parse(serverPort));
                //连接服务器
                clientSocket.Connect(ipEndPoint);
                if (clientSocket.Connected)
                {
                    newDataTime = DateTime.Now;
                    DebOutPut.DebLog("CQ12客户端连接成功!");
                    //if (!PubField.devNetName.Contains("虫情"))
                    //{
                    //    PubField.devNetName.Add("虫情");
                    //    DevOverviewMain.devRunNetCountUpdata();
                    //}
                    timer1.Start();//发送数据
                    timer2.Start();//接收数据
                    timer3.Start();//心跳
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        //本次上传图像是否成功
        bool isUpladCom = false;
        Encoding encoding = Encoding.GetEncoding("gb2312");
        List<string> sendDataList_ = new List<string>();
        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer1, 1) == 0)
                {
                    if (clientSocket != null && clientSocket.Connected && newDataTime.AddMinutes(3) > DateTime.Now)
                    {
                        if (sendDataList.Count > 0)
                            sendDataList_.Clear();
                        for (int i = 0; i < sendDataList.Count; i++)
                        {
                            if (clientSocket != null && clientSocket.Connected && newDataTime.AddMinutes(3) > DateTime.Now)
                            {
                                byte[] sendBytes = encoding.GetBytes(sendDataList[i] + "\r\n");
                                int x = clientSocket.Send(sendBytes);
                                JObject obj1 = JObject.Parse(sendDataList[i]);
                                string func = obj1["func"].ToString();
                                if (func == "101")
                                {
                                    if (x != 0)
                                    {
                                        DateTime startTime = DateTime.Now;
                                        bool isUpladResult = true;//本张图像服务器回应结果
                                        while (!isUpladCom)
                                        {
                                            Console.WriteLine("未收到回应");
                                            //这条采集数据发送3分钟之后仍然没有收到回应，就停止等待，终止本次发送
                                            if (startTime.AddMinutes(3) < DateTime.Now)
                                            {
                                                isUpladResult = false;//代表本张图像上传失败
                                                break;
                                            }
                                            Thread.Sleep(1000);
                                        }
                                        if (!isUpladResult)
                                        {
                                            DebOutPut.DebLog("数据上传未收到回应，本次发送将被终止");
                                            DebOutPut.WriteLog(LogType.Normal, "数据上传未收到回应，本次发送将被终止");
                                            break;
                                        }
                                        else if (isUpladResult)
                                        {
                                            DebOutPut.DebLog("数据上传已收到回应!");
                                            isUpladCom = false;
                                        }
                                    }
                                    else
                                    {
                                        DebOutPut.DebLog("发现无法上传数据");
                                        DebOutPut.WriteLog(LogType.Normal, "发现无法上传数据");
                                    }
                                }
                                if (sendDataList[i].Length > 5000)
                                    DebOutPut.DebLog("CQ12_发送数据完成 sendDataList[i].Length：" + sendDataList[i].Length);
                                else
                                    DebOutPut.DebLog("发送:" + sendDataList[i]);
                                sendDataList_.Add(sendDataList[i]);
                                Thread.Sleep(1000);
                            }
                        }
                        for (int i = 0; i < sendDataList_.Count; i++)
                            if (sendDataList.Contains(sendDataList_[i]))
                                sendDataList.Remove(sendDataList_[i]);
                    }
                    Interlocked.Exchange(ref inTimer1, 0);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (clientSocket != null && clientSocket.Connected)
                {
                    DebOutPut.DebLog("发送异常，断开连接");
                    closeSocket();
                }
                Interlocked.Exchange(ref inTimer1, 0);
            }
        }

        /// <summary>
        /// 接受数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer2, 1) == 0)
                {
                    if (clientSocket != null && clientSocket.Connected)
                    {
                        newDataTime = DateTime.Now;
                        byte[] receive = new byte[1024];
                        clientSocket.Receive(receive);
                        string receiceMsg = Encoding.Default.GetString(receive);
                        receiceMsg = receiceMsg.Substring(0, receiceMsg.LastIndexOf("}") + 1);
                        DebOutPut.DebLog("接收:" + receiceMsg);
                        JObject obj = JObject.Parse(receiceMsg);
                        string fucn = obj["func"].ToString();
                        string devId = obj["devId"].ToString();
                        if (devId == SaveDataModel.cQ12ConfigParamModel.deviceID)
                            ScoketReceiveDataHandle(fucn, receiceMsg);
                    }
                    Interlocked.Exchange(ref inTimer2, 0);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (clientSocket != null && clientSocket.Connected)
                {
                    DebOutPut.DebLog("接收异常，断开连接");
                    closeSocket();
                }
                Interlocked.Exchange(ref inTimer2, 0);
            }
        }

        bool isConnect = true;
        /// <summary>
        /// 心跳
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer3_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Interlocked.Exchange(ref inTimer3, 1) == 0)
                {
                    if (clientSocket == null || !clientSocket.Connected || newDataTime.AddMinutes(3) < DateTime.Now)
                    {
                        if (PubField.devNetName.Contains("虫情"))
                        {
                            PubField.devNetName.Remove("虫情");
                            DevOverviewMain.devRunNetCountUpdata();
                        }
                        if (isConnect)
                        {
                            closeSocket();
                            DebOutPut.DebLog("连接已断开，正在重新连接...");
                            DebOutPut.WriteLog(LogType.Normal, "连接已断开，正在重新连接...");
                            Thread.Sleep(1000 * 60);
                            CreateScoket();
                        }
                    }
                    else
                    {
                        //保活包体
                        CQ12KeepLive cq12KeepLive = new CQ12KeepLive();
                        cq12KeepLive.message = "keep-alive";//数据
                        cq12KeepLive.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;//设备id
                        cq12KeepLive.func = 100;//功能码
                        cq12KeepLive.err = "";//错误
                        string data = Tools.ObjectToJson(cq12KeepLive);
                        sendDataList.Add(data);
                    }
                    Interlocked.Exchange(ref inTimer3, 0);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (clientSocket != null && clientSocket.Connected)
                {
                    DebOutPut.DebLog("心跳异常，断开连接");
                    closeSocket();
                }
                Interlocked.Exchange(ref inTimer3, 0);
            }
        }


        public void closeSocket()
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                    Thread.Sleep(10);
                    clientSocket.Disconnect(false);
                    DebOutPut.DebLog("断开连接");
                    DebOutPut.WriteLog(LogType.Normal, "CQ12断开连接");
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
            finally
            {
                if (clientSocket != null)
                {
                    clientSocket.Close();
                    clientSocket = null;
                }
            }
        }
        /// <summary>
        /// 串口收到数据执行的方法
        /// </summary>
        private void DataReceived(object sender, EventArgs e)
        {
            try
            {
                Thread.Sleep(250);  //延迟100ms等待接收完成数据
                int ilen = serialPort.BytesToRead;
                byte[] bytes = new byte[ilen];
                serialPort.Read(bytes, 0, ilen);
                if (bytes.Length <= 0) return;
                string recStr = Tools.ByteToHexStr(bytes);//接收到的
                DebOutPut.DebLog("CQ12接收:" + recStr.ToUpper());
                //if (this.InvokeRequired)
                //{
                //    this.Invoke(new Action(() =>
                //    {
                //        ReceivedDataHandle(recStr);
                //        //this.Refresh();
                //    }));
                //}
                ReceivedDataHandle(recStr);
                serialPort.DiscardInBuffer();
                bytes = null;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        /// <summary>
        /// Scoket收到数据之后的处理
        /// </summary>
        /// <param name="fucn">功能码</param>
        /// <param name="receiceMsg">收到的数据</param>
        private void ScoketReceiveDataHandle(string fucn, string receiceMsg)
        {
            try
            {
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                switch (fucn)
                {
                    case "101"://发送采集信息返回
                        JObject obj1 = JObject.Parse(receiceMsg);
                        string collectTime = DateTime.Parse(obj1["collectTime"].ToString()).ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                        string sql = "update Record Set Flag = '1' where CollectTime='" + collectTime + "'";
                        int x = DB_CQ12.updateDatabase(sql);
                        if (x != -1)
                            isUpladCom = true;
                        else
                        {
                            isUpladCom = false;
                            DebOutPut.DebLog("收到采集信息上传回复，但数据库标志位更改失败！");
                            DebOutPut.WriteLog(LogType.Normal, "收到采集信息上传回复，但数据库标志位更改失败！");
                        }
                        break;
                    case "110"://上传设备参数
                        UpLoadDevParamRoot upLoadDevParamRoot = new UpLoadDevParamRoot();
                        upLoadDevParamRoot.func = 110;
                        upLoadDevParamRoot.err = "";
                        upLoadDevParamRoot.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        UpLoadDevParamMessage upLoadDevParamMessage = new UpLoadDevParamMessage();
                        upLoadDevParamMessage.electricity = int.Parse(this.LabChargingCurrent.Text.Trim());
                        upLoadDevParamMessage.voltage = int.Parse(this.LabVoltage.Text.Trim());
                        upLoadDevParamMessage.autoRollOverTime = this.LabTurn.Text.Trim().Substring(this.LabTurn.Text.Trim().LastIndexOf("：") + 1);
                        upLoadDevParamMessage.squence_time = int
                            .Parse(this.TxtZhendongTime.Text.Trim());
                        upLoadDevParamMessage.squence_strength = int.Parse(this.TxtZhendongqiangdu.Text.Trim());
                        upLoadDevParamMessage.inbug_time = int.Parse(this.TxtJinchong.Text.Trim());
                        upLoadDevParamMessage.killTime = int.Parse(this.Txtluochong.Text.Trim());
                        upLoadDevParamMessage.hotTime = int.Parse(this.TxtJiare.Text.Trim());
                        upLoadDevParamRoot.message = upLoadDevParamMessage;
                        string data = Tools.ObjectToJson(upLoadDevParamRoot);
                        sendDataList.Add(data);
                        break;
                    case "114"://上传工作模式
                        SendWorkModeModel sendWorkModeModel = new SendWorkModeModel();
                        sendWorkModeModel.func = 144;
                        sendWorkModeModel.err = "";
                        sendWorkModeModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        //sendWorkModeModel.message = workMode;
                        if (this.ForceWork.Value == true)
                        {
                            sendWorkModeModel.message = "force";
                        }
                        else if (this.TimeSlotWork.Value == true)
                        {
                            sendWorkModeModel.message = "worktimes";
                        }
                        else if (this.WorkMode.Value == false)
                        {
                            sendWorkModeModel.message = "normal";
                        }
                        else if (this.WorkMode.Value == true)
                        {
                            sendWorkModeModel.message = "debug";
                        }
                        string data2 = Tools.ObjectToJson(sendWorkModeModel);
                        sendDataList.Add(data2);
                        break;
                    case "120"://远程设置报警号码
                               //AlarmNumRoot alarmNumRoot = new AlarmNumRoot();
                        LongRangeSetAlarmNum longRangeSetAlarmNum = JsonMapper.ToObject<LongRangeSetAlarmNum>(receiceMsg);
                        string alarmNum = "";
                        for (int i = 0; i < longRangeSetAlarmNum.message.Count; i++)
                        {
                            if (alarmNum == "")
                            {
                                alarmNum += longRangeSetAlarmNum.message[i].ToString();
                            }
                            else
                            {
                                alarmNum += "," + longRangeSetAlarmNum.message[i].ToString();
                            }
                        }
                        this.TxtAlarmNum.Text = alarmNum;
                        Tools.ConfigParmSet("Basic Parameters", "AlarmNumber", TxtAlarmNum.Text.Trim(), cq12ConfigSavePath);

                        LongRangeReceive longRangeReceive = new LongRangeReceive();
                        longRangeReceive.func = 120;
                        longRangeReceive.err = "";
                        longRangeReceive.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive.message = "";
                        string data3 = Tools.ObjectToJson(longRangeReceive);
                        sendDataList.Add(data3);
                        break;
                    case "121"://远程设置害虫阀值
                        LongRangeSetBugLimitRoot longRangeSetBugLimitRoot = JsonMapper.ToObject<LongRangeSetBugLimitRoot>(receiceMsg);
                        this.TxtLimit.Text = longRangeSetBugLimitRoot.message.bugMax.ToString();
                        Tools.ConfigParmSet("Basic Parameters", "BugLimit", this.TxtLimit.Text.Trim(), cq12ConfigSavePath);
                        LongRangeReceive longRangeReceive1 = new LongRangeReceive();
                        longRangeReceive1.func = 121;
                        longRangeReceive1.err = "";
                        longRangeReceive1.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive1.message = "";
                        string data4 = Tools.ObjectToJson(longRangeReceive1);
                        sendDataList.Add(data4);
                        break;
                    case "123"://远程设置设备参数
                        LongRangeSetDevParamRoot longRangeSetDevParamRoot = JsonMapper.ToObject<LongRangeSetDevParamRoot>(receiceMsg);
                        //自动转仓时间
                        string zhuancangTime = longRangeSetDevParamRoot.message.autoRollOverTime;
                        DebOutPut.DebLog("转仓时间");
                        Thread.Sleep(500);
                        this.TurnTime.Text = zhuancangTime;
                        string time = this.TurnTime.Text.Trim().Replace("：", ":");
                        string hour = Tools.TenToSixteen(time.Substring(0, time.LastIndexOf(":"))).PadLeft(2, '0');
                        string min = Tools.TenToSixteen(time.Substring(time.LastIndexOf(":") + 1, time.Length - time.LastIndexOf(":") - 1)).PadLeft(2, '0');
                        string frameHead = "AB5A";
                        string func = "01";
                        string ad = "00";
                        string content = hour + min;
                        string frameTail = "5AAF";
                        string length = Tools.TenToSixteen((((func + ad + content).Length + 2) / 2).ToString()).PadLeft(2, '0');
                        string msg = frameHead + length + func + ad + content + frameTail;
                        SerialPortCtrl.DataSend_CQ12(serialPort, msg);
                        //震动时间
                        string zhendongTime = longRangeSetDevParamRoot.message.squence_time.ToString();
                        DebOutPut.DebLog("震动时间");
                        Thread.Sleep(500);
                        this.TxtShockTime.Text = zhendongTime;
                        string time1 = Tools.TenToSixteen(this.TxtShockTime.Text.Trim()).PadLeft(2, '0');
                        string msg1 = "AB 5A 04 54 00 " + time1.ToUpper() + " 5A AF";
                        SerialPortCtrl.DataSend_CQ12(serialPort, msg1);
                        //震动强度
                        string zhendongqiangdu = longRangeSetDevParamRoot.message.squence_strength.ToString();
                        DebOutPut.DebLog("震动强度");
                        Thread.Sleep(500);
                        this.TxtShock.Text = zhendongqiangdu;
                        string strength = Tools.TenToSixteen(this.TxtShock.Text.Trim()).PadLeft(4, '0');
                        string msg2 = "AB 5A 05 53 00 " + strength.ToUpper() + " 5A AF";
                        SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                        //进虫时间
                        string jinchongTime = longRangeSetDevParamRoot.message.inbug_time.ToString();
                        DebOutPut.DebLog("进虫等待");
                        Thread.Sleep(500);
                        this.TxtEnteringWorm.Text = jinchongTime;
                        string time2 = Tools.TenToSixteen(this.TxtEnteringWorm.Text.Trim()).PadLeft(2, '0');
                        string msg3 = "AB 5A 04 55 00 " + time2.ToUpper() + " 5A AF";
                        SerialPortCtrl.DataSend_CQ12(serialPort, msg3);
                        //落虫时间
                        string luochongTime = longRangeSetDevParamRoot.message.killTime.ToString();
                        DebOutPut.DebLog("落虫时间");
                        Thread.Sleep(500);
                        this.TxtBugTime.Text = luochongTime;
                        string time3 = Tools.TenToSixteen(this.TxtBugTime.Text.Trim()).PadLeft(2, '0');
                        string msg4 = "AB 5A 04 11 00 " + time3.ToUpper() + " 5A AF";
                        SerialPortCtrl.DataSend_CQ12(serialPort, msg4);
                        //加热时间
                        string jiareTime = longRangeSetDevParamRoot.message.hotTime.ToString();
                        DebOutPut.DebLog("加热时间");
                        Thread.Sleep(500);
                        this.TxtHeatingTime.Text = jiareTime;
                        string time4 = Tools.TenToSixteen(this.TxtHeatingTime.Text.Trim()).PadLeft(2, '0');
                        string msg5 = "AB 5A 04 10 00 " + time4.ToUpper() + " 5A AF";
                        SerialPortCtrl.DataSend_CQ12(serialPort, msg5);


                        LongRangeReceive longRangeReceive2 = new LongRangeReceive();
                        longRangeReceive2.func = 123;
                        longRangeReceive2.err = "";
                        longRangeReceive2.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive2.message = "";
                        string data5 = Tools.ObjectToJson(longRangeReceive2);
                        sendDataList.Add(data5);
                        break;
                    case "126"://远程拍照
                        LongRangeSetPhotographRoot longRangeSetPhotographRoot = JsonMapper.ToObject<LongRangeSetPhotographRoot>(receiceMsg);
                        JObject obj = JObject.Parse(longRangeSetPhotographRoot.message);
                        string type = obj["type"].ToString();
                        if (type == "manual")
                        {
                            DebOutPut.DebLog("获取害虫数目");
                            Thread.Sleep(500);
                            string msg6 = "AB 5A 03 30 00 5A AF";
                            SerialPortCtrl.DataSend_CQ12(serialPort, msg6);

                            LongRangeReceive longRangeReceive3 = new LongRangeReceive();
                            longRangeReceive3.func = 126;
                            longRangeReceive3.err = "";
                            longRangeReceive3.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                            longRangeReceive3.message = "";
                            string data6 = Tools.ObjectToJson(longRangeReceive3);
                            sendDataList.Add(data6);
                        }
                        break;
                    case "127"://远程开关灯
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot.message == "open")//开灯
                        {
                            this.Youchong.Value = true;
                            Youchong_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot.message == "close")//关灯
                        {
                            this.Youchong.Value = false;
                            Youchong_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive4 = new LongRangeReceive();
                        longRangeReceive4.func = 127;
                        longRangeReceive4.err = "";
                        longRangeReceive4.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive4.message = "";
                        string data7 = Tools.ObjectToJson(longRangeReceive4);
                        sendDataList.Add(data7);
                        break;
                    case "128"://远程转仓
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot1 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot1.message == "turn")
                        {
                            RunWare_Click(null, null);
                            LongRangeReceive longRangeReceive5 = new LongRangeReceive();
                            longRangeReceive5.func = 128;
                            longRangeReceive5.err = "";
                            longRangeReceive5.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                            longRangeReceive5.message = "";
                            string data8 = Tools.ObjectToJson(longRangeReceive5);
                            sendDataList.Add(data8);
                        }
                        break;
                    case "129"://单片机重启复位
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot2 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot2.message == "restart")
                        {
                            //单片机重启复位
                            Thread.Sleep(500);
                            DebOutPut.DebLog("单片机重启复位");
                            string msg7 = "AB 5A 03 60 00 5A AF";
                            SerialPortCtrl.DataSend_CQ12(serialPort, msg7);
                            Thread.Sleep(1000);
                            //单片机重启复位
                            DebOutPut.DebLog("单片机重启复位");
                            //serialPort.Close();
                            //Thread.Sleep(500);
                            //clientSocket.Close();
                            //Thread.Sleep(500);
                            //this.Invoke(new Action(() =>
                            //{
                            //    CloseCamera();
                            //    Thread.Sleep(500);
                            //    CQ12MainForm cQ12MainForm = new CQ12MainForm();
                            //    Tools.AgainOpenMdiForm(Main.instance, cQ12MainForm);
                            //}));
                        }
                        break;
                    case "130"://远程上落虫
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot3 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot3.message == "open")
                        {
                            this.Shangluochong.Value = true;
                            Shangluochong_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot3.message == "close")
                        {
                            this.Shangluochong.Value = false;
                            Shangluochong_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive6 = new LongRangeReceive();
                        longRangeReceive6.func = 130;
                        longRangeReceive6.err = "";
                        longRangeReceive6.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive6.message = "";
                        string data9 = Tools.ObjectToJson(longRangeReceive6);
                        sendDataList.Add(data9);
                        break;
                    case "131"://远程补光灯
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot4 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot4.message == "open")
                        {
                            this.Buguangdeng.Value = true;
                            Buguangdeng_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot4.message == "close")
                        {
                            this.Buguangdeng.Value = false;
                            Buguangdeng_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive7 = new LongRangeReceive();
                        longRangeReceive7.func = 131;
                        longRangeReceive7.err = "";
                        longRangeReceive7.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive7.message = "";
                        string data10 = Tools.ObjectToJson(longRangeReceive7);
                        sendDataList.Add(data10);
                        break;
                    case "132"://远程排水
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot5 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot5.message == "open")
                        {
                            this.Paishui.Value = true;
                            Paishui_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot5.message == "close")
                        {
                            this.Paishui.Value = false;
                            Paishui_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive8 = new LongRangeReceive();
                        longRangeReceive8.func = 132;
                        longRangeReceive8.err = "";
                        longRangeReceive8.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive8.message = "";
                        string data11 = Tools.ObjectToJson(longRangeReceive8);
                        sendDataList.Add(data11);
                        break;
                    case "133"://远程加热管
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot6 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot6.message == "open")
                        {
                            this.Jiareguan.Value = true;
                            Jiareguan_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot6.message == "close")
                        {
                            this.Jiareguan.Value = false;
                            Jiareguan_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive9 = new LongRangeReceive();
                        longRangeReceive9.func = 133;
                        longRangeReceive9.err = "";
                        longRangeReceive9.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive9.message = "";
                        string data12 = Tools.ObjectToJson(longRangeReceive9);
                        sendDataList.Add(data12);
                        break;
                    case "134"://远程履带
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot7 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot7.message == "open")
                        {
                            this.Lvdai.Value = true;
                            Lvdai_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot7.message == "close")
                        {
                            this.Lvdai.Value = false;
                            Lvdai_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive10 = new LongRangeReceive();
                        longRangeReceive10.func = 134;
                        longRangeReceive10.err = "";
                        longRangeReceive10.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive10.message = "";
                        string data13 = Tools.ObjectToJson(longRangeReceive10);
                        sendDataList.Add(data13);
                        break;
                    case "135"://远程下落虫
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot8 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot8.message == "open")
                        {
                            this.Xialuochong.Value = true;
                            Xialuochong_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot8.message == "close")
                        {
                            this.Xialuochong.Value = false;
                            Xialuochong_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive11 = new LongRangeReceive();
                        longRangeReceive11.func = 135;
                        longRangeReceive11.err = "";
                        longRangeReceive11.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive11.message = "";
                        string data14 = Tools.ObjectToJson(longRangeReceive11);
                        sendDataList.Add(data14);
                        break;
                    case "136"://远程设置工作模式
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot9 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot9.message == "debug")
                        {
                            WorkMode.Value = true;
                            WorkMode_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot9.message == "normal")
                        {
                            WorkMode.Value = false;
                            WorkMode_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot9.message == "force")
                        {
                            ForceWork.Value = true;
                            ForceWork_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot9.message == "worktimes")
                        {
                            TimeSlotWork.Value = true;
                            TimeSlotWork_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive12 = new LongRangeReceive();
                        longRangeReceive12.func = 136;
                        longRangeReceive12.err = "";
                        longRangeReceive12.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive12.message = "";
                        string data15 = Tools.ObjectToJson(longRangeReceive12);
                        sendDataList.Add(data15);
                        break;
                    case "137"://远程震动
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot10 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot10.message == "open")
                        {
                            this.Zhendong.Value = true;
                            Zhendong_ValueChanged(null, null);
                        }
                        else if (longRangeSetYouChongRoot10.message == "close")
                        {
                            this.Zhendong.Value = false;
                            Zhendong_ValueChanged(null, null);
                        }
                        LongRangeReceive longRangeReceive13 = new LongRangeReceive();
                        longRangeReceive13.func = 137;
                        longRangeReceive13.err = "";
                        longRangeReceive13.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive13.message = "";
                        string data16 = Tools.ObjectToJson(longRangeReceive13);
                        sendDataList.Add(data16);
                        break;
                    case "140"://远程更改清虫接虫
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot11 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot11.message == "asc")//接虫
                        {
                            Thread.Sleep(500);
                            DebOutPut.DebLog("远程更改接虫");
                            string msg8 = "AB 5A 04 56 00 00 5A AF";
                            SerialPortCtrl.DataSend_CQ12(serialPort, msg8);
                        }
                        else if (longRangeSetYouChongRoot11.message == "Desc")//清虫
                        {
                            Thread.Sleep(500);
                            DebOutPut.DebLog("远程更改清虫");
                            string msg8 = "AB 5A 04 56 00 01 5A AF";
                            SerialPortCtrl.DataSend_CQ12(serialPort, msg8);
                        }
                        LongRangeReceive longRangeReceive14 = new LongRangeReceive();
                        longRangeReceive14.func = 140;
                        longRangeReceive14.err = "";
                        longRangeReceive14.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive14.message = "";
                        string data17 = Tools.ObjectToJson(longRangeReceive14);
                        sendDataList.Add(data17);
                        break;
                    case "141"://远程更改服务器地址
                        LongRangeSetIPRoot longRangeSetIPRoot = JsonMapper.ToObject<LongRangeSetIPRoot>(receiceMsg);
                        string ip = longRangeSetIPRoot.message.ip;
                        string port = longRangeSetIPRoot.message.port;
                        Tools.ConfigParmSet("Basic Parameters", "UploadAddress", ip, cq12ConfigSavePath);
                        Tools.ConfigParmSet("Basic Parameters", "UploadPort", port, cq12ConfigSavePath);
                        DebOutPut.DebLog("程序重启");
                        serialPort.Close();
                        Thread.Sleep(500);
                        clientSocket.Close();
                        Thread.Sleep(500);
                        this.Invoke(new Action(() =>
                        {
                            CQ12MainForm cQ12MainForm = new CQ12MainForm();
                            Tools.AgainOpenMdiForm(Main.instance, cQ12MainForm);
                        }));

                        break;
                    case "142"://远程控制测报灯开启
                        LongRangeSetYouChongRoot longRangeSetYouChongRoot15 = JsonMapper.ToObject<LongRangeSetYouChongRoot>(receiceMsg);
                        if (longRangeSetYouChongRoot15.message == "open")
                        {
                            Thread.Sleep(500);
                            DebOutPut.DebLog("测报灯开");
                            string msg8 = "AB 5A 04 14 00 00 5A AF";
                            SerialPortCtrl.DataSend_CQ12(serialPort, msg8);
                        }
                        else if (longRangeSetYouChongRoot15.message == "close")
                        {
                            Thread.Sleep(500);
                            DebOutPut.DebLog("测报灯关");
                            string msg8 = "AB 5A 04 14 00 01 5A AF";
                            SerialPortCtrl.DataSend_CQ12(serialPort, msg8);
                        }
                        LongRangeReceive longRangeReceive15 = new LongRangeReceive();
                        longRangeReceive15.func = 140;
                        longRangeReceive15.err = "";
                        longRangeReceive15.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        longRangeReceive15.message = "";
                        string data18 = Tools.ObjectToJson(longRangeReceive15);
                        sendDataList.Add(data18);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        int MCURestartCount = 0;//单片机重启次数
        DateTime startEXETime;//exe启动时间
        /// <summary>
        /// 是否可以拍照
        /// </summary>
        static bool isPhoto = true;

        /// <summary>
        /// 串口收到数据之后的处理
        /// </summary>
        /// <param name="recStr"></param>
        private void ReceivedDataHandle(string recStr)
        {
            try
            {
                string recData = recStr;
                if (!recData.Replace(" ", "").ToUpper().Contains("AB5A") || !recData.Replace(" ", "").ToUpper().Contains("5AAF"))
                {
                    return;
                }
                string frameHead = recData.Substring(0, 4).Replace(" ", "").ToUpper();
                string frameTail = recData.Substring(recData.Length - 4).Replace(" ", "").ToUpper();
                if (frameHead != "AB5A" || frameTail != "5AAF")
                {
                    return;
                }
                //判断功能码
                string length = (int.Parse(Tools.SixteenToTen(recData.Substring(4, 2))) * 2).ToString();//长度
                string func = recData.Substring(6, 2);//功能码
                string address = recData.Substring(8, 2);//地址
                string content = recData.Substring(10, int.Parse(length) - 6);//内容
                switch (func)
                {
                    case "20"://电压电流状态
                        string f1 = content.Substring(0, 2);//f1
                        switch (f1)
                        {
                            case "01":
                                string state = Tools.SixteenToTwo(content.Substring(2, 2)).PadLeft(2, '0');//状态
                                if (state.Substring(1, 1) == "0")
                                {
                                    this.LabElectricType.Text = "交流供电";
                                }
                                else if (state.Substring(1, 1) == "1")
                                {
                                    this.LabElectricType.Text = "直流供电";
                                }
                                if (state.Substring(0, 1) == "0")
                                {
                                    this.LabElectricState.Text = "放电";
                                }
                                else if (state.Substring(0, 1) == "1")
                                {
                                    this.LabElectricState.Text = "充电";

                                }
                                string voltage = Tools.SixteenToTen(content.Substring(4, 2));//电压
                                if (this.LabElectricType.Text == "交流供电")
                                {
                                    this.LabVoltage.Text = (float.Parse(voltage) * 1).ToString("F1") + "V";

                                }
                                else if (this.LabElectricType.Text == "直流供电")
                                {
                                    this.LabVoltage.Text = (float.Parse(voltage) * 0.1f).ToString("F1") + "V";

                                }
                                string chargingCurrent = content.Substring(6, 4);//充电电流
                                this.LabChargingCurrent.Text = (float.Parse(Tools.SixteenToTen(chargingCurrent.Substring(0, 2))) * 0.01f).ToString("F1") + "A";
                                string workCurrent = content.Substring(10, 4);//工作电流
                                this.LabWorkCurrent.Text = (float.Parse(Tools.SixteenToTen(workCurrent.Substring(0, 2))) * 0.01f).ToString("F1") + "A";
                                string temp = content.Substring(14, 4);//温度
                                this.LabTemp.Text = float.Parse(Tools.SixteenToTen(temp.Substring(2, 2))).ToString("F1") + "℃";
                                break;
                            case "02"://系统配置
                                string workMode = content.Substring(2, 2);//工作模式
                                if (workMode == "01")//正常工作
                                {
                                    this.WorkMode.Value = false;
                                    this.ForceWork.Value = false;
                                    this.TimeSlotWork.Value = false;
                                    DevOverviewMain.cQ12WorkModeUpdata(CQ12WorkMode.Normal);
                                    isCanClick(false);
                                }
                                else if (workMode == "02")//强制工作
                                {
                                    this.WorkMode.Value = false;
                                    this.ForceWork.Value = true;
                                    this.TimeSlotWork.Value = false;
                                    DevOverviewMain.cQ12WorkModeUpdata(CQ12WorkMode.Force);
                                    isCanClick(false);
                                }
                                else if (workMode == "03")//工作时间段工作
                                {
                                    this.WorkMode.Value = false;
                                    this.ForceWork.Value = false;
                                    this.TimeSlotWork.Value = true;
                                    DevOverviewMain.cQ12WorkModeUpdata(CQ12WorkMode.TimeSolt);
                                    isCanClick(false);
                                }
                                else if (workMode == "04")//调试模式
                                {
                                    this.WorkMode.Value = true;
                                    this.ForceWork.Value = false;
                                    this.TimeSlotWork.Value = false;
                                    DevOverviewMain.cQ12WorkModeUpdata(CQ12WorkMode.Debug);
                                    isCanClick(true);
                                }
                                string turnTime = content.Substring(4, 4);//转仓时间
                                string hour = Tools.SixteenToTen(turnTime.Substring(0, 2));
                                string minu = Tools.SixteenToTen(turnTime.Substring(2, 2));
                                if (JudgeTime(hour, minu))
                                {
                                    this.LabTurn.Text = "转仓时间：" + hour.PadLeft(2, '0') + ":" + minu.PadLeft(2, '0');
                                    this.Txtzhuancang.Text = hour.PadLeft(2, '0') + ":" + minu.PadLeft(2, '0');
                                    this.TurnTime.Text = hour.PadLeft(2, '0') + ":" + minu.PadLeft(2, '0');
                                }
                                else
                                {
                                    this.LabTurn.Text = "转仓时间：00:00";
                                    this.Txtzhuancang.Text = "00:00";
                                    this.TurnTime.Text = "00:00";
                                }

                                TxtJiare.Text = Tools.SixteenToTen(content.Substring(8, 2));//加热时间
                                TxtHeatingTime.Text = TxtJiare.Text;

                                Txtluochong.Text = Tools.SixteenToTen(content.Substring(10, 2));//落虫时间
                                TxtBugTime.Text = Txtluochong.Text;


                                TxtJinchong.Text = Tools.SixteenToTen(content.Substring(12, 2));//进虫等待
                                TxtEnteringWorm.Text = TxtJinchong.Text;

                                TxtZhendongTime.Text = Tools.SixteenToTen(content.Substring(14, 2));//震动时间
                                TxtShockTime.Text = TxtZhendongTime.Text;

                                TxtZhendongqiangdu.Text = Tools.SixteenToTen(content.Substring(16, 4));//震动强度
                                TxtShock.Text = TxtZhendongqiangdu.Text;

                                //待修改
                                string workTimeSlot1 = content.Substring(20, 8);
                                string startTime1 = workTimeSlot1.Substring(0, 4);
                                string startHour1 = Tools.SixteenToTen(startTime1.Substring(0, 2));
                                string startMin1 = Tools.SixteenToTen(startTime1.Substring(2, 2));
                                if (JudgeTime(startHour1, startMin1))
                                {
                                    startTime1 = startHour1.PadLeft(2, '0') + ":" + startMin1.PadLeft(2, '0');
                                }
                                else
                                {
                                    startTime1 = "00:00";
                                    startHour1 = "00";
                                    startMin1 = "00";
                                }
                                string endTime1 = workTimeSlot1.Substring(4, 4);
                                string endHour1 = Tools.SixteenToTen(endTime1.Substring(0, 2));
                                string endMin1 = Tools.SixteenToTen(endTime1.Substring(2, 2));
                                if (JudgeTime(endHour1, endMin1))
                                {
                                    endTime1 = endHour1.PadLeft(2, '0') + ":" + endMin1.PadLeft(2, '0');
                                }
                                else
                                {
                                    endTime1 = "00:00";
                                    endHour1 = "00";
                                    endMin1 = "00";
                                }
                                this.TimeStartHour1.Value = int.Parse(startHour1);
                                this.TimeStartMin1.Value = int.Parse(startMin1);
                                this.TimeEndHour1.Value = int.Parse(endHour1);
                                this.TimeEndMin1.Value = int.Parse(endMin1);
                                workTimeSlot1 = startTime1 + "-" + endTime1;

                                string workTimeSlot2 = content.Substring(28, 8);
                                string startTime2 = workTimeSlot2.Substring(0, 4);
                                string startHout2 = Tools.SixteenToTen(startTime2.Substring(0, 2));
                                string startMin2 = Tools.SixteenToTen(startTime2.Substring(2, 2));
                                if (JudgeTime(startHout2, startMin2))
                                {
                                    startTime2 = startHout2.PadLeft(2, '0') + ":" + startMin2.PadLeft(2, '0');
                                }
                                else
                                {
                                    startTime2 = "00:00";
                                    startHout2 = "00";
                                    startMin2 = "00";
                                }
                                string endTime2 = workTimeSlot2.Substring(4, 4);
                                string endHour2 = Tools.SixteenToTen(endTime2.Substring(0, 2));
                                string endMin2 = Tools.SixteenToTen(endTime2.Substring(2, 2));
                                if (JudgeTime(endHour2, endMin2))
                                {
                                    endTime2 = endHour2.PadLeft(2, '0') + ":" + endMin2.PadLeft(2, '0');
                                }
                                else
                                {
                                    endTime2 = "00:00";
                                    endHour2 = "00";
                                    endMin2 = "00";
                                }
                                this.TimeStartHour2.Value = int.Parse(startHout2);
                                this.TimeStartMin2.Value = int.Parse(startMin2);
                                this.TimeEndHour2.Value = int.Parse(endHour2);
                                this.TimeEndMin2.Value = int.Parse(endMin2);
                                workTimeSlot2 = startTime2 + "-" + endTime2;
                                string workTimeSlot3 = content.Substring(36, 8);
                                string startTime3 = workTimeSlot3.Substring(0, 4);
                                string startHour3 = Tools.SixteenToTen(startTime3.Substring(0, 2));
                                string startMin3 = Tools.SixteenToTen(startTime3.Substring(2, 2));
                                if (JudgeTime(startHour3, startMin3))
                                {
                                    startTime3 = startHour3.PadLeft(2, '0') + ":" + startMin3.PadLeft(2, '0');
                                }
                                else
                                {
                                    startTime3 = "00:00";
                                    startHour3 = "00";
                                    startMin3 = "00";
                                }
                                string endTime3 = workTimeSlot3.Substring(4, 4);
                                string endHour3 = Tools.SixteenToTen(endTime3.Substring(0, 2));
                                string endMin3 = Tools.SixteenToTen(endTime3.Substring(2, 2));
                                if (JudgeTime(endHour3, endMin3))
                                {
                                    endTime3 = endHour3.PadLeft(2, '0') + ":" + endMin3.PadLeft(2, '0');
                                }
                                else
                                {
                                    endTime3 = "00:00";
                                    endHour3 = "00";
                                    endMin3 = "00";
                                }
                                this.TimeStartHour3.Value = int.Parse(startHour3);
                                this.TimeStartMin3.Value = int.Parse(startMin3);
                                this.TimeEndHour3.Value = int.Parse(endHour3);
                                this.TimeEndMin3.Value = int.Parse(endMin3);
                                workTimeSlot3 = startTime3 + "-" + endTime3;
                                string workTimeSlot4 = content.Substring(44, 8);
                                string startTime4 = workTimeSlot4.Substring(0, 4);
                                string startHour4 = Tools.SixteenToTen(startTime4.Substring(0, 2));
                                string startMin4 = Tools.SixteenToTen(startTime4.Substring(2, 2));
                                if (JudgeTime(startHour4, startMin4))
                                {
                                    startTime4 = startHour4.PadLeft(2, '0') + ":" + startMin4.PadLeft(2, '0');
                                }
                                else
                                {
                                    startTime4 = "00:00";
                                    startHour4 = "00";
                                    startMin4 = "00";
                                }
                                string endTime4 = workTimeSlot4.Substring(4, 4);
                                string endHour4 = Tools.SixteenToTen(endTime4.Substring(0, 2));
                                string endMin4 = Tools.SixteenToTen(endTime4.Substring(2, 2));
                                if (JudgeTime(endHour4, endMin4))
                                {
                                    endTime4 = endHour4.PadLeft(2, '0') + ":" + endMin4.PadLeft(2, '0');
                                }
                                else
                                {
                                    endTime4 = "00:00";
                                    endHour4 = "00";
                                    endMin4 = "00";
                                }
                                this.TimeStartHour4.Value = int.Parse(startHour4);
                                this.TimeStartMin4.Value = int.Parse(startMin4);
                                this.TimeEndHour4.Value = int.Parse(endHour4);
                                this.TimeEndMin4.Value = int.Parse(endMin4);
                                workTimeSlot4 = startTime4 + "-" + endTime4;
                                string workTimeSlot5 = content.Substring(52, 8);
                                string startTime5 = workTimeSlot5.Substring(0, 4);
                                string startHour5 = Tools.SixteenToTen(startTime5.Substring(0, 2));
                                string startMin5 = Tools.SixteenToTen(startTime5.Substring(2, 2));
                                if (JudgeTime(startHour5, startMin5))
                                {
                                    startTime5 = startHour5.PadLeft(2, '0') + ":" + startMin5.PadLeft(2, '0');
                                }
                                else
                                {
                                    startTime5 = "00:00";
                                    startHour5 = "00";
                                    startMin5 = "00";
                                }
                                string endTime5 = workTimeSlot5.Substring(4, 4);
                                string endHour5 = Tools.SixteenToTen(endTime5.Substring(0, 2));
                                string endMin5 = Tools.SixteenToTen(endTime5.Substring(2, 2));
                                if (JudgeTime(endHour5, endMin5))
                                {
                                    endTime5 = endHour5.PadLeft(2, '0') + ":" + endMin5.PadLeft(2, '0');
                                }
                                else
                                {
                                    endTime5 = "00:00";
                                    endHour5 = "00";
                                    endMin5 = "00";
                                }
                                this.TimeStartHour5.Value = int.Parse(startHour5);
                                this.TimeStartMin5.Value = int.Parse(startMin5);
                                this.TimeEndHour5.Value = int.Parse(endHour5);
                                this.TimeEndMin5.Value = int.Parse(endMin5);
                                workTimeSlot5 = startTime5 + "-" + endTime5;
                                this.LabTimeLoat1.Text = workTimeSlot1;
                                this.LabTimeLoat2.Text = workTimeSlot2;
                                this.LabTimeLoat3.Text = workTimeSlot3;
                                this.LabTimeLoat4.Text = workTimeSlot4;
                                this.LabTimeLoat5.Text = workTimeSlot5;
                                break;
                            case "03"://运行指示状态
                                string runState = Tools.SixteenToTwo(content.Substring(2, 4)).PadLeft(11, '0');//运行状态  0关   1开
                                string youchongdeng = runState.Substring(runState.Length - 1, 1);//诱虫灯
                                if (youchongdeng == "0")
                                    this.Youchong.Value = false;
                                else if (youchongdeng == "1")
                                    this.Youchong.Value = true;
                                string paishui = runState.Substring(runState.Length - 2, 1);//排水
                                if (paishui == "0")
                                    this.Paishui.Value = false;
                                else if (paishui == "1")
                                    this.Paishui.Value = true;
                                string shangfanban = runState.Substring(runState.Length - 3, 1);//上翻板
                                if (shangfanban == "0")
                                    this.Shangluochong.Value = false;
                                else if (shangfanban == "1")
                                    this.Shangluochong.Value = true;
                                string xiafanban = runState.Substring(runState.Length - 4, 1);//下翻板
                                if (xiafanban == "0")
                                    this.Xialuochong.Value = false;
                                else if (xiafanban == "1")
                                    this.Xialuochong.Value = true;
                                string zhendong = runState.Substring(runState.Length - 5, 1);//震动
                                if (zhendong == "0")
                                    this.Zhendong.Value = false;
                                else if (zhendong == "1")
                                    this.Zhendong.Value = true;
                                string buguangdeng = runState.Substring(runState.Length - 6, 1);//补光灯
                                if (buguangdeng == "0")
                                    this.Buguangdeng.Value = false;
                                else if (buguangdeng == "1")
                                    this.Buguangdeng.Value = true;
                                string jiare = runState.Substring(runState.Length - 7, 1);//加热
                                if (jiare == "0")
                                    this.Jiareguan.Value = false;
                                else if (jiare == "1")
                                    this.Jiareguan.Value = true;
                                string lvdai = runState.Substring(runState.Length - 8, 1);//履带
                                if (lvdai == "0")
                                    this.Lvdai.Value = false;
                                else if (lvdai == "1")
                                    this.Lvdai.Value = true;
                                string currState = Tools.SixteenToTwo(content.Substring(6, 2)).PadLeft(7, '0');//当前状态
                                string waibucunchuqi = currState.Substring(6, 1);//外部存储器
                                string lvdaidianji = currState.Substring(5, 1);//履带电机
                                string zhuancangdianji = currState.Substring(4, 1);//转仓电机
                                string lvdaidianjipaizhao = currState.Substring(3, 1);//履带电机拍照
                                string yushui = currState.Substring(2, 1);//雨水
                                string paizhao = currState.Substring(1, 1);//是否能拍照
                                string paizhaopo = currState.Substring(0, 1);//拍照是否错误
                                if (waibucunchuqi == "0" && lvdaidianji == "0" && zhuancangdianji == "0" && lvdaidianjipaizhao == "0" && yushui == "0" && paizhao == "0" && paizhaopo == "0")
                                {
                                    this.LabCurrState.Text = "正常";
                                   // isPhoto = true;
                                }
                                else if (waibucunchuqi == "1")
                                {
                                    this.LabCurrState.Text = "外部存储器错误";
                                    //isPhoto = false;
                                }

                                else if (lvdaidianji == "1")
                                {
                                    this.LabCurrState.Text = "履带电机初始化错误";
                                   // isPhoto = true;
                                }
                                else if (zhuancangdianji == "1")
                                {
                                    this.LabCurrState.Text = "转仓电机初始化错误";
                                   // isPhoto = true;
                                }
                                else if (lvdaidianjipaizhao == "1")
                                {
                                    this.LabCurrState.Text = "履带电机拍照位置错误";
                                   // isPhoto = true;
                                }
                                else if (yushui == "1")
                                {
                                    this.LabCurrState.Text = "有雨水";
                                    //isPhoto = true;
                                }

                        
                                else if (paizhaopo == "1")
                                {
                                    this.LabCurrState.Text = "拍照错误";
                                    isPhoto = false ;
                                }
                                if (paizhao == "1")
                                {
                                    this.LabCurrState.Text = "可以拍照";
                                    if (isPhoto)
                                    {
                                        isPhoto = false;
                                        Thread thread = new Thread(Photograph);
                                        thread.IsBackground = true;
                                        thread.Start();
                                    }
                                }
                                DevOverviewMain.cQ12RunStateUpdata(runState, this.LabCurrState.Text.Trim());
                                if (this.LabCurrState.Text.Trim().Contains("错误"))
                                {
                                    //30分钟之后
                                    if (startEXETime.AddMinutes(30) < DateTime.Now)
                                    {
                                        if (MCURestartCount<3)
                                        {
                                            DebOutPut.DebLog("CQ12超过30分钟读取状态为错误信息");
                                            DebOutPut.DebLog("CQ12单片机重启复位:第 "+ (MCURestartCount+1)+" 次");
                                            string msg7 = "AB 5A 03 60 00 5A AF";
                                            SerialPortCtrl.DataSend_CQ12(serialPort, msg7);
                                            Thread.Sleep(1000);
                                            startEXETime = DateTime.Now;
                                        }
                                    }
                                }
                                else//时间重置
                                {
                                    startEXETime = DateTime.Now;
                                    MCURestartCount = 0;
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case "00"://手动转仓
                        DebOutPut.DebLog("转仓成功");
                        break;
                    case "01"://转仓时间
                        DebOutPut.DebLog("设置转仓时间成功");
                        break;
                    case "52":
                        DebOutPut.DebLog("更换工作模式成功");
                        break;
                    case "55":
                        DebOutPut.DebLog("更改进虫等待时间完成");
                        break;
                    case "10":
                        DebOutPut.DebLog("更改加热时间完成");
                        break;
                    case "53":
                        DebOutPut.DebLog("更改震动强度完成");
                        break;
                    case "54":
                        DebOutPut.DebLog("更改震动时间完成");
                        break;
                    case "11":
                        DebOutPut.DebLog("更改落虫时间完成");
                        break;
                    case "21":
                        DebOutPut.DebLog("更改工作时间段完成");
                        break;
                    case "30":
                        DebOutPut.DebLog("获取害虫数目完成");
                        bugNum = Tools.SixteenToTen(content);
                        if (bugNum == null)
                            bugNum = "0";
                        TxtCount.Text = bugNum;
                        break;
                    case "18":
                        DebOutPut.DebLog("打开/关闭拍照");
                        break;
                    case "56":
                        DebOutPut.DebLog("更改清虫/接虫成功");
                        break;
                    case "31":
                        DebOutPut.DebLog("清空计数");
                        break;
                    case "14":
                        DebOutPut.DebLog("控制测报灯开关");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        //bool isManual = false;
        /// <summary>
        /// 拍照
        /// </summary>
        private void Photograph()
        {
            try
            {
                this.BtnPhotograph.Text = "拍照中...";
                //获取害虫数目
                DebOutPut.DebLog("获取害虫数目");
                string msg2 = "AB 5A 03 30 00 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                Thread.Sleep(10000);
                //内存检测
                DetectMemory();
                Thread.Sleep(10000);
                StartOldNewCamera();
                if (m_pMyCamera != null)
                    PhotographUpLoad();//拍照并上传数据
                bugNum = "0";
                Thread.Sleep(10000);
                DebOutPut.DebLog("关闭拍照");
                string msg1 = "AB 5A 04 18 00 01 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg1);
                isPhoto = true;
                this.BtnPhotograph.Text = "拍照";
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }



        /// <summary>
        /// 清空计数
        /// </summary>
        private void BtnClear_Click(object sender, EventArgs e)
        {
            try
            {
                Thread.Sleep(500);
                DebOutPut.DebLog("清空计数");
                string msg8 = "AB 5A 03 31 00 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg8);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 刷新状态展示
        /// </summary>
        private void UpdataStateShow_Tick(object sender, EventArgs e)
        {
            try
            {
                Thread thread = new Thread(SerialPortDataSend);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 串口通信 获取状态
        /// </summary>
        private void SerialPortDataSend()
        {
            try
            {
                //获取电压电流状态
                Thread.Sleep(500);
                string msg1 = "AB 5A 04 20 00 01 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg1);
                Thread.Sleep(500);
                //获取系统配置
                string msg2 = "AB 5A 04 20 00 02 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                Thread.Sleep(500);
                //获取运行指示状态
                string msg3 = "AB 5A 04 20 00 03 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg3);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        ///  串口通信 转仓键
        /// </summary>
        private void RunWare_Click(object sender, EventArgs e)
        {
            try
            {
                //手动转仓
                Thread.Sleep(500);
                DebOutPut.DebLog("手动转仓");
                string msg3 = "AB 5A 03 00 00 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg3);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 拍照
        /// </summary>
        private void BtnPhotograph_Click(object sender, EventArgs e)
        {
            this.BtnPhotograph.Text = "拍照准备";
            DebOutPut.DebLog("打开拍照");
            string msg = "AB 5A 04 18 00 02 5A AF";
            SerialPortCtrl.DataSend_CQ12(serialPort, msg);
        }

        #region 内存检测
        /// <summary>
        /// 检测内存
        /// </summary>
        private void DetectMemory()
        {
            try
            {
                float remainingDiskSpace = GetHardDiskSpace(PubField.basePath.Substring(0, PubField.basePath.IndexOf(':')));
                DebOutPut.DebLog("程序运行磁盘剩余空间：" + remainingDiskSpace + " GB");
                DebOutPut.WriteLog(LogType.Normal, "程序运行磁盘剩余空间：" + remainingDiskSpace + " GB");
                if (remainingDiskSpace < 1f)
                {
                    string sql = "Delete * FROM Record";
                    int ret = DB_CQ12.updateDatabase(sql);
                    if (ret == -1)
                    {
                        DebOutPut.DebLog("程序运行磁盘剩余空间不足，清空数据失败！");
                        DebOutPut.WriteLog(LogType.Normal, "程序运行磁盘剩余空间不足，清空数据失败！");
                    }
                    else
                    {
                        string path = PubField.basePath + "\\CQ12Config\\CQ12GrabImg";
                        DeleteFolder(path);
                        float remainingDiskSpace_ = GetHardDiskSpace(PubField.basePath.Substring(0, PubField.basePath.IndexOf(':')));
                        DebOutPut.DebLog("程序运行磁盘剩余空间不足，清空数据成功！目前剩余存储空间：" + remainingDiskSpace_.ToString("F4") + " GB");
                        DebOutPut.WriteLog(LogType.Normal, "程序运行磁盘剩余空间不足，清空数据成功！目前剩余存储空间：" + remainingDiskSpace_.ToString("F4") + " GB");
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 获取程序运行所在磁盘，可使用存储空间，单位GB
        /// </summary>
        /// <param name="str_HardDiskName"></param>
        /// <returns></returns>
        public float GetHardDiskSpace(string str_HardDiskName)
        {
            try
            {
                float totalSize = 0;
                str_HardDiskName = str_HardDiskName + ":\\";
                DriveInfo[] drives = DriveInfo.GetDrives();
                foreach (DriveInfo drive in drives)
                    if (drive.Name == str_HardDiskName)
                        totalSize = (float)drive.TotalFreeSpace / 1073741824;
                return totalSize;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// 删除文件夹
        /// </summary>
        /// <param name="dir"></param>
        public void DeleteFolder(string dir)
        {
            try
            {
                foreach (string d in Directory.GetFileSystemEntries(dir))
                {
                    if (File.Exists(d))
                    {
                        FileInfo fi = new FileInfo(d);
                        if (fi.Attributes.ToString().IndexOf("ReadOnly") != -1)
                            fi.Attributes = FileAttributes.Normal;
                        File.Delete(d);//直接删除其中的文件   
                    }
                    else
                        DeleteFolder(d);//递归删除子文件夹   
                }
                Directory.Delete(dir);//删除已空文件夹   
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        #endregion 内存检测

        #region 强制工作

        /// <summary>
        /// 强制工作
        /// </summary>
        private void ForceWork_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                //更改工作模式
                DebOutPut.DebLog("更改工作模式");
                if (this.ForceWork.Value == true)
                {
                    Thread.Sleep(500);
                    string msg1 = "AB 5A 04 52 00 02 5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg1);
                    Thread.Sleep(500);
                    SendWorkModeModel sendWorkModeModel = new SendWorkModeModel();
                    sendWorkModeModel.func = 143;
                    sendWorkModeModel.err = "";
                    sendWorkModeModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                    sendWorkModeModel.message = "force";
                    string data = Tools.ObjectToJson(sendWorkModeModel);
                    sendDataList.Add(data);
                }
                else if (this.ForceWork.Value == false)//回归到正常工作模式
                {
                    //正常工作
                    Thread.Sleep(500);
                    string msg1 = "AB 5A 04 52 00 01 5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg1);

                    Thread.Sleep(500);
                    SendWorkModeModel sendWorkModeModel = new SendWorkModeModel();
                    sendWorkModeModel.func = 143;
                    sendWorkModeModel.err = "";
                    sendWorkModeModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                    sendWorkModeModel.message = "normal";
                    string data = Tools.ObjectToJson(sendWorkModeModel);
                    sendDataList.Add(data);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 正常工作和调试工作

        /// <summary>
        /// 更改工作模式
        /// </summary>
        private void WorkMode_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                //更改工作模式
                DebOutPut.DebLog("更改工作模式");
                if (WorkMode.Value == false)
                {
                    //正常工作
                    Thread.Sleep(500);
                    string msg1 = "AB 5A 04 52 00 01 5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg1);

                    Thread.Sleep(500);
                    SendWorkModeModel sendWorkModeModel = new SendWorkModeModel();
                    sendWorkModeModel.func = 143;
                    sendWorkModeModel.err = "";
                    sendWorkModeModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                    sendWorkModeModel.message = "normal";
                    string data = Tools.ObjectToJson(sendWorkModeModel);
                    sendDataList.Add(data);

                }
                else if (WorkMode.Value == true)
                {
                    //调试
                    Thread.Sleep(500);
                    string msg2 = "AB 5A 04 52 00 04 5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                    Thread.Sleep(500);
                    SendWorkModeModel sendWorkModeModel = new SendWorkModeModel();
                    sendWorkModeModel.func = 143;
                    sendWorkModeModel.err = "";
                    sendWorkModeModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                    sendWorkModeModel.message = "debug";
                    string data = Tools.ObjectToJson(sendWorkModeModel);
                    sendDataList.Add(data);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        #endregion

        #region 时间段工作

        /// <summary>
        /// 时间段工作
        /// </summary>
        private void TimeSlotWork_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                //更改工作模式
                DebOutPut.DebLog("更改工作模式");
                if (this.TimeSlotWork.Value == true)
                {
                    //时间段工作模式
                    Thread.Sleep(500);
                    string msg2 = "AB 5A 04 52 00 03 5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                    Thread.Sleep(500);
                    SendWorkModeModel sendWorkModeModel = new SendWorkModeModel();
                    sendWorkModeModel.func = 143;
                    sendWorkModeModel.err = "";
                    sendWorkModeModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                    sendWorkModeModel.message = "worktimes";
                    string data = Tools.ObjectToJson(sendWorkModeModel);
                    sendDataList.Add(data);
                }
                else if (this.TimeSlotWork.Value == false)//回归到正常工作模式
                {
                    //正常工作
                    Thread.Sleep(500);
                    string msg1 = "AB 5A 04 52 00 01 5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg1);

                    Thread.Sleep(500);
                    SendWorkModeModel sendWorkModeModel = new SendWorkModeModel();
                    sendWorkModeModel.func = 143;
                    sendWorkModeModel.err = "";
                    sendWorkModeModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                    sendWorkModeModel.message = "normal";
                    string data = Tools.ObjectToJson(sendWorkModeModel);
                    sendDataList.Add(data);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        #endregion

        #region 上翻板控制
        bool isClick = false;
        private void Shangluochong_MouseMove(object sender, MouseEventArgs e)
        {
            isClick = true;
        }
        /// <summary>
        /// 上翻板控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Shangluochong_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Shangluochong.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "000000100";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }
                else if (this.Shangluochong.Value == false)//关闭
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 下翻板控制

        /// <summary>
        /// 下翻版控制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Xialuochong_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Xialuochong.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "000001000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }
                else if (this.Xialuochong.Value == false)//关闭
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        #endregion

        #region 补光灯控制
        /// <summary>
        /// 补光灯
        /// </summary>
        private void Buguangdeng_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Buguangdeng.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "000100000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }
                else if (this.Buguangdeng.Value == false)//关闭
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 履带控制
        private void Lvdai_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Lvdai.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "010000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }
                else if (this.Lvdai.Value == false)//关闭
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        #endregion

        #region 排水控制
        /// <summary>
        /// 排水
        /// </summary>
        private void Paishui_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Paishui.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "000000010";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }
                else if (this.Paishui.Value == false)//关闭
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        #endregion

        #region 震动控制

        /// <summary>
        /// 震动
        /// </summary>
        private void Zhendong_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Zhendong.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "000010000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);

                }
                else if (this.Zhendong.Value == false)
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 加热管控制
        /// <summary>
        /// 加热管
        /// </summary>
        private void Jiareguan_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Jiareguan.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "001000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }
                else if (this.Jiareguan.Value == false)//关闭
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 诱虫灯控制
        /// <summary>
        /// 诱虫灯
        /// </summary>
        private void Youchong_ValueChanged(object sender, EventArgs e)
        {
            if (!isClick)
                return;
            isClick = false;
            try
            {
                if (this.Youchong.Value == true)//打开
                {
                    Thread.Sleep(500);
                    string content = "000000001";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }
                else if (this.Youchong.Value == false)//关闭
                {
                    Thread.Sleep(500);
                    string content = "000000000";
                    content = Tools.TwoToSixteen(content).PadLeft(4, '0');
                    string msg2 = "AB 5A 05 12 00 " + content + "5A AF";
                    SerialPortCtrl.DataSend_CQ12(serialPort, msg2);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 报警开关
        /// <summary>
        /// 报警开关
        /// </summary>
        private void Btnwitch_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                if (this.Btnwitch.Value == false)
                {
                    Tools.ConfigParmSet("Basic Parameters", "AlarmSwitch", "NO", cq12ConfigSavePath);
                }
                else
                {
                    Tools.ConfigParmSet("Basic Parameters", "AlarmSwitch", "OK", cq12ConfigSavePath);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 进虫等待
        /// <summary>
        /// 进虫等待
        /// </summary>
        private void BtnEnteringWormDefine_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("进虫等待");
                Thread.Sleep(500);
                string time = Tools.TenToSixteen(this.TxtEnteringWorm.Text.Trim()).PadLeft(2, '0');
                string msg = "AB 5A 04 55 00 " + time.ToUpper() + " 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 加热时间控制
        /// <summary>
        /// 加热时间
        /// </summary>
        private void BtnHeatingTimeDefine_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("加热时间");
                Thread.Sleep(500);
                string time = Tools.TenToSixteen(this.TxtHeatingTime.Text.Trim()).PadLeft(2, '0');
                string msg = "AB 5A 04 10 00 " + time.ToUpper() + " 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 震动强度
        /// <summary>
        /// 震动强度
        /// </summary>
        private void BtnShockDefine_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("震动强度");
                Thread.Sleep(500);
                string strength = Tools.TenToSixteen(this.TxtShock.Text.Trim()).PadLeft(4, '0');
                string msg = "AB 5A 05 53 00 " + strength.ToUpper() + " 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 震动时间
        /// <summary>
        /// 震动时间
        /// </summary>
        private void BtnShockTimeDefine_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("震动时间");
                Thread.Sleep(500);
                string time = Tools.TenToSixteen(this.TxtShockTime.Text.Trim()).PadLeft(2, '0');
                string msg = "AB 5A 04 54 00 " + time.ToUpper() + " 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 落虫时间
        /// <summary>
        /// 落虫时间
        /// </summary>
        private void BtnBugTimeDefine_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("落虫时间");
                Thread.Sleep(500);
                string time = Tools.TenToSixteen(this.TxtBugTime.Text.Trim()).PadLeft(2, '0');
                string msg = "AB 5A 04 11 00 " + time.ToUpper() + " 5A AF";
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 转仓时间控制
        /// <summary>
        /// 转仓时间
        /// </summary>
        private void BtnTurnTimeDefine_Click(object sender, EventArgs e)
        {
            try
            {
                //转仓时间
                DebOutPut.DebLog("转仓时间");
                Thread.Sleep(500);
                string time = this.TurnTime.Text.Trim().Replace("：", ":");
                string hour = Tools.TenToSixteen(time.Substring(0, time.LastIndexOf(":"))).PadLeft(2, '0');
                string min = Tools.TenToSixteen(time.Substring(time.LastIndexOf(":") + 1, time.Length - time.LastIndexOf(":") - 1)).PadLeft(2, '0');
                string frameHead = "AB5A";
                string func = "01";
                string ad = "00";
                string content = hour + min;
                string frameTail = "5AAF";
                string length = Tools.TenToSixteen((((func + ad + content).Length + 2) / 2).ToString()).PadLeft(2, '0');
                string msg = frameHead + length + func + ad + content + frameTail;
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        #endregion

        #region 设置工作时间段
        /// <summary>
        /// 设置工作时间段1
        /// </summary>
        private void BtnTimeSlotDefine1_Click(object sender, EventArgs e)
        {
            try
            {
                //待修改
                DebOutPut.DebLog("设置工作时间段1");
                Thread.Sleep(500);
                string f1 = Tools.TenToSixteen("1").PadLeft(2, '0');
                string dt1Front = this.TimeStartHour1.Value.ToString() + ":" + this.TimeStartMin1.Value.ToString();
                string dt1FrontBack = this.TimeEndHour1.Value.ToString() + ":" + this.TimeEndMin1.Value.ToString();
                string msg = SetTimeSlot(dt1Front, dt1FrontBack, f1);
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 设置工作时间段2
        /// </summary>
        private void BtnTimeSlotDefine2_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("设置工作时间段2");
                Thread.Sleep(500);
                string f1 = Tools.TenToSixteen("2").PadLeft(2, '0');
                string dt2Front = this.TimeStartHour2.Value.ToString() + ":" + this.TimeStartMin2.Value.ToString();
                string dt2FrontBack = this.TimeEndHour2.Value.ToString() + ":" + this.TimeEndMin2.Value.ToString();
                string msg = SetTimeSlot(dt2Front, dt2FrontBack, f1);
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 设置工作时间段3
        /// </summary>
        private void BtnTimeSlotDefine3_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("设置工作时间段3");
                Thread.Sleep(500);
                string f1 = Tools.TenToSixteen("3").PadLeft(2, '0');
                string dt3Front = this.TimeStartHour3.Value.ToString() + ":" + this.TimeStartMin3.Value.ToString();
                string dt3FrontBack = this.TimeEndHour3.Value.ToString() + ":" + this.TimeEndMin3.Value.ToString();
                string msg = SetTimeSlot(dt3Front, dt3FrontBack, f1);
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 设置工作时间段4
        /// </summary>
        private void BtnTimeSlotDefine4_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("设置工作时间段4");
                Thread.Sleep(500);
                string f1 = Tools.TenToSixteen("4").PadLeft(2, '0');
                string dt4Front = this.TimeStartHour4.Value.ToString() + ":" + this.TimeStartMin4.Value.ToString();
                string dt4FrontBack = this.TimeEndHour4.Value.ToString() + ":" + this.TimeEndMin4.Value.ToString();
                string msg = SetTimeSlot(dt4Front, dt4FrontBack, f1);
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 设置工作时间段5
        /// </summary>
        private void BtnTimeSlotDefine5_Click(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("设置工作时间段5");
                Thread.Sleep(500);
                string f1 = Tools.TenToSixteen("5").PadLeft(2, '0');
                string dt5Front = this.TimeStartHour5.Value.ToString() + ":" + this.TimeStartMin5.Value.ToString();
                string dt5FrontBack = this.TimeEndHour5.Value.ToString() + ":" + this.TimeEndMin5.Value.ToString();
                string msg = SetTimeSlot(dt5Front, dt5FrontBack, f1);
                SerialPortCtrl.DataSend_CQ12(serialPort, msg);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 设置工作时间段
        /// </summary>
        /// <param name="startTime">开始时间</param>
        /// <param name="endTime">结束时间</param>
        /// <param name="f1">第几个时间</param>
        private string SetTimeSlot(string startTime, string endTime, string f1)
        {
            try
            {
                string startHour = Tools.TenToSixteen(startTime.Substring(0, startTime.LastIndexOf(":"))).PadLeft(2, '0');
                string startMin = Tools.TenToSixteen(startTime.Substring(startTime.LastIndexOf(":") + 1)).PadLeft(2, '0');
                string endHour = Tools.TenToSixteen(endTime.Substring(0, endTime.LastIndexOf(":"))).PadLeft(2, '0');
                string endMin = Tools.TenToSixteen(endTime.Substring(endTime.LastIndexOf(":") + 1)).PadLeft(2, '0');
                string timeSlot = startHour + startMin + endHour + endMin;
                string msg = "AB 5A 08 21 00 " + f1 + timeSlot + " 5A AF";
                return msg;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }
        #endregion



        /// <summary>
        /// 拍照并上传数据
        /// </summary>
        private void PhotographUpLoad()
        {
            try
            {
                //开始拍照
                DebOutPut.DebLog("CQ12拍照");
                Bitmap img = (Bitmap)GetPic();
                if (img == null)
                {
                    DebOutPut.DebLog("CQ12相机不存在");
                    return;
                }
                //打水印
                string collectTime = "";
                using (Graphics g = Graphics.FromImage(img))
                {
                    collectTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
                    g.DrawString(collectTime, new Font("宋体", 100), System.Drawing.Brushes.Yellow, new PointF(2500, 100));
                    g.DrawString(SaveDataModel.cQ12ConfigParamModel.deviceID, new Font("宋体", 100), System.Drawing.Brushes.Yellow, new PointF(100, 100));
                }
                //保存在本地
                Tools.SaveImage(img, DateTime.Parse(collectTime).ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".bmp");
                //TODO
                String sql = "insert into Record (Flag,CollectTime,BugNum) values ('0','" + collectTime + "','" + bugNum + "')";
                if (DB_CQ12.updateDatabase(sql) != 1)
                    DebOutPut.DebLog("图像采集时间为：" + collectTime + "  插入数据库失败");
                else
                    DebOutPut.DebLog("图像采集时间为：" + collectTime + "  插入数据库成功");

                Thread thread = new Thread(SendData);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private static readonly object SendDataLock = new object();
        private void SendData()
        {
            lock (SendDataLock)
            {
                try
                {
                    string sql = "select * from Record where Flag='0'";
                    DataTable UploadTable = DB_CQ12.QueryDatabase(sql).Tables[0];
                    string path = "";
                    DebOutPut.DebLog("未传共  " + UploadTable.Rows.Count + "  个");
                    for (int i = 0; i < UploadTable.Rows.Count; i++)
                    {
                        path = BZ10.Param.BasePath + "\\CQ12Config\\CQ12GrabImg\\" + DateTime.Parse(UploadTable.Rows[i]["CollectTime"].ToString()).ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".bmp";
                        if (!File.Exists(path))
                        {
                            DebOutPut.DebLog("文件不存在：" + path);
                            continue;
                        }
                        //上传数据
                        collectInfoModel = new CollectInfoModel();
                        collectInfoModel.func = 101;//采集信息
                        collectInfoModel.err = "";//错误信息
                        collectInfoModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;//设备Id
                        collectInfoModel.devtype = "0";
                        CollectInfoMessage collectInfoMessage = new CollectInfoMessage();
                        collectInfoMessage.collectTime = UploadTable.Rows[i]["CollectTime"].ToString();  //采集时间
                        collectInfoMessage.bugNum = int.Parse(UploadTable.Rows[i]["BugNum"].ToString());//害虫数量
                        collectInfoMessage.picStr = getBase64FromPic(path);//图片
                        List<EnvironmentsItem> environmentsItems = new List<EnvironmentsItem>();
                        collectInfoMessage.environments = environmentsItems;
                        collectInfoModel.message = collectInfoMessage;
                        string data = Tools.ObjectToJson(collectInfoModel);
                        sendDataList.Add(data);
                        //发送位置信息
                        RequestCtrl.SearchIPAddress();
                        GPSInfoModel gPSInfoModel = new GPSInfoModel();
                        gPSInfoModel.func = 102;
                        gPSInfoModel.err = "";
                        gPSInfoModel.devId = SaveDataModel.cQ12ConfigParamModel.deviceID;
                        GPSInfoMessage gPSInfoMessage = new GPSInfoMessage();
                        gPSInfoMessage.lat = double.Parse((SaveDataModel.iPAddressRoot.content.point.y == "") ? "0" : SaveDataModel.iPAddressRoot.content.point.y);//纬度
                        gPSInfoMessage.lon = double.Parse((SaveDataModel.iPAddressRoot.content.point.x == "") ? "0" : SaveDataModel.iPAddressRoot.content.point.x);//经度
                        gPSInfoModel.message = gPSInfoMessage;
                        string data1 = Tools.ObjectToJson(gPSInfoModel);
                        sendDataList.Add(data1);
                    }
                    UploadTable.Dispose();
                }
                catch (Exception ex)
                {
                    DebOutPut.DebErr(ex.ToString());
                    DebOutPut.WriteLog(LogType.Error, ex.ToString());
                }
            }
        }

        public string getBase64FromPic(string picPath)
        {
            try
            {
                FileStream fs = new FileStream(picPath, FileMode.Open, FileAccess.Read, FileShare.Read);
                long size = fs.Length;
                byte[] array = new byte[size];
                fs.Read(array, 0, array.Length);
                fs.Close();
                Base64Encoder en = new Base64Encoder();
                return en.GetEncoded(array);
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return "";
            }


        }


        /// <summary>
        /// 判断时间是否合法
        /// </summary>
        /// <param name="hour">时</param>
        /// <param name="min">分</param>
        /// <returns></returns>
        private bool JudgeTime(string hour, string min)
        {
            try
            {
                if (int.Parse(hour) < 0 || int.Parse(hour) > 24)
                {
                    return false;
                }
                if (int.Parse(min) < 0 || int.Parse(min) > 60)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return false;
            }

        }

        /// <summary>
        /// 开关是否能点击
        /// </summary>
        /// <param name="isCan">是否能点击</param>
        /// <returns></returns>
        private void isCanClick(bool isCan)
        {
            try
            {
                foreach (Control item in this.groupBox3.Controls)
                {
                    if (item is Panel)
                    {
                        Panel panel = (Panel)item;
                        foreach (var item1 in panel.Controls)
                        {
                            if (item1 is SwitchButton)
                            {
                                SwitchButton switchButton = (SwitchButton)item1;
                                if (switchButton.Name != "WorkMode")
                                {
                                    //switchButton.Enabled = isCan;
                                    Tools.SetControlEnabled(switchButton, isCan);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void CQ12MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (clientSocket != null && clientSocket.Connected)
                {
                    isConnect = false;
                    clientSocket.Close();
                    DebOutPut.DebLog("CQ12连接已读断开!");
                }
                CloseCamera();
                if (isOpen)
                {
                    if (SerialPortCtrl.CloseSerialPort(serialPort))
                    {
                        isOpen = false;
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 刷新数据链表展示
        /// </summary>
        private void UpdataDataList()
        {
            try
            {
                //刷新展示
                Thread thread = new Thread(RefreshFLP);
                thread.IsBackground = true;
                thread.Start();
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        private int currentPage = 1;//当前页
        private int pageCount = 1;//总页数
        private const int perPageCount = 2;//每页显示数量
                                           //历史数据集
        DataSet RecordDt = new DataSet();
        private ObservableCollection<ImageItem> collectionDispData = new ObservableCollection<ImageItem>();
        List<string> ImgNames = new List<string>();
        //图片路径
        List<string> imagePathLists = new List<string>();
        //采集时间
        List<string> picTime = new List<string>();
        //数量
        List<string> bugCount = new List<string>();
        /// <summary>
        /// 刷新数据展示currentPage
        /// </summary>
        private void RefreshFLP()
        {
            try
            {
                imageList1.Images.Clear();
                listView1.Items.Clear();
                picTime.Clear();
                bugCount.Clear();
                imagePathLists.Clear();
                string sql = "select * from Record order by CollectTime desc";
                DataTable dt = DB_CQ12.QueryDatabase(sql).Tables[0];
                //刷新总页数和当前页
                UpdataPage(dt);

                for (int i = currentPage * perPageCount - perPageCount; i < (((dt.Rows.Count - (currentPage * perPageCount - perPageCount)) > perPageCount) ? currentPage * perPageCount : dt.Rows.Count); i++)
                {

                    //图片采集时间
                    string time = dt.Rows[i]["CollectTime"].ToString();
                    //图片路径
                    string picPath = BZ10.Param.BasePath + "\\CQ12Config\\CQ12GrabImg\\" + Convert.ToDateTime(time).ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".bmp";
                    //图片名字
                    string picName = Convert.ToDateTime(time).ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo) + ".bmp";
                    //害虫数量
                    string bugNum = dt.Rows[i]["BugNum"].ToString();

                    Image image = Tools.FileToBitmap(picPath); //获取文件
                    imageList1.Images.Add(image);

                    picTime.Add(time);
                    //害虫数量
                    bugCount.Add(bugNum);
                    imagePathLists.Add(picPath);
                }
                listView1.View = System.Windows.Forms.View.LargeIcon;
                listView1.LargeImageList = imageList1;
                this.listView1.BeginUpdate();
                for (int i = 0; i < imageList1.Images.Count; i++)
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = imagePathLists.ElementAt(i);
                    lvi.ImageIndex = i;
                    lvi.Text = picTime.ElementAt(i) + "    数目:" + bugCount.ElementAt(i);
                    listView1.Items.Add(lvi);
                }
                this.listView1.EndUpdate();
                if (pageCount > currentPage)
                    buttonX1.Enabled = true;
                else
                    buttonX1.Enabled = false;
                if (currentPage <= 1)
                    prepage.Enabled = false;
                else
                    prepage.Enabled = true;
                if (currentPage == 1)
                    button10.Enabled = false;
                else
                    button10.Enabled = true;
                if (currentPage == pageCount)
                    button11.Enabled = false;
                else
                    button11.Enabled = true;
                if (this.BtnLookData.Enabled == false)
                    this.BtnLookData.Enabled = true;
                PageInfo.Text = currentPage.ToString() + "/" + pageCount.ToString();

            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private DataTable MakeNamesTable(string tablename)
        {
            try
            {
                DataTable namesTable = new DataTable();

                DataColumn idColumn = new DataColumn();
                idColumn.DataType = System.Type.GetType("System.Int32");
                idColumn.ColumnName = "ID";
                namesTable.Columns.Add(idColumn);

                DataColumn Column1 = new DataColumn();
                Column1.DataType = System.Type.GetType("System.String");
                Column1.ColumnName = "CollectTime";
                namesTable.Columns.Add(Column1);

                DataColumn Column2 = new DataColumn();
                Column2.DataType = System.Type.GetType("System.String");
                Column2.ColumnName = "path";
                namesTable.Columns.Add(Column2);

                return namesTable;
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                return null;
            }

        }

        /// <summary>
        /// 删除全部
        /// </summary>
        private void BtnDeleteAll_Click(object sender, EventArgs e)
        {

            try
            {
                if (listView1.SelectedItems.Count <= 0)
                {
                    MessageBox.Show("请选择要删除的数据项！");
                    return;
                }
                else
                {
                    string picTime = "";//创建时间
                    string bugCount = "";//害虫数目
                    if (MessageBox.Show("确定要删除选中的数据吗？", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                    {
                        string picName = (String)listView1.SelectedItems[0].Text;
                        picTime = picName.Substring(0, picName.LastIndexOf(" ")).Trim();
                        bugCount = picName.Substring(picName.LastIndexOf(":") + 1, picName.Length - picName.LastIndexOf(":") - 1);
                        picName = DateTime.Parse(picTime).ToString("yyyyMMddHHmmss") + "_" + ((bugCount == null) ? "0" : bugCount) + ".bmp";

                        string picPath = PubField.basePath + "\\CQ12Config\\CQ12GrabImg\\" + picName;
                        if (File.Exists(picPath))
                        {
                            File.Delete(picPath);
                            MessageBox.Show(this, "删除数据成功！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            UpdataDataList();
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        /// <summary>
        /// 刷新当前页和总页数
        /// </summary>
        /// <param name="files">文件数组</param>
        private void UpdataPage(DataTable files)
        {
            try
            {
                int recordnum = files.Rows.Count;
                if ((recordnum % perPageCount) == 0)
                {
                    pageCount = recordnum / perPageCount;
                }
                else
                {
                    pageCount = recordnum / perPageCount + 1;
                }
                this.PageInfo.Text = ((currentPage == 0) ? 1 : currentPage) + "/" + ((pageCount == 0) ? 0 : pageCount);
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 上一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void prepage_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPage > 1)
                {
                    prepage.Enabled = false;
                    currentPage -= 1;
                    UpdataDataList();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }


        }

        /// <summary>
        /// 下一页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void nextpage_Click(object sender, EventArgs e)
        {
            try
            {
                if (currentPage < pageCount)
                {
                    buttonX1.Enabled = false;
                    currentPage += 1;
                    UpdataDataList();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 尾页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button11_Click(object sender, EventArgs e)
        {
            try
            {
                if (pageCount > 1)
                {
                    button11.Enabled = false;
                    currentPage = pageCount;
                    UpdataDataList();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }

        /// <summary>
        /// 首页
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button10_Click(object sender, EventArgs e)
        {
            try
            {
                if (pageCount > 1)
                {
                    button10.Enabled = false;
                    currentPage = 1;
                    UpdataDataList();
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }


        #region 控件等比例缩放
        private float x;//定义当前窗体的宽度
        private float y;//定义当前窗体的高度
        private void SetTag(Control cons)
        {
            try
            {
                foreach (Control con in cons.Controls)
                {
                    con.Tag = con.Width + ";" + con.Height + ";" + con.Left + ";" + con.Top + ";" + con.Font.Size;
                    if (con.Controls.Count > 0)
                    {
                        SetTag(con);
                        Application.DoEvents();
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }

        }
        private void SetControls(float newx, float newy, Control cons)
        {
            try
            {


                //遍历窗体中的控件，重新设置控件的值
                foreach (Control con in cons.Controls)
                {
                    //获取控件的Tag属性值，并分割后存储字符串数组
                    if (con.Tag != null)
                    {
                        string[] mytag = con.Tag.ToString().Split(new char[] { ';' });
                        if (con.Name == "listView1")
                        {
                            con.Left = (con.Parent.Width - con.Width) / 2;
                        }
                        else
                        {
                            //根据窗体缩放的比例确定控件的值
                            con.Width = Convert.ToInt32(System.Convert.ToSingle(mytag[0]) * newx);//宽度
                            //con.Height = Convert.ToInt32(System.Convert.ToSingle(mytag[1]) * newy);//高度
                            con.Left = Convert.ToInt32(System.Convert.ToSingle(mytag[2]) * newx);//左边距
                            con.Top = Convert.ToInt32(System.Convert.ToSingle(mytag[3]) * newy);//顶边距
                            Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                            con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                        }
                        Application.DoEvents();
                        if (con.Controls.Count > 0)
                            SetControls(newx, newy, con);
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }
        bool isChangeSize = true;
        private void CQ12MainForm_Shown(object sender, EventArgs e)
        {
            isChangeSize = false;
        }

        private void CQ12MainForm_Resize(object sender, EventArgs e)
        {
            try
            {
                DebOutPut.DebLog("是否执行SizeChanged----------->" + isChangeSize);
                if (isChangeSize)
                {
                    float newx = (this.Width) / x;
                    float newy = (this.Height) / y;
                    SetControls(newx, newy, this);
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }


        #endregion

        /// <summary>
        /// 锁
        /// </summary>
        static readonly object ImageLookLock = new object();
        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                lock (ImageLookLock)
                {
                    ListViewHitTestInfo info = listView1.HitTest(e.X, e.Y);
                    if (info.Item != null)
                    {
                        String path = (String)listView1.SelectedItems[0].Tag;
                        BZ10.PicturePreview pic = new BZ10.PicturePreview();
                        pic.setImageSource(path);
                        pic.Show();
                    }
                }
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        /// <summary>
        /// 清空
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonX14_Click(object sender, EventArgs e)
        {
            bool isDeb = true;
            try
            {
                string str = "Delete * FROM Record";
                int ret = DB_CQ12.updateDatabase(str);
                if (ret == -1)
                {
                    DebOutPut.DebLog("清空数据失败！");
                    DebOutPut.WriteLog(LogType.Normal, "清空数据失败！");
                    isDeb = false;
                    MessageBox.Show("清空数据失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string imagePath = PubField.basePath + "\\CQ12Config\\CQ12GrabImg\\";
                DirectoryInfo file = new DirectoryInfo(imagePath);
                FileInfo[] fileInfos = file.GetFiles("*.bmp");
                DebOutPut.DebLog("图库中图像总数:" + fileInfos.Length);
                for (int i = 0; i < fileInfos.Length; i++)
                {
                    File.Delete(fileInfos[i].FullName);
                    DebOutPut.DebLog("删除图像:" + fileInfos[i].FullName);
                }
                DebOutPut.DebLog("所有图像已清空");
                UpdataDataList();
            }
            catch (Exception ex)
            {
                DebOutPut.DebLog("清空数据失败！");
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
                if (isDeb)
                {
                    MessageBox.Show("清空数据失败！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void buttonX2_Click(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text.Trim() == "")
                {
                    MessageBox.Show("请填入设备编号！", "提示");
                    return;
                }
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                if (SaveDataModel.cQ12ConfigParamModel.deviceID != textBox1.Text.Trim())
                {
                    Tools.ConfigParmSet("Basic Parameters", "DeviceID", textBox1.Text.Trim(), cq12ConfigSavePath);
                    DialogResult dialogResult = MessageBox.Show("检测到您更改了系统关键性配置，将在系统重启之后生效。点击“确定”将立即重启本程序，点击“取消”请稍后手动重启！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    if (dialogResult == DialogResult.OK)
                    {
                        Tools.RestStart();
                    }
                }
                textBox1.Enabled = false;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void buttonX3_Click(object sender, EventArgs e)
        {
            textBox1.Enabled = true;
        }

        private void buttonX4_Click(object sender, EventArgs e)
        {
            LabSerAddress.Enabled = true;
            LabSerPort.Enabled = true;
        }

        private void buttonX5_Click(object sender, EventArgs e)
        {
            try
            {
                if (LabSerAddress.Text.Trim() == "" || LabSerPort.Text.Trim() == "")
                {
                    MessageBox.Show("请填入服务器地址和服务器端口号！", "提示");
                    return;
                }
                string cq12ConfigSavePath = Path.Combine(PubField.basePath, "CQ12Config");
                cq12ConfigSavePath = Path.Combine(cq12ConfigSavePath, PubField.cQ12ConfigName);
                if (SaveDataModel.cQ12ConfigParamModel.uploadAddress != LabSerAddress.Text.Trim() || SaveDataModel.cQ12ConfigParamModel.uploadPort != LabSerPort.Text.Trim())
                {
                    Tools.ConfigParmSet("Basic Parameters", "UploadAddress", LabSerAddress.Text.Trim(), cq12ConfigSavePath);
                    Tools.ConfigParmSet("Basic Parameters", "UploadPort", LabSerPort.Text.Trim(), cq12ConfigSavePath);
                    DialogResult dialogResult = MessageBox.Show("检测到您更改了系统关键性配置，将在系统重启之后生效。点击“确定”将立即重启本程序，点击“取消”请稍后手动重启！", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
                    if (dialogResult == DialogResult.OK)
                    {
                        Tools.RestStart();
                    }
                }
                LabSerAddress.Enabled = false;
                LabSerPort.Enabled = false;
            }
            catch (Exception ex)
            {
                DebOutPut.DebErr(ex.ToString());
                DebOutPut.WriteLog(LogType.Error, ex.ToString());
            }
        }

        private void buttonX6_Click(object sender, EventArgs e)
        {
            Thread thread = new Thread(SendData);
            thread.IsBackground = true;
            thread.Start();
        }
    }

    /// <summary>
    /// 判断文件的创建时间
    /// </summary>
    public class FileComparer : IComparer
    {
        int IComparer.Compare(Object o1, Object o2)
        {
            FileInfo fi1 = o1 as FileInfo;
            FileInfo fi2 = o2 as FileInfo;
            return fi1.CreationTime.CompareTo(fi2.CreationTime);
        }
    }
}
