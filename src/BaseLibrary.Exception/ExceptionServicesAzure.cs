using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        this.exceptionDetails = exceptionDetails ?? Locator.Current.GetService<IExceptionDetailsServices>()
            ?? throw new ArgumentNullException(nameof(exceptionDetails));
        this.httpServices = httpServices ?? Locator.Current.GetService<IHTTPServices>()
            ?? throw new ArgumentNullException(nameof(httpServices));

        var config = TelemetryConfiguration.CreateDefault();
        config.ConnectionString = azureKey;
        this.telemetryClient = new TelemetryClient(config);
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
