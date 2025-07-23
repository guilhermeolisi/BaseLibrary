using BaseLibrary.DependencyInjection;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BaseLibrary;

public class EmailSenderSendGrid : IEmailSender
{
    private IHTTPServices httpServices;
    private string emailSender;
    EmailAddress emailFrom;
    SendGridClient client;
    public EmailSenderSendGrid(string emailSender, string apiKey, IHTTPServices? httpServices = null)
    {
        this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        this.httpServices = httpServices ?? Locator.ConstanteContainer.Resolve<IHTTPServices>()! ?? throw new ArgumentNullException(nameof(httpServices));
        CreateObjects(apiKey);
    }
    private void CreateObjects(string apiKey)
    {
        //var apiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
        client = new SendGridClient(apiKey);
        emailFrom = new(emailSender);
    }
    public async Task<GOSResult> SendMessage(string emailTo, string subject, string message, bool isAsync)
    {

        if (!httpServices.IsConnectedToInternet())
        {
            return new GOSResult(false);
        }

        //https://app.sendgrid.com/guide/integrate/langs/csharp

        var to = new EmailAddress(emailTo);
        string plainTextContent = message;
        string htmlContent = null;
        var msg = MailHelper.CreateSingleEmail(emailFrom, to, subject, plainTextContent, htmlContent);
        var response = await client.SendEmailAsync(msg);
        if (response.IsSuccessStatusCode)
        {
            return new GOSResult(true);
        }
        else
        {
            string result = response.StatusCode + Environment.NewLine +
                response.Headers + Environment.NewLine +
                response.Body + Environment.NewLine;
            return new GOSResult(result);
        }
    }
}