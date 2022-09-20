using Com.Zoho.Crm.API.SendMail;
using Com.Zoho.Crm.API.Util;
using Newtonsoft.Json;
using Splat;
using System.Reflection;

namespace BaseLibrary;

public class ZohoCRMServices : IEmailSender
{
    SendMailOperations sendMailOperations = new();
    UserAddress emailFrom;

    private IHTTPServices httpServices;
    private string emailSender, moduleAPIName;
    long? recordId;
    public ZohoCRMServices(string emailSender, long recordId, string moduleAPIName, IHTTPServices? httpServices = null)
    {
        this.emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        this.recordId = recordId;
        this.moduleAPIName = moduleAPIName ?? throw new ArgumentNullException(nameof(moduleAPIName));
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
    public async Task<GOSResult> SendEmail(string emailTo, string subject, string message, bool isAsync)
    {
        //Get instance of SendMailOperations Class
        if (httpServices.IsConnectedToInternet())
            return new(false);

        Mail mail = new()
        {
            From = emailFrom,
        };

        UserAddress to = new UserAddress();
        to.UserName = "to";
        to.Email = emailTo;
        mail.To = new() { to };

        mail.Subject = subject;
        mail.Content = message;
        mail.ConsentEmail = false;
        mail.MailFormat = "text";

        Com.Zoho.Crm.API.InventoryTemplates.InventoryTemplate template = new();
        template.Id = 347706179;
        mail.Template = template;

        BodyWrapper wrapper = new BodyWrapper();
        wrapper.Data = new() { mail };

        //Call SendMail method
        var response = sendMailOperations.SendMail(recordId, moduleAPIName, wrapper);

        if (response is not null)
        {
            //Get the status code from response
            Console.WriteLine("Status Code: " + response.StatusCode);

            //Check if expected response is received
            if (response.IsExpected)
            {
                //Get object from response
                ActionHandler actionHandler = response.Object;

                if (actionHandler is ActionWrapper actionWrapper)
                {
                    //Get the received ActionWrapper instance
                    //ActionWrapper actionWrapper = (ActionWrapper)actionHandler;

                    //Get the list of obtained ActionResponse instances
                    var actionResponses = actionWrapper.Data;

                    foreach (ActionResponse actionResponse in actionResponses)
                    {
                        //Check if the request is successful
                        if (actionResponse is SuccessResponse)
                        {
                            //Get the received SuccessResponse instance
                            SuccessResponse successResponse = (SuccessResponse)actionResponse;

                            //Get the Status
                            Console.WriteLine("Status: " + successResponse.Status.Value);

                            //Get the Code
                            Console.WriteLine("Code: " + successResponse.Code.Value);

                            Console.WriteLine("Details: ");

                            //Get the details map
                            foreach (var entry in successResponse.Details)
                            {
                                //Get each value in the map
                                Console.WriteLine(entry.Key + " : " + JsonConvert.SerializeObject(entry.Value));
                            }

                            //Get the Message
                            Console.WriteLine("Message: " + successResponse.Message.Value);
                        }
                        //Check if the request returned an exception
                        else if (actionResponse is APIException exception)
                        {
                            //Get the received APIException instance
                            //APIException exception = (APIException)actionResponse;

                            //Get the Status
                            Console.WriteLine("Status: " + exception.Status.Value);

                            //Get the Code
                            Console.WriteLine("Code: " + exception.Code.Value);

                            Console.WriteLine("Details: ");

                            //Get the details map
                            foreach (var entry in exception.Details)
                            {
                                //Get each value in the map
                                Console.WriteLine(entry.Key + " : " + JsonConvert.SerializeObject(entry.Value));
                            }

                            //Get the Message
                            Console.WriteLine("Message: " + exception.Message.Value);

                            return new GOSResult(false, new Exception(GetAPIExceptionMessage(exception)), null);
                        }
                    }
                }
                //Check if the request returned an exception
                else if (actionHandler is APIException exception)
                {
                    //Get the received APIException instance
                    //APIException exception = (APIException)actionHandler;

                    //Get the Status
                    Console.WriteLine("Status: " + exception.Status.Value);

                    //Get the Code
                    Console.WriteLine("Code: " + exception.Code.Value);

                    Console.WriteLine("Details: ");

                    //Get the details map
                    foreach (var entry in exception.Details)
                    {
                        //Get each value in the map
                        Console.WriteLine(entry.Key + " : " + JsonConvert.SerializeObject(entry.Value));
                    }

                    //Get the Message
                    Console.WriteLine("Message: " + exception.Message.Value);
                    return new GOSResult(false, new Exception(GetAPIExceptionMessage(exception)), null);
                }
            }
            else
            { //If response is not as expected

                //Get model object from response
                Model responseObject = response.Model;

                //Get the response object's class
                Type type = responseObject.GetType();

                string result = string.Empty;

                //Get all declared fields of the response class
                Console.WriteLine("Type is: {0}", type.Name);
                result += type.Name + Environment.NewLine;

                PropertyInfo[] props = type.GetProperties();

                Console.WriteLine("Properties (N = {0}):", props.Length);
                result += string.Format("Properties (N = {0}):", props.Length) + Environment.NewLine;
                foreach (var prop in props)
                {
                    if (prop.GetIndexParameters().Length == 0)
                    {
                        Console.WriteLine("{0} ({1}) in {2}", prop.Name, prop.PropertyType.Name, prop.GetValue(responseObject));
                        result += string.Format("{0} ({1}) in {2}", prop.Name, prop.PropertyType.Name, prop.GetValue(responseObject)) + Environment.NewLine;
                    }
                    else
                    {
                        Console.WriteLine("{0} ({1}) in ", prop.Name, prop.PropertyType.Name);
                        result += string.Format("{0} ({1}) in ", prop.Name, prop.PropertyType.Name) + Environment.NewLine;
                    }
                }
                return new GOSResult(false, null, result);
            }
        }
        return new GOSResult(false);
    }
    private string GetAPIExceptionMessage(APIException exception)
    {
        string exceptionMessage = exception.Status.Value + Environment.NewLine +
                                exception.Code.Value + Environment.NewLine;

        foreach (var entry in exception.Details)
        {
            //Get each value in the map
            exceptionMessage += entry.Key + " : " + JsonConvert.SerializeObject(entry.Value) + Environment.NewLine;
        }

        exceptionMessage += exception.Message.Value;
        return exceptionMessage;
    }

    
}

