using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public struct GOSResult
{
    private bool _success;

    public bool Success { get => Exception is null ? _success : false; private set => _success = value; }
    public Exception Exception { get; private set; }
    public string Message { get; private set; }
    public GOSResult(bool success) : this()
    {
        Success = success; 
        Exception = null;
        Message = null;
    }
    public GOSResult(bool success, Exception exception, string message) : this()
    {
        Success = success;
        Exception = exception;
        Message = message;
    }

}
