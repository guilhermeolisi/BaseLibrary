namespace BaseLibrary;

public interface IFileServicesName
{

    void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension);
    (string?, string?, string?) InfoFromFilePath(in string filepath);
    void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent);
    string FileNameAvailable(string fullPath, char? mode = null);
    string CutCharacterFileForbiden(string filePath, bool cut);
    /// <summary>
    /// Encurta o NOME do arquivo de <paramref name="fullPath"/> (a extensao fica) ate o nome com a extensao caber em
    /// <paramref name="maxBytes"/> bytes UTF-8, sem partir um caractere. A pasta nao muda.
    /// </summary>
    string FitFileNameLength(string fullPath, int maxBytes = FileServicesName.DefaultMaxFileNameBytes);
    bool IsDirectoryPathValid(string folderPath);
    bool IsFilePathValid(string filePath);
}
