using System.Collections;
using System.Reflection;
using System.Text;

namespace BaseLibrary;

public class ExceptionDetailsServices : IExceptionDetailsServices
{
    /// <remarks>
    /// Compatibilizado com Native AOT. Enumera os membros base de
    /// <see cref="Exception"/> (Message, Source, HResult, HelpLink, StackTrace, Data,
    /// InnerException, InnerExceptions) e, via <see cref="KnownExceptionMembers"/>
    /// (switch AOT-safe, sem reflexão), as propriedades dos tipos derivados mais
    /// comuns (ArgumentException, FileNotFoundException, HttpRequestException,
    /// DbException, JsonException, XmlException, etc.). Tipos derivados fora dessa
    /// lista expõem só os membros base — leia-os pelo tipo real se necessário.
    /// </remarks>
    public string ToDetailedString(Exception exception)
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
    public string ToDetailedString(Exception exception, ExceptionOptions options)
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

    private void AppendCollection(StringBuilder stringBuilder, string propertyName, IEnumerable collection, ExceptionOptions options)
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

    private void AppendException(StringBuilder stringBuilder, string propertyName, Exception exception, ExceptionOptions options)
    {
        var innerExceptionString = ToDetailedString(
            exception,
            new ExceptionOptions(options, options.CurrentIndentLevel + 1));

        stringBuilder.AppendLine($"{options.Indent}{propertyName} =");
        stringBuilder.AppendLine(innerExceptionString);
    }

    private string IndentString(string value, ExceptionOptions options)
    {
        return value.Replace(Environment.NewLine, Environment.NewLine + options.Indent);
    }

    public void AppendValue(StringBuilder stringBuilder, string propertyName, object value, ExceptionOptions options)
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
}
