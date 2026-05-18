using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using BaseLibrary.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;

namespace BaseLibrary;

public class ExceptionServicesAzure : IExceptionServices, IDisposable
{
    private readonly string[] assembliesName;
    private string? folder, azureKey;
    private IHTTPServices? httpServices;
    private IExceptionDetailsServices? exceptionDetails;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger logger;

    public ExceptionServicesAzure(string azurKey, string[] assembliesName, IExceptionDetailsServices? exceptionDetails = null, IHTTPServices? httpServices = null)
    {
        this.azureKey = azurKey ?? throw new ArgumentNullException(nameof(azurKey));
        this.assembliesName = assembliesName ?? throw new ArgumentNullException(nameof(assembliesName));
        this.exceptionDetails = exceptionDetails ?? Locator.ConstantContainer.Resolve<IExceptionDetailsServices>()
            ?? throw new ArgumentNullException(nameof(exceptionDetails));
        this.httpServices = httpServices ?? Locator.ConstantContainer.Resolve<IHTTPServices>()
            ?? throw new ArgumentNullException(nameof(httpServices));

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

            using (logger.BeginScope(new Dictionary<string, object?>
            {
                ["os"] = OSversion,
                ["main assembly"] = mainAssemblyName,
                ["main assembly Version"] = mainAssemblyVersion?.ToString() ?? "Unknown",
                ["local time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff"),
                ["program name"] = appName,
                ["program version"] = version.ToString(),
                ["extra message"] = messageExtra,
            }))
            {
                logger.LogError(e, "{ExceptionMessage}", e.Message);
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

    public void SetFolder(string folder)
    {
        if (this.folder == folder)
            return;
        this.folder = folder ?? throw new ArgumentNullException(nameof(folder));
    }

    public async Task VerifyLocalException(bool isAsync)
    {

    }

    public void Dispose() => loggerFactory.Dispose();
}
