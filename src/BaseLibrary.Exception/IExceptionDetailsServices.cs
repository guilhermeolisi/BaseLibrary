using BaseLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IExceptionDetailsServices
{
    /// <remarks>
    /// Compatibilizado com Native AOT. As implementações enumeram os membros base
    /// de <see cref="Exception"/> (Message, Source, HResult, HelpLink, StackTrace,
    /// Data, InnerException, InnerExceptions) e, via <see cref="KnownExceptionMembers"/>
    /// (switch AOT-safe, sem reflexão), as propriedades dos tipos derivados mais
    /// comuns. Tipos derivados fora dessa lista expõem só os membros base.
    /// </remarks>
    string ToDetailedString(Exception exception);
    /// <inheritdoc cref="ToDetailedString(Exception)"/>
    string ToDetailedString(Exception exception, ExceptionOptions options);
    void AppendValue(StringBuilder stringBuilder, string propertyName, object value, ExceptionOptions options);
}
