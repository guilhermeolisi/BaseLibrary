using System.Text.RegularExpressions;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// Catraca da conta compartilhada no BaseLibrary (`.claude/rules/calculo-compartilhado.md`, secao 7, na raiz do
/// workspace; E269). Le o <c>src</c> deste repositorio, como o <c>SourceEncodingConfigTests</c> do Nimloth, e afirma:
/// <list type="bullet">
/// <item>(a) nenhum metodo estatico, de qualquer visibilidade, com nome da lista da regra fora das casas, salvo as
/// excecoes nomeadas abaixo, cada uma com o motivo;</item>
/// <item>(b) a contagem EXATA de conversao de grau feita a mao (<c>PI / 180</c> e <c>180 / PI</c>), fora das definicoes
/// de <c>MathMethods.Radians</c> e <c>Degrees</c>, que so desce;</item>
/// <item>(c) que o nome da classe antiga migrada nao existe mais no <c>src</c>.</item>
/// </list>
/// Mudar a lista de excecoes ou a contagem para cima e decisao de revisao, nunca conserto de teste vermelho: a conta que
/// faltar vai para a casa (`calculo-compartilhado.md`, secao 4). Cada regra tem um teste irmao de presenca, que prova
/// que o leitor acha o que procura, para a catraca nao responder verde sem olhar.
/// </summary>
public class CalculationPlacementConfigTests
{
    /// <summary>As casas da conta compartilhada no BaseLibrary (`calculo-compartilhado.md`, secao 2).</summary>
    private static readonly HashSet<string> Casas = new(StringComparer.Ordinal)
    {
        "MathMethods", "NumericMethods", "GeometryMethods", "SpecialFunctions", "Distributions",
        "ConvolutionMethods", "MatrixMethods", "PseudoVoigtMethods",
    };

    /// <summary>
    /// Excecoes nomeadas a regra (a), no formato <c>Classe.Metodo</c>, com o motivo. Nasceram na revisao do plano da
    /// E269 (achado 3).
    /// </summary>
    private static readonly Dictionary<string, string> Excecoes = new(StringComparer.Ordinal)
    {
        ["Point3D.Interpolation"] =
            "privado: o ponto de corte de isolinha entre dois Point3D num nivel z, que devolve Point2D e faz parte do " +
            "tracado de contorno do proprio tipo; o NumericMethods.Interpolation e escalar e tem outra ordem de conta",
    };

    /// <summary>A lista de nomes da regra (a), com <c>*</c> como curinga, sensivel a caixa.</summary>
    private static readonly Regex NomeDaLista = new(
        "^(Bragg\\w*|TwoTheta\\w*|Median|Linspace|Interpolat\\w*|Determinant|\\w*Volume\\w*|\\w*Metric\\w*|Dot|Cross|" +
        "PseudoVoigt|psVoigt|Gaussian|Lorentz\\w*|Degrees|Radians|\\w*ToCartesian\\w*|\\w*FromCartesian\\w*|\\w*Orthogonal\\w*)$",
        RegexOptions.CultureInvariant);

    /// <summary>
    /// Declaracao de metodo estatico numa linha: modificadores, <c>static</c>, o tipo de retorno (tupla inclusive, sem
    /// <c>=</c>, para nao casar campo inicializado com lambda) e o nome antes do parentese.
    /// </summary>
    private static readonly Regex MetodoEstatico = new(
        "^\\s*(?:\\[[^\\]]*\\]\\s*)*(?:(?:public|private|internal|protected|unsafe|extern|new|async)\\s+)*static\\s+" +
        "(?!class\\b|readonly\\b)[^=;{]+?\\s+(?<nome>\\w+)\\s*(?:<[^>]*>)?\\s*\\(",
        RegexOptions.CultureInvariant);

    private static readonly Regex DeclaracaoDeTipo = new(
        "\\b(?:class|struct|record(?:\\s+struct)?|interface)\\s+(?<nome>\\w+)", RegexOptions.CultureInvariant);

    /// <summary>
    /// Conversao de grau a mao: <c>PI / 180</c> e <c>180 / PI</c>, aceitando <c>180.0</c> e <c>Math.PI</c>, que sao a
    /// mesma conta escrita de outro jeito.
    /// </summary>
    private static readonly Regex ConversaoDeGrau = new(
        "PI\\s*/\\s*180(?:\\.0*)?\\b|\\b180(?:\\.0*)?\\s*/\\s*(?:System\\.)?(?:Math\\.)?PI\\b", RegexOptions.CultureInvariant);

