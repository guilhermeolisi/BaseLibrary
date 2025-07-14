using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BasicLibrary.Events;

public interface Subscriber<TArg> where TArg : EventArgs
{
    void OnEvent(object sender, TArg args);
}