using System.Diagnostics;
using System.Text;

namespace BaseLibrary;

public class ConsoleServices : IConsoleServices
{
    private const char Block = '■';
    private const string Back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
    private const string Twirl = "-\\|/";

    private readonly IConsoleOutput consoleOutput;
    private readonly IProcessRunner processRunner;
    private readonly object syncRoot = new();
    private StringBuilder internalLog = new();

    // Linhas de status fixas ("sticky") no rodapé do terminal.
    private string[] _statusLines = [];
    private int _statusLineCount = 0;

    public ConsoleServices(IConsoleOutput? consoleOutput = null, IProcessRunner? processRunner = null)
    {
        this.consoleOutput = consoleOutput ?? new SystemConsoleOutput();
        this.processRunner = processRunner ?? new DefaultProcessRunner();
    }

    public void InitializeInternalLog() => InitizaliseInternalLog();

    public void InitizaliseInternalLog()
    {
        lock (syncRoot)
        {
            internalLog = new StringBuilder();
        }
    }

    public string? GetInternalLog()
    {
        lock (syncRoot)
        {
            return internalLog.ToString();
        }
    }

    public void WriteProgressBar(int percent, int progress = -1, bool update = false)
    {
        int normalizedPercent = Math.Clamp(percent, 0, 100);
        int blocks = (int)((normalizedPercent / 10f) + .5f);
        int spinnerIndex = NormalizeSpinnerIndex(progress);
        string spinner = progress >= 0 ? Twirl[spinnerIndex].ToString() : string.Empty;
        // Só reescreve a linha (backspaces) em terminal interativo. Com a saída
        // redirecionada (pipe/arquivo/log), os \b virariam lixo no log.
        bool canRewrite = update && !consoleOutput.IsOutputRedirected;
        string rewritePrefix = canRewrite ? (progress >= 0 ? "\b" : string.Empty) + Back : string.Empty;
        string message = rewritePrefix +
            spinner +
            "[" + new string(Block, blocks) + new string(' ', 10 - blocks) + string.Format("] {0,3:##0}%", normalizedPercent);

        consoleOutput.Write(message);
    }

    public void WriteProgress(int progress, bool update = false)
    {
        if (update && !consoleOutput.IsOutputRedirected)
            consoleOutput.Write("\b");

        consoleOutput.Write(Twirl[NormalizeSpinnerIndex(progress)].ToString());
    }

    public GOSResult ExecCommandLine(string cmd, string? args, string? workFolder, bool isAsync, bool isShell, bool isQuite, bool isEscaped)
    {
        if (string.IsNullOrWhiteSpace(cmd))
            return new GOSResult(false, null, "Command cannot be null or empty.");

        if (!string.IsNullOrWhiteSpace(workFolder) && !Directory.Exists(workFolder))
            return new GOSResult(false, null, $"Working directory does not exist: {workFolder}");

        string arguments = args ?? string.Empty;
        string fileName = isEscaped ? cmd.Replace("\"", "\\\"") : cmd;
        string finalArguments = isEscaped ? arguments.Replace("\"", "\\\"") : arguments;

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = fileName,
                Arguments = finalArguments,
                WorkingDirectory = string.IsNullOrWhiteSpace(workFolder) ? Environment.CurrentDirectory : workFolder,
                UseShellExecute = isShell,
                RedirectStandardOutput = !isShell,
                RedirectStandardError = !isShell,
                CreateNoWindow = isQuite,
                WindowStyle = isQuite ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
            };

            ProcessRunResult runResult = processRunner.Run(startInfo, waitForExit: !isAsync);

            if (runResult.Exception is not null)
            {
                if (!isQuite)
                    consoleOutput.WriteLine(runResult.Exception.ToString());

                return new GOSResult(false, runResult.Exception, runResult.Exception.Message);
            }

            if (!runResult.Started)
                return new GOSResult(false, null, "The process could not be started.");

            if (isAsync)
                return new GOSResult(true, null, "Process started asynchronously.");

            string standardOutput = runResult.StandardOutput;
            string standardError = runResult.StandardError;
            string message = BuildProcessMessage(runResult.ExitCode ?? 0, standardOutput, standardError);

            if (!isQuite && !string.IsNullOrWhiteSpace(standardOutput))
                consoleOutput.WriteLine(standardOutput);

