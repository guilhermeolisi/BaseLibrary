using System.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

/// <summary>
/// Reducao forte do texto que sai na telemetria de excecoes (E189). Chama primeiro o <see cref="TelemetryScrubber"/>
/// (conta do SO em <c>C:\Users\</c> e <c>/home</c>) e depois, linha a linha:
/// <list type="bullet">
/// <item>caminho (Windows com <c>\</c> ou <c>/</c>, <c>\\?\</c>, <c>\\?\UNC\</c>, UNC, <c>file:///</c>, raizes Unix de
/// usuario e de midia): pasta fora da lista do app vira <c>&lt;dir&gt;</c> (duas ou mais seguidas, <c>...</c>), host e
/// share viram <c>&lt;host&gt;</c> e <c>&lt;share&gt;</c>, arquivo vira <c>&lt;file&gt;</c> com a extensao, salvo os nomes
/// fixos do app e o fonte <c>.cs</c>/<c>.axaml</c> (que a pilha precisa);</item>
/// <item>nome de arquivo de dado do usuario solto na mensagem: <c>&lt;file&gt;</c> com a extensao;</item>
/// <item>e-mail, nome da maquina e nome do usuario (palavra inteira, 3 caracteres ou mais);</item>
/// <item>linha de CIF e linha de inscricao (com as vizinhas no mesmo formato): a linha inteira sai, seguidas viram uma;</item>
/// <item>trecho entre aspas com mais de 80 caracteres e linha com mais de 300 caracteres.</item>
/// </list>
/// Linha de pilha (<c>at ...</c>) so tem caminho e nomes reduzidos.
/// </summary>
/// <remarks>
/// Aplicada so na fronteira de exportacao; nunca no texto mostrado ao proprio usuario. AOT: as expressoes fixas sao
/// <see cref="GeneratedRegexAttribute"/>; os nomes conhecidos sao procurados sem regex.
/// </remarks>
public sealed partial class TelemetryExportScrubber : ITelemetryScrubber
{
    public const string DirToken = "<dir>";
    public const string DirRunToken = "...";
    public const string FileToken = "<file>";
    public const string HostToken = "<host>";
    public const string ShareToken = "<share>";
    public const string EmailToken = "<email>";
    public const string MachineToken = "<machine>";
    public const string UserToken = "<user>";
    public const string FileContentToken = "<file content removed>";
    public const string InscriptionToken = "<inscription line removed>";
    public const string TextToken = "<text removed>";
    public const int MaxQuotedLength = 80;
    public const int MaxLineLength = 300;

