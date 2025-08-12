using System.Diagnostics;
using System.Text;

namespace BaseLibrary;

public class ConsoleServices : IConsoleServices
{
    StringBuilder? internalLog = null;

    public void InitizaliseInternalLog() => internalLog = new();
    public string? GetInternalLog() => internalLog?.ToString();

    //https://www.codeproject.com/Tips/5255878/A-Console-Progress-Bar-in-Csharp
    const char _block = '■';
    const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b"; //total: 17
    const string _twirl = "-\\|/";
    public void WriteProgressBar(int percent, int progress = -1, bool update = false)
    {
        var p = (int)((percent / 10f) + .5f);
        //int progress;
        //if (currentValue < 0)
        //    progress = -1;
        //else
        //    progress = ;
        if (update)
            Console.Write((progress >= 0 ? "\b" : "") + _back +
                (progress >= 0 ? _twirl[progress % _twirl.Length] : "") +
                "[" + new string(_block, p) + new string(' ', 10 - p) + string.Format("] {0,3:##0}%", percent));
        else
            Console.Write((progress >= 0 ? _twirl[progress % _twirl.Length] : "") +
                "[" + new string(_block, p) + new string(' ', 10 - p) + string.Format("] {0,3:##0}%", percent));
    }
    public void WriteProgress(int progress, bool update = false)
    {
        if (update)
            Console.Write("\b");
        Console.Write(_twirl[progress % _twirl.Length]);
    }
    public GOSResult ExecCommandLine(string cmd, string? args, string? workFolder, bool isAsync, bool isShell, bool isQuite, bool isEscaped)
    {
        if (args is null)
        {
            args = string.Empty;
        }
        if (workFolder is null)
        {
            workFolder = string.Empty;
        }

        string cmdEscaped = cmd.Replace("\"", "\\\"");
        var argsEscaped = args.Replace("\"", "\\\"");
        string message = string.Empty;


        using (Process process = new())
        {
            process.StartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = !isShell,
                UseShellExecute = isShell,
                CreateNoWindow = isQuite,
                WindowStyle = isQuite ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
                WorkingDirectory = workFolder,
                //FileName = "bash",//"/bin/bash"
                //Arguments = $"-c \"{escapedArgs}\""
                //FileName = cmdEscaped,
                //Arguments = argsEscaped
                FileName = isEscaped ? cmdEscaped : cmd,
                Arguments = isEscaped ? argsEscaped : args,
                //ArgumentList = argList
            };
            try
            {
                //if (!isQuite)
                //{
                //    Console.Write("Executing " + cmd + "...");
                //}
                process.Start();
                if (process.StartInfo.RedirectStandardOutput && !isAsync)
                {
                    message = process.StandardOutput.ReadToEnd();

                    if (!isQuite)
                        Console.WriteLine(message);

                }
                if (!isAsync)
                {
                    process.WaitForExit();
                    message = "Exit code: " + process.ExitCode + (string.IsNullOrWhiteSpace(message) ? "" : Environment.NewLine + message);
                    if (!isQuite && process.ExitCode != 0)
                        Console.Write(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new GOSResult(false, e, message);
            }
        }
        ;
        //Console.WriteLine();
        return new GOSResult(true, null, message);
    }
    public GOSResult RunProcess(string fileName, string arguments, string? workFolder, bool useShellWindow)
    {
        StringBuilder sb = new();
        //dotnet publish SindarinProgram.csproj -p:PublishProfile=windowsx64
        using (Process process = new())
        {
            // redirect the output
            //process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.RedirectStandardError = true;

            process.StartInfo = new()
            {
                FileName = fileName,
                WorkingDirectory = workFolder,
                //ArgumentList = { "publish", "Nimloth.Desktop.csproj", "-p:PublishProfile=" + name },
                Arguments = arguments,
                CreateNoWindow = !useShellWindow,
                UseShellExecute = useShellWindow,
                //WindowStyle = ProcessWindowStyle.Normal
                RedirectStandardOutput = !useShellWindow,
                RedirectStandardError = !useShellWindow,
            };
            if (workFolder is not null)
                process.StartInfo.WorkingDirectory = workFolder;

            if (!useShellWindow)
            {
                process.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
                process.ErrorDataReceived += (sender, args) => sb.AppendLine($"ERR: {args.Data}");
            }
            process.Start();

            //string message = process.StandardOutput.ReadToEnd();
            //string messageerror = process.StandardError.ReadToEnd();

            if (!useShellWindow)
            {
                // start our event pumps
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }

            process.WaitForExit();

            string trash = sb.ToString();

            if (process.ExitCode == 0)
            {
                //console.WriteLine(" done", 1);
            }
            else
            {
                WriteLine("Exit Code error: " + process.ExitCode);

                if (!useShellWindow)
                {
                    //WriteLine(sb.ToString());

                    //Console.WriteLine("Output: " + message);
                    //Console.WriteLine("Error: " + messageerror);
                    //Console.WriteLine("--------");
                    //Console.WriteLine("Output: " + process.StandardOutput.ReadToEnd());
                    //Console.WriteLine("--------");
                    //Console.WriteLine("Error: " + process.StandardError.ReadToEnd());
                }
                return new(false, sb.ToString());
            }
        }

        return new(true, sb.ToString());

    }
    public bool DialogYesNo(string message)
    {
        Console.Write(message);
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(" [Y/n]: ");
        Console.ResetColor();
        bool answer = true;
        do
        {
            ConsoleKeyInfo x = Console.ReadKey();

            if (x.Key == ConsoleKey.Y || x.Key == ConsoleKey.Enter)
            {
                if (x.Key == ConsoleKey.Enter)
                {
                    Console.Write("Y");
                }
                break;
            }
            else if (x.Key == ConsoleKey.N || x.Key == ConsoleKey.Escape)
            {
                if (x.Key == ConsoleKey.Escape)
                {
                    Console.Write("n");
                }
                answer = false;
                break;
            }
            else
            {
                Console.Write("\b \b");
            }
        } while (true);
        Console.WriteLine();
        return answer;
    }
    public void Write(string str, int color = 0, StringBuilder? sb = null)
    {
        if (color != 0)
        {
            if (color == 1)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (color == 2)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (color == 3)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (color == 4)
                Console.ForegroundColor = ConsoleColor.Gray;
        }
        Console.Write(str);

        sb?.Append(str);
        internalLog?.Append(str);

        if (color != 0)
        {
            Console.ResetColor();
        }
    }
    public void WriteLine(string str = "", int color = 0, StringBuilder? sb = null)
    {
        Write(str, color, sb);
        Console.WriteLine();
        sb?.AppendLine();
        internalLog?.AppendLine();
    }
    public void Clear()
    {
        Console.Clear();
        internalLog.Clear();
    }
    public void EraseAndWrite(int eraseLength, string str, int color = 0)
    {
        Write(new string('\b', eraseLength) + str + (str.Length < eraseLength ? new string(' ', eraseLength - str.Length) : ""), color);
    }
    public void EraseAndWrite(string oldStr, string newStr, int color = 0)
    {
        Write(new string('\b', oldStr.Length) + newStr + (newStr.Length < oldStr.Length ? new string(' ', oldStr.Length - newStr.Length) + new string('\b', oldStr.Length - newStr.Length) : ""), color);
    }
    public bool ProcessGOSResult(GOSResult gosResult, StringBuilder logTemp)
    {
        if (!gosResult.Success)
        {
            if (!string.IsNullOrWhiteSpace(gosResult.Message))
            {
                Write(gosResult.Message, 3);
                logTemp.Append(gosResult.Message);
            }
            if (gosResult.Exception is not null)
            {
                Write("EXCEPTION: " + gosResult.Exception.Message, 3);
                logTemp.Append("EXCEPTION: " + gosResult.Exception.Message);
            }
        }
        return gosResult.Success;
    }
    public void OpenFile(string fileName, sbyte os)
    {
        using (Process process = new())
        {
            string? commandProcess = null;
            string argsProcess = "";
            if (os == 0)
            {
                commandProcess = "\"" + fileName + "\"";// Path.Combine(sindarinFolder, "Sindarin manual.pdf");
            }
            else if (os == 1)
            {
                commandProcess = "xdg-open";
                argsProcess = fileName;
            }
            else if (os == 2)
            {
                commandProcess = "open";
                argsProcess = fileName;
            }
            process.StartInfo = new()
            {
                FileName = fileName, //"\"" + fileManual + "\"",
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                //WorkingDirectory = sindarinFolder,
                Arguments = argsProcess
            };

#pragma warning disable CS0168 // Variable is declared but never used
            try
            {
                if (File.Exists(fileName))
                {
                    process.Start();
                }
                else
                {
                    //Console.WriteLine("File not found: " + Path.Combine(sindarinFolder, fileManual));
                }
            }
            catch (Exception e)
            {
                //Console.WriteLine("Problem trying to open the file \"" + Path.Combine(sindarinFolder, fileManual) + "\"");
                //Console.WriteLine(e.Message);
                //ExceptionMethods.SendException(emailTo, e, false, null);
            }
#pragma warning restore CS0168 // Variable is declared but never used
        }
    }
}