            if (!isQuite && !runResult.Success && !string.IsNullOrWhiteSpace(standardError))
                consoleOutput.WriteLine(standardError);

            return new GOSResult(runResult.Success, null, message);
        }
        catch (Exception e)
        {
            if (!isQuite)
                consoleOutput.WriteLine(e.ToString());

            return new GOSResult(false, e, e.Message);
        }
    }

    public GOSResult RunProcess(string fileName, string arguments, string? workFolder, bool useShellWindow)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return new GOSResult(false, null, "File name cannot be null or empty.");

        if (!string.IsNullOrWhiteSpace(workFolder) && !Directory.Exists(workFolder))
            return new GOSResult(false, null, $"Working directory does not exist: {workFolder}");

        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = fileName,
                Arguments = arguments ?? string.Empty,
                WorkingDirectory = string.IsNullOrWhiteSpace(workFolder) ? Environment.CurrentDirectory : workFolder,
                CreateNoWindow = !useShellWindow,
                UseShellExecute = useShellWindow,
                RedirectStandardOutput = !useShellWindow,
                RedirectStandardError = !useShellWindow,
            };

            ProcessRunResult runResult = processRunner.Run(startInfo, waitForExit: true);

            if (runResult.Exception is not null)
            {
                WriteLine(runResult.Exception.Message, 3);
                return new GOSResult(false, runResult.Exception, runResult.Exception.Message);
            }

            if (!runResult.Started)
                return new GOSResult(false, null, "The process could not be started.");

            string message = BuildProcessMessage(runResult.ExitCode ?? 0, runResult.StandardOutput, runResult.StandardError);
            if (!runResult.Success)
                WriteLine("Exit Code error: " + (runResult.ExitCode ?? -1), 3);

            return new GOSResult(runResult.Success, null, message);
        }
        catch (Exception e)
        {
            WriteLine(e.Message, 3);
            return new GOSResult(false, e, e.Message);
        }
    }

    public bool DialogYesNo(string message)
    {
        consoleOutput.Write(message);
        TrySetForegroundColor(ConsoleColor.Blue);
        consoleOutput.Write(" [Y/n]: ");
        TryResetColor();

        if (consoleOutput.IsInputRedirected)
        {
            consoleOutput.WriteLine("Y");
            return true;
        }

        bool answer = true;
        do
        {
            ConsoleKeyInfo x = consoleOutput.ReadKey();

            if (x.Key == ConsoleKey.Y || x.Key == ConsoleKey.Enter)
            {
                if (x.Key == ConsoleKey.Enter)
                    consoleOutput.Write("Y");

                break;
            }

            if (x.Key == ConsoleKey.N || x.Key == ConsoleKey.Escape)
            {
                if (x.Key == ConsoleKey.Escape)
                    consoleOutput.Write("n");

                answer = false;
                break;
            }

            consoleOutput.Write("\b \b");
        }
        while (true);

        consoleOutput.WriteLine();
        return answer;
    }

    public void Write(string str, int color = 0, StringBuilder? sb = null)
    {
        string value = str ?? string.Empty;

        lock (syncRoot)
        {
            if (color != 0)
                TrySetForegroundColor(GetConsoleColor(color));

            consoleOutput.Write(value);
            sb?.Append(value);
            internalLog.Append(value);

            if (color != 0)
                TryResetColor();
        }
    }

    public void WriteLine(string str = "", int color = 0, StringBuilder? sb = null)
    {
        string value = str ?? string.Empty;

        lock (syncRoot)
        {
            if (color != 0)
                TrySetForegroundColor(GetConsoleColor(color));

            consoleOutput.WriteLine(value);
            sb?.AppendLine(value);
            internalLog.AppendLine(value);

            if (color != 0)
                TryResetColor();
        }
    }

    public void Clear()
    {
        lock (syncRoot)
        {
            try
            {
                consoleOutput.Clear();
            }
            catch (IOException)
            {
            }
            catch (PlatformNotSupportedException)
            {
            }
            catch (InvalidOperationException)
            {
            }

            internalLog.Clear();
        }
    }

    public void EraseAndWrite(int eraseLength, string str, int color = 0)
    {
        int safeEraseLength = Math.Max(eraseLength, 0);
        string value = str ?? string.Empty;
        Write(new string('\b', safeEraseLength) + value + (value.Length < safeEraseLength ? new string(' ', safeEraseLength - value.Length) : string.Empty), color);
    }

    public void EraseAndWrite(string oldStr, string newStr, int color = 0)
    {
        string previous = oldStr ?? string.Empty;
        string current = newStr ?? string.Empty;
        Write(new string('\b', previous.Length) + current + (current.Length < previous.Length ? new string(' ', previous.Length - current.Length) + new string('\b', previous.Length - current.Length) : string.Empty), color);
    }

    public bool ProcessGOSResult(GOSResult gosResult, StringBuilder logTemp)
    {
        ArgumentNullException.ThrowIfNull(logTemp);

        if (!gosResult.Success)
        {
            if (!string.IsNullOrWhiteSpace(gosResult.Message))
            {
                Write(gosResult.Message, 3);
                logTemp.Append(gosResult.Message);
            }

            if (gosResult.Exception is not null)
            {
                string exceptionMessage = "EXCEPTION: " + gosResult.Exception.Message;
                Write(exceptionMessage, 3);
                logTemp.Append(exceptionMessage);
            }
        }

        return gosResult.Success;
    }

    public void OpenFile(string fileName, sbyte os)
    {
        if (string.IsNullOrWhiteSpace(fileName) || !File.Exists(fileName))
            return;

        try
        {
            processRunner.Run(CreateOpenFileStartInfo(fileName, os), waitForExit: false);
        }
        catch (Exception)
        {
        }
    }

    private static ProcessStartInfo CreateOpenFileStartInfo(string fileName, sbyte os)
    {
        return os switch
        {
            1 => new ProcessStartInfo
            {
                FileName = "xdg-open",
                Arguments = fileName,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            },
            2 => new ProcessStartInfo
            {
                FileName = "open",
                Arguments = fileName,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            },
            _ => new ProcessStartInfo
            {
                FileName = fileName,
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
            }
        };
    }

    // ------------------------------------------------------------------
    // Área de status fixa ("sticky") no rodapé do terminal
    // ------------------------------------------------------------------

    /// <inheritdoc/>
    public void UpdateStatusLines(params string[] lines)
    {
        lock (syncRoot)
        {
            // Saída redirecionada: não emite escape ANSI; o chamador deve usar
            // WriteLine para progresso periódico quando necessário.
            if (consoleOutput.IsOutputRedirected)
            {
                _statusLines = lines;
                _statusLineCount = lines.Length;
                return;
            }

            // Sobe o cursor até o início da área sticky anterior.
            if (_statusLineCount > 0)
                consoleOutput.Write($"\x1B[{_statusLineCount}A");

            int width = GetSafeWindowWidth();
            foreach (string line in lines)
            {
                // Apaga a linha inteira e escreve o novo conteúdo.
                consoleOutput.Write("\r\x1B[2K");
                consoleOutput.Write(TruncateLine(line, width));
                consoleOutput.Write("\n");
            }

            _statusLines = lines;
            _statusLineCount = lines.Length;
        }
    }

    /// <inheritdoc/>
    public void WriteLineKeepingStatus(string line, int color = 0)
    {
        string value = line ?? string.Empty;

        lock (syncRoot)
        {
            if (consoleOutput.IsOutputRedirected)
            {
                // Sem área sticky: escreve a linha normalmente.
                if (color != 0) TrySetForegroundColor(GetConsoleColor(color));
                consoleOutput.WriteLine(value);
                internalLog.AppendLine(value);
                if (color != 0) TryResetColor();
                return;
            }

            // Sobe até o início da área sticky, apaga a linha atual e imprime o log.
            if (_statusLineCount > 0)
                consoleOutput.Write($"\x1B[{_statusLineCount}A");

            consoleOutput.Write("\r\x1B[2K");
            if (color != 0) TrySetForegroundColor(GetConsoleColor(color));
            consoleOutput.WriteLine(value);
            internalLog.AppendLine(value);
            if (color != 0) TryResetColor();

            // Reimprime as linhas sticky abaixo da linha de log recém-escrita.
            int width = GetSafeWindowWidth();
            foreach (string statusLine in _statusLines)
            {
                consoleOutput.Write("\r\x1B[2K");
                consoleOutput.Write(TruncateLine(statusLine, width));
                consoleOutput.Write("\n");
            }
        }
    }

    /// <inheritdoc/>
    public void ClearStatusLines()
    {
        lock (syncRoot)
        {
            if (!consoleOutput.IsOutputRedirected && _statusLineCount > 0)
            {
                // Sobe até o início da área sticky, apaga cada linha e desce.
                consoleOutput.Write($"\x1B[{_statusLineCount}A");
                for (int i = 0; i < _statusLineCount; i++)
                {
                    consoleOutput.Write("\r\x1B[2K");
                    if (i < _statusLineCount - 1)
                        consoleOutput.Write("\n");
                }
                // Cursor fica na primeira linha da antiga área sticky, pronto para
                // novos writes sobrescreverem o espaço previamente ocupado.
            }

            _statusLines = [];
            _statusLineCount = 0;
        }
    }

    // ------------------------------------------------------------------

    private int GetSafeWindowWidth()
    {
        try { return consoleOutput.GetWindowWidth(); }
        catch { return 120; }
    }

    private static string TruncateLine(string line, int maxWidth)
    {
        if (maxWidth <= 3 || line.Length <= maxWidth) return line;
        return string.Concat(line.AsSpan(0, maxWidth - 3), "...");
    }

    private static int NormalizeSpinnerIndex(int progress)
        => ((progress % Twirl.Length) + Twirl.Length) % Twirl.Length;

    private static string BuildProcessMessage(int exitCode, string standardOutput, string standardError)
    {
        StringBuilder sb = new();
        sb.Append("Exit code: ");
        sb.Append(exitCode);

        if (!string.IsNullOrWhiteSpace(standardOutput))
        {
            sb.AppendLine();
            sb.Append(standardOutput.TrimEnd());
        }

        if (!string.IsNullOrWhiteSpace(standardError))
        {
            sb.AppendLine();
            sb.Append("ERR: ");
            sb.Append(standardError.TrimEnd());
        }

        return sb.ToString();
    }

    private static ConsoleColor GetConsoleColor(int color)
        => color switch
        {
            1 => ConsoleColor.Green,
            2 => ConsoleColor.Yellow,
            3 => ConsoleColor.Red,
            4 => ConsoleColor.Gray,
            _ => ConsoleColor.White
        };

    private void TrySetForegroundColor(ConsoleColor color)
    {
        try
        {
            consoleOutput.SetForegroundColor(color);
        }
        catch (IOException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private void TryResetColor()
    {
        try
        {
            consoleOutput.ResetColor();
        }
        catch (IOException)
        {
        }
        catch (PlatformNotSupportedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private sealed class SystemConsoleOutput : IConsoleOutput
    {
        public bool IsInputRedirected => Console.IsInputRedirected;

        public bool IsOutputRedirected => Console.IsOutputRedirected;

        public int GetWindowWidth()
        {
            try { return Console.WindowWidth; }
            catch { return 120; }
        }

        public void Clear() => Console.Clear();

        public ConsoleKeyInfo ReadKey(bool intercept = false) => Console.ReadKey(intercept);

        public void ResetColor() => Console.ResetColor();

        public void SetForegroundColor(ConsoleColor color) => Console.ForegroundColor = color;

        public void Write(string value) => Console.Write(value);

        public void WriteLine(string? value = null) => Console.WriteLine(value);
    }

    private sealed class DefaultProcessRunner : IProcessRunner
    {
        public ProcessRunResult Run(ProcessStartInfo startInfo, bool waitForExit)
        {
            try
            {
                using Process process = new() { StartInfo = startInfo };
                bool started = process.Start();
                if (!started)
                    return new ProcessRunResult(false, false, null, string.Empty, string.Empty, null);

                if (!waitForExit)
                    return new ProcessRunResult(true, true, null, string.Empty, string.Empty, null);

                string standardOutput = string.Empty;
                string standardError = string.Empty;

                if (startInfo.RedirectStandardOutput)
                {
                    Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                    Task<string> errorTask = process.StandardError.ReadToEndAsync();
                    process.WaitForExit();
                    Task.WhenAll(outputTask, errorTask).GetAwaiter().GetResult();
                    standardOutput = outputTask.Result;
                    standardError = errorTask.Result;
                }
                else
                {
                    process.WaitForExit();
                }

                return new ProcessRunResult(true, process.ExitCode == 0, process.ExitCode, standardOutput, standardError, null);
            }
            catch (Exception e)
            {
                return new ProcessRunResult(false, false, null, string.Empty, string.Empty, e);
            }
        }
    }
}
