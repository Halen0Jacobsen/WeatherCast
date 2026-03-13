using Microsoft.AspNetCore.Mvc;
using WeatherCast.Services;

namespace WeatherCast.Controllers;

public class HomeController : Controller
{
    private readonly IWeatherService _weather;

    public HomeController(IWeatherService weather)
    {
        _weather = weather;
    }

    public IActionResult Index()
    {
        var mockData = _weather.GetMockData();
        return View(mockData);
    }
}
