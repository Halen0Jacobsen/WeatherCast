using Microsoft.AspNetCore.Mvc;
using WeatherCast.Services;

namespace WeatherCast.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IWeatherService _weather;

    public WeatherController(IWeatherService weather)
    {
        _weather = weather;
    }

    [HttpGet("forecast")]
    public async Task<IActionResult> GetForecast([FromQuery] string location, [FromQuery] string units = "imperial")
    {
        if (string.IsNullOrWhiteSpace(location))
            return BadRequest(new { error = "Location is required." });

        try
        {
            var data = await _weather.GetWeatherAsync(location, units);
            return Ok(data);
        }
        catch (Exception ex)
        {
            return StatusCode(502, new { error = ex.Message });
        }
    }

    [HttpGet("demo")]
    public IActionResult GetDemo()
    {
        return Ok(_weather.GetMockData());
    }
}
