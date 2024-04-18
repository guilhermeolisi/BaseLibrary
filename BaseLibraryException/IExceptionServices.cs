namespace BaseLibrary;

public interface IExceptionServices
{
    void SetFolder(string folder);
    Task VerifyLocalException(bool isAsync);
    Task SendException(Exception e, bool isAsync, string? messageExtra, string? OSversion);
    string GetExceptionText(Exception e, string? messageExtra);
    bool IsConnectedToInternet();
    void SaveException(Exception e, string? messageExtra, string? OSversion);
}
