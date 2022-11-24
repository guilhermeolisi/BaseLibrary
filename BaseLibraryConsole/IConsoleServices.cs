using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IConsoleServices
{
    void WriteProgressBar(int percent, bool update = false);
    void WriteProgress(int progress, bool update = false);
    GOSResult ExecCommandLine(string cmd, string args, bool isAsync, bool isShell, bool isQuite, bool isEscaped);
    bool DialogYesNo(string message);
    /// <summary>
    /// 0: White; 1: Green; 2: Yellow; 3: Red
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void Write(string str, int color = 0);
    /// <summary>
    ///0: White; 1: Green; 2: Yellow; 3: Red
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void WriteLine(string str, int color = 0);
    /// <summary>
    /// 0: White; 1: Green; 2: Yellow; 3: Red
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void EraseAndWrite(int eraseLength, string str, int color = 0);
    /// <summary>
    /// 0: White; 1: Green; 2: Yellow; 3: Red
    /// </summary>
    /// <param name="str"></param>
    /// <param name="color"></param>
    void EraseAndWrite(string oldStr, string newStr, int color = 0);
    bool ProcessGOSResult(GOSResult gosResult, StringBuilder logTemp);
}
