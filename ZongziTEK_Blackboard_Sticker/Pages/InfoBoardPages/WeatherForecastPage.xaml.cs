using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json;
using ZongziTEK_Blackboard_Sticker.Helpers.Weather;
using static ZongziTEK_Blackboard_Sticker.Helpers.Weather.WeatherHelper;

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

            Timer_Tick(null, null);

            timer.Interval = TimeSpan.FromMinutes(30);
            timer.Tick += Timer_Tick;
            timer.Start();

            MainWindow.Settings.InfoBoard.PropertyChanged += InfoBoard_PropertyChanged;
            Unloaded += Page_Unloaded;
        }

        private DispatcherTimer timer = new DispatcherTimer();

        private ForecastDaily forecastWeather = new();

        private List<ForecastWeatherItemData> ForecastWeatherItemDatas = new();

        private async void Timer_Tick(object sender, EventArgs e)
        {
            timer.Stop();
            await Task.Run(() =>
            {
                if (!new DirectoryInfo("Weather/").Exists)
                {
                    try
                    {
                        new DirectoryInfo("Weather/").Create();
                    }
                    catch { }
                }

                if (File.Exists(xiaomiWeatherFilePath))
                {
                    DateTime currentWeatherFetchTime = new FileInfo(
                        xiaomiWeatherFilePath
                    ).LastWriteTime;
                    if (DateTime.Now - currentWeatherFetchTime > TimeSpan.FromHours(1))
                    {
                        UpdateXiaomiWeather();
                    }
                    else
                    {
                        xiaomiWeather = JsonConvert.DeserializeObject<XiaomiWeather>(
                            File.ReadAllText(xiaomiWeatherFilePath)
                        );

                        if (forecastWeather != null)
                        {
                            if (lastCityCode != MainWindow.Settings.InfoBoard.WeatherCity)
                            {
                                UpdateXiaomiWeather();
                                if (File.Exists(xiaomiWeatherFilePath))
                                {
                                    File.Delete(xiaomiWeatherFilePath);
                                }
                            }
                        }
                        else
                        {
                            UpdateXiaomiWeather();
                        }
                    }
                }
                else
                {
                    UpdateXiaomiWeather();
                }

                forecastWeather = xiaomiWeather.ForecastDaily;
                lastCityCode = MainWindow.Settings.InfoBoard.WeatherCity;

                Dispatcher.BeginInvoke(() =>
                {
                    //ShowForecastWeathers();
                    UpdateForecastWeatherItemDatas();
                });
            });
            timer.Start();
        }

        private void UpdateForecastWeatherItemDatas()
        {
            int countLimit = 4;
            if (MainWindow.Settings.Look.LookMode != 0) countLimit = 2;

            ForecastWeatherItemDatas.Clear();

            for (int i = 0; i < countLimit; i++)
            {
                ForecastWeatherItemData itemData = new()
                {
                    DayName = TransformIndexToDay(i),
                    TemperatureFrom = forecastWeather.Temperature.Values[i].From + forecastWeather.Temperature.Unit,
                    TemperatureTo = forecastWeather.Temperature.Values[i].To + forecastWeather.Temperature.Unit,
                    WeatherFromIcon = GetWeatherIcon(Convert.ToInt32(forecastWeather.Weather.Values[i].From), false),
                    WeatherToIcon = GetWeatherIcon(Convert.ToInt32(forecastWeather.Weather.Values[i].To), true)
                };
                if (
                        GetWeatherName(Convert.ToInt32(forecastWeather.Weather.Values[i].From)).Contains("雨")
                        || GetWeatherName(Convert.ToInt32(forecastWeather.Weather.Values[i].To)).Contains("雨")
                    )
                {
                    itemData.IsRainy = true;
                }

                ForecastWeatherItemDatas.Add(itemData);
            }

            ItemsControlForecastWeather.ItemsSource = ForecastWeatherItemDatas;
        }

        private void ShowForecastWeathers()
        {
            if (forecastWeather != null && forecastWeather.Weather.Values.Length != 0)
            {
                string rainDays = "";

                int index = 0;

                foreach (var forecastWeather in forecastWeather.Weather.Values)
                {
                    int countLimit = 3;
                    if (MainWindow.Settings.Look.LookMode != 0) countLimit = 2;
                    if (index > countLimit) break;

                    if (
                        GetWeatherName(Convert.ToInt32(forecastWeather.From)).Contains("雨")
                        || GetWeatherName(Convert.ToInt32(forecastWeather.To)).Contains("雨")
                    )
                    {
                        rainDays += TransformIndexToDay(index) + "、";
                    }
                    index++;
                }
                if (rainDays.Length > 0)
                {
                    rainDays = rainDays.Remove(rainDays.Length - 1);
                    TextRainyDays.Text = rainDays + "有雨";
                }
                else
                {
                    TextRainyDays.Text =
                        "未来 " + (forecastWeather.Weather.Values.Length - 1) + " 天不会下雨";
                }
            }
            else
            {
                TextRainyDays.Text = "暂无天气预报信息";
            }
        }

        private string TransformIndexToDay(int index)
        {
            string weekstring = "";
            DateTime today = DateTime.Today;

            switch (index)
            {
                case 0:
                    return "今天";
                case 1:
                    return "明天";
            }

            DateTime targetDate = today.AddDays(index);

            return targetDate.DayOfWeek switch
            {
                DayOfWeek.Sunday => "周日",
                DayOfWeek.Monday => "周一",
                DayOfWeek.Tuesday => "周二",
                DayOfWeek.Wednesday => "周三",
                DayOfWeek.Thursday => "周四",
                DayOfWeek.Friday => "周五",
                DayOfWeek.Saturday => "周六",
                _ => weekstring,
            };
        }

        private void InfoBoard_PropertyChanged(
            object sender,
            System.ComponentModel.PropertyChangedEventArgs e
        )
        {
            Timer_Tick(null, null);
        }

        private void Page_Unloaded(object sender, EventArgs e)
        {
            timer.Stop();
            timer.Tick -= Timer_Tick;
            Unloaded -= Page_Unloaded;
            MainWindow.Settings.InfoBoard.PropertyChanged -= InfoBoard_PropertyChanged;
        }
    }
}
