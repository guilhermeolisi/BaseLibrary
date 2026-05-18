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
    /// Compatibilizado com Native AOT. As implementações enumeram apenas os membros
    /// conhecidos de <see cref="Exception"/> (Message, Source, HResult, HelpLink,
    /// StackTrace, Data, InnerException, InnerExceptions). Propriedades específicas
    /// de exceções derivadas NÃO são incluídas — leia a exceção pelo tipo real.
    /// </remarks>
    string ToDetailedString(Exception exception);
    /// <inheritdoc cref="ToDetailedString(Exception)"/>
    string ToDetailedString(Exception exception, ExceptionOptions options);
    void AppendValue(StringBuilder stringBuilder, string propertyName, object value, ExceptionOptions options);
}
