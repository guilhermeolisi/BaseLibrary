namespace BaseLibrary;

public struct GOSResult
{
    private bool _success;

    public bool Success { get => _success; private set => _success = value; }
    public Exception? Exception { get; private set; }
    public string? Message { get; private set; }
    public byte Option { get; private set; }
    public object? Object { get; private set; }
    public GOSResult(bool success) : this()
    {
        Success = success;
        Exception = null;
        Message = null;
        Option = 0;
        Object = null;
    }
    public GOSResult(string message) : this()
    {
        Success = false;
        Exception = null;
        Message = message;
        Option = 0;
        Object = null;
    }
    public GOSResult(Exception exception) : this()
    {
        Success = false;
        Exception = exception;
        Message = null;
        Option = 0;
        Object = null;
    }
    public GOSResult(bool success, string message) : this()
    {
        Success = success;
        Exception = null;
        Message = message;
        Option = 0;
        Object = null;
    }
    public GOSResult(bool success, Exception exception) : this()
    {
        Success = success;
        Exception = exception;
        Message = null;
        Option = 0;
        Object = null;
    }
    public GOSResult(bool success, Exception? exception, string message) : this()
    {
        Success = success;
        Exception = exception;
        Message = message;
        Option = 0;
        Object = null;
    }
    public GOSResult(bool success, Exception? exception, string? message, byte option) : this()
    {
        Success = success;
        Exception = exception;
        Message = message;
        Option = option;
        Object = null;
    }
    public GOSResult(bool success, Exception? exception, string? message, byte option, object obj) : this()
    {
        Success = success;
        Exception = exception;
        Message = message;
        Option = option;
        Object = obj;
    }
    public GOSResult(bool success, byte option) : this()
    {
        Success = success;
        Exception = null;
        Message = null;
        Option = option;
        Object = null;
    }
    public GOSResult(byte option) : this()
    {
        Success = false;
        Exception = null;
        Message = null;
        Option = option;
        Object = null;
    }
    public override string ToString()
    {
        string result = (Success ? "Success" : "FAIL") +
            (string.IsNullOrEmpty(Message) ? string.Empty : Environment.NewLine + "MESSAGE" + Environment.NewLine + Message) +
            (Exception is null ? string.Empty : Environment.NewLine + "EXCEPTION" + Environment.NewLine + Exception.Message + (Exception.InnerException is not null ? Environment.NewLine + Exception.InnerException : string.Empty) + (Exception.StackTrace is not null ? Environment.NewLine + Exception.StackTrace : string.Empty));
        ;

        return result;
    }
}
