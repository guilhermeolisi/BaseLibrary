
using BaseLibrary.DependencyInjection;

namespace BaseLibrary;

public class FileServices : IFileServices
{
    public IFileServicesText Text { get; }
    public IFileServicesDirectory Directory { get; }
    public IFileServicesCheck Check { get; }
    public IFileServicesName Name { get; }
    public IFileServicesFile File { get; }
    public FileServices(IFileServicesText? _servicesText = null, IFileServicesDirectory? _servicesDirectory = null, IFileServicesCheck? _servicesCheck = null, IFileServicesName? _servicesName = null, IFileServicesFile file = null)
    {
        Text = _servicesText ?? Locator.ConstantContainer!.Resolve<IFileServicesText>()! ?? new FileServicesText();
        Directory = _servicesDirectory ?? Locator.ConstantContainer!.Resolve<IFileServicesDirectory>()! ?? new FileServicesDirectory();
        Check = _servicesCheck ?? Locator.ConstantContainer!.Resolve<IFileServicesCheck>()! ?? new FileServicesCheck();
        Name = _servicesName ?? Locator.ConstantContainer!.Resolve<IFileServicesName>()! ?? new FileServicesName();
        File = file ?? Locator.ConstantContainer!.Resolve<IFileServicesFile>()! ?? new FileServicesFile();
    }


    //#region Text
    //public bool WriteTXT(string pathFile, in string parTXT) => servicesText.WriteTXT(pathFile, parTXT);
    //public async Task<bool> WriteTXTAsync(string pathFile, string parTXT) => await servicesText.WriteTXTAsync(pathFile, parTXT);
    //public string? ReadTXT(string pathFile) => servicesText.ReadTXT(pathFile);
    //public async Task<string?> ReadTXTAsync(string pathFile) => await servicesText.ReadTXTAsync(pathFile);
    //#endregion

    //public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, bool preserveTime) => servicesDirectory.DirectoryCopy(sourceDirName, destDirName, copySubDirs, preserveTime);
    //public void DirectoryClear(string sourceDirName) => servicesDirectory.DirectoryClear(sourceDirName);
    //public void CreatAllPath(string goal) => servicesDirectory.CreatAllPath(goal);

    //public bool CheckZipFile(string path) => servicesCheck.CheckZipFile(path);
    //public bool CheckTextFileByChars(string filePath) => servicesCheck.CheckTextFileByChars(filePath);
    //public bool CheckTextFile(string filePath) => servicesCheck.CheckTextFile(filePath);
    //public bool CheckImageFile(string path) => servicesCheck.CheckImageFile(path);

    //public void InfoFromFilePath(in string filepath, ref string fileName, ref string folder, ref string extension) => servicesName.InfoFromFilePath(filepath, ref fileName, ref folder, ref extension);
    //public (string, string, string) InfoFromFilePath(in string filepath) => servicesName.InfoFromFilePath(filepath);
    //public void InfoFromFolderPath(in string filepath, ref string actualFolder, ref string folderParent) => servicesName.InfoFromFolderPath(filepath, ref actualFolder, ref folderParent);
    //public string FileNameAvailable(string fullPath, char? mode) => servicesName.FileNameAvailable(fullPath, mode);
    //public string CutCharacterFileForbiden(string filePath) => servicesName.CutCharacterFileForbiden(filePath);
}
