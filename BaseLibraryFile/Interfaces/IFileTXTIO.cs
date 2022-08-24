using Splat;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IFileTXTIO
{
    void SetStayBak(bool value);
    void SetPathFile(string? pathFile);
    void Closing();
    Exception eWrite { get; }
    Task<bool> WriteTXTAsync(string parTXT);
    bool WriteTXT(string parTXT);
    string TextResult { get; }
    Exception eRead { get; }
    Task<bool> ReadTXTAsync();
    bool ReadTXT();
}