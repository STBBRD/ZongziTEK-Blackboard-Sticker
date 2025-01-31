using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = iNKORE.UI.WPF.Modern.Controls.MessageBox;

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages
{
    /// <summary>
    /// LookSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class LookSettingsPage : Page
    {
        public LookSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.Look;
        }

        private bool isCurrentWindowChromeDisabled = MainWindow.Settings.Look.IsWindowChromeDisabled;
        MainWindow mainWindow = Application.Current.MainWindow as MainWindow;

        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.SaveSettings();
            MainWindow.SetTheme();
        }

        private void ComboBoxLookMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.SaveSettings();
            mainWindow.SwitchLookMode(MainWindow.Settings.Look.LookMode);
        }

        private void SliderWindowScaleMultiplier_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            MainWindow.SetWindowScaleTransform(MainWindow.Settings.Look.WindowScaleMultiplier);
        }

        private void SliderWindowScaleMultiplier_ValueChangeStart(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Settings.Look.LookMode != 0) mainWindow.SwitchLookMode(0);
        }

        private void SliderWindowScaleMultiplier_ValueChangeEnd(object sender, RoutedEventArgs e)
        {
            if (MainWindow.Settings.Look.LookMode != 0) mainWindow.SwitchLookMode(MainWindow.Settings.Look.LookMode);
        }

        private void ToggleSwitchIsWindowChromeDisabled_Toggled(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            if (ToggleSwitchIsWindowChromeDisabled.IsOn != isCurrentWindowChromeDisabled) MessageBox.Show("此更改需重启黑板贴后生效", "ZongziTEK 黑板贴", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
