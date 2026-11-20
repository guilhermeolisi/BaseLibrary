using System.Globalization;
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
    public MenssageAzureTrackTrace(string azureKey, IHTTPServices? httpServices = null,
        Action<AzureMonitorExporterOptions>? configureExporter = null)
    {
        if (string.IsNullOrWhiteSpace(azureKey))
            throw new ArgumentException("Connection string cannot be null or empty.", nameof(azureKey));

        // E189: recurso sem nome da maquina e IP do cliente mascarado (AzureMonitorTelemetry).
        loggerFactory = AzureMonitorTelemetry.CreateLoggerFactory(azureKey, configureExporter: configureExporter);

        logger = loggerFactory.CreateLogger<MenssageAzureTrackTrace>();
    }

    public Task<GOSResult> SendMessage(string emailTo, string subject, string message, bool isAsync)
    {
        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["subject"] = subject,
            ["local time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff", CultureInfo.InvariantCulture),
        }))
        {
            logger.LogInformation("{TraceMessage}", message);
        }

        return Task.FromResult(new GOSResult(true));
    }

    public void Dispose() => loggerFactory.Dispose();
}
