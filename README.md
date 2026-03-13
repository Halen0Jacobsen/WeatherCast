# WeatherCast

A C# ASP.NET Core weather forecasting dashboard converted from the original HTML Pastel Weather Dashboard.

## Features

- **Live weather** from OpenWeather One Call API 3.0
- **Geocoding** via OpenStreetMap Nominatim (no API key required)
- **7-day forecast**, **hourly forecast**, **active alerts**, and **provider comparison**
- **Demo mode** with realistic mock data (no API keys needed)
- Imperial / Metric unit toggle
- Pastel dashboard UI preserved from the original HTML design

## Project Structure

```
WeatherCast/
├── Controllers/
│   ├── HomeController.cs       # Serves the dashboard view
│   └── WeatherController.cs    # REST API: /api/weather/forecast, /api/weather/demo
├── Models/
│   ├── WeatherData.cs
│   ├── CurrentConditions.cs
│   ├── HourlyForecast.cs
│   ├── DailyForecast.cs
│   ├── WeatherAlert.cs
│   ├── ProviderSource.cs
│   ├── GeoLocation.cs
│   └── WeatherRequest.cs
├── Services/
│   ├── IWeatherService.cs
│   ├── WeatherService.cs       # Fetches & merges weather data
│   ├── IGeocodingService.cs
│   └── GeocodingService.cs     # Geocodes city names via Nominatim
├── Views/
│   ├── Home/Index.cshtml       # Main dashboard (Razor)
│   └── Shared/_Layout.cshtml
├── appsettings.json
└── WeatherCast.csproj
```

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Configuration

Add your API keys to `appsettings.json`:

```json
{
  "WeatherApi": {
    "GoogleApiKey": "YOUR_GOOGLE_WEATHER_API_KEY",
    "OpenWeatherApiKey": "YOUR_OPENWEATHER_API_KEY"
  }
}
```

> The app works without API keys — it will use demo data automatically.

### Run

```bash
dotnet run
```

Open `https://localhost:5001` in your browser.

## API Endpoints

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/weather/forecast?location=Denver,CO&units=imperial` | Live forecast |
| GET | `/api/weather/demo` | Mock/demo data |
