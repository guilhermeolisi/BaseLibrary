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
    static string fileExceptions = "Exceptions.txt";
    public static void VerifyLocalException(string emailTo, bool isAsync)
    {
        string folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExceptionMethods)).Location);
        if (File.Exists(Path.Combine(folder, fileExceptions)))
        {
            Console.Write("Some old internal errors messages were found in local file. Trying to send to developer...");
            string message = FileMethods.ReadTXT(Path.Combine(folder, fileExceptions));
            if (SendOnlineException(emailTo, message, isAsync))
            {
                File.Delete(Path.Combine(folder, fileExceptions));
                Console.WriteLine("done");
                Console.WriteLine("Content of sent message:");
                Console.WriteLine(message);
            }
            else
            {
                Console.WriteLine("fail. A new attempt will be made in the future");
            }
        }
    }
    public static void SendException(string emailTo, Exception e, bool isAsync, string messageExtra)
    {
        Console.WriteLine();
        Console.Write("A internal error is found. Trying to send to developer...");
        var program = Assembly.GetEntryAssembly().GetName();
        string Name = program?.Name;
        Version ver = program?.Version;
        string message = "Program: " + Name + Environment.NewLine +
            "Version: " + ver.ToString() + Environment.NewLine + 
            ToDetailedString(e) + 
            (!String.IsNullOrWhiteSpace(messageExtra) ? Environment.NewLine + Environment.NewLine + messageExtra : "");

        if (SendOnlineException(emailTo, message, isAsync))
        {
            Console.WriteLine("done");
            VerifyLocalException(emailTo, isAsync);
            Console.WriteLine("Content of sent message:");
            Console.WriteLine(message);
        }
        else
        {
            Console.WriteLine("fail. A new attempt will be made in the future");
            SaveLocalException(message, isAsync);
        }
    }
    private static async void SaveLocalException(string message, bool isAsync)
    {
        string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        message = "[" + DateTime.Now + "]" + Environment.NewLine + message;

        if (File.Exists(Path.Combine(folder, fileExceptions)))
        {
            if (isAsync)
                message = await FileMethods.ReadTXTAsync(Path.Combine(folder, fileExceptions)) + Environment.NewLine + Environment.NewLine + message;
            else
                message = FileMethods.ReadTXT(Path.Combine(folder, fileExceptions)) + Environment.NewLine + Environment.NewLine + message;
        }
        if (isAsync)
            await FileMethods.WriteTXTAsync(Path.Combine(folder, fileExceptions), message);
        else
            FileMethods.WriteTXT(Path.Combine(folder, fileExceptions), message);
    }
    private static bool SendOnlineException(string emailTo, string message, bool isAsync)
    {
        if (!HTTPMethods.IsConnectedToInternetPing())
            return false;

        string sender, keypass;
        sender = "sindarinsender@gmail.com";
        keypass = "%hw.87&-";
        try
        {
            HTTPMethods.SendEmail(sender, keypass, "Exception", emailTo, message, isAsync);
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }
    public static string ToDetailedString(this Exception exception)
    {
        if (exception == null)
        {
            throw new ArgumentNullException(nameof(exception));
        }

        return ToDetailedString(exception, ExceptionOptions.Default);
    }

    public static string ToDetailedString(this Exception exception, ExceptionOptions options)
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
}

public struct ExceptionOptions
{
    public static readonly ExceptionOptions Default = new ExceptionOptions()
    {
        CurrentIndentLevel = 0,
        IndentSpaces = 4,
        OmitNullProperties = true
    };

    internal ExceptionOptions(ExceptionOptions options, int currentIndent)
    {
        this.CurrentIndentLevel = currentIndent;
        this.IndentSpaces = options.IndentSpaces;
        this.OmitNullProperties = options.OmitNullProperties;
    }

    internal string Indent { get { return new string(' ', this.IndentSpaces * this.CurrentIndentLevel); } }

    internal int CurrentIndentLevel { get; set; }

    public int IndentSpaces { get; set; }

    public bool OmitNullProperties { get; set; }
}
