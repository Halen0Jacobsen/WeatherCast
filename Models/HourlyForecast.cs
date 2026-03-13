namespace WeatherCast.Models;

public class HourlyForecast
{
    public string Time { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int Temp { get; set; }
    public int Precip { get; set; }
    public int Wind { get; set; }
}
