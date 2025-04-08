using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Maui.Storage;
using CommunityToolkit.Mvvm.Input;
using WeatherDataApp.Models;
using WeatherDataApp.Services;

namespace WeatherDataApp.ViewModels
{
    public partial class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly WeatherService _weatherService = new();

        private readonly FileService _fileService = new();

        private ObservableCollection<WeatherData> _weatherItems = [];
        public ObservableCollection<WeatherData> WeatherItems
        {
            get => _weatherItems;
            set { _weatherItems = value; OnPropertyChanged(); }
        }

        public ICommand LoadWeatherCommand { get; }

        public List<int> EntriesPerFileOptions { get; } = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100];

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
            HasChartData = false;

            try
            {
                var allData = await _weatherService.GetWeatherDataAsync(
                StartDate.ToString("yyyy-MM-dd"),
                EndDate.ToString("yyyy-MM-dd"));

                WeatherItems = new ObservableCollection<WeatherData>(allData);

                if (allData.Count != 0)
                {
                    var stats = await Task.Run(() =>
                    {
                        var parallelData = allData.AsParallel();

                        return new
                        {
                            avgTemp = parallelData.Average(w => w.Temperature),
                            maxTemp = parallelData.Max(w => w.Temperature),
                            minTemp = parallelData.Min(w => w.Temperature),
                            avgWind = parallelData.Average(w => w.WindSpeed),
                            avgPrecip = parallelData.Average(w => w.Precipitation),
                            avgCloud = parallelData.Average(w => w.CloudCover),
                            avgHumid = parallelData.Average(w => w.Humidity),
                            avgPress = parallelData.Average(w => w.Pressure),
                        };
                    });

                    AverageTemperature = stats.avgTemp;
                    MaxTemperature = stats.maxTemp;
                    MinTemperature = stats.minTemp;
                    AverageWindSpeed = stats.avgWind;
                    AveragePrecipitation = stats.avgPrecip;
                    AverageCloudCover = stats.avgCloud;
                    AverageHumidity = stats.avgHumid;
                    AveragePressure = stats.avgPress;
                }

                HasChartData = WeatherItems.Count > 0;
                Status = $"{WeatherItems.Count} data points loaded";
                StatusColor = "Lime";
            }
            catch (Exception ex)
            {
                Status = $"Error: {ex.Message}";
                StatusColor = "Red";
            }
        }

        [RelayCommand]
        public async Task PickFolder(CancellationToken cancellationToken)
        {
            var result = await FolderPicker.Default.PickAsync(cancellationToken);
            if (result.IsSuccessful)
            {
                await _fileService.ExportWeatherDataAsync(
                    [.. WeatherItems],
                    result.Folder.Path,
                    _entriesPerFile);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
