namespace WeatherDataApp.Models
{
    public class WeatherData
    {
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double Precipitation { get; set; }
        public double CloudCover { get; set; }
        public double Humidity { get; set; }
        public double Pressure { get; set; }
    }
}