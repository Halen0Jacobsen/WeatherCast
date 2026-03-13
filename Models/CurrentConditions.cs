namespace WeatherCast.Models;

public class CurrentConditions
{
    public int Temp { get; set; }
    public int FeelsLike { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public int High { get; set; }
    public int Low { get; set; }
    public int Humidity { get; set; }
    public int Wind { get; set; }
    public int PrecipProbability { get; set; }
    public int UvIndex { get; set; }
    public string UvLabel { get; set; } = string.Empty;
}
