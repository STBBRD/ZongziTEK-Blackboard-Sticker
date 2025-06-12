using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Resources;
using static ZongziTEK_Blackboard_Sticker.Helpers.WeatherHelper;

namespace ZongziTEK_Blackboard_Sticker.Helpers
{
    public class WeatherHelper
    {
        public class WeatherCityData
        {
            [JsonProperty("provinces")] public Province[] Provinces { get; set; }
            [JsonProperty("citys")] public City[] Cities { get; set; }
        }

        public class Province
        {
            [JsonProperty("_id")] public int Id { get; set; }
            [JsonProperty("name")] public string Name { get; set; }
        }

        public class City
        {
            [JsonProperty("_id")] public int Id { get; set; }
            [JsonProperty("province_id")] public int ProvinceId { get; set; } // ProvinceId = Province.Id - 1
            [JsonProperty("name")] public string Name { get; set; }
            [JsonProperty("city_num")] public string CityCode { get; set; }
        }

        public static WeatherCityData GetWeatherCityData()
        {
            string jsonContent;
            WeatherCityData weatherCityInfo;

            Uri uri = new Uri("Resources/XiaomiWeather/xiaomi_weather.json", UriKind.Relative);
            StreamResourceInfo streamInfo = Application.GetResourceStream(uri);
            using (StreamReader reader = new StreamReader(streamInfo.Stream))
            {
                jsonContent = reader.ReadToEnd();
            }

            weatherCityInfo = JsonConvert.DeserializeObject<WeatherCityData>(jsonContent);
            return weatherCityInfo;
        }
    }

    public class CityCodeToNameConverter : IValueConverter
    {
        private static readonly List<City> Cities = GetWeatherCityData().Cities.ToList();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string cityCode)
            {
                var city = Cities.Find(c => c.CityCode == cityCode);
                return city?.Name ?? "未知";
            }
            return "Invalid input";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
