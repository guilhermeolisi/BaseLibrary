using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

/// <summary>
/// https://stackoverflow.com/questions/8039660/net-how-to-convert-exception-to-string
/// </summary>
public static class ExceptionMethods
{
    // Caminho de e-mail SMTP legado (envio de exceções por e-mail) REMOVIDO.
    // Continha uma credencial Gmail hardcoded; a telemetria de exceções agora usa
    // ExceptionServicesAzure (OpenTelemetry/Azure Monitor). Apenas os helpers de
    // formatação (ToDetailedString/GetExceptionText) permanecem nesta classe.
    /// <remarks>
    /// Compatibilizado com Native AOT. Enumera os membros base de
    /// <see cref="Exception"/> (Message, Source, HResult, HelpLink, StackTrace, Data,
    /// InnerException, InnerExceptions) e, via <see cref="KnownExceptionMembers"/>
    /// (switch AOT-safe, sem reflexão), as propriedades dos tipos derivados mais
    /// comuns (ArgumentException, FileNotFoundException, HttpRequestException,
    /// DbException, JsonException, XmlException, etc.). Tipos derivados fora dessa
    /// lista expõem só os membros base — leia-os pelo tipo real se necessário.
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
    /// Compatibilizado com Native AOT. Enumera os membros base de
    /// <see cref="Exception"/> (Message, Source, HResult, HelpLink, StackTrace, Data,
    /// InnerException, InnerExceptions) e, via <see cref="KnownExceptionMembers"/>
    /// (switch AOT-safe, sem reflexão), as propriedades dos tipos derivados mais
    /// comuns (ArgumentException, FileNotFoundException, HttpRequestException,
    /// DbException, JsonException, XmlException, etc.). Tipos derivados fora dessa
    /// lista expõem só os membros base — leia-os pelo tipo real se necessário.
    /// </remarks>
    public static string ToDetailedString(this Exception exception, ExceptionOptions options)
    {
        var stringBuilder = new StringBuilder();

        AppendValue(stringBuilder, "Type", exception.GetType().FullName, options);

        foreach ((string name, object? value) in GetKnownMembers(exception))
        {
            object? memberValue = value;
            if (memberValue is null && options.OmitNullProperties)
                continue;

            AppendValue(stringBuilder, name, memberValue ?? string.Empty, options);
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
        foreach ((string name, object? value) in KnownExceptionMembers.GetDerived(exception))
            yield return (name, value);
        if (exception is AggregateException aggregate)
            yield return ("InnerExceptions", aggregate.InnerExceptions);
        yield return ("InnerException", exception.InnerException);
    }

    private static void AppendCollection(StringBuilder stringBuilder, string propertyName, IEnumerable collection, ExceptionOptions options)
    {
        stringBuilder.AppendLine($"{options.Indent}{propertyName} =");

        var innerOptions = new ExceptionOptions(options, options.CurrentIndentLevel + 1);

        var i = 0;
        foreach (var item in collection)
        {
            var innerPropertyName = $"[{i}]";

            if (item is Exception)
            {
                var innerException = (Exception)item;
                AppendException(
                    stringBuilder,
                    innerPropertyName,
                    innerException,
                    innerOptions);
            }
            else
            {
                AppendValue(
                    stringBuilder,
                    innerPropertyName,
                    item,
                    innerOptions);
            }

            ++i;
        }
    }

    private static void AppendException(StringBuilder stringBuilder, string propertyName, Exception exception, ExceptionOptions options)
    {
        var innerExceptionString = ToDetailedString(
            exception,
            new ExceptionOptions(options, options.CurrentIndentLevel + 1));

        stringBuilder.AppendLine($"{options.Indent}{propertyName} =");
        stringBuilder.AppendLine(innerExceptionString);
    }

    private static string IndentString(string value, ExceptionOptions options)
    {
        return value.Replace(Environment.NewLine, Environment.NewLine + options.Indent);
    }

    private static void AppendValue(
        StringBuilder stringBuilder,
        string propertyName,
        object value,
        ExceptionOptions options)
    {
        if (value is DictionaryEntry)
        {
            DictionaryEntry dictionaryEntry = (DictionaryEntry)value;
            stringBuilder.AppendLine($"{options.Indent}{propertyName} = {dictionaryEntry.Key} : {dictionaryEntry.Value}");
        }
        else if (value is Exception)
        {
            var innerException = (Exception)value;
            AppendException(
                stringBuilder,
                propertyName,
                innerException,
                options);
        }
        else if (value is IEnumerable && !(value is string))
        {
            var collection = (IEnumerable)value;
            if (collection.GetEnumerator().MoveNext())
            {
                AppendCollection(
                    stringBuilder,
                    propertyName,
                    collection,
                    options);
            }
        }
        else
        {
            stringBuilder.AppendLine($"{options.Indent}{propertyName} = {value}");
        }
    }
    public static string GetExceptionText(Exception e, string messageExtra)
    {
        var program = Assembly.GetEntryAssembly()?.GetName();
        string? appName = program?.Name;
        Version? ver = program?.Version;
        string message = "[" + DateTime.Now + "]" + Environment.NewLine + "Program: " + appName + Environment.NewLine +
            "Version: " + ver?.ToString() + Environment.NewLine +
            ToDetailedString(e) +
            (!string.IsNullOrWhiteSpace(messageExtra) ? Environment.NewLine + Environment.NewLine + messageExtra : "");
        return message;
    }
}
