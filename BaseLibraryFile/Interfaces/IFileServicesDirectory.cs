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
    void CreatAllPath(string goal);
}
