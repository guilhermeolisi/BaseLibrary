namespace BaseLibrary;

public interface IFileServices
{
    IFileServicesText Text { get; }
    IFileServicesDirectory Directory { get; }
    IFileServicesCheck Check { get; }
    IFileServicesName Name { get; }

    //bool WriteTXT(string pathFile, in string parTXT);
    //Task<bool> WriteTXTAsync(string pathFile, string parTXT);
    //string? ReadTXT(string pathFile);
    //Task<string?> ReadTXTAsync(string pathFile);
    //void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension);
    //(string, string, string) InfoFromFilePath(in string filepath);
    //void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent);
    //void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool preserveTime);
    //void DirectoryClear(string sourceDirName);
    //void CreatAllPath(string goal);
    ///// <summary>
    ///// Verifica se é um arquivo zip
    ///// </summary>
    ///// <param name="path"></param>
    ///// <returns></returns>
    //bool CheckZipFile(string path);
    //bool CheckTextFileByChars(string filePath);
    ///// <summary>
    ///// Verifica se é um arquivo binário ou de texto
    ///// </summary>
    ///// <returns>text return true, binary return false</returns>
    //bool CheckTextFile(string filePath);
    ///// <summary>
    ///// Verifica se é um arquivo imagem
    ///// </summary>
    ///// <param name="path"></param>
    ///// <returns></returns>
    //bool CheckImageFile(string path);
    ///// <summary>
    ///// Verifica se é um arquivo XML
    ///// </summary>
    ///// <param name="fullPath"></param>
    ///// <param name="mode">C copied; E exported; I imported; M moved</param>
    ///// <returns></returns>
    //string FileNameAvailable(string fullPath, char? mode);
    //string CutCharacterFileForbiden(string filePath);
}
