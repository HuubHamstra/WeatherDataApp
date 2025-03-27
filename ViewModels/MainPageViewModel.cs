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
            set 
            { 
                _startDate = value; OnPropertyChanged();
                OnPropertyChanged(nameof(IsLargeDataSet));
            }
        }

        private DateTime _endDate = new(2022, 1, 5);
        public DateTime EndDate
        {
            get => _endDate;
            set 
            { 
                _endDate = value; OnPropertyChanged();
                OnPropertyChanged(nameof(IsLargeDataSet));
            }
        }

        public bool IsLargeDataSet => (EndDate - StartDate).TotalDays > 31;

        private double _averageWindSpeed;
        public double AverageWindSpeed
        {
            get => _averageWindSpeed;
            set { _averageWindSpeed = value; OnPropertyChanged(); }
        }

        private double _averagePrecipitation;
        public double AveragePrecipitation
        {
            get => _averagePrecipitation;
            set { _averagePrecipitation = value; OnPropertyChanged(); }
        }

        private double _averageCloudCover;
        public double AverageCloudCover
        {
            get => _averageCloudCover;
            set { _averageCloudCover = value; OnPropertyChanged(); }
        }

        private double _averageHumidity;
        public double AverageHumidity
        {
            get => _averageHumidity;
            set { _averageHumidity = value; OnPropertyChanged(); }
        }

        private double _averagePressure;
        public double AveragePressure
        {
            get => _averagePressure;
            set { _averagePressure = value; OnPropertyChanged(); }
        }

        private bool _showTemperature = true;
        public bool ShowTemperature
        {
            get => _showTemperature;
            set { _showTemperature = value; OnPropertyChanged(); }
        }

        private bool _showWind = true;
        public bool ShowWind
        {
            get => _showWind;
            set { _showWind = value; OnPropertyChanged(); }
        }

        private bool _showPrecipitation = true;
        public bool ShowPrecipitation
        {
            get => _showPrecipitation;
            set { _showPrecipitation = value; OnPropertyChanged(); }
        }

        private bool _showCloud = true;
        public bool ShowCloud
        {
            get => _showCloud;
            set { _showCloud = value; OnPropertyChanged(); }
        }

        private bool _showHumidity = true;
        public bool ShowHumidity
        {
            get => _showHumidity;
            set { _showHumidity = value; OnPropertyChanged(); }
        }

        private bool _showPressure = true;
        public bool ShowPressure
        {
            get => _showPressure;
            set { _showPressure = value; OnPropertyChanged(); }
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
                AverageWindSpeed = data.AsParallel().Average(w => w.WindSpeed);
                AveragePrecipitation = data.AsParallel().Average(w => w.Precipitation);
                AverageCloudCover = data.AsParallel().Average(w => w.CloudCover);
                AverageHumidity = data.AsParallel().Average(w => w.Humidity);
                AveragePressure = data.AsParallel().Average(w => w.Pressure);
            }

            HasChartData = WeatherItems.Count > 0;
            Status = $"{WeatherItems.Count} data points loaded";
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
