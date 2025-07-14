namespace BaseLibrary;

public class GOSNotification
{
    public string Message { get; private set; }
    /// <summary>
    /// 1: Success, 2: Warning, 3: Error, 4: Informational
    /// </summary>
    public byte Operation { get; private set; }
    public object Object { get; private set; }
    public GOSNotification(string? messages, byte operation, object _object)
    {
        Message = messages;
        Operation = operation;
        Object = _object;
    }
    public GOSNotification(string messages)
    {
        Message = messages;
        Operation = byte.MaxValue;
        Object = null;
    }
}
