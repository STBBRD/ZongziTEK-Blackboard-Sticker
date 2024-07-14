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

        private void ComboBoxTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.SaveSettings();
            MainWindow.SetTheme();            
        }

        private void ComboBoxLookMode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MainWindow.SaveSettings();
            MainWindow.SwitchLookMode();
        }

        private void SliderWindowScaleMultiplier_ValueChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();
            MainWindow.SetWindowScaleTransform(MainWindow.Settings.Look.WindowScaleMultiplier);
        }
    }
}
