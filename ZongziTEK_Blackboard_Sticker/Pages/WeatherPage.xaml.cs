using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Threading;
using ZongziTEK_Weather_API;

namespace ZongziTEK_Blackboard_Sticker.Pages
{
    /// <summary>
    /// WeatherPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherPage : Page
    {
        public WeatherPage()
        {
            InitializeComponent();

            Timer_Tick(null,null);

            timer.Interval = TimeSpan.FromHours(1);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private DispatcherTimer timer = new DispatcherTimer();

        private string liveWeatherFilePath = "Weather/LiveWeather.json";

        private LiveWeather liveWeather = new LiveWeather();

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!new DirectoryInfo("Weather/").Exists)
            {
                try { new DirectoryInfo("Weather/").Create(); }
                catch { }
            }

            if (File.Exists(liveWeatherFilePath))
            {
                DateTime liveWeatherFetchTime = new FileInfo(liveWeatherFilePath).LastWriteTime;
                if (DateTime.Now - liveWeatherFetchTime > TimeSpan.FromHours(1))
                {
                    UpdateLiveWeather();
                }
                else
                {
                    liveWeather = JsonConvert.DeserializeObject<LiveWeather>(File.ReadAllText(liveWeatherFilePath));

                    if (liveWeather.adcode != Weather.GetCityCode(MainWindow.Settings.InfoBoard.WeatherCity))
                    {
                        UpdateLiveWeather();
                        if (File.Exists("Weather/CastWeather.json"))
                        {
                            File.Delete("Weather/CastWeather.json");
                        }
                    }
                    else if (liveWeather.isError)
                    {
                        UpdateLiveWeather();
                    }
                }
            }
            else
            {
                UpdateLiveWeather();
            }

            ShowLiveWeather();
        }

        private void UpdateLiveWeather()
        {
            liveWeather = Weather.GetLiveWeather(MainWindow.Settings.InfoBoard.WeatherCity);
            File.WriteAllText(liveWeatherFilePath, JsonConvert.SerializeObject(liveWeather, Formatting.Indented));
        }

        private void ShowLiveWeather()
        {
            if (!liveWeather.isError)
            {
                LabelWeatherInfo.Content = liveWeather.weather + " " + liveWeather.temperature + "℃";
                LabelCity.Content = liveWeather.province + " " + liveWeather.city;

                //加载天气图标
                string imagePath = "../Resources/WeatherIcons/";
                if (liveWeather.weather.Contains("雷"))
                {
                    imagePath += "Thundery.png";
                }
                else if (liveWeather.weather.Contains("雨"))
                {
                    imagePath += "Rainy.png";
                }
                else if (liveWeather.weather.Contains("雪"))
                {
                    imagePath += "Snowy.png";
                }
                else if (liveWeather.weather.Contains("阴"))
                {
                    imagePath += "Cloudy.png";
                }
                else if (liveWeather.weather.Contains("晴"))
                {
                    imagePath += "Sunny.png";
                }
                else
                {
                    imagePath += "MostCloudy.png";
                }
                ImageWeather.Source = new BitmapImage(new Uri(imagePath, UriKind.Relative));
            }
            else
            {
                LabelWeatherInfo.Content = "暂无实况天气信息";
                LabelCity.Content = "天气";
                ImageWeather.Visibility = Visibility.Collapsed;
            }
        }
    }
}
