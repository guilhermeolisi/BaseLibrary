using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public static class EventHandlers
{
    public delegate void AsyncMethodFinishedEventHandler(string sender);
}
