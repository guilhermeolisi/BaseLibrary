using FileTypeChecker;
using FileTypeChecker.Abstracts;
using FileTypeChecker.Extensions;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace BaseLibrary;

public static class FileMethods
{
    static FileServices services = new();
    public static bool WriteTXT(string pathFile, in string parTXT) => services.Text.WriteTXT(pathFile, parTXT);
    public static async Task<bool> WriteTXTAsync(string pathFile, string parTXT) => await services.Text.WriteTXTAsync(pathFile, parTXT);
    public static string? ReadTXT(string pathFile) => services.Text.ReadTXT(pathFile);
    public static async Task<string?> ReadTXTAsync(string pathFile) => await services.Text.ReadTXTAsync(pathFile);
    public static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool preserveTime) => services.Directory.DirectoryCopy(sourceDirName, destDirName, copySubDirs, preserveTime);
    public static void DirectoryClear(string sourceDirName) => services.Directory.DirectoryClear(sourceDirName);
    public static void CreatAllPath(string goal) => services.Directory.CreatAllPath(goal);
    /// <summary>
    /// Verifica se é um arquivo zip
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool CheckZipFile(string path) => services.Check.CheckZipFile(path);
    /// <summary>
    /// Verifica se é um arquivo binário ou de texto
    /// </summary>
    /// <returns>text return true, binary return false</returns>
    public static bool CheckTextFileByChars(string filePath) => services.Check.CheckTextFileByChars(filePath);
    public static bool CheckTextFile(string filePath) => services.Check.CheckTextFile(filePath);
    /// <summary>
    /// Verifica se é um arquivo imagem
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool CheckImageFile(string path) => services.Check.CheckImageFile(path);
    /// <summary>
    /// Verifica se é um arquivo XML
    /// </summary>
    /// <param name="fullPath"></param>
    /// <param name="mode">C copied; E exported; I imported; M moved</param>
    /// <returns></returns>
    public static void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension) => services.Name.InfoFromFilePath(filepath, ref fileName, ref folder, ref extension);
    public static (string, string, string) InfoFromFilePath(in string filepath) => services.Name.InfoFromFilePath(filepath);
    public static void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent) => services.Name.InfoFromFolderPath(filepath, ref actualFolder, ref folderParent);
    public static string FileNameAvailable(this string fullPath, char? mode) => services.Name.FileNameAvailable(fullPath, mode);
    public static string CutCharacterFileForbiden(this string filePath) => services.Name.CutCharacterFileForbiden(filePath);
}
