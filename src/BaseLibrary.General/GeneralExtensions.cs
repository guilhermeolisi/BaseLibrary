using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public static class GeneralExtensions
{
    public static bool IsNull(this object? obj) => obj is null;
    public static bool IsNotNull(this object? obj) => obj is not null;
}
