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

namespace ZongziTEK_Blackboard_Sticker.Pages.SettingsPages.InfoBoardSettingsPages
{
    /// <summary>
    /// WeatherSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherSettingsPage : Page
    {
        public WeatherSettingsPage()
        {
            InitializeComponent();

            DataContext = MainWindow.Settings.InfoBoard;
            LastCity = MainWindow.Settings.InfoBoard.WeatherCity.ToString();
        }

        private string LastCity;

        private void TextBoxCity_TextChanged(object sender, RoutedEventArgs e)
        {
            MainWindow.SaveSettings();

            if (MainWindow.Settings.InfoBoard.WeatherCity != LastCity)
            {
                (Application.Current.MainWindow as MainWindow).SwitchFrameInfoPage();
                LastCity = MainWindow.Settings.InfoBoard.WeatherCity;
            }
        }
    }
}
