using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Splat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;

namespace BaseLibrary;

public class MenssageAzureTrackTrace : IEmailSender
{
    private string azureKey;
    private IHTTPServices? httpServices;
    private TelemetryClient telemetryClient;
    public MenssageAzureTrackTrace(string azureKey, IHTTPServices? httpServices = null)
    {
        this.azureKey = azureKey ?? throw new ArgumentNullException(nameof(azureKey));
        this.httpServices = httpServices ?? Locator.Current.GetService<IHTTPServices>()
            ?? throw new ArgumentNullException(nameof(httpServices));

        var config = TelemetryConfiguration.CreateDefault();
        config.ConnectionString = azureKey;
        this.telemetryClient = new TelemetryClient(config);
    }
    public async Task<GOSResult> SendMessage(string emailTo, string subject, string message, bool isAsync)
    {
        //if (!httpServices.IsConnectedToInternet())
        //{
        //    return new GOSResult(false);
        //}

        //if (isAsync)
        //{
        //    await Task.Run(() => Method());
        //}
        //else
        //{
        //    Method();
        //}

        Method();

        void Method()
        {

            telemetryClient.TrackTrace(message, SeverityLevel.Information, new Dictionary<string, string>
            {
                { "subject", subject },
                { "local time", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fffffff") },
                
            });
        }
        

        return new GOSResult(true);
    }
}
