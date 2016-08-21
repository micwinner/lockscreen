namespace LockScreen
{
    using System;
    using System.Management;
    using System.Windows.Forms;
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        string _type = string.Empty;
        public Form1(string type) : this()
        {
            _type = type;
            switch(type)
            {
                case "1":
                    ezUSB.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 3));
                    break;
                case "2":
                    timer1.Enabled = true;
                    break;
                case "all":
                    timer1.Enabled = true;
                    ezUSB.AddUSBEventWatcher(USBEventHandler, USBEventHandler, new TimeSpan(0, 0, 3));
                    break;
                default:
                    MessageBox.Show("启动参数错误:(1:使用USB,2:使用微信退出,默认为使用所有方式)");
                    Environment.Exit(0);
                    break;
            }
        }
        private USB ezUSB = new USB();
        private void Form1_Load(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            notifyIcon1.ShowBalloonTip(3000, "提示", "监控已启动,启动方式:" + _type, ToolTipIcon.Info);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ezUSB.RemoveUSBEventWatcher();
            Environment.Exit(0);
        }
        private void USBEventHandler(Object sender, EventArrivedEventArgs e)
        {
            var what = USB.WhoUSBControllerDevice(e);
            if (e.NewEvent.ClassPath.ClassName == "__InstanceCreationEvent")
            {
                notifyIcon1.ShowBalloonTip(3000, "提示", "设备已插入", ToolTipIcon.Info);
            }
            else if (e.NewEvent.ClassPath.ClassName == "__InstanceDeletionEvent")
            {
                notifyIcon1.ShowBalloonTip(3000, "提示", "设备已拔出", ToolTipIcon.Info);
                Win32Helper.LockWorkStation();
            }
        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                ShowInTaskbar = false;
                notifyIcon1.Visible = true;
            }
        }
        private void 关闭监控ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(MessageBox.Show("确定退出监控吗?","提示",MessageBoxButtons.OKCancel,MessageBoxIcon.Question) == DialogResult.OK)
            {
                notifyIcon1.ShowBalloonTip(3000, "提示", "监控关闭", ToolTipIcon.Warning);
                Close();
            }
        }
        bool isFirst = false;
        private object lockObj = new object();
        private void timer1_Tick(object sender, EventArgs e)
        {
            var hwnd = Win32Helper.FindWindow("AlertDialog", "微信");
            if (hwnd != IntPtr.Zero)
            {
                lock (lockObj)
                {
                    isFirst = true;
                    timer1.Enabled = false;
                    timer2.Enabled = true;
                }
                Win32Helper.LockWorkStation();
            }
        }
        private void timer2_Tick(object sender, EventArgs e)
        {
            var hwnd = Win32Helper.FindWindow("AlertDialog", "微信");
            if(hwnd == IntPtr.Zero)
            {
                if (isFirst)
                {
                    lock (lockObj)
                    {
                        isFirst = false;
                        timer2.Enabled = false;
                        timer1.Enabled = true;
                    }
                }
            }
        }
    }
}
