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

namespace ZongziTEK_Blackboard_Sticker.Pages.WelcomePages
{
    /// <summary>
    /// WelcomePage3.xaml 的交互逻辑
    /// </summary>
    public partial class WelcomePage3 : Page
    {
        public WelcomePage3()
        {
            InitializeComponent();

            TextBoxWeatherCity.Text = MainWindow.Settings.InfoBoard.WeatherCity;
            isLoaded = true;
        }

        private bool isLoaded = false;

        private void TextBoxWeatherCity_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!isLoaded) return;

            MainWindow.Settings.InfoBoard.WeatherCity = TextBoxWeatherCity.Text;
            MainWindow.SaveSettings();
        }
    }
}
