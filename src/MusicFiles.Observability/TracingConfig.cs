using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace MusicFiles.Observability
{
    public static class TracingConfig
    {
        public static void ConfigureTracing(this IServiceCollection services)
        {
            services.AddOpenTelemetry()
                .WithTracing(builder =>
                {
                    builder
                        .AddAspNetCoreInstrumentation() // Capture HTTP requests
                        .AddHttpClientInstrumentation() // Capture outgoing HTTP requests
                        .AddSqlClientInstrumentation() // Capture database calls
                        .AddConsoleExporter(); // Export traces to the console for now
                });
        }
    }
}