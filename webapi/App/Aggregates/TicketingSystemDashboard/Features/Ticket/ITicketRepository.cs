using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Aggregates.Common;
using Infrastructure.Repositories;
using webapi.App.Model.User;
using Comm.Commons.Extensions;
using webapi.App.RequestModel.Common;
using webapi.App.Aggregates.Common.Dto;
using webapi.App.Features.UserFeature;
using webapi.Services.Dependency;
using System.Net.Mail;
using System.Net;

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.Ticket
{
    [Service.ITransient(typeof(TicketRepository))]
    public interface ITicketRepository
    {
        Task<(Results result, String message)> SaveTicketAsync(TicketModel request);
        Task<(Results result, String message)> UpdateTicketAsync(TicketModel request);
        Task<(Results result, object ticket)> LoadPendingTicketAsync(FilterRequest req);
        Task<(Results result, object comment)> LoadTicketComment(string TransactioNo);
        Task<(Results result, String message)> SendCommentAsyn(TicketCommentModel request);
        Task<(Results result, object cntticket)> LoadCntTicketAsync();
        Task<(Results result, String message)> TestNotificationAsyn();
        Task<(Results result, object obj)> SeenAsync(String transactionNo);
        Task<(Results result, object isassigned)> IsAssignedAsync(String transactionNo);
        Task<(Results result, String message)> RessolvedAsync(TicketRessolve request);
    }
    public class TicketRepository : ITicketRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        private readonly IFileData _fd;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public TicketRepository(ISubscriber identity, IRepository repo, IFileData fd)
        {
            _identity = identity;
            _repo = repo;
            _fd = fd;
        }

        public async Task<(Results result, string message)> SaveTicketAsync(TicketModel request)
        {
            string supportAccount = _fd.String("Company:Support");
            var split = supportAccount.Split(':');
            if (split.Length == 2)
            {
                string guser = split[0], gpass = split[1];
                request.TicketNo = ((int)DateTime.Now.ToTimeMillisecond()).ToString("X");
                var result = _repo.DSpQueryMultiple("dbo.spfn_AEARP0A", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmsssid", account.SessionID },
                { "parmdepartment", request.Department },
                { "parmcategory", request.Category },
                { "parmpersonnel", request.Personnel },
                { "parmticket", request.TicketNo },
                { "parmsubject", request.TitleTicket },
                { "parmbody", request.TicketDescription },
                { "parmxattchmnt", request.iTicketAttachment },
                { "parmprioritylevel", request.PriorityLevel }
            }).ReadSingleOrDefault();
                if (result != null)
                {
                    var row1 = ((IDictionary<string, object>)result);
                    string ResultCode = row1["RESULT"].Str();
                    if (ResultCode == "1")
                    {
                        request.TransactionNo = row1["transactionNo"].Str();
                        request.TicketNo = row1["ticketNo"].Str();
                        request.IssuedDate = row1["dateCreated"].Str();
                        request.CreatedDate = Convert.ToDateTime(row1["dateCreated"].Str()).ToString("MMM dd, yyyy");
                        request.Status = row1["status"].Str();
                        request.Statusname = row1["ticketStatus"].Str();
                        request.TicketStatus = row1["status"].Str();
                        request.TicketStatusname = row1["ticketStatus"].Str();
                        string stremail = row1["Email_Address"].Str();
                        if (!stremail.IsEmpty())
                        {
                            //var resAsync = await PrepareSendingToGmail(request, guser, gpass, row1["Email_Address"].Str());
                            await PrepareSendingToGmail(request, guser, gpass, row1["Email_Address"].Str());
                        }

                        if (account.ACT_TYP == "6")
                            await PostTicketRequestorHead(result);
                        if (account.ACT_TYP == "5")
                            await PostTicketRequestCommunicator(result);
                        return (Results.Success, "Successfully save.");
                    }
                    else if (ResultCode == "0")
                        return (Results.Failed, "Please check data. Try again");
                }
                return (Results.Null, null);
            }

            return (Results.Failed, "Support account not set, please contact to your Administrator Account");
        }
        public async Task<(Results result, string message)> UpdateTicketAsync(TicketModel request)
        {
            var result = _repo.DSpQueryMultiple("dbo.spfn_AEARP0A1", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmsssid", account.SessionID },
                { "parmtrnno", request.TransactionNo },
                { "parmcategory", request.Category },
                { "parmticket", request.TicketNo },
                { "parmsubject", request.TitleTicket },
                { "parmbody", request.TicketDescription },
                { "parmxattchmnt", request.iTicketAttachment },
                { "parmprioritylevel", request.PriorityLevel }
            }).ReadSingleOrDefault();
            if (result != null)
            {
                var row1 = ((IDictionary<string, object>)result);
                string ResultCode = row1["RESULT"].Str();
                if (ResultCode == "1")
                {
                    request.TransactionNo = row1["transactionNo"].Str();
                    request.TicketNo = row1["ticketNo"].Str();
                    request.IssuedDate = row1["dateCreated"].Str();
                    request.CreatedDate = Convert.ToDateTime(row1["dateCreated"].Str()).ToString("MMM dd, yyyy");
                    request.Status = row1["status"].Str();
                    request.Statusname = row1["ticketStatus"].Str();
                    request.TicketStatus = row1["status"].Str();
                    request.TicketStatusname = row1["ticketStatus"].Str();
                    request.Attachment = row1["ATTCHMNT"].Str();
                    //await PostTicketRequest(result);
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<bool> PostTicketRequestorHead(IDictionary<string, object> data)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/5/{account.DEPT_ID}/requestorhead/",
                new { type = "requestorhead-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<bool> PostTicketRequestCommunicator(IDictionary<string, object> data)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/4/communicator/",
                new { type = "communicator-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<bool> PostTicketRequest(IDictionary<string, object> data)
        {

            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/1/ticketrequest/iscommunicator/",
                new { type = "communicator-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            //new { type = "communicator-notification", content = SubscriberDto.RequestTicketNotification(data) });
            return true;

            //await Pusher.PushAsync($"/{account.PL_ID}/{account.PGRP_ID}/notify"
            //    , new { type = "app-update", content = data });
            //return false;
        }

        //public async Task<bool> PostTicketRequest(object data)
        //{
        //    await Pusher.PushAsync($"/{account.PL_ID}/{account.PGRP_ID}/ticketrequest/iscommunicator",
        //        new { type = "communicator-notification", content = data });
        //    return true;

        //    //await Pusher.PushAsync($"/{account.PL_ID}/{account.PGRP_ID}/notify"
        //    //    , new { type = "app-update", content = data });
        //    //return false;
        //}

        public async Task<(Results result, object ticket)> LoadPendingTicketAsync(FilterRequest req)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_AEARP0B", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmsticketstatus", req.Status},
                {"parmrownum", req.num_row },
                {"parmsearch", req.Search },
                {"parmuserid", account.USR_ID }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetRequestTicketList(results, 25));
            return (Results.Null, null);
        }

        public async Task<(Results result, object comment)> LoadTicketComment(string TransactionNo)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_AEARP0C", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmuserid", account.USR_ID},
                {"parmtrnno", TransactionNo }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetTicketCommentList(results));
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> SendCommentAsyn(TicketCommentModel request)
        {
            int intif = Convert.ToInt32(request.isFile);
            int intim = Convert.ToInt32(request.isImage);
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_AECRT0A", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmtransactionno", request.TransactionNo },
                { "parmmsg", request.Message },
                { "parmisfile", Convert.ToInt32(request.isFile) },
                { "parmisimage", Convert.ToInt32(request.isImage) },
                { "parmxattchmnt", request.iFileAttachment },
                { "parmuserid", account.USR_ID }
            }).FirstOrDefault();
            if (result != null)
            {
                var row1 = ((IDictionary<string, object>)result);
                string ResultCode = row1["RESULT"].Str();
                request.Branch_ID = account.PGRP_ID;
                request.CommentID = row1["CommentID"].Str();
                request.Company_ID = account.PL_ID;
                request.SenderID = account.USR_ID;
                request.DisplayName = row1["DisplayName"].Str();
                //request.ImageAttachment = request.iFileAttachment;
                request.ProfilePicture = account.PRF_PIC;
                request.IsYou = true;
                request.CommentDate = Convert.ToDateTime(row1["RGS_TRN_TS"].Str()).ToString("MMM dd yyyy HH:mm tt");
                if (ResultCode == "1")
                {
                    /*
                    function to send comment to communicator or department head
                    */
                    return (Results.Success, "Successfully send");
                }
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check message. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> TestNotificationAsyn()
        {
            this.Stranger_Connection_Request();
            return (Results.Success, "Successfully Notify");
        }

        public async Task<bool> Stranger_Connection_Request()
        {
            await Pusher.PushAsync($"/{account.PL_ID}/test/notify"
                , new { type = "test-notification", content = "Test Notification" });
            return false;
        }

        public async Task<(Results result, object cntticket)> LoadCntTicketAsync()
        {
            var results = _repo.DSpQueryMultiple($"dbo.spfn_AEARP0D", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmuserid", account.USR_ID},
            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.LoadCountComment(results.ReadSingleOrDefault()));
            return (Results.Null, null);
        }

        public async Task<(Results result, object obj)> SeenAsync(string transactionNo)
        {
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_AEAAEA100A", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmtransactionno", transactionNo },
            });
            return (Results.Success, null);
        }

        public async Task<(Results result, string message)> RessolvedAsync(TicketRessolve request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_AEARP0H", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmticketno", request.TicketNo},
                {"parmtransactionno", request.TransactionNo},
                {"parmstatus", request.Status},
                {"parmactionevents", request.ActionEvent},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                    return (Results.Success, "Successfully save.");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, object isassigned)> IsAssignedAsync(string transactionNo)
        {
            var result = _repo.DSpQueryMultiple("dbo.spfn_AEAAEA100B", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmtransactionno", transactionNo },
            }).ReadSingleOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                return (Results.Success, row["isAssigned"].Str());
            }
            return (Results.Null, null);
        }

        //Send Email to Department Head, Communicator and Assigned Personnel
        private async Task<(Results result, String message)> PrepareSendingToGmail(TicketModel request, String gUser, String gPass, String to_emailaddress)
        {
            MailMessage message = new MailMessage();
            message.From = new MailAddress(gUser);
            message.To.Add(to_emailaddress);
            message.Subject = $"{account.FLL_NM} send Request with Ticketing No.: (" + request.TicketNo + ")";
            message.IsBodyHtml = true;
            message.Body = getBodyFullMessageProblemRequest(request);
            return await TrySendToGmail(request, gUser, gPass, message);
        }
        private async Task<(Results result, String message)> TrySendToGmail(TicketModel request, String gUser, String gPass, MailMessage message, int attemp = 5)
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

        private string getBodyFullMessageProblemRequest(TicketModel request)
        {
            string htmlAttachment = "";
            if (!request.iTicketAttachment.IsEmpty())
            {
                foreach (var attachment in request.TicketAttachment)
                    htmlAttachment += (htmlAttachment.IsEmpty() ? "" : "<br/>") + ($"<a href='{ attachment }' target='_blank'>{ attachment }</a>");
                htmlAttachment = $"<tr><td><b>Attachment(s): </b></td><td>{ htmlAttachment }</td></tr>";
            }
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
                <div style='margin: 10px' align='center'>
                    <table cellspacing='0' cellpadding='0'>
                        <tr><td colspan='2' align='center'><h3>Account Information</h3></td></tr>
                        <tr><td><b>Account #:</b></td><td>{ account.USR_ID }</td></tr>
                        <tr><td><b>Account Name:</b></td><td>{ request.Personnel }</td></tr>
                        <tr><td><b>Department: </b></td><td>{ request.DepartmentName }</td></tr>
                        <tr><td colspan='2'><h3>Reported Problem</h3></td></tr>
                        <tr><td><b>Subject: </b></td><td>{ request.TitleTicket }</td></tr>
                        <tr><td><b>Problem: </b></td><td>{ request.TicketDescription }</td></tr>
                        { htmlAttachment }
                    </table>
                </div>
                </body>
                </html>";
        }
    }
}
