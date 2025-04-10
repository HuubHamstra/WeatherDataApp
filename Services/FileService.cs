using System.Text.Json;
using WeatherDataApp.Models;

namespace WeatherDataApp.Services
{
    public class FileService
    {
        /// <summary>
        /// Exports weather data to JSON files asynchronously
        /// </summary>
        /// <param name="weatherDataList">All WeatherData to export</param>
        /// <param name="folderPath">Folderpath to write all files to</param>
        /// <param name="entriesPerFile">How many WeatherData entries should be in each file</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task ExportWeatherDataAsync(List<WeatherData> weatherDataList, string folderPath, int entriesPerFile)
        {
            if (weatherDataList == null || weatherDataList.Count == 0)
                throw new ArgumentException("Weather data list cannot be null or empty.");

            if (string.IsNullOrWhiteSpace(folderPath))
                throw new ArgumentException("Folder path cannot be null or empty.");

            if (entriesPerFile <= 0)
                throw new ArgumentException("Entries per file must be greater than zero.");

            var tasks = new List<Task>();
            int fileIndex = 1;

            for (int i = 0; i < weatherDataList.Count; i += entriesPerFile)
            {
                var chunk = weatherDataList.Skip(i).Take(entriesPerFile).ToList();
                string filePath = Path.Combine(folderPath, $"WeatherData_{fileIndex++}.json");

                tasks.Add(Task.Run(() => WriteToFileAsync(chunk, filePath)));
            }

            await Task.WhenAll(tasks);
        }

        /// <summary>
        /// Writes a list of WeatherData to a JSON file asynchronously
        /// </summary>
        /// <param name="weatherData">WeatherData list to export</param>
        /// <param name="filePath">Filepath to write file to</param>
        /// <returns></returns>
        private async Task WriteToFileAsync(List<WeatherData> weatherData, string filePath)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await JsonSerializer.SerializeAsync(stream, weatherData, options);
            }
        }
    }
}
