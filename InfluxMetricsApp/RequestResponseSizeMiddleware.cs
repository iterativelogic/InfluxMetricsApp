using System.Buffers;
using System.Collections;
using System.Drawing;
using System.IO.Pipelines;
using System.Net.Mime;
using System.Net.Sockets;
using System.Text;

namespace InfluxMetricsApp
{
    public class RequestResponseSizeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseSizeMiddleware> _logger;

        public RequestResponseSizeMiddleware(RequestDelegate next, ILogger<RequestResponseSizeMiddleware> logger)
        {
            _next = next;
            this._logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethod.Post.Method)
            {
                var request = context.Request;
                request.EnableBuffering();
                using MemoryStream memoryStream = new MemoryStream();
                await request.Body.CopyToAsync(memoryStream);
                long requestBytes = memoryStream.Length;
                _logger.LogInformation($"Request size in bytes: {requestBytes} bytes.");
                request.Body.Position = 0;
            }            

            await _next(context);
        }
    }
}
