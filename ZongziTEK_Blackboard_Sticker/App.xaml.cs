using System.Linq;
using System.Reflection;
using System;
using System.Windows;
using System.Windows.Data;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using AutoUpdaterDotNET;

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
                MessageBox.Show("已有一个黑板贴正在运行", "ZongziTEK 黑板贴", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(0);
            }

            // Auto Updater
            AutoUpdater.PersistenceProvider = new JsonFilePersistenceProvider("AutoUpdater.json");
            AutoUpdater.Start($"http://s.zztek.top:1573/zbsupdate.xml");
            AutoUpdater.ApplicationExitEvent += () =>
            {
                Environment.Exit(0);
            };
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool)) throw new InvalidOperationException("The target must be a boolean");
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
