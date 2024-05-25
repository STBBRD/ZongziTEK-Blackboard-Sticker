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
using ZongziTEK_Weather_API;

namespace ZongziTEK_Blackboard_Sticker.Pages
{
    /// <summary>
    /// WeatherForecastPage.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherForecastPage : Page
    {
        public WeatherForecastPage()
        {
            InitializeComponent();

            if (!new DirectoryInfo("Weather/").Exists)
            {
                try { new DirectoryInfo("Weather/").Create(); }
                catch { }
            }

            if (File.Exists(castWeatherFilePath))
            {
                DateTime castWeatherFetchTime = new FileInfo(castWeatherFilePath).LastWriteTime;
                if (castWeatherFetchTime.Date != DateTime.Today)
                {
                    UpdateCastWeathers();
                }
                else
                {
                    castWeathers = JsonConvert.DeserializeObject<List<CastWeather>>(File.ReadAllText(castWeatherFilePath));

                    if (castWeathers.Count == 0)
                    {
                        UpdateCastWeathers();
                    }
                }
            }
            else
            {
                UpdateCastWeathers();
            }

            ShowCastWeathers();
        }

        private string castWeatherFilePath = "Weather/CastWeather.json";

        private List<CastWeather> castWeathers = new List<CastWeather>();

        private void UpdateCastWeathers()
        {
            castWeathers = Weather.GetCastWeathers(MainWindow.Settings.InfoBoard.WeatherCity);
            File.WriteAllText(castWeatherFilePath, JsonConvert.SerializeObject(castWeathers, Formatting.Indented));
        }

        private bool isTodayRainy = false;

        private void ShowCastWeathers()
        {
            if (castWeathers.Count != 0)
            {
                string rainDays = "";
                int today = (int)DateTime.Now.DayOfWeek;
                if (today == 0) today = 7;
                foreach (var castWeather in castWeathers)
                {
                    if (castWeather.dayweather.Contains("雨") || castWeather.nightweather.Contains("雨"))
                    {
                        if (castWeather.week == today)
                        {
                            isTodayRainy = true;
                        }
                        else
                        {
                            rainDays += TransformDayOfWeek(castWeather.week) + "、";
                        }
                    }
                }
                if (rainDays.Length > 0)
                {
                    rainDays = rainDays.Remove(rainDays.Length - 1);
                    LabelWeatherForecast.Content = rainDays + "将会下雨";
                }
                else
                {
                    LabelWeatherForecast.Content = "未来 " + (castWeathers.Count - 1) + " 天不会下雨";
                }
                if (isTodayRainy) LabelWeatherForecast.Content = "今天有雨，" + LabelWeatherForecast.Content;
            }
            else
            {
                LabelWeatherForecast.Content = "暂无未来天气信息";
            }
        }

        private string TransformDayOfWeek(int week)
        {
            string weekstring = "";
            switch (week)
            {
                case 1:
                    weekstring = "周一";
                    break;
                case 2:
                    weekstring = "周二";
                    break;
                case 3:
                    weekstring = "周三";
                    break;
                case 4:
                    weekstring = "周四";
                    break;
                case 5:
                    weekstring = "周五";
                    break;
                case 6:
                    weekstring = "周六";
                    break;
                case 7:
                    weekstring = "周日";
                    break;
            }
            return weekstring;
        }
    }
}
