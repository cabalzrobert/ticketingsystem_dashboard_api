using Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.App.Aggregates.Common;
using webapi.Commons.AutoRegister;
using Comm.Commons.Extensions;
using webapi.App.TSDashboardModel;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Model.User;
using webapi.App.Aggregates.Common.Dto;
using webapi.App.Features.UserFeature;
using webapi.Services;
using webapi.Services.Dependency;

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features
{
    [Service.ITransient(typeof(CommunicatorRepository))]
    public interface ICommunicatorRepository
    {
        Task<(Results result, string message)> CreateTicket(TicketInfo ticket);
        Task<(Results result, object tickets)> GetTickets(FilterTickets param);
        Task<(Results result, string message)> ForwardTicket(TicketInfo ticket);
        Task<(Results result, string message)> ConfirmationForwardTicket(TicketInfo ticket);
        Task<(Results result, object comments)> GetComments(string transactionNo);
        Task<(Results result, object cntticket)> LoadCntTicketAsync();
    }
    public class CommunicatorRepository : ICommunicatorRepository
    {
        private readonly ISubscriber _account;
        private readonly IRepository _repo;
        private readonly IFileData _fd;
        private TicketingUser account { get { return _account.AccountIdentity(); } }
        public CommunicatorRepository(IRepository repo, ISubscriber account, IFileData fd)
        {
            _account = account;
            _repo = repo;
            _fd = fd;
        }

        public async Task<(Results result, string message)> CreateTicket(TicketInfo ticket)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_ADDTICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID },
                {"parmrequestId", account.USR_ID},
                {"parmcategory",  ticket.categoryId},
                {"parmtitle",  ticket.title},
                {"parmdescription",  ticket.description},
                {"parmattachment", ticket.attachment},
                {"parmprioritylevel", ticket.priorityLevel},
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<(Results result, object tickets)> GetTickets(FilterTickets param)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_COMTICKETS", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmtab", param.tab },
                {"parmrow", param.row },
                {"parmsearch", param.search },
                {"parmuserid", account.USR_ID }
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, string message)> ForwardTicket(TicketInfo ticket)
        {
            string supportAccount = _fd.String("Company:Support");
            var splitAccount = supportAccount.Split(':');
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_FORWARDTICKET", new Dictionary<string, object>()
            {
                //{"parmplid", account.PL_ID},
                //{"parmpgrpid", account.PGRP_ID},
                //{"parmticketno",ticket.ticketNo },
                //{"parmassigneddepartment",ticket.assignedDepartment },
                //{"parmforwarddepartment",ticket.forwardDepartment },
                //{"parmforwardto",ticket.forwardTo },
                //{"parmremarks",ticket.forwardRemarks },
                //{"parmstatus",ticket.status },
                //{"parmforwardedby", account.USR_ID}

                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmticketno",ticket.ticketNo },
                {"parmforwarddepartment",ticket.forwardDepartment },
                {"parmforwardcategory",ticket.forwardCategory },
                {"parmforwardto",ticket.forwardTo },
                {"parmremarks",ticket.forwardRemarks },
                {"parmstatus",ticket.status },
                {"parmforwardedby", account.USR_ID}
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                //Email Service
                Timeout.Set(() => EmailServices.PrepareSendingToGmail(account.FLL_NM, $"Forward this ticket no. #{row["ticketNo"].Str()} to {row["departmentName"].Str()}", row, splitAccount[0], splitAccount[1], row["forwardEmail"].Str()), 275);

                //for pusher function department head receiver
                //await PostTicketRequest(results, ticket.assignedDepartment);
                await PostForwardTicket(results, ticket.forwardTo, ticket.forwardDepartment);
                await RequestorTicket(results, row["requestId"].Str());

                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<bool> PostForwardTicket(IDictionary<string, object> data, string forwardto, string forwardDepartment)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{forwardDepartment}/{forwardto}/forwardticket",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }
        public async Task<bool> RequestorTicket(IDictionary<string, object> data, string requestorid)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{requestorid}/forwardticket",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<(Results result, string message)> ConfirmationForwardTicket(TicketInfo ticket)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_CONFIRMFORWARD", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid",account.PGRP_ID},
                {"parmticketno",ticket.ticketNo },
                {"parmforwarddepartment",ticket.forwardDepartment },
                {"parmforwardto",ticket.forwardTo },
                {"parmpermission", ticket.permission },
                {"parmremarks",ticket.forwardRemarks },
                {"parmcommentid", ticket.commentId },
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                //for pusher function personnel receiver
                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<(Results result, object comments)> GetComments(string transactionNo)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_COMTICKETCOMMENTS", new Dictionary<string, object>()
            {
                {"parmtransactionno", transactionNo}
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, object cntticket)> LoadCntTicketAsync()
        {
            var results = _repo.DSpQueryMultiple($"dbo.spfn_AEARP0E", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID}
            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.LoadCountTicketCommunicator(results.ReadSingleOrDefault()));
            return (Results.Null, null);
        }
        public async Task<bool> PostTicketRequest(IDictionary<string, object> data, string departmentid)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{departmentid}/forwardticket/depthead/1",
                new { type = "departmenthead-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }
    }
}
