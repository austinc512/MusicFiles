using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;

namespace MusicFiles.Observability
{
    public static class MetricsConfig
    {
        public static void ConfigureMetrics(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .WithMetrics(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation() // Capture HTTP metrics
                        .AddRuntimeInstrumentation() // Capture process and runtime metrics
                        .AddConsoleExporter(); // Export metrics to the console
                });
        }
    }
}