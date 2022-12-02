using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class ConsoleServices : IConsoleServices
{
    //https://www.codeproject.com/Tips/5255878/A-Console-Progress-Bar-in-Csharp
    const char _block = '■';
    const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
    const string _twirl = "-\\|/";
    public void WriteProgressBar(int percent, bool update = false)
    {
        var p = (int)((percent / 10f) + .5f);
        if (update)
            Console.Write(_back + "[" + new string(_block, p) + new string(' ', 10 - p) + string.Format("] {0,3:##0}%", percent));
        else
            Console.Write("[" + new string(_block, p) + new string(' ', 10 - p) + string.Format("] {0,3:##0}%", percent));
    }
    public void WriteProgress(int progress, bool update = false)
    {
        if (update)
            Console.Write("\b");
        Console.Write(_twirl[progress % _twirl.Length]);
    }
    public GOSResult ExecCommandLine(string cmd, string args, bool isAsync, bool isShell, bool isQuite, bool isEscaped)
    {
        string cmdEscaped = cmd.Replace("\"", "\\\"");
        var argsEscaped = args.Replace("\"", "\\\"");
        string message = string.Empty;
        using (Process process = new())
        {
            process.StartInfo = new ProcessStartInfo
            {
                RedirectStandardOutput = isShell ? false : true,
                UseShellExecute = isShell,
                CreateNoWindow = isQuite,
                WindowStyle = isQuite ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
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
                if (process.StartInfo.RedirectStandardOutput)
                {
                    message = process.StandardOutput.ReadToEnd();

                    if (!isQuite)
                        Console.WriteLine(message);

                }
                if (!isAsync)
                {
                    process.WaitForExit();
                    message = "Exit code: " + process.ExitCode + Environment.NewLine + message;
                    if (!isQuite && process.ExitCode != 0)
                        Console.Write(message);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                return new GOSResult(false, e, message);
            }
        };
        //Console.WriteLine();
        return new GOSResult(true, null, message);
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
    public void Write(string str, int color = 0)
    {
        if (color != 0)
        {
            if (color == 1)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (color == 2)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else if (color == 3)
                Console.ForegroundColor = ConsoleColor.Red;
        }
        Console.Write(str);
        if (color != 0)
        {
            Console.ResetColor();
        }
    }
    public void WriteLine(string str = "", int color = 0)
    {
        Write(str, color);
        Console.WriteLine();
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
