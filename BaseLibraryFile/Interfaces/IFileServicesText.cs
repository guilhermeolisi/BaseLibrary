using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IFileServicesText
{
    bool WriteTXT(string pathFile, in string parTXT);

    Task<bool> WriteTXTAsync(string pathFile, string parTXT);
    string? ReadTXT(string pathFile);
    Task<string?> ReadTXTAsync(string pathFile);
}
