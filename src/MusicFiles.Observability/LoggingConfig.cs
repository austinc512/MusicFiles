using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.File;
using System;

namespace MusicFiles.Observability;

public static class LoggingConfig
{
    public static void ConfigureLogging(this ILoggingBuilder loggingBuilder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()  // Capture detailed exception info
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {NewLine}{Exception}")
            .WriteTo.File("Logs/musicfiles.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
                fileSizeLimitBytes: 10_000_000,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();

        loggingBuilder.ClearProviders();
        loggingBuilder.AddSerilog();
    }
}
