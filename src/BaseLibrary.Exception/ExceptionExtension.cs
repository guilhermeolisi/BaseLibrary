using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public static class ExceptionExtension
{
    private static ExceptionDetailsServices details = new();
    /// <remarks>
    /// Compatibilizado com Native AOT. Enumera apenas os membros conhecidos de
    /// <see cref="Exception"/> (Message, Source, HResult, HelpLink, StackTrace, Data,
    /// InnerException, InnerExceptions). Propriedades específicas de exceções
    /// derivadas NÃO são incluídas — se precisar delas, leia a exceção pelo tipo real.
    /// </remarks>
    public static string ToDetailedString(this Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return ToDetailedString(exception, ExceptionOptions.Default);
    }

    /// <remarks>
    /// Compatibilizado com Native AOT. Enumera apenas os membros conhecidos de
    /// <see cref="Exception"/> (Message, Source, HResult, HelpLink, StackTrace, Data,
    /// InnerException, InnerExceptions). Propriedades específicas de exceções
    /// derivadas NÃO são incluídas — se precisar delas, leia a exceção pelo tipo real.
    /// </remarks>
    public static string ToDetailedString(this Exception exception, ExceptionOptions options)
    {
        var stringBuilder = new StringBuilder();

        details.AppendValue(stringBuilder, "Type", exception.GetType().FullName, options);

        foreach ((string name, object? value) in GetKnownMembers(exception))
        {
            object? memberValue = value;
            if (memberValue is null && options.OmitNullProperties)
                continue;

            details.AppendValue(stringBuilder, name, memberValue ?? string.Empty, options);
        }

        return stringBuilder.ToString().TrimEnd('\r', '\n');
    }

    private static IEnumerable<(string Name, object? Value)> GetKnownMembers(Exception exception)
    {
        yield return ("Message", exception.Message);
        yield return ("Source", exception.Source);
        yield return ("HResult", exception.HResult);
        yield return ("HelpLink", exception.HelpLink);
        yield return ("StackTrace", exception.StackTrace);
        yield return ("Data", exception.Data);
        if (exception is AggregateException aggregate)
            yield return ("InnerExceptions", aggregate.InnerExceptions);
        yield return ("InnerException", exception.InnerException);
    }
}
