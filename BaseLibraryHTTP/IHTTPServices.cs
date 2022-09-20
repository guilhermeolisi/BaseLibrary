using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IHTTPServices
{
    bool IsConnectedToInternetPing(string host);
    bool IsConnectedToInternet();
    bool IsValidURL(string url);
}
