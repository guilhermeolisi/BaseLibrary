using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Splat;

namespace BaseLibrary;

public class EmailSender : IEmailSender
{
    private IHTTPServices httpServices;
    private string emailSender, password;
    private EmailSMTPInfo smtpInfo;
    SmtpClient smtp;
    public EmailSender(string emailSender, EmailSMTPInfo smtpInfo, IHTTPServices? httpServices = null)
    {



        this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        //this.password = password ?? throw new ArgumentNullException(nameof(password));
        this.smtpInfo = smtpInfo;
        this.httpServices = httpServices ?? Locator.Current.GetService<IHTTPServices>()! ?? throw new ArgumentNullException(nameof(httpServices));
        CreateSmtp();
    }
    private void CreateSmtp()
    {
        smtp = new();
        smtp.Host = smtpInfo.Host; //"smtp.gmail.com";
        smtp.EnableSsl = smtpInfo.EnableSsl; // true; // GMail requer SSL
        smtp.Port = smtpInfo.Port; //587;       // porta para SSL 587
        smtp.DeliveryMethod = smtpInfo.DeliveryMethod; //SmtpDeliveryMethod.Network; // modo de envio
        
        if (smtpInfo.Credentials is not null)
        {
            smtp.Credentials = smtp.Credentials;
        }
        else
        {
            smtp.UseDefaultCredentials = true;
        }
    }
    public GOSResult SendEmail(string emailTo, string subject, string message, bool isAsync)
    {

        //TODO Verificar conexão com internet
        if (httpServices.IsConnectedToInternetPing())
        {
            return new GOSResult(false, new Exception(@"There doest seem to be a network/internet connection.\r\nPlease contact your system administrator"), null);
        }


        //https://pt.stackoverflow.com/questions/630/como-posso-enviar-um-e-mail-pelo-gmail
        //https://www.c-sharpcorner.com/blogs/send-email-using-gmail-smtp

        MailMessage mail = new()
        {
            From = new(emailSender),
            To = { emailTo },
            Subject = subject,
            Body = message,

        };
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
            return new(false, ex, null);
        }
        return new(true);
    }
}
