using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using Microsoft.VisualBasic;
using WeatherDataApp.Models;
using WeatherDataApp.Services;

namespace WeatherDataApp.ViewModels
{
    public partial class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService = new();

        private readonly FileService _fileService = new();

        private ObservableCollection<WeatherData> _weatherItems = new();
        public ObservableCollection<WeatherData> WeatherItems
        {
            get => _weatherItems;
            set { _weatherItems = value; OnPropertyChanged(); }
        }

        public ICommand LoadWeatherCommand { get; }

        public List<int> EntriesPerFileOptions { get; } = new() { 10,20,30,40,50,60,70,80,90,100 };

        private int _entriesPerFile;
        public int EntriesPerFile
        {
            get => _entriesPerFile;
            set 
            { _entriesPerFile = value; OnPropertyChanged(); }
        }

        private string _status = "Status: nothing loaded yet";
        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        private string _statusColor = "Orange";

        public string StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
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
            _entriesPerFile = EntriesPerFileOptions.First();
        }

        private async Task LoadWeatherAsync()
        {
            Status = "Started loading weather data...";
            StatusColor = "LightGray";
            WeatherItems.Clear();

            var start = new DateTime(2022, 1, 1);
            var end = new DateTime(2022, 1, 5);

            try
            {
                var data = await _weatherService.GetWeatherDataRangeParallelAsync(StartDate, EndDate);

                WeatherItems = new ObservableCollection<WeatherData>(data);

                if (data.Count != 0)
                {
                    (var stats, var weatherData) = await Task.Run(() =>
                    {
                        var avgTemp = data.AsParallel().Average(w => w.Temperature);
                        var maxTemp = data.AsParallel().Max(w => w.Temperature);
                        var minTemp = data.AsParallel().Min(w => w.Temperature);
                        var avgWind = data.AsParallel().Average(w => w.WindSpeed);
                        var avgPrecip = data.AsParallel().Average(w => w.Precipitation);
                        var avgCloud = data.AsParallel().Average(w => w.CloudCover);
                        var avgHumid = data.AsParallel().Average(w => w.Humidity);
                        var avgPress = data.AsParallel().Average(w => w.Pressure);

                        var collection = new ObservableCollection<WeatherData>(data);

                        return (new { avgTemp, maxTemp, minTemp, avgWind, avgPrecip, avgCloud, avgHumid, avgPress }, collection);
                    });

                    AverageTemperature = stats.avgTemp;
                    MaxTemperature = stats.maxTemp;
                    MinTemperature = stats.minTemp;
                    AverageWindSpeed = stats.avgWind;
                    AveragePrecipitation = stats.avgPrecip;
                    AverageCloudCover = stats.avgCloud;
                    AverageHumidity = stats.avgHumid;
                    AveragePressure = stats.avgPress;

                    WeatherItems = weatherData;
                }

                HasChartData = WeatherItems.Count > 0;
                Status = $"{WeatherItems.Count} data points loaded";
                StatusColor = "Lime";
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task PickFolder(CancellationToken cancellationToken)
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            if (result.IsSuccessful)
            {
                _fileService.ExportWeatherDataAsync(WeatherItems.ToList(), result.Folder.Path, _entriesPerFile);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
