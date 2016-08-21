namespace LockScreen
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool createNew;
            using (Mutex mutex = new Mutex(true, Application.ProductName, out createNew))
            {
                if (createNew)
                {
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    if (args.Length > 0)
                    {
                        var parm = args[0];
                        Application.Run(new Form1(parm));
                    }
                    else
                    {
                        Application.Run(new Form1("all"));
                    }
                }
                else
                {
                    MessageBox.Show("应用程序已经在运行中...");
                    Thread.Sleep(1000);
                    Environment.Exit(1);
                }
            }
        }
    }
}
