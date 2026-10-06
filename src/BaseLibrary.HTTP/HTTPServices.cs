using System.Diagnostics;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace BaseLibrary;

public class HTTPServices : IHTTPServices
{
    public bool IsConnectedToInternetPing(string? host = null)
    {
        //Está retornando falso mesmo quando estou com conexão com internet, verificar se o host do google está correto

        bool isWSL = SystemUtility.IsRunningInWSL(); //o Ping não funciona no WSL

        if (isWSL)
        {
            return false;
        }

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
        catch (PlatformNotSupportedException)
        {
            return IsConnectedToInternet();
        }
        catch (Exception)
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
    public bool IsValidEmail(string email)
    {
        //https://mailtrap.io/blog/validate-email-address-c/
        var valid = true;

        try
        {
            var emailAddress = new MailAddress(email);
        }
        catch (Exception)
        {
            valid = false;
        }

        return valid;
    }
    public bool OpenUrlDefaultBrowse(string url)
    {

        //https://github.com/dotnet/runtime/issues/17938
        try
        {
            Process.Start(url);
        }
        catch (Exception)
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
            }
        }
        return true;
    }
}
