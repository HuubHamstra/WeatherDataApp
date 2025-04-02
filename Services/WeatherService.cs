using System.Text.Json;
using WeatherDataApp.Models;

namespace WeatherDataApp.Services
{
    public class WeatherService
    {
        private readonly HttpClient _httpClient = new();

        public async Task<List<WeatherData>> GetWeatherDataAsync(string startDate, string endDate)
        {
            DateTime start = DateTime.Parse(startDate);
            DateTime end = DateTime.Parse(endDate);

            var allData = new List<WeatherData>();
            var semaphore = new SemaphoreSlim(3);
            var tasks = new List<Task>();

            const int chunkSize = 30;

            DateTime current = start;
            while (current <= end)
            {
                var chunkEnd = current.AddDays(chunkSize - 1);
                if (chunkEnd > end) chunkEnd = end;

                await semaphore.WaitAsync();

                var from = current;
                var to = chunkEnd;

                var task = FetchWeatherDataForRangeAsync(from, to)
                    .ContinueWith(t =>
                    {
                        if (t.Status == TaskStatus.RanToCompletion && t.Result != null)
                        {
                            lock (allData)
                            {
                                allData.AddRange(t.Result);
                            }
                        }
                        semaphore.Release();
                    });

                tasks.Add(task);
                current = chunkEnd.AddDays(1);
            }

            await Task.WhenAll(tasks);
            return allData;
        }


        private async Task<List<WeatherData>?> FetchWeatherDataForRangeAsync(DateTime from, DateTime to)
        {
            try
            {
                var url = $"https://archive-api.open-meteo.com/v1/archive" +
                        $"?latitude=52.37&longitude=4.89" +
                        $"&start_date={from:yyyy-MM-dd}&end_date={to:yyyy-MM-dd}" +
                        $"&hourly=temperature_2m,wind_speed_10m,precipitation,cloudcover,relative_humidity_2m,surface_pressure";

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
                    precipitation.MoveNext() && cloud.MoveNext() &&
                    humidity.MoveNext() && pressure.MoveNext())
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching range {from:yyyy-MM-dd} to {to:yyyy-MM-dd}: {ex.Message}");
                return null;
            }
        }
    }
}