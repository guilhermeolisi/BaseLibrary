namespace BaseLibrary;

public interface IFileServicesCheck
{
    bool CheckZipFile(string path);
    bool CheckTextFileByChars(string filePath);
    bool CheckTextFileByEncoding(string filePath); 
    bool CheckTextFile(string filePath);
    bool CheckImageFile(string path);
}
