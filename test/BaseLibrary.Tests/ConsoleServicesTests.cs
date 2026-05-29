using BaseLibrary;
using FluentAssertions;
using System.Globalization;
using System.Diagnostics;
using System.Text;

namespace BaseLibrary.Tests;

public class ConsoleServicesTests : IDisposable
{
    private readonly CultureInfo previousCulture;
    private readonly CultureInfo previousUICulture;

    public ConsoleServicesTests()
    {
        previousCulture = Thread.CurrentThread.CurrentCulture;
        previousUICulture = Thread.CurrentThread.CurrentUICulture;

        CultureInfo culture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;
    }

    public void Dispose()
    {
        Thread.CurrentThread.CurrentCulture = previousCulture;
        Thread.CurrentThread.CurrentUICulture = previousUICulture;
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
        Console.SetIn(new StreamReader(Console.OpenStandardInput()));
    }

    [Fact]
    public void GetInternalLog_ShouldReturnEmptyString_ByDefault()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        services.GetInternalLog().Should().BeEmpty();
    }

    [Fact]
    public void InitializeInternalLog_ShouldResetExistingContent()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());
        services.Write("before");

        services.InitializeInternalLog();

        services.GetInternalLog().Should().BeEmpty();
    }

    [Fact]
    public void Clear_ShouldNotThrow_WhenConsoleIsRedirectedAndLogExists()
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());
        services.Write("abc");

        var action = () => services.Clear();

        action.Should().NotThrow();
        services.GetInternalLog().Should().BeEmpty();
    }

    [Fact]
    public void Write_ShouldAppendToProvidedBuilder_AndInternalLog()
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());
        var builder = new StringBuilder();

        services.Write("abc", sb: builder);

        builder.ToString().Should().Be("abc");
        services.GetInternalLog().Should().Be("abc");
        console.Writes.Should().ContainSingle().Which.Should().Be("abc");
    }

    [Fact]
    public void WriteLine_ShouldAppendNewLineToProvidedBuilder_AndInternalLog()
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());
        var builder = new StringBuilder();

        services.WriteLine("abc", sb: builder);

        builder.ToString().Should().Be("abc" + Environment.NewLine);
        services.GetInternalLog().Should().Be("abc" + Environment.NewLine);
        console.Writes.Should().ContainSingle().Which.Should().Be("abc" + Environment.NewLine);
    }

    [Fact]
    public void Write_ShouldTreatNullStringAsEmpty()
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.Write(null!);

        services.GetInternalLog().Should().BeEmpty();
        console.Writes.Should().ContainSingle().Which.Should().BeEmpty();
    }

    [Fact]
    public void EraseAndWrite_WithNegativeLength_ShouldNotThrow()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var action = () => services.EraseAndWrite(-5, "abc");

        action.Should().NotThrow();
        services.GetInternalLog().Should().Be("abc");
    }

    [Fact]
    public void EraseAndWrite_WithOldAndNewText_ShouldWriteExpectedReplacementPattern()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        services.EraseAndWrite("abcd", "xy");

        services.GetInternalLog().Should().Be("\b\b\b\bxy  \b\b");
    }

    [Fact]
    public void ProcessGOSResult_ShouldReturnTrue_WhenResultIsSuccessful()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());
        var log = new StringBuilder();

        var result = services.ProcessGOSResult(new GOSResult(true), log);

        result.Should().BeTrue();
        log.ToString().Should().BeEmpty();
        services.GetInternalLog().Should().BeEmpty();
    }

    [Fact]
    public void ProcessGOSResult_ShouldWriteMessageAndException_WhenResultFails()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());
        var log = new StringBuilder();
        var exception = new InvalidOperationException("boom");

        var result = services.ProcessGOSResult(new GOSResult(false, exception, "failed"), log);

        result.Should().BeFalse();
        log.ToString().Should().Be("failedEXCEPTION: boom");
        services.GetInternalLog().Should().Be("failedEXCEPTION: boom");
    }

    [Fact]
    public void ProcessGOSResult_ShouldThrow_WhenLogBuilderIsNull()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var action = () => services.ProcessGOSResult(new GOSResult(false, "error"), null!);

        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RunProcess_ShouldReturnFailure_WhenFileNameIsEmpty()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var result = services.RunProcess(" ", string.Empty, null, useShellWindow: false);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("File name cannot be null or empty");
    }

    [Fact]
    public void RunProcess_ShouldReturnFailure_WhenWorkingDirectoryDoesNotExist()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var result = services.RunProcess("dotnet", string.Empty, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")), useShellWindow: false);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Working directory does not exist");
    }

    [Fact]
    public void ExecCommandLine_ShouldReturnFailure_WhenCommandIsEmpty()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var result = services.ExecCommandLine(" ", null, null, isAsync: false, isShell: false, isQuite: true, isEscaped: false);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Command cannot be null or empty");
    }

    [Fact]
    public void ExecCommandLine_ShouldReturnFailure_WhenWorkingDirectoryDoesNotExist()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var result = services.ExecCommandLine("dotnet", null, Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")), isAsync: false, isShell: false, isQuite: true, isEscaped: false);

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Working directory does not exist");
    }

    [Fact]
    public void OpenFile_ShouldNotThrow_WhenPathIsInvalid()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var action = () => services.OpenFile(" ", 0);

        action.Should().NotThrow();
    }

    [Fact]
    public void OpenFile_ShouldNotThrow_WhenFileDoesNotExist()
    {
        var services = new ConsoleServices(new FakeConsoleOutput(), new FakeProcessRunner());

        var action = () => services.OpenFile(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt"), 0);

        action.Should().NotThrow();
    }

    [Fact]
    public void DialogYesNo_ShouldReturnTrue_WhenInputIsRedirected()
    {
        var console = new FakeConsoleOutput { IsInputRedirectedValue = true };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        var result = services.DialogYesNo("continue?");

        result.Should().BeTrue();
        console.Writes.Should().Contain(x => x.Contains("continue?"));
        console.Writes.Should().Contain(x => x.Contains("[Y/n]: "));
    }

    [Fact]
    public void WriteProgress_ShouldHandleNegativeProgress()
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.WriteProgress(-1);

        console.Writes.Should().ContainSingle().Which.Should().Be("/");
    }

    [Theory]
    [InlineData(-10, "[          ]   0%")]
    [InlineData(0, "[          ]   0%")]
    [InlineData(56, "[■■■■■■    ]  56%")]
    [InlineData(100, "[■■■■■■■■■■] 100%")]
    [InlineData(150, "[■■■■■■■■■■] 100%")]
    public void WriteProgressBar_ShouldClampPercent(int percent, string expected)
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.WriteProgressBar(percent);

        console.Writes.Should().ContainSingle().Which.Should().Be(expected);
    }

    [Fact]
    public void WriteProgressBar_ShouldSupportUpdateMode()
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.WriteProgressBar(20, progress: 1, update: true);

        console.Writes.Should().ContainSingle();
        console.Writes[0].Should().StartWith("\b" + new string('\b', 17) + "\\");
        console.Writes[0].Should().EndWith("[■■        ]  20%");
    }

    [Fact]
    public void WriteProgressBar_ShouldNotEmitBackspaces_WhenOutputRedirected()
    {
        var console = new FakeConsoleOutput { IsOutputRedirectedValue = true };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.WriteProgressBar(20, progress: 1, update: true);

        console.Writes.Should().ContainSingle();
        console.Writes[0].Should().NotContain("\b");
        console.Writes[0].Should().EndWith("[■■        ]  20%");
    }

    [Fact]
    public void WriteProgress_ShouldNotEmitBackspace_WhenOutputRedirected()
    {
        var console = new FakeConsoleOutput { IsOutputRedirectedValue = true };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.WriteProgress(1, update: true);

        console.Writes.Should().NotContain("\b");
    }

    [Fact]
    public void Write_ShouldSetAndResetColor_WhenColorIsProvided()
    {
        var console = new FakeConsoleOutput();
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.Write("abc", 3);

        console.ColorCalls.Should().ContainInOrder("Set:Red", "Reset");
    }

    [Fact]
    public void ExecCommandLine_ShouldUseInjectedRunner()
    {
        var console = new FakeConsoleOutput();
        var runner = new FakeProcessRunner
        {
            Result = new ProcessRunResult(true, true, 0, "ok", string.Empty, null)
        };
        var services = new ConsoleServices(console, runner);

        var result = services.ExecCommandLine("tool", "--arg", null, isAsync: false, isShell: false, isQuite: true, isEscaped: false);

        result.Success.Should().BeTrue();
        runner.LastStartInfo.Should().NotBeNull();
        runner.LastStartInfo!.FileName.Should().Be("tool");
        runner.LastStartInfo.Arguments.Should().Be("--arg");
        runner.LastWaitForExit.Should().BeTrue();
    }

    [Fact]
    public void RunProcess_ShouldUseInjectedRunner()
    {
        var console = new FakeConsoleOutput();
        var runner = new FakeProcessRunner
        {
            Result = new ProcessRunResult(true, true, 0, "version", string.Empty, null)
        };
        var services = new ConsoleServices(console, runner);

        var result = services.RunProcess("dotnet", "--version", null, useShellWindow: false);

        result.Success.Should().BeTrue();
        runner.LastStartInfo.Should().NotBeNull();
        runner.LastStartInfo!.FileName.Should().Be("dotnet");
        runner.LastWaitForExit.Should().BeTrue();
    }

    [Fact]
    public void OpenFile_ShouldUseInjectedRunner_WhenFileExists()
    {
        var file = Path.GetTempFileName();
        try
        {
            var console = new FakeConsoleOutput();
            var runner = new FakeProcessRunner
            {
                Result = new ProcessRunResult(true, true, null, string.Empty, string.Empty, null)
            };
            var services = new ConsoleServices(console, runner);

            services.OpenFile(file, 0);

            runner.LastStartInfo.Should().NotBeNull();
            runner.LastWaitForExit.Should().BeFalse();
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void RunProcess_ShouldCaptureStdoutAndReturnSuccess_ForDotnetVersion()
    {
        var services = new ConsoleServices();

        var result = services.RunProcess("dotnet", "--version", null, useShellWindow: false);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Exit code: 0");
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ExecCommandLine_ShouldCaptureOutput_ForDotnetVersion()
    {
        var services = new ConsoleServices();

        var result = services.ExecCommandLine("dotnet", "--version", null, isAsync: false, isShell: false, isQuite: true, isEscaped: false);

        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Exit code: 0");
        result.Message.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ExecCommandLine_ShouldReturnSuccess_WhenStartedAsynchronously()
    {
        var services = new ConsoleServices();

        var result = services.ExecCommandLine("dotnet", "--version", null, isAsync: true, isShell: false, isQuite: true, isEscaped: false);

        result.Success.Should().BeTrue();
        result.Message.Should().Be("Process started asynchronously.");
    }

    // ------------------------------------------------------------------
    // Testes: área de status fixa (UpdateStatusLines / WriteLineKeepingStatus / ClearStatusLines)
    // ------------------------------------------------------------------

    [Fact]
    public void UpdateStatusLines_NonRedirected_EmitsAnsiAndStoresLines()
    {
        var console = new FakeConsoleOutput { WindowWidthValue = 80 };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.UpdateStatusLines("Processados: 1/10", "Arquivo: foo.cif");

        // Primeira chamada: _statusLineCount era 0, não há cursor-up.
        // Deve conter apaga-linha (\r\x1B[2K) + conteúdo + \n para cada linha.
        string allWrites = string.Concat(console.Writes);
        allWrites.Should().Contain("\r\x1B[2K");
        allWrites.Should().Contain("Processados: 1/10");
        allWrites.Should().Contain("Arquivo: foo.cif");
    }

    [Fact]
    public void UpdateStatusLines_SecondCall_EmitsCursorUpToOverwrite()
    {
        var console = new FakeConsoleOutput { WindowWidthValue = 80 };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.UpdateStatusLines("Linha 1", "Linha 2");
        console.Writes.Clear();

        services.UpdateStatusLines("Linha 1 nova", "Linha 2 nova");

        // Segunda chamada deve subir o cursor 2 linhas (\x1B[2A).
        string allWrites = string.Concat(console.Writes);
        allWrites.Should().Contain("\x1B[2A");
        allWrites.Should().Contain("Linha 1 nova");
        allWrites.Should().Contain("Linha 2 nova");
    }

    [Fact]
    public void UpdateStatusLines_Redirected_DoesNotEmitAnsiEscape()
    {
        var console = new FakeConsoleOutput { IsOutputRedirectedValue = true, WindowWidthValue = 80 };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.UpdateStatusLines("Progresso: 50%", "Arquivo: bar.cif");

        // Saída redirecionada: nenhum escape ANSI deve ser emitido.
        string allWrites = string.Concat(console.Writes);
        allWrites.Should().NotContain("\x1B");
        allWrites.Should().BeEmpty(); // nada escrito no output redirecionado
    }

    [Fact]
    public void WriteLineKeepingStatus_NonRedirected_ReprinstsStatusAfterLogLine()
    {
        var console = new FakeConsoleOutput { WindowWidthValue = 80 };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.UpdateStatusLines("Status 1", "Status 2");
        console.Writes.Clear();

        services.WriteLineKeepingStatus("[ERRO] algo deu errado", color: 3);

        string allWrites = string.Concat(console.Writes);
        // Deve conter: cursor-up, a linha de log, e reimpressão do status.
        allWrites.Should().Contain("\x1B[2A");
        allWrites.Should().Contain("[ERRO] algo deu errado");
        allWrites.Should().Contain("Status 1");
        allWrites.Should().Contain("Status 2");
        // Deve aplicar cor vermelha.
        console.ColorCalls.Should().Contain("Set:Red");
    }

    [Fact]
    public void WriteLineKeepingStatus_Redirected_WritesLineDirectly()
    {
        var console = new FakeConsoleOutput { IsOutputRedirectedValue = true, WindowWidthValue = 80 };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.UpdateStatusLines("status");
        console.Writes.Clear();

        services.WriteLineKeepingStatus("[ERRO] falhou", color: 3);

        // Saída redirecionada: apenas a linha de log, sem escape ANSI.
        string allWrites = string.Concat(console.Writes);
        allWrites.Should().Contain("[ERRO] falhou");
        allWrites.Should().NotContain("\x1B");
    }

    [Fact]
    public void ClearStatusLines_NonRedirected_EmitsEraseThenResetsCount()
    {
        var console = new FakeConsoleOutput { WindowWidthValue = 80 };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.UpdateStatusLines("L1", "L2");
        console.Writes.Clear();

        services.ClearStatusLines();

        string allWrites = string.Concat(console.Writes);
        // Deve conter cursor-up e apagar linhas.
        allWrites.Should().Contain("\x1B[2A");
        allWrites.Should().Contain("\r\x1B[2K");

        // Após clear: segunda chamada a UpdateStatusLines não deve emitir cursor-up.
        console.Writes.Clear();
        services.UpdateStatusLines("Nova linha");
        string after = string.Concat(console.Writes);
        after.Should().NotContain("\x1B[1A").And.NotContain("\x1B[2A");
    }

    [Fact]
    public void UpdateStatusLines_TruncatesLongLines()
    {
        var console = new FakeConsoleOutput { WindowWidthValue = 20 };
        var services = new ConsoleServices(console, new FakeProcessRunner());

        services.UpdateStatusLines("Esta linha é muito longa para a janela");

        string allWrites = string.Concat(console.Writes);
        // Nenhuma escrita individual deve exceder a largura da janela + escapes.
        // O conteúdo visível deve ser truncado com "..."
        allWrites.Should().Contain("...");
        // Primeiros (20-3)=17 chars + "..." — cabe exatamente em 20 colunas.
        allWrites.Should().Contain("Esta linha é muit...");
    }

    // ------------------------------------------------------------------

    private sealed class FakeConsoleOutput : IConsoleOutput
    {
        public bool IsInputRedirectedValue { get; set; }
        public bool IsOutputRedirectedValue { get; set; }
        public int WindowWidthValue { get; set; } = 120;
        public List<string> Writes { get; } = new();
        public List<string> ColorCalls { get; } = new();

        public bool IsInputRedirected => IsInputRedirectedValue;

        public bool IsOutputRedirected => IsOutputRedirectedValue;

        public int GetWindowWidth() => WindowWidthValue;

        public void Clear() => Writes.Add("<clear>");

        public ConsoleKeyInfo ReadKey(bool intercept = false) => new('\r', ConsoleKey.Enter, false, false, false);

        public void ResetColor() => ColorCalls.Add("Reset");

        public void SetForegroundColor(ConsoleColor color) => ColorCalls.Add($"Set:{color}");

        public void Write(string value) => Writes.Add(value);

        public void WriteLine(string? value = null) => Writes.Add((value ?? string.Empty) + Environment.NewLine);
    }

    private sealed class FakeProcessRunner : IProcessRunner
    {
        public ProcessRunResult Result { get; set; } = new(true, true, 0, string.Empty, string.Empty, null);
        public ProcessStartInfo? LastStartInfo { get; private set; }
        public bool LastWaitForExit { get; private set; }

        public ProcessRunResult Run(ProcessStartInfo startInfo, bool waitForExit)
        {
            LastStartInfo = startInfo;
            LastWaitForExit = waitForExit;
            return Result;
        }
    }
}
