using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BaseLibrary;

public interface IEmailSender
{
    GOSResult SendEmail(string emailTo, string subject, string message, bool isAsync);
}
