namespace WeatherCast.Models;

public class GeoLocation
{
    public double Lat { get; set; }
    public double Lng { get; set; }
    public string DisplayName { get; set; } = string.Empty;
}
