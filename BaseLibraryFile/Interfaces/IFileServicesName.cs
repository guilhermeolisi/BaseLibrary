using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using Splat;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

public interface IFileServicesName
{

    void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension);
    (string, string, string) InfoFromFilePath(in string filepath);
    void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent);
    string FileNameAvailable(string fullPath, char? mode);
    string CutCharacterFileForbiden(string filePath);
}
