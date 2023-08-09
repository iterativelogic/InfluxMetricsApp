using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace InfluxMetricsApp.Filters
{
    public class RequestLoggerActionFilter : ActionFilterAttribute
    {
        readonly Stopwatch _stopwatch = new Stopwatch();
        private readonly ILogger<RequestLoggerActionFilter> _logger;

        public RequestLoggerActionFilter(ILogger<RequestLoggerActionFilter> logger)
        {
            this._logger = logger;
        }

        public override void OnActionExecuting(ActionExecutingContext context) => _stopwatch.Start();

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            _stopwatch.Stop();

            MinimalEventCounterSource.Log.Request(
                context.HttpContext.Request.GetDisplayUrl(), _stopwatch.ElapsedMilliseconds);

            string requestUrl = context.HttpContext.Request.Path;
            long elapsedTime = _stopwatch.ElapsedMilliseconds;

            MeasureInfluxMetric(requestUrl, elapsedTime);

            _logger.LogInformation($"Request {requestUrl} took {elapsedTime} ms.");
        }

        public void MeasureInfluxMetric(string requestUrl, long elapsedTime)
        {
            string token = "8CLJNig-9qYdL2494chuAh8krpaviNLohTSP8BMqbNb5hBaPhoR7hyuztlZfieIK4JEUzGgMRx29HbZGBglu3w==";
            using var client = new InfluxDBClient("http://localhost:8086", token);
            using var writeApi = client.GetWriteApi();

            var point = PointData.Measurement("request_processing_time")
                    .Tag("url", requestUrl)
                    .Field("processing_time", elapsedTime)
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            writeApi.WritePoint(point, "TestBucket", "Citi");
        }
    }
}
