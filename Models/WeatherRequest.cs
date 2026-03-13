namespace WeatherCast.Models;

public class WeatherRequest
{
    public string Location { get; set; } = string.Empty;
    public string Units { get; set; } = "imperial";
}
