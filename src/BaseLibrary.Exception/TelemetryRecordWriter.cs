using System.Text;
using System.Text.Json;

namespace BaseLibrary;

/// <summary>
/// Escreve um registro baixado do Application Insights como texto "Nome: valor" (o formato dos arquivos da skill
/// <c>/excecoes</c>), ja limpo (E189): os campos que identificam a maquina ou o local (<see cref="RedactedFields"/>)
/// saem <see cref="Redacted"/> em todo registro; os textos do registro de excecao passam pelo scrubber. O registro de
/// feedback do usuario guarda o texto e o e-mail de resposta que ele mesmo escreveu.
/// </summary>
public sealed class TelemetryRecordWriter
{
    public const string Redacted = "<redacted>";

    /// <summary>Campos do registro que trazem o nome da maquina, a cidade, o estado ou o IP.</summary>
    public static readonly IReadOnlySet<string> RedactedFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "AppRoleInstance", "ClientCity", "ClientStateOrProvince", "ClientIP",
    };

    private readonly ITelemetryScrubber scrubber;

    /// <param name="scrubber">Scrubber dos textos do registro de excecao; nulo usa o <see cref="TelemetryExportScrubber"/>.</param>
    public TelemetryRecordWriter(ITelemetryScrubber? scrubber = null)
    {
        this.scrubber = scrubber ?? new TelemetryExportScrubber();
    }

    /// <summary>
    /// Diz se o registro e uma excecao: <c>Type</c> = AppExceptions (legado), severidade Error ou acima (as excecoes
    /// enviadas por <c>LogError</c>) ou a categoria do logger de excecoes.
    /// </summary>
    public static bool IsExceptionRecord(JsonElement root)
    {
        if (root.ValueKind != JsonValueKind.Object)
            return false;

        if (root.TryGetProperty("Type", out var typeEl)
            && typeEl.ValueKind == JsonValueKind.String
            && string.Equals(typeEl.GetString(), "AppExceptions", StringComparison.OrdinalIgnoreCase))
            return true;

        if (root.TryGetProperty("SeverityLevel", out var sevEl)
            && sevEl.ValueKind == JsonValueKind.Number
            && sevEl.TryGetInt32(out int severity)
            && severity >= 3)
            return true;

        if (root.TryGetProperty("Properties", out var props)
            && props.ValueKind == JsonValueKind.Object
            && props.TryGetProperty("CategoryName", out var cat)
            && cat.ValueKind == JsonValueKind.String
            && (cat.GetString()?.Contains("ExceptionServicesAzure", StringComparison.OrdinalIgnoreCase) ?? false))
            return true;

        return false;
    }

    /// <summary>
    /// Devolve o texto do registro.
    /// </summary>
    /// <param name="root">O registro (objeto JSON).</param>
    /// <param name="localTime">O valor de <c>local time</c> (ou <c>Local date hour</c>), se houver: da nome ao arquivo.</param>
    public string Write(JsonElement root, out string? localTime)
    {
        bool isException = IsExceptionRecord(root);
        var writer = new StringBuilder();
        string? time = null;
        WriteElement(root, null, writer, 0, isException, ref time);
        localTime = time;
        return writer.ToString();
    }

    private void WriteElement(JsonElement element, string? name, StringBuilder writer, int level, bool isException, ref string? time)
    {
        string indent = new string(' ', level * 4);

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    writer.Append($"{indent}{property.Name}: ");
                    if ((property.Name == "local time" || property.Name == "Local date hour")
                        && property.Value.ValueKind == JsonValueKind.String)
                        time = property.Value.GetString();

                    if (property.Value.ValueKind == JsonValueKind.Object)
                        writer.AppendLine();
                    WriteElement(property.Value, property.Name, writer, level + 1, isException, ref time);
                }
                break;

            case JsonValueKind.Array:
                int i = 0;
                writer.AppendLine();
                foreach (var item in element.EnumerateArray())
                {
                    writer.AppendLine($"{indent}[{i}]");
                    WriteElement(item, name, writer, level + 1, isException, ref time);
                    i++;
                }
                break;

            case JsonValueKind.String:
                string text = element.GetString() ?? string.Empty;
                if (name is not null && RedactedFields.Contains(name) && text.Length > 0)
                    text = Redacted;
                else if (isException)
                    text = scrubber.Scrub(text) ?? string.Empty;

                if (text.Contains(Environment.NewLine))
                {
                    writer.AppendLine();
                    writer.AppendLine($"{indent}{indent}{text.Replace(Environment.NewLine, Environment.NewLine + indent + indent)}");
                }
                else
                {
                    writer.AppendLine(text);
                }
                break;

            case JsonValueKind.Number:
                writer.AppendLine(name is not null && RedactedFields.Contains(name) ? Redacted : element.GetRawText());
                break;

            case JsonValueKind.True:
            case JsonValueKind.False:
                writer.AppendLine($"{element.GetBoolean()}");
                break;

            case JsonValueKind.Null:
                writer.AppendLine("null");
                break;
        }
    }
}
