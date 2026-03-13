namespace WeatherCast.Models;

public class ProviderSource
{
    public string Status { get; set; } = string.Empty;
    public int Temp { get; set; }
    public int FeelsLike { get; set; }
    public int UvIndex { get; set; }
    public int PrecipProbability { get; set; }
    public string Condition { get; set; } = string.Empty;
}

public class WeatherSources
{
    public ProviderSource Google { get; set; } = new();
    public ProviderSource OpenWeather { get; set; } = new();
}
