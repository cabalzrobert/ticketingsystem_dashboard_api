using System.Net.Mail;
using System.Net;
using System.Threading.Tasks;
using System;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using Comm.Commons.Extensions;

namespace webapi.Services
{
    public static class EmailServices
    {
        public static async Task<(Results result, String message)> PrepareSendingToGmail(string emailType,string subject, dynamic request, String gUser, String gPass, String to_emailaddress)
        {

            MailMessage message = new MailMessage();
            message.From = new MailAddress(gUser);
            message.To.Add(to_emailaddress);
            message.Subject = subject;
            message.IsBodyHtml = true;
            message.Body = (emailType=="otp")?getOtpMessage(request):getBodyFullMessageProblemRequest(request);
            return await TrySendToGmail(request, gUser, gPass, message);
        }
        private static async Task<(Results result, String message)> TrySendToGmail(dynamic request, String gUser, String gPass, MailMessage message, int attemp = 5)
        {
            try
            {
                using (var stmp = new SmtpClient
                {
                    Host = "smtp.mail.yahoo.com",
                    Port = 587,
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Credentials = new NetworkCredential(gUser, gPass),
                    Timeout = 20000,
                })
                {
                    stmp.Send(message);
                    //stmp.Send(gUser, gUser, "Abacos Report Problem (" + request.TicketNo + ")", getBodyFullMessageProblemRequest(request));
                    /*
			SendEmailExternal(GlobalEmailSupport,"Abacos Report Problem (" + ticketno + ")", htmlString);
			SendDirectSMS(userid,"Thank you for contacting us. This is an automated response confirming the receipt of your ticket ID: "+ticketno+". One of our agents will get back to you as soon as possible.");
			SendSMSDirectMobile(GlobalMobileSupport, "You have new reported problem of Abacos App with ticket ID: "+ticketno+". please check your admin support email to view full report details");
                    */
                    return (Results.Success, "Problem successfully reported");
                }
            }
            catch (Exception ex)
            {
                String exMessage = ex.Message;
            }
            if (attemp > 0)
                return await TrySendToGmail(request, gUser, gPass, message, attemp - 1);
            return (Results.Failed, "Cannot send right now, please try again later");
        }

        private static string getBodyFullMessageProblemRequest(dynamic request)
        {
            string htmlAttachment = "";
            //if (!request.iTicketAttachment.IsEmpty())
            //{
            //    foreach (var attachment in request.TicketAttachment)
            //        htmlAttachment += (htmlAttachment.IsEmpty() ? "" : "<br/>") + ($"<a href='{attachment}' target='_blank'>{attachment}</a>");
            //    htmlAttachment = $"<tr><td><b>Attachment(s): </b></td><td>{htmlAttachment}</td></tr>";
            //}
            return $@"
                <!DOCTYPE html>
                <html><head>
                <style type='text/css'>
                body{{ font-family: Helvetica, Verdana; font-size:2vw; color: #4d4c4c; margin: 0; }}
                table{{ border-collapse: collapse; border: 1px solid #d1d1d1; width: 100% }}
                td{{ border: 1px solid #d1d1d1; padding: 5px 10px; border: 1px solid #d1d1d1; }}
                tr{{ padding: 2px }}
                h1,h2,h3,h4{{ margin: 2px; vertical-align: bottom; }}
                </style>
                </head>
                <body>
                <h2>Request</h2>
                <div><p>{request.remarks}</p></div>
                <div style='margin: 10px' align='center'>
                    <table cellspacing='0' cellpadding='0'>
                        <tr><td colspan='2' align='center'><h3>Ticket Information</h3></td></tr>
                        <tr><td><b>Account #:</b></td><td>{request.requestId}</td></tr>
                        <tr><td><b>Account Name:</b></td><td>{request.requestName}</td></tr>
                        <tr><td><b>Department: </b></td><td>{request.requestDepartmentName}</td></tr>
                        <tr><td colspan='2'><h3>Reported Problem</h3></td></tr>
                        <tr><td><b>Subject: </b></td><td>{request.title}</td></tr>
                        <tr><td><b>Detail: </b></td><td>{request.description}</td></tr>
                    </table>
                </div>
                </body>
                </html>";
        }

        private static string getOtpMessage(dynamic request)
        {
            string htmlAttachment = "";
            //if (!request.iTicketAttachment.IsEmpty())
            //{
            //    foreach (var attachment in request.TicketAttachment)
            //        htmlAttachment += (htmlAttachment.IsEmpty() ? "" : "<br/>") + ($"<a href='{attachment}' target='_blank'>{attachment}</a>");
            //    htmlAttachment = $"<tr><td><b>Attachment(s): </b></td><td>{htmlAttachment}</td></tr>";
            //}
            return $@"
                <!DOCTYPE html>
                <html><head>
                <style type='text/css'>
                body{{ font-family: Helvetica, Verdana; font-size:2vw; color: #4d4c4c; margin: 0; }}
                table{{ border-collapse: collapse; border: 1px solid #d1d1d1; width: 100% }}
                td{{ border: 1px solid #d1d1d1; padding: 5px 10px; border: 1px solid #d1d1d1; }}
                tr{{ padding: 2px }}
                h1,h2,h3,h4{{ margin: 2px; vertical-align: bottom; }}
                </style>
                </head>
                <body>
                <h2>OTP</h2>
                <div><p>Your 6 digit code {request.otp}</p></div>
                <div style='margin: 10px' align='center'>
                </div>
                </body>
                </html>";
        }
    }
}
