using System.Text.RegularExpressions;

namespace BaseLibrary;

/// <summary>
/// Implementação AOT-safe (usa <see cref="GeneratedRegexAttribute"/>, sem regex
/// interpretada em runtime) que mascara o nome da conta do usuário embutido em
/// caminhos de arquivo (Windows/Linux/macOS). Tags de operação e textos sem caminho
/// passam inalterados, preservando a utilidade da telemetria.
/// </summary>
/// <remarks>
/// Aplicada apenas na fronteira de exportação (o que sai da máquina), nunca na
/// formatação usada para exibição local ao próprio usuário.
/// </remarks>
public partial class TelemetryScrubber : ITelemetryScrubber
{
    private const string Redacted = "<redacted>";

    // C:\Users\<conta>\...  ->  C:\Users\<redacted>\...
    [GeneratedRegex(@"(?<root>[A-Za-z]:\\Users\\)[^\\]+", RegexOptions.IgnoreCase)]
    private static partial Regex WindowsUserProfileRegex();

    // /home/<conta>/...  e  /Users/<conta>/...  (Linux e macOS)
    [GeneratedRegex(@"(?<root>/(?:home|Users)/)[^/]+")]
    private static partial Regex UnixHomeRegex();

    public string? Scrub(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = WindowsUserProfileRegex().Replace(text, "${root}" + Redacted);
        text = UnixHomeRegex().Replace(text, "${root}" + Redacted);
        return text;
    }
}
