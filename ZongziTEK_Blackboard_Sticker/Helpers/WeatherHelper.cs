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

    public class XiaomiWeather
    {
        [JsonProperty("current")] public Current Current { get; set; }
    }

    public class Current
    {
        [JsonProperty("weather")] public string Weather { get; set; }
        [JsonProperty("uvIndex")] public string UvIndex { get; set; }
        [JsonProperty("pubTime")] public string PubTime { get; set; }
        [JsonProperty("feelsLike")] public FeelsLike FeelsLike { get; set; }
        [JsonProperty("humidity")] public Humidity Humidity { get; set; }
        [JsonProperty("pressure")] public Pressure Pressure { get; set; }
        [JsonProperty("temperature")] public Temperature Temperature { get; set; }
        [JsonProperty("visibility")] public Visibility Visibility { get; set; }
        [JsonProperty("wind")] public Wind Wind { get; set; }
    }

    public class FeelsLike
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Humidity
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Pressure
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Temperature
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Visibility
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Wind
    {
        [JsonProperty("direction")] public Direction Direction { get; set; }
        [JsonProperty("speed")] public Speed Speed { get; set; }
    }
    public class Direction
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
    }
    public class Speed
    {
        [JsonProperty("unit")] public string Unit { get; set; }
        [JsonProperty("value")] public string Value { get; set; }
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
