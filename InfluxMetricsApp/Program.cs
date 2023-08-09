using InfluxDB.Client;
using InfluxMetricsApp.Controllers;
using InfluxMetricsApp.Filters;
using InfluxMetricsApp.Metrics;
using Newtonsoft.Json.Linq;

namespace InfluxMetricsApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
           // var m = new RuntimeEventListener();
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options => 
            {
                options.Filters.Add<RequestLoggerActionFilter>();
            });

            builder.Services.AddScoped(_ => 
            {
                string token = "8CLJNig-9qYdL2494chuAh8krpaviNLohTSP8BMqbNb5hBaPhoR7hyuztlZfieIK4JEUzGgMRx29HbZGBglu3w==";
                var client = new InfluxDBClient("http://localhost:8086", token);
                return client;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();


            app.UseMiddleware<RequestResponseSizeMiddleware>();

            app.Run();
        }
    }
}