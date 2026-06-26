using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using BaseLibrary.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace BaseLibrary;

public class ExceptionServicesAzure : IExceptionServices, IDisposable
{
    private readonly string[] assembliesName;
    private string? azureKey;
    private IHTTPServices? httpServices;
    private IExceptionDetailsServices? exceptionDetails;
    private readonly ITelemetryScrubber scrubber;
    private readonly ITelemetryGate gate;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger logger;

    /// <inheritdoc/>
    public bool TelemetryEnabled { get; set; } = true;

    /// <remarks>
    /// Telemetria compatibilizada com Native AOT: usa OpenTelemetry + Azure Monitor
    /// Exporter (substitui o pacote depreciado Microsoft.ApplicationInsights).
    /// </remarks>
    public ExceptionServicesAzure(string azurKey, string[] assembliesName, IExceptionDetailsServices? exceptionDetails = null, IHTTPServices? httpServices = null, ITelemetryScrubber? scrubber = null, ITelemetryGate? gate = null)
    {
        this.azureKey = azurKey ?? throw new ArgumentNullException(nameof(azurKey));
        this.assembliesName = assembliesName ?? throw new ArgumentNullException(nameof(assembliesName));
        this.exceptionDetails = exceptionDetails ?? Locator.ConstantContainer.Resolve<IExceptionDetailsServices>()
            ?? throw new ArgumentNullException(nameof(exceptionDetails));
        this.httpServices = httpServices ?? Locator.ConstantContainer.Resolve<IHTTPServices>()
            ?? throw new ArgumentNullException(nameof(httpServices));
        this.scrubber = scrubber ?? new TelemetryScrubber();
        this.gate = gate ?? new TelemetryGate();

        loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                options.AddAzureMonitorLogExporter(exporter => exporter.ConnectionString = azureKey);
            });
        });

        logger = loggerFactory.CreateLogger<ExceptionServicesAzure>();
    }

    public string GetExceptionText(Exception e, string? messageExtra)
    {
        var program = Assembly.GetEntryAssembly()?.GetName();
        string? appName = program?.Name;
        Version? ver = program?.Version;
        string message = "[" + DateTime.Now + "]" + Environment.NewLine + "Program: " + appName + Environment.NewLine +
            "Version: " + ver?.ToString() + Environment.NewLine +
            exceptionDetails!.ToDetailedString(e) +
            (!string.IsNullOrWhiteSpace(messageExtra) ? Environment.NewLine + Environment.NewLine + messageExtra : "");
        return message;
    }

    public bool IsConnectedToInternet() => httpServices?.IsConnectedToInternet() ?? false;

    public async Task SendException(Exception e, bool isAsync, string? messageExtra, string? OSversion)
    {
        // Gate de consentimento: opt-out do app (Settings/CLI) + supressão universal
        // (opt-out por env var / região China). Sem consentimento, nada é exportado.
        if (!TelemetryEnabled || !gate.IsExportAllowed())
            return;

        if (isAsync)
        {
            await Task.Run(() => Send());
        }
        else
        {
            Send();
        }

        void Send()
        {
            var mainAssembly = Assembly.GetEntryAssembly()?.GetName();
            string mainAssemblyName = mainAssembly?.Name ?? "unknowApp";
            Version? mainAssemblyVersion = mainAssembly?.Version;

            AssemblyName? programName = ResolveAssemblyName() ?? mainAssembly;
            Version version = programName?.Version ?? new Version();
            string appName = programName?.Name ?? "unknowApp";

            // Redige dados pessoais incidentais (nome da conta do SO embutido em caminhos)
            // ANTES de exportar. Exportamos a string já redigida em vez do objeto Exception
            // cru: o exporter extrairia StackTrace/Message direto da exceção, contornando a redação.
            string detail = scrubber.Scrub(GetExceptionText(e, messageExtra)) ?? string.Empty;

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["os"] = OSversion,
                ["main assembly"] = mainAssemblyName,
                ["main assembly Version"] = mainAssemblyVersion?.ToString() ?? "Unknown",
                ["local time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff"),
                ["program name"] = appName,
                ["program version"] = version.ToString(),
                ["exception type"] = e.GetType().FullName,
                ["extra message"] = scrubber.Scrub(messageExtra),
            }))
            {
                logger.LogError("{ExceptionDetail}", detail);
            }
        }
    }

    private AssemblyName? ResolveAssemblyName()
    {
        foreach (var item in assembliesName)
        {
            Assembly? loaded = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => string.Equals(a.GetName().Name, item, StringComparison.OrdinalIgnoreCase));
            if (loaded is not null)
                return loaded.GetName();
        }
        return null;
    }

    public void Dispose() => loggerFactory.Dispose();
}
