﻿namespace BaseLibrary;

public interface IFileServicesName
{

    void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension);
    (string?, string?, string?) InfoFromFilePath(in string filepath);
    void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent);
    string FileNameAvailable(string fullPath, char? mode = null);
    string CutCharacterFileForbiden(string filePath, bool cut);
    bool IsDirectoryPathValid(string folderPath);
    bool IsFilePathValid(string filePath);
}
