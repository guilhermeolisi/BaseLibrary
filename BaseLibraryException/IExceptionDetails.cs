using BaseLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IExceptionDetails
{
    string ToDetailedString(Exception exception);
    string ToDetailedString(Exception exception, ExceptionOptions options);
    void AppendValue(StringBuilder stringBuilder, string propertyName, object value, ExceptionOptions options);
}
