using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace BaseLibrary;

public class MenssageAzureTrackTrace : IEmailSender
{
    //private string azureKey;
    //private IHTTPServices? httpServices;
    private TelemetryClient telemetryClient;
    public MenssageAzureTrackTrace(string azureKey, IHTTPServices? httpServices = null)
    {
        //this.azureKey = azureKey ?? throw new ArgumentNullException(nameof(azureKey));
        //this.httpServices = httpServices ?? Locator.ConstanteContainer.Resolve<IHTTPServices>()
        //    ?? throw new ArgumentNullException(nameof(httpServices));

        //A recomendação da microsoft é uma nova instância e não reutilizar a mesma, mas isso pode ser um problema para o desempenho, então estou usando a mesma instância, mas criando uma nova configuração para cada instância, para evitar problemas de concorrência.
        var config = new TelemetryConfiguration
        {
            ConnectionString = azureKey
        };
        this.telemetryClient = new TelemetryClient(config);
        //var config = TelemetryConfiguration.CreateDefault();
        //config.ConnectionString = azureKey;
        //this.telemetryClient = new TelemetryClient(config);
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
