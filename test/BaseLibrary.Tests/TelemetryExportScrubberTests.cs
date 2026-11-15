using BaseLibrary;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// E189: saida EXATA da reducao de telemetria para cada forma de caminho e de texto do usuario vista no censo das
/// excecoes baixadas. Os nomes sao de fixture (maquina LAB-PC-0042, usuario jdoe), nunca de log real.
/// </summary>
public class TelemetryExportScrubberTests
{
    private readonly TelemetryExportScrubber sut = new("LAB-PC-0042", "jdoe");

    [Theory]
    // Library do Nimloth com projeto e work: as pastas do usuario somem, a do app e o Workfile.sin ficam
    [InlineData(@"Could not find file 'C:\Users\jdoe\Documents\Library\Quartz study\Sample 7\Workfile.sin'.",
        @"Could not find file 'C:\Users\<redacted>\<dir>\Library\...\Workfile.sin'.")]
    // arquivo de dado do usuario dentro de pasta do app
    [InlineData(@"Cannot read 'C:\Users\jdoe\AppData\Local\Nimloth\Nimloth User Files\scan 07.xye'",
        @"Cannot read 'C:\Users\<redacted>\AppData\Local\Nimloth\Nimloth User Files\<file>.xye'")]
    // OneDrive com organizacao
    [InlineData(@"Path: 'C:\Users\jdoe\OneDrive - Contoso Ltd\Documents\Library\P1\W1\results.json'",
        @"Path: 'C:\Users\<redacted>\...\Library\...\results.json'")]
    // caminho com barra normal e conta nao redigida pelo TelemetryScrubber
    [InlineData(@"Access to the path 'C:/Users/jdoe/Desktop/run.raw' is denied.",
        @"Access to the path 'C:/Users/<redacted>/<dir>/<file>.raw' is denied.")]
    // prefixo de caminho longo
    [InlineData(@"Opening '\\?\C:\Users\jdoe\AppData\Local\Nimloth\settings.json'",
        @"Opening '\\?\C:\Users\<redacted>\AppData\Local\Nimloth\settings.json'")]
    // UNC de servidor de laboratorio, sem aspas, com ponto de frase
    [InlineData(@"Network path \\labserver\xrd-data\2026\run1.raw was not found.",
        @"Network path \\<host>\<share>\<dir>\<file>.raw was not found.")]
    // UNC por caminho longo
    [InlineData(@"File '\\?\UNC\nas01\group\quartz.cif' locked",
        @"File '\\?\UNC\<host>\<share>\<file>.cif' locked")]
    // URI de arquivo
    [InlineData(@"Uri file:///C:/Users/jdoe/Desktop/scan.xy failed",
        @"Uri file:///C:/Users/<redacted>/<dir>/<file>.xy failed")]
    // Linux e macOS
    [InlineData(@"Missing /home/jdoe/data/scan.xye now", @"Missing /home/<redacted>/<dir>/<file>.xye now")]
    [InlineData(@"Missing /Volumes/USB Stick/x.raw", @"Missing /Volumes/<dir>/<file>.raw")]
    // pasta fora do perfil com o nome do usuario
    [InlineData(@"Saved D:\jdoe\Lab data\q.sin", @"Saved D:\...\<file>.sin")]
    public void Scrub_ShouldReducePath_ToAppFoldersAndExtension(string input, string expected)
    {
        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Scrub_ShouldKeepSourceFileAndLine_InStackLine()
    {
        // Arrange
        string input = @"   at Sindarin.Run() in C:\Users\jdoe\source\repos\Sindarin\src\Interpreter\Run.cs:line 42";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(@"   at Sindarin.Run() in C:\Users\<redacted>\...\Sindarin\src\<dir>\Run.cs:line 42");
    }

    [Fact]
    public void Scrub_ShouldReplaceEmailMachineAndUserNames_AsWholeWords()
    {
        // Arrange
        string input = "Host LAB-PC-0042 refused jdoe; write to jdoe@example.org (jdoex stays)";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("Host <machine> refused <user>; write to <email> (jdoex stays)");
    }

    [Fact]
    public void Scrub_ShouldReplaceBareUserFileName_ButKeepFixedAppFile()
    {
        // Arrange
        string input = "Could not parse my-quartz_07.xrdml next to Workfile.sin";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("Could not parse <file>.xrdml next to Workfile.sin");
    }

    [Fact]
    public void Scrub_ShouldCollapseCifLines_IntoOneMarker()
    {
        // Arrange
        string input = "Invalid CIF:\r\ndata_quartz\r\n_cell_length_a 4.913\r\nloop_\r\n_atom_site_label\r\nEnd";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("Invalid CIF:\r\n<file content removed>\r\nEnd");
    }

    [Fact]
    public void Scrub_ShouldRemoveInscriptionBlock_IncludingNameLineWithoutNumbers()
    {
        // Arrange: o nome da fase e da amostra vem numa linha sem numero, vizinha das linhas de parametro
        string input = "Parse error near:\n  Phase Quartz\n    Cell 4.913 4.913 @5.405 90 90 120\n    Scale @0.12\nStop";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("Parse error near:\n  <inscription line removed>\nStop");
    }

    [Fact]
    public void Scrub_ShouldRemoveInscriptionAfterLabel_AndKeepEnglishSentences()
    {
        // Arrange
        string input = "Bad line: Cell 4.9 4.9 5.4 90 90 120\nIndex was outside the bounds of the array.\nSequence contains no elements";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("Bad line: <inscription line removed>\nIndex was outside the bounds of the array.\nSequence contains no elements");
    }

    [Fact]
    public void Scrub_ShouldKeepExceptionDetailFields_AndRemoveInscriptionAfterEqualsLabel()
    {
        // Arrange: o formato do ExceptionDetailsServices ("Nome = valor") nao e inscricao, mas o valor pode ser
        string input = "Source = Sindarin.Interpreter\nHResult = -2147024893\nMessage = Cell 4.9 4.9 5.4 90 90 120";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be("Source = Sindarin.Interpreter\nHResult = -2147024893\nMessage = <inscription line removed>");
    }

    [Fact]
    public void Scrub_ShouldRemoveLongQuotedText_AndKeepApostrophes()
    {
        // Arrange
        string quoted = new('a', TelemetryExportScrubber.MaxQuotedLength + 1);
        string shortQuoted = new('b', TelemetryExportScrubber.MaxQuotedLength);
        string input = $"Can't parse \"{quoted}\" nor '{shortQuoted}' it's done";

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be($"Can't parse \"<text removed>\" nor '{shortQuoted}' it's done");
    }

    [Fact]
    public void Scrub_ShouldCutLongLine_AndSayHowManyCharactersWereRemoved()
    {
        // Arrange
        string input = new string('x', TelemetryExportScrubber.MaxLineLength + 25);

        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(new string('x', TelemetryExportScrubber.MaxLineLength) + " <25 characters removed>");
    }

    [Fact]
    public void Scrub_ShouldNotTouchShortNames()
    {
        // Arrange: nome com menos de 3 caracteres nao e procurado (trocaria letras soltas)
        var scrubber = new TelemetryExportScrubber("PC", "jo");

        // Act
        string? result = scrubber.Scrub("PC jo");

        // Assert
        result.Should().Be("PC jo");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Scrub_ShouldReturnInput_WhenNullOrEmpty(string? input)
    {
        // Act
        string? result = sut.Scrub(input);

        // Assert
        result.Should().Be(input);
    }
}
