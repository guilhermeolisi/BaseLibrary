using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;

namespace BaseLibrary;

public class HTTPServices : IHTTPServices
{
    public bool IsConnectedToInternetPing(string host)
    {
        //Está retornando falso mesmo quando estou com conexão com internet, verificar se o host do google está correto

        Ping myPing = new Ping();
        if (string.IsNullOrWhiteSpace(host))
            host = "google.com";
        byte[] buffer = new byte[32];
        int timeout = 1000;
        PingOptions pingOptions = new PingOptions();
        try
        {
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            if (reply.Status == IPStatus.Success)
            {
                // presumably online
                return true;
            }
        }
        catch (Exception ex)
        {
            return false;
        }
        return false;
    }
    public bool IsConnectedToInternet() => NetworkInterface.GetIsNetworkAvailable();
    public bool IsValidURL(string url)
    {
        Uri uriResult;
        return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    public bool OpenUrlDefaultBrowse(string url)
    {

        //https://github.com/dotnet/runtime/issues/17938
        try
        {
            Process.Start(url);
        }
        catch (Exception e)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {

                return false;
                throw;
            }
        }
        return true;

        //https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp
        //Funciona no windows
        Process.Start("explorer", url);
        return true;

        //Não funciona
        Process myProcess = new Process();

        try
        {
            // true is the default, but it is important not to set it to false
            myProcess.StartInfo.UseShellExecute = true;
            myProcess.StartInfo.FileName = "http://some.domain.tld/bla";
            myProcess.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            return false;
        }

        return true;
    }
}
