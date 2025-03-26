using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherDataApp.Models;

namespace WeatherDataApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient = new();

        public async Task<List<WeatherData>> GetWeatherDataAsync(string startDate, string endDate)
        {
            string url = $"https://archive-api.open-meteo.com/v1/archive?latitude=52.37&longitude=4.89&start_date={startDate}&end_date={endDate}&hourly=temperature_2m,wind_speed_10m,precipitation,cloudcover,relative_humidity_2m,surface_pressure";

            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var times = root.GetProperty("hourly").GetProperty("time").EnumerateArray();
            var temps = root.GetProperty("hourly").GetProperty("temperature_2m").EnumerateArray();
            var wind = root.GetProperty("hourly").GetProperty("wind_speed_10m").EnumerateArray();
            var precipitation = root.GetProperty("hourly").GetProperty("precipitation").EnumerateArray();
            var cloud = root.GetProperty("hourly").GetProperty("cloudcover").EnumerateArray();
            var humidity = root.GetProperty("hourly").GetProperty("relative_humidity_2m").EnumerateArray();
            var pressure = root.GetProperty("hourly").GetProperty("surface_pressure").EnumerateArray();

            var list = new List<WeatherData>();
            while (times.MoveNext() && temps.MoveNext() && wind.MoveNext() &&
                precipitation.MoveNext() && cloud.MoveNext() && humidity.MoveNext() && pressure.MoveNext())
            {
                list.Add(new WeatherData
                {
                    Time = DateTime.Parse(times.Current.GetString()!),
                    Temperature = temps.Current.GetDouble(),
                    WindSpeed = wind.Current.GetDouble(),
                    Precipitation = precipitation.Current.GetDouble(),
                    CloudCover = cloud.Current.GetDouble(),
                    Humidity = humidity.Current.GetDouble(),
                    Pressure = pressure.Current.GetDouble()
                });
            }


            return list;
        }

        public async Task<List<WeatherData>> GetWeatherDataRangeParallelAsync(DateTime start, DateTime end)
        {
            var tasks = new List<Task<List<WeatherData>>>();
            DateTime current = start.Date;

            while (current <= end.Date)
            {
                string from = current.ToString("yyyy-MM-dd");
                string to = current.ToString("yyyy-MM-dd");

                tasks.Add(GetWeatherDataAsync(from, to));

                current = current.AddDays(1);
            }

            var results = await Task.WhenAll(tasks);

            return [.. results.SelectMany(r => r)];
        }

    }
}