    // Pastas do app, do SO e das raizes que nao identificam ninguem: ficam no caminho.
    private static readonly HashSet<string> KeptFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Users", "AppData", "Local", "Roaming", "Temp", "Nimloth", "Nimloth Debug", "Nimloth User Files", "Library",
        "Trash", "Sindarin", "sindarin-study", "conversations", "roles", "Exceptions", "Program Files",
        "Program Files (x86)", "WindowsApps", "src",
    };

    // Arquivos de nome fixo do app: o nome nao e do usuario.
    private static readonly HashSet<string> KeptFiles = new(StringComparer.OrdinalIgnoreCase)
    {
        "Workfile.sin", "defaults.json", "settings.json", "results.json",
    };

    private static readonly HashSet<string> SourceExtensions = new(StringComparer.OrdinalIgnoreCase) { ".cs", ".axaml" };

    private readonly ITelemetryScrubber accountScrubber = new TelemetryScrubber();
    private readonly (string Name, string Token)[] knownNames;

    /// <summary>Usa o nome da maquina e o nome do usuario do ambiente.</summary>
    public TelemetryExportScrubber()
        : this(ReadEnvironment(() => Environment.MachineName), ReadEnvironment(() => Environment.UserName))
    {
    }

    /// <param name="machineName">Nome da maquina a trocar por <see cref="MachineToken"/> (nulo: nenhum).</param>
    /// <param name="userName">Nome do usuario a trocar por <see cref="UserToken"/> (nulo: nenhum).</param>
    public TelemetryExportScrubber(string? machineName, string? userName)
    {
        var names = new List<(string, string)>();
        AddKnown(names, machineName, MachineToken);
        AddKnown(names, userName, UserToken);
        knownNames = names.ToArray();
    }

    private static void AddKnown(List<(string, string)> names, string? name, string token)
    {
        string? trimmed = name?.Trim();
        if (trimmed is { Length: >= 3 })
            names.Add((trimmed, token));
    }

    private static string? ReadEnvironment(Func<string> read)
    {
        try { return read(); }
        catch (InvalidOperationException) { return null; }
        catch (PlatformNotSupportedException) { return null; }
    }

    private enum LineKind { Normal, Stack, FileContent, Inscription }

    public string? Scrub(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        text = accountScrubber.Scrub(text)!;
        var lines = SplitLines(text);

        var kinds = new LineKind[lines.Count];
        for (int i = 0; i < lines.Count; i++)
            kinds[i] = Classify(lines[i].Text);
        ExtendInscriptionBlocks(lines, kinds);

        var sb = new StringBuilder(text.Length);
        LineKind previousRemoved = LineKind.Normal;
        string previousNewline = string.Empty;
        for (int i = 0; i < lines.Count; i++)
        {
            (string line, string newline) = lines[i];
            LineKind kind = kinds[i];
            if (kind is LineKind.FileContent or LineKind.Inscription)
            {
                if (kind == previousRemoved)
                {
                    // Seguida da mesma especie: some; a ultima linha do texto devolve o fim de linha que a anterior tinha.
                    if (newline.Length == 0 && previousNewline.Length > 0)
                    {
                        sb.Length -= previousNewline.Length;
                        previousNewline = string.Empty;
                    }
                    continue;
                }
                sb.Append(LeadingWhitespace(line)).Append(kind == LineKind.FileContent ? FileContentToken : InscriptionToken);
                previousRemoved = kind;
            }
            else
            {
                sb.Append(ScrubLine(line, kind == LineKind.Stack));
                previousRemoved = LineKind.Normal;
            }
            sb.Append(newline);
            previousNewline = newline;
        }
        return sb.ToString();
    }

    private static List<(string Text, string Newline)> SplitLines(string text)
    {
        var lines = new List<(string, string)>();
        int pos = 0;
        while (true)
        {
            int nl = text.IndexOf('\n', pos);
            if (nl < 0)
            {
                lines.Add((text[pos..], string.Empty));
                return lines;
            }
            int lineEnd = nl > pos && text[nl - 1] == '\r' ? nl - 1 : nl;
            lines.Add((text[pos..lineEnd], text[lineEnd..(nl + 1)]));
            pos = nl + 1;
            if (pos == text.Length)
                return lines;
        }
    }

    private static LineKind Classify(string line)
    {
        if (StackLineRegex().IsMatch(line))
            return LineKind.Stack;
        string trimmed = line.Trim();
        if (trimmed.Length == 0)
            return LineKind.Normal;
        if (CifLineRegex().IsMatch(trimmed))
            return LineKind.FileContent;
        if (IsInscriptionLine(trimmed))
            return LineKind.Inscription;
        return LineKind.Normal;
    }

    /// <summary>
    /// Linha no formato de inscricao: palavra-chave com inicial maiuscula seguida de argumentos, sem dois-pontos, sem
    /// ponto final, e com ao menos um argumento numerico (com ou sem marcador de refino). Frase de mensagem de
    /// excecao tem dois-pontos, ponto final ou nenhum numero.
    /// </summary>
    private static bool IsInscriptionLine(string trimmed) =>
        LooseInscriptionRegex().IsMatch(trimmed) && !trimmed.EndsWith('.') && NumericArgumentRegex().IsMatch(trimmed);

    // A linha vizinha de uma linha de inscricao, no mesmo formato mas sem numero (ex.: o nome da fase), sai junto.
    private static void ExtendInscriptionBlocks(List<(string Text, string Newline)> lines, LineKind[] kinds)
    {
        bool changed = true;
        while (changed)
        {
            changed = false;
            for (int i = 0; i < kinds.Length; i++)
            {
                if (kinds[i] != LineKind.Normal)
                    continue;
                bool nextToBlock = (i > 0 && kinds[i - 1] == LineKind.Inscription)
                                   || (i + 1 < kinds.Length && kinds[i + 1] == LineKind.Inscription);
                string trimmed = lines[i].Text.Trim();
                if (nextToBlock && LooseInscriptionRegex().IsMatch(trimmed) && !trimmed.EndsWith('.'))
                {
                    kinds[i] = LineKind.Inscription;
                    changed = true;
                }
            }
        }
    }

    private static string LeadingWhitespace(string line)
    {
        int n = 0;
        while (n < line.Length && char.IsWhiteSpace(line[n]))
            n++;
        return line[..n];
    }

    private string ScrubLine(string line, bool isStack)
    {
        line = ReducePaths(line);
        if (!isStack)
        {
            line = InscriptionAfterLabel(line);
            line = BareUserFileRegex().Replace(line, m =>
                KeptFiles.Contains(m.Value) ? m.Value : FileToken + "." + m.Groups["ext"].Value);
            line = EmailRegex().Replace(line, EmailToken);
        }
        line = ReplaceKnownNames(line);
        if (!isStack)
        {
            line = LongQuotedRegex().Replace(line, m => m.Groups["q"].Value + TextToken + m.Groups["q"].Value);
            if (line.Length > MaxLineLength)
                line = line[..MaxLineLength] + $" <{line.Length - MaxLineLength} characters removed>";
        }
        return line;
    }

    // "Bad line: Cell 4.9 4.9 5.4" -> "Bad line: <inscription line removed>"; o mesmo com " = " (formato do
    // ExceptionDetailsServices: "Message = ...").
    private static string InscriptionAfterLabel(string line)
    {
        foreach (string separator in LabelSeparators)
        {
            int at = line.IndexOf(separator, StringComparison.Ordinal);
            if (at < 0)
                continue;
            int valueStart = at + separator.Length;
            string value = line[valueStart..].Trim();
            if (value.Length > 0 && IsInscriptionLine(value))
                return line[..valueStart] + InscriptionToken;
        }
        return line;
    }

    private static readonly string[] LabelSeparators = [": ", " = "];

    private string ReplaceKnownNames(string line)
    {
        foreach ((string name, string token) in knownNames)
        {
            int start = 0;
            while (start < line.Length)
            {
                int at = line.IndexOf(name, start, StringComparison.OrdinalIgnoreCase);
                if (at < 0)
                    break;
                int after = at + name.Length;
                bool wordStart = at == 0 || !IsNameChar(line[at - 1]);
                bool wordEnd = after == line.Length || !IsNameChar(line[after]);
                if (wordStart && wordEnd)
                {
                    line = string.Concat(line.AsSpan(0, at), token, line.AsSpan(after));
                    start = at + token.Length;
                }
                else
                {
                    start = at + 1;
                }
            }
        }
        return line;
    }

    private static bool IsNameChar(char c) => char.IsLetterOrDigit(c) || c == '_';

    private static string ReducePaths(string line)
    {
        MatchCollection matches = PathStartRegex().Matches(line);
        if (matches.Count == 0)
            return line;

        var sb = new StringBuilder(line.Length);
        int copied = 0;
        foreach (Match m in matches)
        {
            if (m.Index < copied)
                continue; // dentro de um caminho ja lido
            sb.Append(line, copied, m.Index - copied);
            char before = m.Index > 0 ? line[m.Index - 1] : '\0';
            char quote = before is '\'' or '"' ? before : '\0';
            bool unc = m.Value == @"\\" || m.Value.Equals(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase);
            var segments = new List<(string Text, char Sep)>();
            int end = ReadSegments(line, m.Index + m.Length, quote, segments);
            sb.Append(m.Value).Append(ReduceSegments(segments, unc));
            copied = end;
        }
        sb.Append(line, copied, line.Length - copied);
        return sb.ToString();
    }

    private static int ReadSegments(string s, int i, char quote, List<(string Text, char Sep)> segments)
    {
        while (i < s.Length)
        {
            int j = i;
            if (s[i] == '<')
            {
                // Marcador de redacao anterior (<redacted>) e um segmento inteiro.
                int close = s.IndexOf('>', i);
                if (close > i)
                    j = close + 1;
            }
            while (j < s.Length && !IsFolderStop(s[j], quote))
                j++;
            if (j > i && j < s.Length && s[j] is '\\' or '/')
            {
                segments.Add((s[i..j], s[j]));
                i = j + 1;
                continue;
            }

            // Ultimo segmento: entre aspas vai ate a aspa; sem aspas, ate o primeiro espaco ou pontuacao.
            int k;
            if (quote != '\0')
            {
                k = s.IndexOf(quote, i);
                if (k < 0)
                    k = s.Length;
            }
            else
            {
                k = i;
                while (k < s.Length && !IsFinalStop(s[k]))
                    k++;
                while (k > i && s[k - 1] is '.' or ',')
                    k--; // pontuacao de frase fica fora do caminho
            }
            if (k > i)
                segments.Add((s[i..k], '\0'));
            return k;
        }
        return i;
    }

    private static bool IsFolderStop(char c, char quote) =>
        c is '\\' or '/' or ':' or '*' or '?' or '"' or '<' or '>' or '|' or '\r' or '\n'
        || (quote != '\0' && c == quote);

    private static bool IsFinalStop(char c) =>
        char.IsWhiteSpace(c) || c is ':' or '*' or '?' or '"' or '<' or '>' or '|' or '\'' or ',' or ';' or ')' or ']';

    private static string ReduceSegments(List<(string Text, char Sep)> segments, bool unc)
    {
        var sb = new StringBuilder();
        int run = 0;
        char runSep = '\0';
        for (int idx = 0; idx < segments.Count; idx++)
        {
            (string text, char sep) = segments[idx];
            string? kept;
            if (unc && idx == 0)
                kept = HostToken;
            else if (unc && idx == 1)
                kept = ShareToken;
            else if (sep == '\0')
                kept = ReduceLast(text);
            else
                kept = KeepFolder(text) ? text : null;

            if (kept is null)
            {
                run++;
                runSep = sep;
                continue;
            }
            FlushRun(sb, ref run, runSep);
            sb.Append(kept);
            if (sep != '\0')
                sb.Append(sep);
        }
        FlushRun(sb, ref run, runSep);
        return sb.ToString();
    }

    private static void FlushRun(StringBuilder sb, ref int run, char runSep)
    {
        if (run == 0)
            return;
        sb.Append(run == 1 ? DirToken : DirRunToken);
        if (runSep != '\0')
            sb.Append(runSep);
        run = 0;
    }

    private static bool KeepFolder(string text) =>
        KeptFolders.Contains(text) || (text.Length > 2 && text[0] == '<' && text[^1] == '>');

    private static string? ReduceLast(string text)
    {
        Match ext = ExtensionRegex().Match(text);
        if (!ext.Success)
            return KeepFolder(text) ? text : null;
        if (KeptFiles.Contains(text) || SourceExtensions.Contains(ext.Value))
            return text;
        return FileToken + ext.Value;
    }

    [GeneratedRegex(@"^\s*at\s")]
    private static partial Regex StackLineRegex();

    // Inicio de caminho: file:///, \\?\UNC\, \\?\C:\, \\host, C:\ ou C:/, e raizes Unix de usuario e de midia.
    [GeneratedRegex(@"(?<![A-Za-z0-9_])(?:file:///(?:[A-Za-z]:/)?|\\\\\?\\UNC\\|\\\\\?\\[A-Za-z]:\\|\\\\(?=[A-Za-z0-9])|[A-Za-z]:[\\/])|(?<![\w/.:<>-])/(?:home|Users|root|mnt|media|Volumes|tmp)/", RegexOptions.IgnoreCase)]
    private static partial Regex PathStartRegex();

    [GeneratedRegex(@"\.[A-Za-z0-9]{1,8}$")]
    private static partial Regex ExtensionRegex();

    [GeneratedRegex(@"(?<![\w.<>\\/-])[\w-]+(?:\.[\w-]+)*\.(?<ext>sin|cif|xy|xye|raw|brml|dat|csv|txt|xrdml|rd|udf|uxd|gsa|fxye|chi|xls|xlsx|docx|pdf|png)\b", RegexOptions.IgnoreCase)]
    private static partial Regex BareUserFileRegex();

    [GeneratedRegex(@"[\w.+-]+@[\w-]+(?:\.[\w-]+)+")]
    private static partial Regex EmailRegex();

    [GeneratedRegex(@"^(?:data_\S+|loop_|_[A-Za-z][\w.\[\]()/-]*(?:\s+\S.*)?)$")]
    private static partial Regex CifLineRegex();

    // Palavra-chave e argumentos; "=" sozinho e rotulo ("HResult = -2147024893"), nao argumento.
    [GeneratedRegex(@"^[A-Z][A-Za-z0-9_]*(?:\s+(?!=(?:\s|$))[^\s:]+)+$")]
    private static partial Regex LooseInscriptionRegex();

    [GeneratedRegex(@"(?:^|\s)[@!~$]?[-+]?\d+(?:[.,]\d+)?(?:\(\d+\))?(?=\s|$)")]
    private static partial Regex NumericArgumentRegex();

    [GeneratedRegex(@"(?<![A-Za-z])(?<q>[""'])[^""'\r\n]{81,}\k<q>(?![A-Za-z])")]
    private static partial Regex LongQuotedRegex();
}
