using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

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
}
