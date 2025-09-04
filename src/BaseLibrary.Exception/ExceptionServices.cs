using BaseLibrary.DependencyInjection;
using System.Reflection;

namespace BaseLibrary;

public class ExceptionServices : IExceptionServices
{
    private readonly string[] AssembliesName;
    private string fileExceptionsPostfix = " - Exceptions";
    private string fileNameExceptionSender = "SendingException";
    //private string emailSender, password, folder, emailTo;
    private string folder, emailTo;
    private bool isConsole;
    private IExceptionDetailsServices exceptionDetails;
    IEmailSender emailSender;
    IHTTPServices httpServices;
    public ExceptionServices(string emailTo, string[] assembliesName, string folder = null, IExceptionDetailsServices? exceptionDetails = null, IEmailSender? emailSender = null, IHTTPServices? httpServices = null, bool isConsole = false)
    {
        this.AssembliesName = assembliesName;
        this.folder = folder;// ?? throw new ArgumentNullException(nameof(folder));
        this.emailTo = emailTo ?? throw new ArgumentNullException(nameof(emailTo));
        this.exceptionDetails = exceptionDetails ?? Locator.ConstantContainer.Resolve<IExceptionDetailsServices>()! ?? throw new ArgumentNullException(nameof(exceptionDetails));
        this.emailSender = emailSender ?? Locator.ConstantContainer.Resolve<IEmailSender>()! ?? throw new ArgumentNullException(nameof(emailSender));

        this.httpServices = httpServices ?? Locator.ConstantContainer.Resolve<IHTTPServices>()! ?? throw new ArgumentNullException(nameof(httpServices));
        this.isConsole = isConsole;
    }
    public void SetFolder(string folder)
    {
        if (this.folder == folder)
            return;
        this.folder = folder ?? throw new ArgumentNullException(nameof(folder));
    }
    public async Task VerifyLocalException(bool isAsync)
    {

        if (string.IsNullOrWhiteSpace(folder))
            throw new NullReferenceException(nameof(folder));
        //string folder = Path.GetDirectoryName(Assembly.GetAssembly(typeof(ExceptionMethods)).Location);

        string[] files = Directory.GetFiles(folder, fileExceptionsPostfix);

        foreach (var item in files)
        {
            if (File.Exists(item))
            {
                if (isConsole)
                    Console.Write("Some old internal errors messages were found in local file. Trying to send to developer...");
                string? message = FileMethods.ReadTXT(item);
                string appName = Path.GetFileNameWithoutExtension(item);
                if (appName.Contains(fileExceptionsPostfix))
                    appName = appName.Replace(fileExceptionsPostfix, "");

                if (await SendOnlineException(message, appName, isAsync) is GOSResult result && result.Success)
                {
                    File.Delete(item);
                    if (isConsole)
                    {
                        Console.WriteLine("done");
                        Console.WriteLine("Content of sent message:");
                        Console.WriteLine(message);
                    }
                }
                else
                {
                    if (isConsole)
                    {
                        Console.WriteLine("fail. A new attempt will be made in the future");
                    }
                    if (result.Exception is not null)
                    {
                        string messageException = exceptionDetails.ToDetailedString(result.Exception);
                        SaveLocalException(messageException, fileNameExceptionSender, isAsync);
                    }
                }
            }
        }

    }
    public async Task SendException(Exception e, bool isAsync, string? messageExtra, string? OSversion)
    {

        if (isConsole)
        {
            Console.WriteLine();
            Console.Write("A internal error is found. Trying to send to developer...");
        }
        //Assembly? assembly = null;

        //foreach (var item in AssembliesName)
        //{
        //    assembly = Assembly.Load(item);
        //    if (assembly is not null)
        //        break;
        //}
        //if (assembly is null)
        //{
        //    assembly = Assembly.GetEntryAssembly();
        //}

        //var program = assembly?.GetName();
        //Version version = program.Version;
        //string? appName = program?.Name;
        //string message =
        //    appName + " " + version.ToString() + Environment.NewLine +
        //    (string.IsNullOrWhiteSpace(OSversion) ? "" : OSversion) + Environment.NewLine +
        //    GetExceptionText(e, messageExtra);

        (string message, string appName) = GetMessageToSend(e, messageExtra, OSversion);

        if (await SendOnlineException(message, appName, isAsync) is GOSResult result && result.Success)
        {
            if (isConsole)
            {
                Console.WriteLine("done");
                Console.WriteLine("Content of sent message:");
                Console.WriteLine(message);
            }
            else
            {

            }
            VerifyLocalException(isAsync);

        }
        else
        {
            if (isConsole)
                Console.WriteLine("fail. A new attempt will be made in the future");
            SaveLocalException(message, appName, isAsync);
            if (result.Exception is not null)
            {
                string messageException = exceptionDetails.ToDetailedString(result.Exception);
                SaveLocalException(messageException, fileNameExceptionSender, isAsync);
            }
        }
    }
    private (string message, string appName) GetMessageToSend(Exception e, string? messageExtra, string? OSversion)
    {
        Assembly? assembly = null;

        foreach (var item in AssembliesName)
        {
            assembly = Assembly.Load(item);
            if (assembly is not null)
                break;
        }
        if (assembly is null)
        {
            assembly = Assembly.GetEntryAssembly();
        }
        var program = assembly?.GetName();

        Version version = (program is null ? null : program.Version) ?? new Version();
        string appName = (program is null ? null : program.Name) ?? "unknowApp";
        string message =
            appName + " " + version.ToString() + Environment.NewLine +
            (string.IsNullOrWhiteSpace(OSversion) ? "" : OSversion) + Environment.NewLine +
            GetExceptionText(e, messageExtra);
        return (message, appName);
    }
    private void SaveException(Exception e, string? messageExtra, string? OSversion)
    {
        (string message, string appName) = GetMessageToSend(e, messageExtra, OSversion);
        SaveLocalException(message, appName, false);
    }
    public bool IsConnectedToInternet() => httpServices.IsConnectedToInternet();
    public string GetExceptionText(Exception e, string? messageExtra)
    {
        var program = Assembly.GetEntryAssembly()?.GetName();
        string? appName = program?.Name;
        Version? ver = program?.Version;
        string message = "[" + DateTime.Now + "]" + Environment.NewLine + "Program: " + appName + Environment.NewLine +
            "Version: " + ver?.ToString() + Environment.NewLine +
            exceptionDetails.ToDetailedString(e) +
            (!string.IsNullOrWhiteSpace(messageExtra) ? Environment.NewLine + Environment.NewLine + messageExtra : "");
        return message;
    }
    private async void SaveLocalException(string message, string appName, bool isAsync)
    {
        //string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        //message = "[" + DateTime.Now + "]" + Environment.NewLine + message;

        string file = Path.Combine(folder, appName + fileExceptionsPostfix + ".txt");

        if (File.Exists(file))
        {
            if (isAsync)
                message = await FileMethods.ReadTXTAsync(file) + Environment.NewLine + Environment.NewLine + message;
            else
                message = FileMethods.ReadTXT(file) + Environment.NewLine + Environment.NewLine + message;
        }
        if (isAsync)
            await FileMethods.WriteTXTAsync(file, message);
        else
            FileMethods.WriteTXT(file, message);
    }
    private async Task<GOSResult> SendOnlineException(string? message, string appName, bool isAsync)
    {

        if (!httpServices.IsConnectedToInternet())
            return new GOSResult(false);

        //string sender, keypass;
        //sender = "sindarinsender@gmail.com";
        //keypass = "%hw.87&-";

        try
        {
            if (isAsync)
                return await emailSender.SendMessage(emailTo, $"[#X@{appName.ToUpper()}] " + DateTime.Now + ":" + DateTime.Now.Millisecond, message, isAsync);
            else
                return emailSender.SendMessage(emailTo, $"[#X@{appName.ToUpper()}] " + DateTime.Now + ":" + DateTime.Now.Millisecond, message, isAsync).Result;
        }
        catch (Exception ex)
        {
            return new GOSResult(ex);
        }

    }
}
