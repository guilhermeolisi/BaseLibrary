using BaseLibrary;
using System.Reflection;

namespace BaseLibrary;

public interface IExceptionServices
{
    void VerifyLocalException(bool isAsync);
    void SendException(Exception e, bool isAsync, string messageExtra);
}
