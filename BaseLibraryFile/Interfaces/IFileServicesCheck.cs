using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using Splat;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

public interface IFileServicesCheck
{
    bool CheckZipFile(string path);
    bool CheckTextFileByChars(string filePath);
    bool CheckTextFile(string filePath);
    bool CheckImageFile(string path);
}
