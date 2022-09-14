using BaseLibrary;
using Splat;
using System.Reflection;

namespace BaseLibrary;

public class ExceptionServices : IExceptionServices
{
    private string fileExceptions = "Exceptions.txt";
    //private string emailSender, password, folder, emailTo;
    private string folder, emailTo;
    private bool isConsole;
    private IExceptionDetails details;
    IEmailSender emailSender;
    public ExceptionServices(string folder, string emailTo, IExceptionDetails? details = null, IEmailSender? emailSender = null, bool isConsole = false)
    {
        this.folder = folder ?? throw new ArgumentNullException(nameof(folder));
        this.emailTo = emailTo ?? throw new ArgumentNullException(nameof(emailTo));
        this.details = details ?? Locator.Current.GetService<IExceptionDetails>()! ?? throw new ArgumentNullException(nameof(details));
        this.emailSender = emailSender ?? Locator.Current.GetService<IEmailSender>()! ?? throw new ArgumentNullException(nameof(emailSender));
        this.isConsole = isConsole;
    }
    public void VerifyLocalException(bool isAsync)
    {
        //string folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExceptionMethods)).Location);
        if (File.Exists(Path.Combine(folder, fileExceptions)))
        {
            if (isConsole)
                Console.Write("Some old internal errors messages were found in local file. Trying to send to developer...");
            string? message = FileMethods.ReadTXT(Path.Combine(folder, fileExceptions));
            if (SendOnlineException(message, isAsync))
            {
                File.Delete(Path.Combine(folder, fileExceptions));
                if (isConsole)
                {
                    Console.WriteLine("done");
                    Console.WriteLine("Content of sent message:");
                    Console.WriteLine(message);
                }
            }
            else
            {
                Console.WriteLine("fail. A new attempt will be made in the future");
            }
        }
    }
    public void SendException(Exception e, bool isAsync, string messageExtra)
    {
        if (isConsole)
        {
            Console.WriteLine();
            Console.Write("A internal error is found. Trying to send to developer...");
        }
        var program = Assembly.GetEntryAssembly()?.GetName();
        string? Name = program?.Name;
        Version? ver = program?.Version;
        string message = "Program: " + Name + Environment.NewLine +
            "Version: " + ver?.ToString() + Environment.NewLine +
            details.ToDetailedString(e) +
            (!string.IsNullOrWhiteSpace(messageExtra) ? Environment.NewLine + Environment.NewLine + messageExtra : "");

        if (SendOnlineException(message, isAsync))
        {
            if (isConsole)
                Console.WriteLine("done");
            if (isConsole)
            {
                Console.WriteLine("Content of sent message:");
                Console.WriteLine(message);
            }
            VerifyLocalException(isAsync);

        }
        else
        {
            if (isConsole)
                Console.WriteLine("fail. A new attempt will be made in the future");
            SaveLocalException(message, isAsync);
        }
    }
    private async void SaveLocalException(string message, bool isAsync)
    {
        //string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

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
    private bool SendOnlineException(string? message, bool isAsync)
    {
        if (!HTTPMethods.IsConnectedToInternetPing())
            return false;

        //string sender, keypass;
        //sender = "sindarinsender@gmail.com";
        //keypass = "%hw.87&-";



        try
        {
            emailSender.SendEmail("Exception", emailTo, message, isAsync);
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }
}
