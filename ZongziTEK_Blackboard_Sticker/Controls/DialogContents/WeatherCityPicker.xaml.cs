using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using ZongziTEK_Blackboard_Sticker.Helpers;
using iNKORE.UI.WPF.Controls;
using static ZongziTEK_Blackboard_Sticker.Helpers.WeatherHelper;

namespace ZongziTEK_Blackboard_Sticker.Controls.DialogContents
{
    /// <summary>
    /// WeatherCityPicker.xaml 的交互逻辑
    /// </summary>
    public partial class WeatherCityPicker : UserControl
    {
        public WeatherCityPicker()
        {
            InitializeComponent();

            LoadCityData();
            FilterCitiesAsync();
        }

        WeatherCityData weatherCityData = new();
        private List<City> filteredCities = new();
        private System.Threading.Timer searchDelayTimer;

        private void LoadCityData()
        {
            weatherCityData = GetWeatherCityData();
        }

        private void TextBoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            // 延迟搜索，避免频繁触发
            searchDelayTimer?.Dispose();
            searchDelayTimer = new System.Threading.Timer(_ =>
            {
                Dispatcher.Invoke(() => FilterCitiesAsync());
            }, null, 300, System.Threading.Timeout.Infinite);
        }

        private void FilterCities()
        {
            string searchText = TextBoxSearch.Text;

            if (string.IsNullOrWhiteSpace(searchText))
            {
                filteredCities = weatherCityData.Cities.ToList();
            }
            else
            {
                filteredCities = weatherCityData.Cities.Where(c => c.Name.Contains(searchText)).ToList();
            }

            var filteredProvinceIds = filteredCities
                .Select(c => c.ProvinceId)
                .Distinct()
                .ToList();

            var filteredProvinces = weatherCityData.Provinces
                .Where(p => filteredProvinceIds.Contains(p.Id - 1))
                .ToList();

            StackPanelSearchResult.Children.Clear();

            if (filteredCities.Count == 0)
            {
                TextNoResult.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                TextNoResult.Visibility = Visibility.Collapsed;
            }

            foreach (Province province in filteredProvinces)
            {
                var citiesInProvince = filteredCities
                    .Where(c => c.ProvinceId == province.Id - 1)
                    .ToList();

                Expander expander = new()
                {
                    Header = province.Name,
                    Padding = new Thickness(8),
                    Content = new SimpleStackPanel() { Spacing = 4 }
                };

                if (citiesInProvince.Count != 0)
                {
                    foreach (City city in citiesInProvince)
                    {
                        RadioButton radioButton = new()
                        {
                            Content = city.Name,
                            GroupName = "WeatherCity"
                        };

                        if (city.CityCode == MainWindow.Settings.InfoBoard.WeatherCity)
                        {
                            radioButton.IsChecked = true;
                        }

                        radioButton.Checked += RadioButton_Checked;

                        ((SimpleStackPanel)expander.Content).Children.Add(radioButton);
                    }
                    if (!string.IsNullOrWhiteSpace(searchText)) expander.IsExpanded = true;
                }
                StackPanelSearchResult.Children.Add(expander);
            }
        }

        private async void FilterCitiesAsync()
        {

            try
            {
                StackPanelSearchResult.Visibility = Visibility.Collapsed;
                ProgressRing.Visibility = Visibility.Visible;
                await Dispatcher.InvokeAsync(FilterCities);
            }
            finally
            {
                ProgressRing.Visibility = Visibility.Collapsed;
                StackPanelSearchResult.Visibility = Visibility.Visible;
            }
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string cityName = (sender as RadioButton).Content.ToString();
            City city = weatherCityData.Cities
                .FirstOrDefault(c => c.Name == cityName);
            string cityCode = city.CityCode;

            MainWindow.Settings.InfoBoard.WeatherCity = cityCode;
            MainWindow.SaveSettings();
        }
    }
}
