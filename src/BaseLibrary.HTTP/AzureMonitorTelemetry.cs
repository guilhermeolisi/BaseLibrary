using System.Reflection;
using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;

namespace BaseLibrary;

/// <summary>
/// Monta o <see cref="ILoggerFactory"/> dos servicos que exportam para o Application Insights (excecoes e feedback)
/// sem dado da maquina: o recurso do OpenTelemetry leva so o nome do programa e uma instancia fixa
/// (<see cref="AnonymousInstance"/>), e cada registro leva o IP do cliente mascarado (<see cref="ClientIpAttribute"/>).
/// </summary>
/// <remarks>
/// E189. Sem recurso, o exportador preenche <c>ai.cloud.roleInstance</c> com o nome da maquina; sem o IP do cliente
/// no registro, o servidor usa o IP de origem da conexao para deduzir cidade e estado. Os dois sairiam de
/// instalacoes de terceiros.
/// </remarks>
public static class AzureMonitorTelemetry
{
    /// <summary>Valor fixo de <c>service.instance.id</c>: nenhum identificador por maquina ou por instalacao.</summary>
    public const string AnonymousInstance = "anonymous";

    /// <summary>Atributo de registro que o exportador leva para <c>ai.location.ip</c>.</summary>
    public const string ClientIpAttribute = "microsoft.client.ip";

    /// <summary>IP que o servidor recebe no lugar do IP de origem: nao geolocaliza.</summary>
    public const string MaskedClientIp = "0.0.0.0";

    /// <summary>
    /// Cria a fabrica de loggers com o exportador do Azure Monitor.
    /// </summary>
    /// <param name="connectionString">Connection string do recurso do Application Insights.</param>
    /// <param name="serviceName">Nome do programa; nulo usa o assembly de entrada.</param>
    /// <param name="configureExporter">Ajuste extra do exportador (transporte de teste, armazenamento offline).</param>
    public static ILoggerFactory CreateLoggerFactory(string connectionString, string? serviceName = null,
        Action<AzureMonitorExporterOptions>? configureExporter = null)
    {
        string name = string.IsNullOrWhiteSpace(serviceName)
            ? Assembly.GetEntryAssembly()?.GetName().Name ?? "unknown"
            : serviceName;

        ResourceBuilder resource = ResourceBuilder.CreateEmpty()
            .AddService(name, serviceInstanceId: AnonymousInstance, autoGenerateServiceInstanceId: false);

        return LoggerFactory.Create(builder =>
        {
            builder.AddOpenTelemetry(options =>
            {
                options.SetResourceBuilder(resource);
                options.IncludeFormattedMessage = true;
                options.IncludeScopes = true;
                // antes do exportador: o processador roda primeiro e o atributo chega ao lote
                options.AddProcessor(new MaskedClientIpProcessor());
                options.AddAzureMonitorLogExporter(exporter =>
                {
                    exporter.ConnectionString = connectionString;
                    configureExporter?.Invoke(exporter);
                });
            });
        });
    }

    /// <summary>
    /// Poe <see cref="ClientIpAttribute"/> = <see cref="MaskedClientIp"/> nos atributos de cada registro. O exportador so
    /// leva o atributo para <c>ai.location.ip</c> quando ele esta nos atributos do registro; no escopo vira propriedade
    /// comum e o servidor geolocaliza pelo IP de origem (medido no corpo exportado, E189).
    /// </summary>
    internal sealed class MaskedClientIpProcessor : BaseProcessor<LogRecord>
    {
        public override void OnEnd(LogRecord data)
        {
            IReadOnlyList<KeyValuePair<string, object?>>? current = data.Attributes;
            var attributes = new List<KeyValuePair<string, object?>>((current?.Count ?? 0) + 1);
            if (current is not null)
            {
                foreach (KeyValuePair<string, object?> attribute in current)
                {
                    if (attribute.Key != ClientIpAttribute)
                        attributes.Add(attribute);
                }
            }
            attributes.Add(new KeyValuePair<string, object?>(ClientIpAttribute, MaskedClientIp));
            data.Attributes = attributes;
        }
    }
}
