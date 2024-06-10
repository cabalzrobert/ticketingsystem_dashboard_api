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

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.Ticket
{
    [Service.ITransient(typeof(TicketRepository))]
    public interface ITicketRepository
    {
        Task<(Results result, String message)> SaveTicketAsync(TicketModel request);
        Task<(Results result, object ticket)> LoadPendingTicketAsync(FilterRequest req);
        Task<(Results result, object comment)> LoadTicketComment(string TransactioNo);
        Task<(Results result, String message)> SendCommentAsyn(TicketCommentModel request);
        Task<(Results result, object cntticket)> LoadCntTicketAsync();
        Task<(Results result, String message)> TestNotificationAsyn();
        Task<(Results result, object obj)> SeenAsync(String transactionNo);
    }
    public class TicketRepository:ITicketRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public TicketRepository(ISubscriber identity, IRepository repo)
        {
            _identity = identity;
            _repo = repo;
        }

        public async Task<(Results result, string message)> SaveTicketAsync(TicketModel request)
        {
            request.TicketNo = ((int)DateTime.Now.ToTimeMillisecond()).ToString("X");
            var result = _repo.DSpQueryMultiple("dbo.spfn_AEARP0A", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },                
                { "parmuserid", account.USR_ID },                 
                { "parmsssid", account.SessionID },
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
                if(ResultCode == "1")
                {
                    request.TransactionNo = row1["transactionNo"].Str();
                    request.TicketNo = row1["ticketNo"].Str();
                    request.IssuedDate = row1["dateCreated"].Str();
                    request.CreatedDate = Convert.ToDateTime(row1["dateCreated"].Str()).ToString("MMM dd, yyyy");
                    request.Status = row1["status"].Str();
                    request.Statusname = row1["ticketStatus"].Str();
                    request.TicketStatus = row1["status"].Str();
                    request.TicketStatusname = row1["ticketStatus"].Str();
                    await PostTicketRequest(result);
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<bool> PostTicketRequest(IDictionary<string, object> data)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/1/ticketrequest/iscommunicator/",
                new { type = "communicator-notification", content = SubscriberDto.RequestTicketNotification(data) });
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
                return (Results.Success, TickectingSubscriberDto.GetRequestTicketList(results, 100));
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
                { "parmxattchmnt", request.FileAttachment },
                { "parmuserid", account.USR_ID }
            }).FirstOrDefault();
            if (result != null)
            {
                var row1 = ((IDictionary<string, object>)result);
                string ResultCode = row1["RESULT"].Str();
                request.CommentID = row1["CommentID"].Str();
                request.SenderID = account.USR_ID;
                request.DisplayName = row1["DisplayName"].Str();
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
    }
}
