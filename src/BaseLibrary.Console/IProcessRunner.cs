using System.Diagnostics;

namespace BaseLibrary;

public interface IProcessRunner
{
    ProcessRunResult Run(ProcessStartInfo startInfo, bool waitForExit);
}
