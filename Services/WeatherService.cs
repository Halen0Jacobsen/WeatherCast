using System.Text.Json;
using WeatherCast.Models;

namespace WeatherCast.Services;

public class WeatherService : IWeatherService
{
    private readonly HttpClient _http;
    private readonly IGeocodingService _geocoding;
    private readonly IConfiguration _config;

    public WeatherService(IHttpClientFactory httpClientFactory, IGeocodingService geocoding, IConfiguration config)
    {
        _http = httpClientFactory.CreateClient();
        _geocoding = geocoding;
        _config = config;
    }

    public async Task<WeatherData> GetWeatherAsync(string location, string units)
    {
        var geo = await _geocoding.GeocodeAsync(location);
        var owmKey = _config["WeatherApi:OpenWeatherApiKey"] ?? string.Empty;

        var (tempUnit, speedUnit) = units == "metric" ? ("°C", "m/s") : ("°F", "mph");

        if (string.IsNullOrEmpty(owmKey) || owmKey == "YOUR_OPENWEATHER_API_KEY")
        {
            var mock = GetMockData();
            mock.LocationName = geo.DisplayName;
            mock.TempUnit = tempUnit;
            mock.SpeedUnit = speedUnit;
            return mock;
        }

        var owmData = await FetchOpenWeatherAsync(geo.Lat, geo.Lng, units, owmKey);
        return BuildWeatherData(geo.DisplayName, owmData, tempUnit, speedUnit);
    }

