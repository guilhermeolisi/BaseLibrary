namespace BaseLibrary;

/// <summary>
/// Remove dados pessoais incidentais (ex.: o nome da conta do SO embutido em caminhos
/// de arquivo) de textos antes de serem exportados para a telemetria.
/// </summary>
public interface ITelemetryScrubber
{
    /// <summary>
    /// Retorna o texto com segmentos sensíveis mascarados. Nunca lança; texto nulo/vazio
    /// é devolvido como veio.
    /// </summary>
    string? Scrub(string? text);
}
