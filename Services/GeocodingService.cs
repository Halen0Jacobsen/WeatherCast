using System.Text.Json;
using WeatherCast.Models;

namespace WeatherCast.Services;

public class GeocodingService : IGeocodingService
{
    private readonly HttpClient _http;

    public GeocodingService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient();
    }

    public async Task<GeoLocation> GeocodeAsync(string query)
    {
        var encoded = Uri.EscapeDataString(query);
        var url = $"https://nominatim.openstreetmap.org/search?q={encoded}&format=json&limit=1";

        var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.Add("User-Agent", "WeatherCast/1.0");

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        if (root.GetArrayLength() == 0)
            throw new InvalidOperationException($"Could not geocode location: {query}");

        var result = root[0];
        var lat = double.Parse(result.GetProperty("lat").GetString()!);
        var lng = double.Parse(result.GetProperty("lon").GetString()!);
        var displayName = result.GetProperty("display_name").GetString()!;

        // Shorten display name to city, state/country
        var parts = displayName.Split(',');
        var shortName = parts.Length >= 2
            ? $"{parts[0].Trim()}, {parts[1].Trim()}"
            : parts[0].Trim();

        return new GeoLocation { Lat = lat, Lng = lng, DisplayName = shortName };
    }
}
