using System.Globalization;

namespace BaseLibrary;

/// <summary>
/// Implementação padrão de <see cref="ITelemetryGate"/>. Suprime a telemetria quando:
/// (a) uma variável de ambiente de opt-out está definida — o mecanismo determinístico
/// para apps de linha de comando (Sindarin CLI), que não têm UI de Settings; ou
/// (b) a região do sistema é a China (PIPL: sem transferência ⇒ sem mecanismo Art. 38/PIPIA).
/// Nunca lança.
/// </summary>
/// <remarks>
/// A detecção de região por <see cref="RegionInfo.CurrentRegion"/> não funciona sob
/// <see cref="CultureInfo.InvariantCulture"/> (usada pelo Sindarin CLI), por isso há
/// fallback para <see cref="CultureInfo.InstalledUICulture"/>. O env var é o opt-out
/// garantido; a supressão por região é best-effort.
/// </remarks>
public class TelemetryGate : ITelemetryGate
{
    // Aceita qualquer um destes (Nimloth e Sindarin compartilham o recurso de telemetria).
    private static readonly string[] OptOutVars =
        ["NIMLOTH_TELEMETRY_OPTOUT", "SINDARIN_TELEMETRY_OPTOUT"];

    public bool IsExportAllowed()
    {
        if (IsEnvOptOut())
            return false;
        if (IsRegionChina())
            return false;
        return true;
    }

    private static bool IsEnvOptOut()
    {
        foreach (string name in OptOutVars)
        {
            string? v = Environment.GetEnvironmentVariable(name);
            if (!string.IsNullOrWhiteSpace(v) && IsTruthy(v.Trim()))
                return true;
        }
        return false;
    }

    private static bool IsTruthy(string v) =>
        v is "1" or "true" or "TRUE" or "True" or "yes" or "YES" or "on" or "ON";

    private static bool IsRegionChina()
    {
        foreach (string? region in CandidateRegions())
        {
            if (string.Equals(region, "CN", StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    private static IEnumerable<string?> CandidateRegions()
    {
        // 1) Região da cultura atual (funciona no GUI; lança sob InvariantCulture).
        yield return TryGetRegion(static () => RegionInfo.CurrentRegion.TwoLetterISORegionName);
        // 2) Fallback para a cultura instalada no SO (sobrevive ao InvariantCulture do CLI).
        yield return TryGetRegion(static () => new RegionInfo(CultureInfo.InstalledUICulture.Name).TwoLetterISORegionName);
    }

    private static string? TryGetRegion(Func<string> get)
    {
        try { return get(); }
        catch { return null; }
    }
}