    /// <summary>
    /// As duas linhas que DEFINEM a conversao, no <c>MathMethods</c>: sao a casa, nao copia.
    /// </summary>
    private static readonly HashSet<string> DefinicoesDaConversao = new(StringComparer.Ordinal)
    {
        "MathMethods.cs|return degrees * (PI / 180.0);",
        "MathMethods.cs|return radians * (180.0 / PI);",
    };

    /// <summary>Contagem medida em 12/11/2026 (E269). So desce: quem remove uma baixa o numero no mesmo commit.</summary>
    private const int ConversoesDeGrauForaDaCasa = 0;

    /// <summary>
    /// Nomes das classes antigas migradas para as casas (E269: a classe de funcoes gerais do Sindarin.Math virou o
    /// <c>NumericMethods</c>, e o <c>GeometryMethods</c> do Sindarin.Math subiu para o daqui).
    /// </summary>
    private static readonly string[] ClassesAntigas = { "GeneralMethods", "Sindarin.Math.GeometryMethods" };

    // ---- (a) ----

    [Fact]
    public void ListedStaticMethods_ShouldLiveOnlyInTheHouses_OrInTheNamedExceptions()
    {
        var fora = MetodosDaLista(ArquivosDoSrc())
            .Where(m => !Casas.Contains(m.Classe) && !Excecoes.ContainsKey($"{m.Classe}.{m.Nome}"))
            .Select(m => m.ToString())
            .ToList();

        fora.Should().BeEmpty(
            "conta com nome da lista da secao 7 de calculo-compartilhado.md mora numa casa; se ela precisa ficar fora, " +
            "a excecao entra na lista do teste com o motivo, por decisao de revisao");
    }

    [Fact]
    public void NamedExceptions_ShouldStillExist_SoTheListDoesNotRot()
    {
        var achados = MetodosDaLista(ArquivosDoSrc()).Select(m => $"{m.Classe}.{m.Nome}").ToHashSet(StringComparer.Ordinal);

        Excecoes.Keys.Where(k => !achados.Contains(k)).Should().BeEmpty(
            "excecao que nao existe mais sai da lista, senao a lista vira passe livre para um nome novo igual");
    }

    [Fact]
    public void Reader_ShouldFindTheListedMethodsOfTheHouses()
    {
        var achados = MetodosDaLista(ArquivosDoSrc()).Select(m => $"{m.Classe}.{m.Nome}").ToHashSet(StringComparer.Ordinal);

        // Presenca: se o leitor nao achar estes, a regra (a) responde verde sem olhar.
        achados.Should().Contain(new[]
        {
            "NumericMethods.Median", "NumericMethods.Linspace", "NumericMethods.Interpolation", "MathMethods.Radians",
            "MathMethods.Degrees", "GeometryMethods.Dot", "GeometryMethods.Cross", "MatrixMethods.Determinant",
            "Point3D.Interpolation",
        });
    }

    [Fact]
    public void Reader_ShouldCatchAListedMethodOutsideTheHouses_AndIgnoreFieldsAndCalls()
    {
        string[] texto =
        {
            "public static class Fake",
            "{",
            "    internal static (double a, double b) Linspace(double s, double e, int n) => default;",
            "    private static Func<double, double> Radians = x => MathMethods.Radians(x);",
            "    public static double Use() => Median(new double[1]);",
            "    [MethodImpl(MethodImplOptions.AggressiveInlining)] public static double CellVolume<T>(T c) => 0;",
            "}",
        };

        var achados = MetodosDaLista(new[] { ("Fake.cs", texto) }).Select(m => $"{m.Classe}.{m.Nome}").ToList();

        achados.Should().Equal("Fake.Linspace", "Fake.CellVolume");
    }

    // ---- (b) ----

    [Fact]
    public void HandWrittenDegreeConversions_ShouldNotGrow()
    {
        var copias = ConversoesDeGrau(ArquivosDoSrc()).ToList();

        copias.Should().HaveCount(ConversoesDeGrauForaDaCasa,
            "grau e radiano se convertem pelo MathMethods.Radians e Degrees; a contagem so desce. Achadas: " +
            string.Join("; ", copias));
    }

