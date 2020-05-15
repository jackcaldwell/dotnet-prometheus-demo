using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Prometheus;

namespace PrometheusDemo.Middleware
{
    public class MonitoringMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        
        public MonitoringMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _next = next;
            _logger = loggerFactory.CreateLogger<MonitoringMiddleware>();
        }
        
        public async Task Invoke(HttpContext httpContext)
        {
            var path = httpContext.Request.Path.Value;
            var method = httpContext.Request.Method;
        
            var counter = Metrics.CreateCounter("prometheus_demo_request_total", "HTTP Requests Total",
                new CounterConfiguration
                {
                    LabelNames = new [] { "path", "method", "status" }
                });
        
            var statusCode = 200;
        
            try
            {
                await _next.Invoke(httpContext);
            }
            catch (Exception)
            {
                statusCode = 500;
                counter.Labels(path, method, statusCode.ToString()).Inc();
        
                throw;
            }
        
            if (path != "/metrics")
            {
                statusCode = httpContext.Response.StatusCode;
                counter.Labels(path, method, statusCode.ToString()).Inc();
            }
        }
    }
}