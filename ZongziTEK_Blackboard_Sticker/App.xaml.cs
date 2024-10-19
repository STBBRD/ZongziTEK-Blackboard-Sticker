using System.Linq;
using System.Reflection;
using System;
using System.Windows;
using System.Windows.Data;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;
using AutoUpdaterDotNET;
using System.Globalization;
using ZongziTEK_Blackboard_Sticker.Helpers;
using System.Diagnostics;
using Sentry;

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
            
            SentrySdk.Init(o =>
            {
                // Tells which project in Sentry to send events to:
                o.Dsn = "https://be9e01f3b4459566b40b8bbc1ce7f8be@o4508149744140288.ingest.de.sentry.io/4508149746040912";
                // When configuring for the first time, to see what the SDK is doing:
                o.Debug = true;
                // Set TracesSampleRate to 1.0 to capture 100% of transactions for tracing.
                // We recommend adjusting this value in production.
                o.TracesSampleRate = 1.0;
            });
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            bool ret;
            mutex = new System.Threading.Mutex(true, "ZongziTEK_Blackboard_Sticker", out ret);

            if (!ret && !e.Args.Contains("-m"))
            {
                MessageBox.Show("已有一个黑板贴正在运行", "ZongziTEK 黑板贴", MessageBoxButton.OK, MessageBoxImage.Warning);
                Environment.Exit(0);
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                SentrySdk.CaptureException(e.Exception);
                LogHelper.WriteLogToFile(e.Exception.Message, LogHelper.LogType.Error);
            }
            catch
            {
                Process.Start(System.Windows.Forms.Application.ExecutablePath, "-m");
                Shutdown();
            }
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

    [ValueConversion(typeof(string), typeof(Visibility))]
    public class TextEmptyToCollapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string text = value as string;
            return string.IsNullOrEmpty(text) ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            bool boolValue = (bool)value;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            bool boolValue = (bool)value;

            return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
