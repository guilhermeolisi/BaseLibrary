namespace BaseLibrary;

public interface IFileServicesDirectory
{
    void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool preserveTime);
    void DirectoryClear(string sourceDirName);
    public double GetDirectorySize(string directory, bool recursive);
    void CreatAllPath(string goal);
    void RenameAllWhithoutSpaces(string folder);
    int CountTotalFiles(string folder, string? pattern = null, string[]? excludePattern = null);
    void SetAttributesNormal(string folder);
    void SetAttributesNormal(DirectoryInfo dir);
}
