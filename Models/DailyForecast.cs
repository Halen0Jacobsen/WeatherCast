namespace WeatherCast.Models;

public class DailyForecast
{
    public string Day { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int High { get; set; }
    public int Low { get; set; }
    public string Description { get; set; } = string.Empty;
}
