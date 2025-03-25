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
            string url = $"https://archive-api.open-meteo.com/v1/archive?latitude=52.37&longitude=4.89&start_date={startDate}&end_date={endDate}&hourly=temperature_2m";

            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var times = root.GetProperty("hourly").GetProperty("time").EnumerateArray();
            var temps = root.GetProperty("hourly").GetProperty("temperature_2m").EnumerateArray();

            var list = new List<WeatherData>();
            while (times.MoveNext() && temps.MoveNext())
            {
                list.Add(new WeatherData
                {
                    Time = DateTime.Parse(times.Current.GetString()!),
                    Temperature = temps.Current.GetDouble()
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