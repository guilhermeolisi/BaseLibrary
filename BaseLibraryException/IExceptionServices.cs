using BaseLibrary;
using System.Reflection;

namespace BaseLibrary;

public interface IExceptionServices
{
    void SetFolder(string folder);
    Task VerifyLocalException(bool isAsync);
    Task SendException(Exception e, bool isAsync, string messageExtra, string OSversion);
    string GetExceptionText(Exception e, string messageExtra);
}
