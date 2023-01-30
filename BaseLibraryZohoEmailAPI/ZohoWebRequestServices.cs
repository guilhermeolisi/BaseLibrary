using Com.Zoho.Crm.API.SendMail;
using Com.Zoho.Crm.API.Util;
using Newtonsoft.Json;
using Splat;
using System.Net;
using System.Reflection;

namespace BaseLibrary;

public class ZohoWebRequestServices : IEmailSender
{
    UserAddress? emailFrom;

    private IHTTPServices httpServices;
    private string emailSender;
    long userId;
    public ZohoWebRequestServices(string emailSender, long userId, IHTTPServices? httpServices = null)
    {
        this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        this.userId = userId;
        //this.password = password ?? throw new ArgumentNullException(nameof(password));
        this.httpServices = httpServices ?? Locator.Current.GetService<IHTTPServices>()! ?? throw new ArgumentNullException(nameof(httpServices));
        CreateObjects();
    }
    private void CreateObjects()
    {
        emailFrom = new UserAddress();
        emailFrom.UserName = "sender";
        emailFrom.Email = emailSender;

    }
    //https://www.zoho.com/crm/developer/docs/api2.1/csharp-sdk/v1/send-mail-samples.html?src=send_mail
    //https://www.zoho.com/crm/developer/docs/api2.1/csharp-sdk/v1/sample-codes.html
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public async Task<GOSResult> SendEmail(string emailTo, string subject, string message, bool isAsync)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    {
        if (!httpServices.IsConnectedToInternet())
            return new GOSResult(false);


        string WEBSERVICE_URL = "https://mail.zoho.com/api/accounts/" + userId + "/messages";
        try
        {
#pragma warning disable SYSLIB0014 // Type or member is obsolete
            var webRequest = WebRequest.Create(WEBSERVICE_URL);
#pragma warning restore SYSLIB0014 // Type or member is obsolete
            if (webRequest is not null)
            {
                webRequest.Method = "POST";
                webRequest.Headers.Add("Authorization", "{token}}");
                webRequest.ContentType = "application/json";

                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    string json = "{\"fromAddress\": \"{emailSender}}\"," +
                                    "\"toAddress\": \"{emailTo}}\"," +
                                    "\"subject\": \"{subject}\"," +
                                    "\"content\": \"{message}\"}";

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }

                var httpResponse = (HttpWebResponse)webRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Console.WriteLine(String.Format("Response: {0}", result));
                    return new GOSResult(true, null, result);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            return new GOSResult(ex);
        }

        return new GOSResult(false);
    }
}

