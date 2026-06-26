namespace BaseLibrary;

public interface IExceptionServices
{
    /// <summary>
    /// Consentimento específico do app (modelo opt-out: <c>true</c> por padrão).
    /// Quando <c>false</c>, nenhuma telemetria é exportada. No Nimloth reflete o toggle de
    /// Settings; no Sindarin CLI reflete <c>--no-telemetry</c> / o contexto de execução.
    /// </summary>
    bool TelemetryEnabled { get; set; }
    Task SendException(Exception e, bool isAsync, string? messageExtra, string? OSversion);
    string GetExceptionText(Exception e, string? messageExtra);
    bool IsConnectedToInternet();
    //void SaveException(Exception e, string? messageExtra, string? OSversion);
}
