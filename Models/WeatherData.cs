namespace WeatherCast.Models;

public class WeatherData
{
    public string LocationName { get; set; } = string.Empty;
    public string UpdatedAt { get; set; } = string.Empty;
    public string TempUnit { get; set; } = "°F";
    public string SpeedUnit { get; set; } = "mph";
    public CurrentConditions Current { get; set; } = new();
    public List<HourlyForecast> Hourly { get; set; } = [];
    public List<DailyForecast> Daily { get; set; } = [];
    public List<WeatherAlert> Alerts { get; set; } = [];
    public WeatherSources Sources { get; set; } = new();
}
