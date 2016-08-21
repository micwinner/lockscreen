namespace LockScreen
{
    using System;
    using System.Management;
    using System.Runtime.InteropServices;
    public struct USBControllerDevice
    {
        /// <summary>  
        /// USB控制器设备ID  
        /// </summary>  
        public string Antecedent;
        /// <summary>  
        /// USB即插即用设备ID  
        /// </summary>  
        public string Dependent;
    }
    public partial class USB
    {
        private ManagementEventWatcher insertWatcher = null;//USB插入事件监视
        private ManagementEventWatcher removeWatcher = null;//USB拔出事件监视
        /// <summary>  
        /// 添加USB事件监视器  
        /// </summary>  
        /// <param name="usbInsertHandler">USB插入事件处理器</param>  
        /// <param name="usbRemoveHandler">USB拔出事件处理器</param>  
        /// <param name="withinInterval">发送通知允许的滞后时间</param>  
        public bool AddUSBEventWatcher(EventArrivedEventHandler usbInsertHandler, EventArrivedEventHandler usbRemoveHandler, TimeSpan withinInterval)
        {
            try
            {
                ManagementScope Scope = new ManagementScope("root\\CIMV2");
                Scope.Options.EnablePrivileges = true;
                if (usbInsertHandler != null)// USB插入监视
                {
                    WqlEventQuery InsertQuery = new WqlEventQuery("__InstanceCreationEvent",withinInterval,"TargetInstance isa 'Win32_USBControllerDevice'");
                    insertWatcher = new ManagementEventWatcher(Scope, InsertQuery);
                    insertWatcher.EventArrived += usbInsertHandler;
                    insertWatcher.Start();
                }
                if (usbRemoveHandler != null)// USB拔出监视
                {
                    WqlEventQuery RemoveQuery = new WqlEventQuery("__InstanceDeletionEvent",withinInterval,"TargetInstance isa 'Win32_USBControllerDevice'");
                    removeWatcher = new ManagementEventWatcher(Scope, RemoveQuery);
                    removeWatcher.EventArrived += usbRemoveHandler;
                    removeWatcher.Start();
                }
                return true;
            }
            catch (Exception)
            {
                RemoveUSBEventWatcher();
                return false;
            }
        }
        /// <summary>  
        /// 移去USB事件监视器  
        /// </summary>  
        public void RemoveUSBEventWatcher()
        {
            if (insertWatcher != null)
            {
                insertWatcher.Stop();
                insertWatcher = null;
            }
            if (removeWatcher != null)
            {
                removeWatcher.Stop();
                removeWatcher = null;
            }
        }
        /// <summary>  
        /// 定位发生插拔的USB设备  
        /// </summary>  
        /// <param name="e">USB插拔事件参数</param>  
        /// <returns>发生插拔现象的USB控制设备ID</returns>  
        public static USBControllerDevice[] WhoUSBControllerDevice(EventArrivedEventArgs e)
        {
            ManagementBaseObject mbo = e.NewEvent["TargetInstance"] as ManagementBaseObject;
            if (mbo != null && mbo.ClassPath.ClassName == "Win32_USBControllerDevice")
            {
                String Antecedent = (mbo["Antecedent"] as String).Replace("\"", String.Empty).Split(new Char[] { '=' })[1];
                String Dependent = (mbo["Dependent"] as String).Replace("\"", String.Empty).Split(new Char[] { '=' })[1];
                return new USBControllerDevice[1] { new USBControllerDevice { Antecedent = Antecedent, Dependent = Dependent } };
            }
            return null;
        }
    }
    public class Win32Helper
    {
        [DllImport("User32.dll")]
        public static extern bool LockWorkStation();
        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
    }
}