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
    public static string ToDetailedString(this Exception exception)
    {
        if (exception is null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return ToDetailedString(exception, ExceptionOptions.Default);
    }

    public static string ToDetailedString(this Exception exception, ExceptionOptions options)
    {
        var stringBuilder = new StringBuilder();

        details.AppendValue(stringBuilder, "Type", exception.GetType().FullName, options);

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

            details.AppendValue(stringBuilder, property.Name, value, options);
        }

        return stringBuilder.ToString().TrimEnd('\r', '\n');
    }
}
