using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace BaseLibrary;

public sealed class MenssageAzureTrackTrace : IEmailSender, IDisposable
{
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger logger;

    /// <remarks>
    /// Telemetria compatibilizada com Native AOT: usa OpenTelemetry + Azure Monitor
    /// Exporter (substitui o pacote depreciado Microsoft.ApplicationInsights).
    /// </remarks>
    public MenssageAzureTrackTrace(string azureKey, IHTTPServices? httpServices = null)
    {
        if (string.IsNullOrWhiteSpace(azureKey))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(azureKey));

        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddAzureMonitorLogExporter(exporter => exporter.ConnectionString = azureKey);
            });
        });

        logger = loggerFactory.CreateLogger<MenssageAzureTrackTrace>();
    }

    public Task<GOSResult> SendMessage(string emailTo, string subject, string message, bool isAsync)
    {
        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["subject"] = subject,
            ["local time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff"),
        }))
        {
            logger.LogInformation("{TraceMessage}", message);
        }

        return Task.FromResult(new GOSResult(true));
    }

    public void Dispose() => loggerFactory.Dispose();
}
