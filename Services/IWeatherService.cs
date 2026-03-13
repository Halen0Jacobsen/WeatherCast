using WeatherCast.Models;

namespace WeatherCast.Services;

public interface IWeatherService
{
    Task<WeatherData> GetWeatherAsync(string location, string units);
    WeatherData GetMockData();
}
