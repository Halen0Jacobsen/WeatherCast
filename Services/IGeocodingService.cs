using WeatherCast.Models;

namespace WeatherCast.Services;

public interface IGeocodingService
{
    Task<GeoLocation> GeocodeAsync(string query);
}
