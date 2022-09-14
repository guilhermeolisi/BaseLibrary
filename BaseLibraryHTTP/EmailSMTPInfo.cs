using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public class EmailSMTPInfo
{
    public EmailSMTPInfo(string host, bool enableSsl, int port, SmtpDeliveryMethod deliveryMethod, ICredentialsByHost? credentials)
    {
        Host = host ?? throw new ArgumentNullException(nameof(host));
        EnableSsl = enableSsl;
        Port = port;
        DeliveryMethod = deliveryMethod;
        Credentials = credentials;
    }

    public string Host { get; private set; }
    public bool EnableSsl { get; private set; }
    public int Port { get; private set; }
    public SmtpDeliveryMethod DeliveryMethod { get; private set; }
    public ICredentialsByHost? Credentials { get; private set; }
}
