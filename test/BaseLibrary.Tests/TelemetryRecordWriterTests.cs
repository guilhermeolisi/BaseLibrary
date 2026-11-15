using System.Text.Json;
using BaseLibrary;
using FluentAssertions;

namespace BaseLibrary.Tests;

/// <summary>
/// E189: o downloader grava o registro ja limpo. Campos de maquina e local saem redigidos em todo registro; o texto so
/// passa pelo scrubber no registro de excecao (o feedback guarda o que o usuario escreveu, com o e-mail de resposta).
/// </summary>
public class TelemetryRecordWriterTests
{
    private readonly TelemetryRecordWriter sut = new(new TelemetryExportScrubber("LAB-PC-0042", "jdoe"));

    private const string ExceptionRecord = """
        {"Type":"AppTraces","SeverityLevel":3,"AppRoleInstance":"LAB-PC-0042","ClientCity":"Springfield",
         "ClientStateOrProvince":"Some State","ClientCountryOrRegion":"Brazil","ClientIP":"0.0.0.0",
         "Message":"Could not find file 'C:\\Users\\jdoe\\Documents\\Library\\Quartz study\\S7\\scan.xye'.",
         "Properties":{"local time":"2026-11-14 10:00:00:0000000","extra message":"mail jdoe@example.org"}}
        """;

    private const string FeedbackRecord = """
        {"Type":"AppTraces","SeverityLevel":1,"AppRoleInstance":"LAB-PC-0042","ClientCity":"Springfield",
         "ClientStateOrProvince":"Some State","ClientIP":"0.0.0.0",
         "Message":"User e-mail: jdoe@example.org\r\nThe export of 'C:\\Users\\jdoe\\Desktop\\my sample.xy' fails",
         "Properties":{"local time":"2026-11-14 11:00:00:0000000"}}
        """;

    [Fact]
    public void Write_ShouldRedactRecordFieldsAndScrubText_ForExceptionRecord()
    {
        // Arrange
        using var doc = JsonDocument.Parse(ExceptionRecord);

        // Act
        string text = sut.Write(doc.RootElement, out string? localTime);

        // Assert
        TelemetryRecordWriter.IsExceptionRecord(doc.RootElement).Should().BeTrue();
        localTime.Should().Be("2026-11-14 10:00:00:0000000");
        text.Should().Contain("AppRoleInstance: <redacted>" + Environment.NewLine)
            .And.Contain("ClientCity: <redacted>" + Environment.NewLine)
            .And.Contain("ClientStateOrProvince: <redacted>" + Environment.NewLine)
            .And.Contain("ClientIP: <redacted>" + Environment.NewLine)
            .And.Contain("ClientCountryOrRegion: Brazil" + Environment.NewLine)
            .And.Contain(@"Message: Could not find file 'C:\Users\<redacted>\<dir>\Library\...\<file>.xye'." + Environment.NewLine)
            .And.Contain("    extra message: mail <email>" + Environment.NewLine);
        text.Should().NotContain("LAB-PC-0042").And.NotContain("Springfield").And.NotContain("Some State")
            .And.NotContain("Quartz study").And.NotContain("jdoe");
    }

    [Fact]
    public void Write_ShouldRedactRecordFieldsButKeepUserText_ForFeedbackRecord()
    {
        // Arrange
        using var doc = JsonDocument.Parse(FeedbackRecord);

        // Act
        string text = sut.Write(doc.RootElement, out _);

        // Assert
        TelemetryRecordWriter.IsExceptionRecord(doc.RootElement).Should().BeFalse();
        text.Should().Contain("AppRoleInstance: <redacted>").And.Contain("ClientCity: <redacted>")
            .And.Contain("ClientStateOrProvince: <redacted>").And.Contain("ClientIP: <redacted>");
        text.Should().Contain("User e-mail: jdoe@example.org", "o autor do feedback precisa do e-mail de resposta")
            .And.Contain(@"C:\Users\jdoe\Desktop\my sample.xy");
        text.Should().NotContain("LAB-PC-0042").And.NotContain("Springfield");
    }
}
