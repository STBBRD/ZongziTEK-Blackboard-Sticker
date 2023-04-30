using System.Linq;
using System.Reflection;
using System;
using System.Windows;

namespace ZongziTEK_Blackboard_Sticker
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;

        public App()
        {
            this.Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "ZongziTEK_Blackboard_Sticker", out ret);

            if (!ret)
            {
                MessageBox.Show("已有一个黑板贴正在运行");
                Environment.Exit(0);
            }
        }
    }
}
