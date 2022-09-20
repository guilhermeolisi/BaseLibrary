using BaseLibrary;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class ExceptionDetailsServices : IExceptionDetailsServices
{
    public string ToDetailedString(Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return ToDetailedString(exception, ExceptionOptions.Default);
    }

    public string ToDetailedString(Exception exception, ExceptionOptions options)
    {
        var stringBuilder = new StringBuilder();

        AppendValue(stringBuilder, "Type", exception.GetType().FullName, options);

        foreach (PropertyInfo property in exception
            .GetType()
            .GetProperties()
            .OrderByDescending(x => string.Equals(x.Name, nameof(exception.Message), StringComparison.Ordinal))
            .ThenByDescending(x => string.Equals(x.Name, nameof(exception.Source), StringComparison.Ordinal))
            .ThenBy(x => string.Equals(x.Name, nameof(exception.InnerException), StringComparison.Ordinal))
            .ThenBy(x => string.Equals(x.Name, nameof(AggregateException.InnerExceptions), StringComparison.Ordinal)))
        {
            var value = property.GetValue(exception, null);
            if (value == null && options.OmitNullProperties)
            {
                if (options.OmitNullProperties)
                {
                    continue;
                }
                else
                {
                    value = string.Empty;
                }
            }

            AppendValue(stringBuilder, property.Name, value, options);
        }

        return stringBuilder.ToString().TrimEnd('\r', '\n');
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
