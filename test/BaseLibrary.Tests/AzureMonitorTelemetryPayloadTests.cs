using System.IO.Compression;
using System.Net;
using System.Text;
using System.Text.Json;
using Azure.Core.Pipeline;
using BaseLibrary;
using FluentAssertions;
using Moq;

namespace BaseLibrary.Tests;

/// <summary>
/// Teste de interacao (E189): a excecao passa pelo <see cref="ExceptionServicesAzure"/> real e pelo exportador do
/// Azure Monitor real; so o transporte HTTP e falso, e captura o corpo que iria ao Application Insights.
/// </summary>
public sealed class AzureMonitorTelemetryPayloadTests
{
    private sealed class CaptureHandler : HttpMessageHandler
    {
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Content is not null)
            {
                byte[] raw = await request.Content.ReadAsByteArrayAsync(cancellationToken);
                Bodies.Add(Decode(raw));
            }
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{\"itemsReceived\":1,\"itemsAccepted\":1,\"errors\":[]}", Encoding.UTF8, "application/json"),
            };
        }

        private static string Decode(byte[] raw)
        {
            if (raw.Length > 2 && raw[0] == 0x1f && raw[1] == 0x8b)
            {
                using var gz = new GZipStream(new MemoryStream(raw), CompressionMode.Decompress);
                using var reader = new StreamReader(gz, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            return Encoding.UTF8.GetString(raw);
        }
    }

    private static (string Body, JsonElement Item) SendThroughExporter(Exception exception, string messageExtra)
    {
        Environment.SetEnvironmentVariable("APPLICATIONINSIGHTS_STATSBEAT_DISABLED", "true");
        var handler = new CaptureHandler();
        var gate = new Mock<ITelemetryGate>();
        gate.Setup(g => g.IsExportAllowed()).Returns(true);
        var http = new Mock<IHTTPServices>();

        // chave propria por chamada: o exportador reaproveita o transmissor de uma connection string ja vista
        string connectionString = $"InstrumentationKey={Guid.NewGuid()};IngestionEndpoint=https://ingest.invalid/";
        using (var sut = new ExceptionServicesAzure(connectionString, ["BaseLibrary.Tests"], new ExceptionDetailsServices(),
                   http.Object, scrubber: null, gate: gate.Object,
                   configureExporter: o =>
                   {
                       o.Transport = new HttpClientTransport(new HttpClient(handler));
                       o.DisableOfflineStorage = true;
                   }))
        {
            sut.SendException(exception, false, messageExtra, "Windows 11").GetAwaiter().GetResult();
        } // Dispose descarrega o lote no transporte

        string body = string.Join("\n", handler.Bodies);
        string line = body.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .First(l => l.Contains("\"ExceptionDetail\"", StringComparison.Ordinal));
        return (body, JsonDocument.Parse(line).RootElement.Clone());
    }

    private static Exception Thrown(string message)
    {
        try { throw new IOException(message); }
        catch (IOException e) { return e; }
    }

    [Fact]
    public void SendException_ShouldExportAnonymousInstanceAndMaskedClientIp_InsteadOfMachineData()
    {
        // Arrange
        var exception = Thrown(@"Could not find file 'C:\Users\jdoe\Documents\Library\Quartz study\Sample 7\scan-07.xye'.");

        // Act
        (string body, JsonElement item) = SendThroughExporter(exception, "extra");

        // Assert
        JsonElement tags = item.GetProperty("tags");
        tags.GetProperty("ai.cloud.roleInstance").GetString().Should().Be(AzureMonitorTelemetry.AnonymousInstance, body);
        tags.GetProperty("ai.cloud.role").GetString().Should().Be(
            System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name, body);
        tags.GetProperty("ai.location.ip").GetString().Should().Be(AzureMonitorTelemetry.MaskedClientIp, body);
    }

    [Fact]
    public void SendException_ShouldNotExportMachineUserOrUserFileNames_WhenMessageCarriesThem()
    {
        // Arrange: o nome real da maquina e do usuario do ambiente, e nomes de fixture de projeto, work, arquivo e e-mail
        string machine = Environment.MachineName;
        string user = Environment.UserName;
        var exception = Thrown(
            $@"Connection to {machine} refused for {user}; reply to jdoe@example.org. Could not find file 'C:\Users\jdoe\Documents\Library\Quartz study\Sample 7\scan-07.xye'.");

        // Act
        (string body, JsonElement item) = SendThroughExporter(exception, $@"Opened D:\Lab data\Quartz study\Sample 7\Workfile.sin on {machine}");

        // Assert
        if (machine.Length >= 3)
            body.Should().NotContainEquivalentOf(machine);
        if (user.Length >= 3)
            body.Should().NotContainEquivalentOf(user);
        body.Should().NotContain("Quartz study").And.NotContain("Sample 7").And.NotContain("scan-07")
            .And.NotContain("jdoe@example.org").And.NotContain("Lab data");
        string? detail = item.GetProperty("data").GetProperty("baseData").GetProperty("properties")
            .GetProperty("ExceptionDetail").GetString();
        detail.Should().Contain(@"'C:\Users\<redacted>\<dir>\Library\...\<file>.xye'", "o caminho chega reduzido, com a pasta do app e a extensao")
            .And.Contain(@"D:\...\Workfile.sin", "o Workfile.sin e nome fixo do app");
    }
}