    public WeatherData GetMockData()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return new WeatherData
        {
            LocationName = "Denver, Colorado",
            UpdatedAt = DateTime.Now.ToString("g"),
            TempUnit = "°F",
            SpeedUnit = "mph",
            Current = new CurrentConditions
            {
                Temp = 58,
                FeelsLike = 55,
                Condition = "Partly Cloudy",
                Summary = "Dry through the afternoon with a cooler evening and moderate UV exposure.",
                High = 66,
                Low = 42,
                Humidity = 41,
                Wind = 11,
                PrecipProbability = 22,
                UvIndex = 6,
                UvLabel = "Moderate"
            },
            Hourly =
            [
                new() { Time = "Now",   Icon = "⛅", Temp = 58, Precip = 10, Wind = 11 },
                new() { Time = "1 PM",  Icon = "⛅", Temp = 60, Precip = 12, Wind = 10 },
                new() { Time = "2 PM",  Icon = "☀️", Temp = 63, Precip = 10, Wind = 9 },
                new() { Time = "3 PM",  Icon = "☀️", Temp = 65, Precip = 8,  Wind = 8 },
                new() { Time = "4 PM",  Icon = "🌤️", Temp = 66, Precip = 7,  Wind = 8 },
                new() { Time = "5 PM",  Icon = "🌤️", Temp = 64, Precip = 10, Wind = 9 },
                new() { Time = "6 PM",  Icon = "⛅", Temp = 60, Precip = 15, Wind = 10 },
                new() { Time = "7 PM",  Icon = "🌥️", Temp = 56, Precip = 18, Wind = 11 }
            ],
            Daily =
            [
                new() { Day = "Mon", Icon = "⛅", High = 66, Low = 42, Description = "Partly cloudy" },
                new() { Day = "Tue", Icon = "🌦️", High = 61, Low = 40, Description = "Light showers" },
                new() { Day = "Wed", Icon = "☀️", High = 68, Low = 43, Description = "Mostly sunny" },
                new() { Day = "Thu", Icon = "🌤️", High = 70, Low = 45, Description = "Pleasant" },
                new() { Day = "Fri", Icon = "🌬️", High = 59, Low = 36, Description = "Windy" },
                new() { Day = "Sat", Icon = "❄️", High = 48, Low = 28, Description = "Snow possible" },
                new() { Day = "Sun", Icon = "🌤️", High = 55, Low = 31, Description = "Clearing" }
            ],
            Alerts =
            [
                new()
                {
                    Title = "High Wind Advisory",
                    Severity = "Moderate",
                    Description = "Gusts may affect travel and exposed outdoor work during the evening period."
                }
            ],
            Sources = new WeatherSources
            {
                Google = new ProviderSource
                {
                    Status = "Demo Mode",
                    Temp = 57,
                    FeelsLike = 54,
                    UvIndex = 6,
                    PrecipProbability = 20,
                    Condition = "Partly cloudy"
                },
                OpenWeather = new ProviderSource
                {
                    Status = "Demo Mode",
                    Temp = 58,
                    FeelsLike = 55,
                    UvIndex = 7,
                    PrecipProbability = 24,
                    Condition = "Broken clouds"
                }
            }
        };
    }

    private async Task<JsonElement> FetchOpenWeatherAsync(double lat, double lng, string units, string apiKey)
    {
        var url = $"https://api.openweathermap.org/data/3.0/onecall?lat={lat}&lon={lng}&appid={apiKey}&units={units}";
        var response = await _http.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        return JsonDocument.Parse(json).RootElement.Clone();
    }

    private WeatherData BuildWeatherData(string locationName, JsonElement owm, string tempUnit, string speedUnit)
    {
        var current = owm.GetProperty("current");
        var daily = owm.GetProperty("daily");
        var hourly = owm.GetProperty("hourly");
        var daily0 = daily[0];

        var currentTemp = (int)Math.Round(current.GetProperty("temp").GetDouble());
        var feelsLike = (int)Math.Round(current.GetProperty("feels_like").GetDouble());
        var humidity = current.GetProperty("humidity").GetInt32();
        var windSpeed = (int)Math.Round(current.GetProperty("wind_speed").GetDouble());
        var uvIndex = daily0.TryGetProperty("uvi", out var uvi) ? (int)Math.Round(uvi.GetDouble()) : 0;
        var conditionText = current.TryGetProperty("weather", out var cw) && cw.GetArrayLength() > 0
            ? cw[0].GetProperty("main").GetString() ?? "Clear"
            : "Clear";

        var high = (int)Math.Round(daily0.GetProperty("temp").GetProperty("max").GetDouble());
        var low = (int)Math.Round(daily0.GetProperty("temp").GetProperty("min").GetDouble());
        var pop = daily0.TryGetProperty("pop", out var popEl) ? (int)Math.Round(popEl.GetDouble() * 100) : 0;
        var summary = daily0.TryGetProperty("summary", out var sumEl) ? sumEl.GetString() ?? string.Empty : string.Empty;

        var hourlyList = new List<HourlyForecast>();
        int hourCount = Math.Min(8, hourly.GetArrayLength());
        for (int i = 0; i < hourCount; i++)
        {
            var h = hourly[i];
            var dt = DateTimeOffset.FromUnixTimeSeconds(h.GetProperty("dt").GetInt64()).LocalDateTime;
            var hCond = h.TryGetProperty("weather", out var hw) && hw.GetArrayLength() > 0
                ? hw[0].GetProperty("main").GetString() ?? "Clear"
                : "Clear";
            hourlyList.Add(new HourlyForecast
            {
                Time = i == 0 ? "Now" : dt.ToString("h tt"),
                Icon = IconFromCondition(hCond),
                Temp = (int)Math.Round(h.GetProperty("temp").GetDouble()),
                Precip = h.TryGetProperty("pop", out var hp) ? (int)Math.Round(hp.GetDouble() * 100) : 0,
                Wind = h.TryGetProperty("wind_speed", out var hw2) ? (int)Math.Round(hw2.GetDouble()) : 0
            });
        }

        var dailyList = new List<DailyForecast>();
        int dayCount = Math.Min(7, daily.GetArrayLength());
        for (int i = 0; i < dayCount; i++)
        {
            var d = daily[i];
            var dt = DateTimeOffset.FromUnixTimeSeconds(d.GetProperty("dt").GetInt64()).LocalDateTime;
            var dCond = d.TryGetProperty("weather", out var dw) && dw.GetArrayLength() > 0
                ? dw[0].GetProperty("main").GetString() ?? "Clear"
                : "Clear";
            var dDesc = d.TryGetProperty("weather", out var dw2) && dw2.GetArrayLength() > 0
                ? dw2[0].GetProperty("description").GetString() ?? string.Empty
                : string.Empty;
            dailyList.Add(new DailyForecast
            {
                Day = dt.ToString("ddd"),
                Icon = IconFromCondition(dCond),
                High = (int)Math.Round(d.GetProperty("temp").GetProperty("max").GetDouble()),
                Low = (int)Math.Round(d.GetProperty("temp").GetProperty("min").GetDouble()),
                Description = dDesc
            });
        }

        var alerts = new List<WeatherAlert>();
        if (owm.TryGetProperty("alerts", out var alertsEl))
        {
            foreach (var alert in alertsEl.EnumerateArray())
            {
                alerts.Add(new WeatherAlert
                {
                    Title = alert.TryGetProperty("event", out var ev) ? ev.GetString() ?? "Weather Alert" : "Weather Alert",
                    Severity = alert.TryGetProperty("tags", out var tags) && tags.GetArrayLength() > 0
                        ? string.Join(", ", tags.EnumerateArray().Select(t => t.GetString()))
                        : "Advisory",
                    Description = alert.TryGetProperty("description", out var desc) ? desc.GetString() ?? string.Empty : string.Empty
                });
            }
        }

        return new WeatherData
        {
            LocationName = locationName,
            UpdatedAt = DateTime.Now.ToString("g"),
            TempUnit = tempUnit,
            SpeedUnit = speedUnit,
            Current = new CurrentConditions
            {
                Temp = currentTemp,
                FeelsLike = feelsLike,
                Condition = conditionText,
                Summary = summary,
                High = high,
                Low = low,
                Humidity = humidity,
                Wind = windSpeed,
                PrecipProbability = pop,
                UvIndex = uvIndex,
                UvLabel = UvLabel(uvIndex)
            },
            Hourly = hourlyList,
            Daily = dailyList,
            Alerts = alerts,
            Sources = new WeatherSources
            {
                Google = new ProviderSource { Status = "Not configured", Condition = "N/A" },
                OpenWeather = new ProviderSource
                {
                    Status = "Connected",
                    Temp = currentTemp,
                    FeelsLike = feelsLike,
                    UvIndex = uvIndex,
                    PrecipProbability = pop,
                    Condition = conditionText
                }
            }
        };
    }

    private static string IconFromCondition(string condition)
    {
        var c = condition.ToLower();
        if (c.Contains("thunder")) return "⛈️";
        if (c.Contains("snow")) return "❄️";
        if (c.Contains("rain") || c.Contains("drizzle")) return "🌧️";
        if (c.Contains("cloud")) return "⛅";
        if (c.Contains("clear") || c.Contains("sun")) return "☀️";
        if (c.Contains("mist") || c.Contains("fog")) return "🌫️";
        if (c.Contains("wind")) return "🌬️";
        return "🌤️";
    }

    private static string UvLabel(int index) => index switch
    {
        <= 2 => "Low",
        <= 5 => "Moderate",
        <= 7 => "High",
        <= 10 => "Very High",
        _ => "Extreme"
    };
}
