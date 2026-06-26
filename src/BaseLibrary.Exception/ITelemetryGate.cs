namespace BaseLibrary;

/// <summary>
/// Política UNIVERSAL de supressão de telemetria, independente do app: opt-out por
/// variável de ambiente (amigável a CLI/scripts) e supressão por região (China/PIPL).
/// Avaliada na fronteira de exportação, ADEMAIS do consentimento específico do app
/// (modelo opt-out via <c>TelemetryEnabled</c>).
/// </summary>
public interface ITelemetryGate
{
    /// <summary>True se a exportação de telemetria é permitida no ambiente atual.</summary>
    bool IsExportAllowed();
}
