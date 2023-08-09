using InfluxMetricsApp.Requests;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace InfluxMetricsApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpPost("request-size")]
        public IActionResult GetRequestResponseSize(TestRequest request)
        {
            var ty = JsonSerializer.SerializeToUtf8Bytes(request, new JsonSerializerOptions 
            {
                WriteIndented = true
            });

            return Ok();
        }

        private static bool cStart = true;

        [HttpGet("retro")]
        public async IAsyncEnumerable<string> GetRetro()
        {
            int j = 0;
            while (cStart)
            {
                j++;
                string[] echo = new string[j];

                for (int i = 0; i < echo.Length; i++)
                {
                    echo[i] = "LONGSTRING_TEST_OVER";
                }

                yield return await ValueTask.FromResult(string.Join(", ", echo));
            }
        }

        [HttpGet("stop")]
        public string StopRetro()
        {
            cStart = false;

            return "PK";
        }
    }
}