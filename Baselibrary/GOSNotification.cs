using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class GOSNotification
{
    public string Message { get; private set; }
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
