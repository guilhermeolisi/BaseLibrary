using System;
using System.Net;
using System.Net.Mail;
using System.Net.NetworkInformation;

namespace BaseLibrary;

public static class HTTPMethods
{
    public static bool IsConnectedToInternetPing(string host = null)
    {
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
    /// <summary>
    /// 
    /// </summary>
    /// <param name="emailSender"></param>
    /// <param name="keypass"></param>
    /// <param name="emailTo"></param>
    /// <param name="message"></param>
    public static void SendEmail(string emailSender, string keypass, string subject, string emailTo, string message, bool isAsync)
    {
        //https://pt.stackoverflow.com/questions/630/como-posso-enviar-um-e-mail-pelo-gmail
        //https://www.c-sharpcorner.com/blogs/send-email-using-gmail-smtp

        MailMessage mail = new()
        {
            From = new(emailSender),
            To = { emailTo },
            Subject = subject,
            Body = message,

        };

        SmtpClient smtp = new();
        smtp.SendCompleted += (s, e) =>
        {
            // após o envio pode chamar o Dispose
            smtp.Dispose();
        };

        if (emailSender.ToLower().Contains("gmail.com"))
        {
            //https://support.google.com/mail/answer/7126229?authuser=2&authuser=2&hl=pt-BR&authuser=2&visit_id=637722547938951094-494706498&rd=2#zippy=%2Cetapa-alterar-o-smtp-e-outras-configura%C3%A7%C3%B5es-no-seu-cliente-de-e-mail
            //Port 465
            smtp.Host = "smtp.gmail.com";
        }
        smtp.EnableSsl = true; // GMail requer SSL
        smtp.Port = 587;       // porta para SSL 587
        smtp.DeliveryMethod = SmtpDeliveryMethod.Network; // modo de envio
        smtp.UseDefaultCredentials = false; // vamos utilizar credencias especificas


        // seu usuário e senha para autenticação
        smtp.Credentials = new NetworkCredential(emailSender, keypass);

        // envia o e-mail
        try
        {
            if (isAsync)
            {
                smtp.SendAsync(mail, null);
            }
            else
            {
                smtp.Send(mail);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
    public static bool IsValidURL(string url)
    {
        Uri uriResult;
        return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
            && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    ////https://techblog.desenvolvedores.net/2011/03/22/checar-se-a-conexao-com-a-internet-esta-ativa/
    //[DllImport("wininet.dll")]
    //private extern static bool InternetGetConnectedState(out int Description, int ReservedValue);

    ////https://www.prugg.at/2019/09/09/properly-detect-windows-version-in-c-net-even-windows-10/
    //public static bool IsRS3OrAbove()
    //{
    //    Version osVersion = Environment.OSVersion.Version;

    //    if (osVersion.Major >= 10 && osVersion.Build >= 16299)
    //    {
    //        return true;
    //    }
    //    else
    //    {
    //        return false;
    //    }
    //}
    ///// <summary>
    ///// verificar se está conectado à internet
    ///// </summary>
    ///// <returns>true se existir uma conexão</returns>
    ///// <remarks>retornar true não quer dizer que o destino esteja alcançável, utilize
    ///// IsReachable(url) para saber se o destino está alcançável</remarks>
    //public static bool IsConnectedToInternet(string uriBase, bool isWindows)
    //{
    //    int Desc;
    //    bool ret = false;
    //    if (isWindows)
    //    {
    //        ret = InternetGetConnectedState(out Desc, 0);

    //        if (ret)
    //            ret = IsReachable(uriBase);
    //    }
    //    else
    //    {
    //        ret = IsReachable(uriBase);
    //    }
    //    return ret;
    //}
    ///// <summary>
    ///// verifica se o destino informado está alcançável. (Acessível)
    ///// </summary>
    ///// <param name="_url">endereço URL</param>
    ///// <returns>true se estiver alcançável</returns>
    //public static bool IsReachable(string _url)
    //{
    //    System.Uri Url = new System.Uri(_url);

    //    System.Net.WebRequest webReq;
    //    System.Net.WebResponse resp;
    //    webReq = System.Net.WebRequest.Create(Url);

    //    try
    //    {
    //        resp = webReq.GetResponse();
    //        resp.Close();
    //        webReq = null;
    //        return true;
    //    }
    //    catch
    //    {
    //        webReq = null;
    //        return false;
    //    }
    //}
    /// <summary>
    /// Verificar conexão
    /// </summary>
    /// <param name="host"></param>
    /// <returns></returns>


    //check for an update on my server
    //public static async Task<bool> CheckUpdate(string uriBase, string fileCheckXml)
    //{
    //    if (IsRS3OrAbove()) return false;

    //    if (!IsConnectedToInternet(uriBase, false)) return false;

    //    //Pega a versão do servidor
    //    XmlDocument xml = new XmlDocument();
    //    try
    //    {
    //        Stream stream;
    //        using (WebClient client = new WebClient())
    //        {
    //            stream = client.OpenRead(uriBase + fileCheckXml);//InstallModel.infos.uriBase + "/Installer.appinstaller"
    //        }
    //        xml.Load(stream);
    //        stream.Dispose();
    //    }
    //    catch
    //    {

    //    }
    //    Version newVersion = new Version(xml.DocumentElement.GetAttribute("Version"));

    //    //Pega a versão instalada atualmente
    //    DirectoryInfo folder = new DirectoryInfo(Environment.CurrentDirectory).Parent;
    //    //DirectoryInfo folder = new DirectoryInfo(@"C:\Program Files\MsixCoreApps\aeaaf581-189a-49fb-a770-955e2f4b0857_0.0.5.0_x86__7wqq6cz424w7t\Nimloth").Parent;
    //    string fileManifest = Path.Combine(folder.FullName, "AppxManifest.xml");
    //    Version currentVersion = null;
    //    try
    //    {
    //        if (File.Exists(fileManifest))
    //        {
    //            Stream stream = File.OpenRead(fileManifest);
    //            xml = new XmlDocument();
    //            xml.Load(stream);
    //            bool found = false;
    //            foreach (XmlNode xe in xml.DocumentElement.ChildNodes)
    //            {
    //                if (xe.Name == "Identity")
    //                    foreach (XmlAttribute xa in xe.Attributes)
    //                        if (xa.Name == "Version")
    //                        {
    //                            currentVersion = new Version(xa.Value);
    //                            found = true;
    //                            break;
    //                        }
    //                if (found)
    //                    break;
    //            }

    //        }
    //    }
    //    catch { }

    //    //compare package versions
    //    if (currentVersion == null || newVersion.CompareTo(currentVersion) > 0)
    //    {
    //        return true;
    //    }
    //    return false;
    //}
    //public static void DownloadFromURL(string url, string toFilePath)
    //{
    //    //Para pegar o tamanho total
    //    //https://www.codeproject.com/articles/35643/get-size-of-a-file-from-the-internet
    //    //WebRequest req = HttpWebRequest.Create(url);
    //    //req.Method = "HEAD";
    //    //WebResponse resp = req.GetResponse();
    //    //long ContentLength = 0;
    //    //if (!long.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
    //    //{

    //    //}

    //    using (WebClient wc = new WebClient())
    //    {
    //        wc.DownloadProgressChanged += wc_DownloadProgressChanged;
    //        wc.DownloadFileCompleted += wc_DownloadFileCompleted;
    //        wc.DownloadFileAsync(new System.Uri(url), toFilePath);
    //    }
    //}
    //public delegate void DownloadProgressChangedEventHandler(int per);
    //public static event DownloadProgressChangedEventHandler DownloadProgressChanged;
    //private static void wc_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
    //{
    //    //Fazer um evento que comunica que o download foi completo
    //    DownloadProgressChanged(-1);
    //}

    ////// Event to track the progress
    //private static void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
    //{
    //    //Fazer um evento para comunicar que já terminou
    //    //progressBar.Value = e.ProgressPercentage;
    //    DownloadProgressChanged(e.ProgressPercentage);
    //}

}
