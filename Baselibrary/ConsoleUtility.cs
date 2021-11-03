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
        public static bool ExecCommandLineBash(string cmd, bool isAsync)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            using (Process process = new())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    FileName = "bash",//"/bin/bash"
                    Arguments = $"-c \"{escapedArgs}\""
                };
                try
                {
                    process.Start();
                    if (!isAsync)
                    {
                        process.WaitForExit();
                    }
                }
                catch (Exception e)
                {
                    return false;
                }
            };
            return true;
        }
    }
}
