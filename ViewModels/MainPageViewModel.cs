using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using WeatherDataApp.Models;
using WeatherDataApp.Services;

namespace WeatherDataApp.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService = new();

        public ObservableCollection<WeatherData> WeatherItems { get; set; } = new();

        public ICommand LoadWeatherCommand { get; }

        private string _status = "Status: nothing loaded yet";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private double _averageTemperature;
        public double AverageTemperature
        {
            get => _averageTemperature;
            set { _averageTemperature = value; OnPropertyChanged(); }
        }

        private double _maxTemperature;
        public double MaxTemperature
        {
            get => _maxTemperature;
            set { _maxTemperature = value; OnPropertyChanged(); }
        }

        private double _minTemperature;
        public double MinTemperature
        {
            get => _minTemperature;
            set { _minTemperature = value; OnPropertyChanged(); }
        }

        private bool _hasChartData;
        public bool HasChartData
        {
            get => _hasChartData;
            set { _hasChartData = value; OnPropertyChanged(); }
        }

        private DateTime _startDate = new(2022, 1, 1);
        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); }
        }

        private DateTime _endDate = new(2022, 1, 5);
        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); }
        }

        public MainPageViewModel()
        {
            LoadWeatherCommand = new Command(async () => await LoadWeatherAsync());
        }

        private async Task LoadWeatherAsync()
        {
            Status = "Started loading weather data...";
            WeatherItems.Clear();

            var start = new DateTime(2022, 1, 1);
            var end = new DateTime(2022, 1, 5);

            var data = await _weatherService.GetWeatherDataRangeParallelAsync(StartDate, EndDate);

            foreach(var item in data)
                WeatherItems.Add(item);
            
            if(data.Count != 0)
            {
                AverageTemperature = data.AsParallel().Average(w => w.Temperature);
                MaxTemperature = data.AsParallel().Max(w => w.Temperature);
                MinTemperature = data.AsParallel().Min(w => w.Temperature);
            }

            HasChartData = WeatherItems.Count > 0;
            Status = $"{WeatherItems.Count} data points loaded";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
