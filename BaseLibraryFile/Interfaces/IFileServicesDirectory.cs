using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using Splat;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

public interface IFileServicesDirectory
{
    void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool preserveTime);
    void DirectoryClear(string sourceDirName);
    public double GetDirectorySize(string directory);
    void CreatAllPath(string goal);
    void RenameAllWhithoutSpaces(string folder);
    int CountTotalFiles(string folder, string? pattern = null, string[]? excludePattern = null);
}
