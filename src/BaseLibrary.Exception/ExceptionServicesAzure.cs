using BaseLibrary.DependencyInjection;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Reflection;

namespace BaseLibrary;

public class ExceptionServicesAzure : IExceptionServices
{
    private readonly string[] assembliesName;
    private string? folder, azureKey;
    private IHTTPServices? httpServices;
    private IExceptionDetailsServices? exceptionDetails;
    TelemetryClient telemetryClient;
    public ExceptionServicesAzure(string azurKey, string[] assembliesName, IExceptionDetailsServices? exceptionDetails = null, IHTTPServices? httpServices = null)
    {
        this.azureKey = azurKey ?? throw new ArgumentNullException(nameof(azurKey));
        this.assembliesName = assembliesName ?? throw new ArgumentNullException(nameof(assembliesName));
        this.exceptionDetails = exceptionDetails ?? Locator.ConstantContainer.Resolve<IExceptionDetailsServices>()
            ?? throw new ArgumentNullException(nameof(exceptionDetails));
        this.httpServices = httpServices ?? Locator.ConstantContainer.Resolve<IHTTPServices>()
            ?? throw new ArgumentNullException(nameof(httpServices));

        //A recomendação da microsoft é uma nova instância e não reutilizar a mesma, mas isso pode ser um problema para o desempenho, então estou usando a mesma instância, mas criando uma nova configuração para cada instância, para evitar problemas de concorrência.
        var config = new TelemetryConfiguration
        {
            ConnectionString = azureKey
        };
        this.telemetryClient = new TelemetryClient(config);

        //try
        //{
        //    var config = TelemetryConfiguration.CreateDefault();
        //    config.ConnectionString = azureKey;
        //    this.telemetryClient = new TelemetryClient(config);
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine("Error initializing Application Insights TelemetryClient: " + ex.Message);
        //    throw;
        //}
    }
    public string GetExceptionText(Exception e, string? messageExtra)
    {
        var program = Assembly.GetEntryAssembly()?.GetName();
        string? appName = program?.Name;
        Version? ver = program?.Version;
        string message = "[" + DateTime.Now + "]" + Environment.NewLine + "Program: " + appName + Environment.NewLine +
            "Version: " + ver?.ToString() + Environment.NewLine +
            exceptionDetails.ToDetailedString(e) +
            (!string.IsNullOrWhiteSpace(messageExtra) ? Environment.NewLine + Environment.NewLine + messageExtra : "");
        return message;
    }

    public bool IsConnectedToInternet() => httpServices?.IsConnectedToInternet() ?? false;

    public async Task SendException(Exception e, bool isAsync, string? messageExtra, string? OSversion)
    {
        if (isAsync)
        {
            await Task.Run(() => send());
        }
        else
        {
            send();
        }
        void send()
        {
            var mainAssembly = Assembly.GetEntryAssembly()?.GetName();
            string? mainAssemblyName = (mainAssembly is null ? null : mainAssembly.Name) ?? "unknowApp";
            Version? mainAssemblyVersion = mainAssembly?.Version;

            Assembly? assembly = null;

            foreach (var item in assembliesName)
            {
                assembly = Assembly.Load(item);
                if (assembly is not null)
                    break;
            }
            if (assembly is null)
            {
                assembly = Assembly.GetEntryAssembly();
            }
            var programName = assembly?.GetName();

            Version version = (programName is null ? null : programName.Version) ?? new Version();
            string appName = (programName is null ? null : programName.Name) ?? "unknowApp";

            telemetryClient.TrackException(e, new Dictionary<string, string>
            {
                { "os", OSversion },
                { "main assembly", mainAssemblyName ?? "Unknown" },
                { "main assembly Version", mainAssemblyVersion?.ToString() ?? "Unknown" },
                { "local time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff") },
                { "program name", appName },
                { "program version", version.ToString() },
                { "extra message", messageExtra },
            });
        }
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
}
