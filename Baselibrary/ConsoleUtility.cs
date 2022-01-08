using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary
{
    public static class ConsoleUtility
    {
        //https://www.codeproject.com/Tips/5255878/A-Console-Progress-Bar-in-Csharp
        const char _block = '■';
        const string _back = "\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b\b";
        const string _twirl = "-\\|/";
        public static void WriteProgressBar(int percent, bool update = false)
        {
            var p = (int)((percent / 10f) + .5f);
            if (update)
                Console.Write(_back + "[" + new string(_block, p) + new string(' ', 10 - p) + string.Format("] {0,3:##0}%", percent));
            else
                Console.Write("[" + new string(_block, p) + new string(' ', 10 - p) + string.Format("] {0,3:##0}%", percent));
        }
        public static void WriteProgress(int progress, bool update = false)
        {
            if (update)
                Console.Write("\b");
            Console.Write(_twirl[progress % _twirl.Length]);
        }
        public static bool ExecCommandLine(string cmd, string args, bool isAsync, bool isShell, bool isQuite, bool isEscaped)
        {
            string cmdEscaped = cmd.Replace("\"", "\\\"");
            var argsEscaped = args.Replace("\"", "\\\"");

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
                    Arguments = isEscaped ? argsEscaped : args
                };
                try
                {
                    if (!isQuite)
                    {
                        Console.Write("Executing " + cmd + "...");
                    }
                    process.Start();
                    if (process.StartInfo.RedirectStandardOutput && !isQuite)
                    {
                        Console.WriteLine(process.StandardOutput.ReadToEnd());
                    }
                    if (!isAsync)
                    {
                        process.WaitForExit();
                        if (!isQuite && process.ExitCode != 0)
                            Console.Write("Exit code: " + process.ExitCode);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    return false;
                }
            };
            return true;
        }
    }
}