    [Fact]
    public void DegreeConversionCounter_ShouldSeeTheDefinitions_AndCountEverySpelling()
    {
        // Presenca no src: as duas definicoes existem e sao as unicas descontadas.
        var definicoes = ArquivosDoSrc()
            .Where(a => Path.GetFileName(a.Caminho) == "MathMethods.cs")
            .SelectMany(a => a.Linhas.Where(l => ConversaoDeGrau.IsMatch(l)))
            .Select(l => "MathMethods.cs|" + l.Trim())
            .ToList();
        definicoes.Should().BeEquivalentTo(DefinicoesDaConversao);

        // Presenca na contagem: cada grafia conta uma vez, e a definicao fora do MathMethods conta.
        string[] texto =
        {
            "double r = d * Math.PI / 180;",
            "double g = r * 180.0 / Math.PI;",
            "double h = r * 180 / PI;",
            "return degrees * (PI / 180.0);",
            "double nada = PI / 18;",
        };
        ConversoesDeGrau(new[] { ("Outro.cs", texto) }).Should().HaveCount(4);
    }

    // ---- (c) ----

    [Fact]
    public void MigratedClassNames_ShouldNotAppearInSrc()
    {
        var restos = ArquivosDoSrc()
            .SelectMany(a => a.Linhas.Select((l, i) => (a.Caminho, Linha: i + 1, Texto: l)))
            .Where(x => ClassesAntigas.Any(c => x.Texto.Contains(c, StringComparison.Ordinal)))
            .Select(x => $"{x.Caminho}:{x.Linha}")
            .ToList();

        restos.Should().BeEmpty("a migracao e completa (secao 6 de calculo-compartilhado.md): a classe antiga nao volta");
    }

    [Fact]
    public void SrcReader_ShouldReadTheSourceFilesOfTheRepository()
    {
        var arquivos = ArquivosDoSrc().Select(a => Path.GetFileName(a.Caminho)).ToHashSet(StringComparer.Ordinal);

        // Presenca: sem os arquivos das casas, as regras (a), (b) e (c) respondem verde sem olhar.
        arquivos.Should().Contain(new[] { "NumericMethods.cs", "GeometryMethods.cs", "MathMethods.cs", "MatrixMethods.cs" });
        arquivos.Should().NotContain(a => a.EndsWith(".g.cs", StringComparison.Ordinal));
    }

    // ---- leitores ----

    private sealed record Metodo(string Arquivo, int Linha, string Classe, string Nome)
    {
        public override string ToString() => $"{Arquivo}:{Linha} {Classe}.{Nome}";
    }

    private static IEnumerable<Metodo> MetodosDaLista(IEnumerable<(string Caminho, string[] Linhas)> arquivos)
    {
        foreach (var (caminho, linhas) in arquivos)
        {
            string? classe = null;
            for (int i = 0; i < linhas.Length; i++)
            {
                string linha = linhas[i];
                if (linha.TrimStart().StartsWith("//", StringComparison.Ordinal))
                    continue;
                Match tipo = DeclaracaoDeTipo.Match(linha);
                if (tipo.Success)
                    classe = tipo.Groups["nome"].Value;
                Match m = MetodoEstatico.Match(linha);
                if (m.Success && NomeDaLista.IsMatch(m.Groups["nome"].Value))
                    yield return new Metodo(caminho, i + 1, classe ?? "?", m.Groups["nome"].Value);
            }
        }
    }

    private static IEnumerable<string> ConversoesDeGrau(IEnumerable<(string Caminho, string[] Linhas)> arquivos)
    {
        foreach (var (caminho, linhas) in arquivos)
        {
            for (int i = 0; i < linhas.Length; i++)
            {
                if (!ConversaoDeGrau.IsMatch(linhas[i]))
                    continue;
                if (Path.GetFileName(caminho) == "MathMethods.cs" &&
                    DefinicoesDaConversao.Contains("MathMethods.cs|" + linhas[i].Trim()))
                    continue;
                yield return $"{caminho}:{i + 1}";
            }
        }
    }

    private static IReadOnlyList<(string Caminho, string[] Linhas)> ArquivosDoSrc()
    {
        string src = Path.Combine(RaizDoRepo(), "src");
        char sep = Path.DirectorySeparatorChar;
        return Directory.EnumerateFiles(src, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{sep}bin{sep}", StringComparison.Ordinal) &&
                        !p.Contains($"{sep}obj{sep}", StringComparison.Ordinal))
            .OrderBy(p => p, StringComparer.Ordinal)
            .Select(p => (Path.GetRelativePath(src, p), File.ReadAllLines(p)))
            .ToList();
    }

    private static string RaizDoRepo()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "BaseLibrary.sln")))
                return dir.FullName;
        throw new InvalidOperationException("BaseLibrary.sln nao encontrado acima de " + AppContext.BaseDirectory);
    }
}
