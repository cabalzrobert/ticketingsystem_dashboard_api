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
using System.Threading;

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features
{
    [Service.ITransient(typeof(DepartmentHeadRepository))]
    public interface IDepartmentHeadRepository
    {
        Task<(Results result, string message)> CreateTicket(TicketInfo ticket);
        Task<(Results result, object tickets)> GetTickets(FilterTickets param);
        Task<(Results result, string message)> AssignedTicket(TicketInfo ticket);
        Task<(Results result, string message)> ReturnTicket(string ticketNo);
        Task<(Results result, object personnels)> LoadPersonnels(string id);
        Task<(Results result, string message)> ForwardTicket(TicketInfo ticket);
        Task<(Results result, string message)> ResolveTicket(string ticketNo);
        Task<(Results result, string message)> HDResolveTicket(string ticketNo);
        Task<(Results result, string message)> DeclineTicket(string ticketNo);
        Task<(Results result, string message)> CancelTicket(CancelInfo cancelInfo);
        Task<(Results result, object comments)> GetComments(string transactionNo);

        Task<(Results result, object cntticket)> LoadCntTicketAsync(string id);
        Task<(Results result, object count)> UnseenCountAssync(string id);

        Task<(Results result, object tickets)> GetTickets(int row); // test only

    }
    public class DepartmentHeadRepository : IDepartmentHeadRepository
    {
        private readonly ISubscriber _account;
        private readonly IRepository _repo;
        private readonly IFileData _fd;
        private TicketingUser account { get { return _account.AccountIdentity(); } }
        public DepartmentHeadRepository(IRepository repo, ISubscriber account, IFileData fd) 
        {
            _account = account;
            _repo= repo;
            _fd= fd;
        }

        public async Task<(Results result, string message)> CreateTicket(TicketInfo ticket)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_ADDTICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
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
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_HEADTICKETS", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdepartmentid", param.departmentId },
                {"parmtab", param.tab },
                {"parmrow", param.row },
                {"parmsearch", param.search },
                {"parmuserid", account.USR_ID }
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, object tickets)> GetTickets(int row)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_TESTTABLE", new Dictionary<string, object>()
            {
                {"parmrow", row}
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, string message)> AssignedTicket(TicketInfo ticket)
        {
            string supportAccount = _fd.String("Company:Support");
            var splitAccount = supportAccount.Split(':');
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_ASSIGNTICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid",account.PGRP_ID},
                {"parmticketno",ticket.ticketNo },
                {"parmdepartment",account.DEPT_ID },
                {"parmcategory",ticket.categoryId },
                {"parmassignedto",ticket.assignedTo },
                {"parmuserid", account.USR_ID },
                {"parmremarks", ticket.forwardRemarks }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                //Email Service
                Timeout.Set(()=>EmailServices.PrepareSendingToGmail("forward", $"Ticket no. #{row["ticketNo"].Str()}", row, splitAccount[0], splitAccount[1], row["assignedEmail"].Str()),275);

                await sendAssignedTicket(results, ticket.assignedTo);
                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<bool> sendAssignedTicket(IDictionary<string, object> data, string assignedTo)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{assignedTo}/assigned",
                new { type = "assigned-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<(Results result, string message)> ReturnTicket(string ticketNo)
        {
            string supportAccount = _fd.String("Company:Support");
            var splitAccount = supportAccount.Split(':');
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_RETURNTICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmuserid", account.USR_ID},
                {"parmticketno", ticketNo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                //Email Service
                Timeout.Set(()=>EmailServices.PrepareSendingToGmail("forward", $"Request Ticket no. #{row["ticketNo"].Str()} for resolve", row, splitAccount[0], splitAccount[1], row["forwardEmail"].Str()),275);

                //Notification
                await sendReturnTicket(results, row["forwardTo"].Str());

                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<bool> sendReturnTicket(IDictionary<string, object> data, string forwardto)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{forwardto}/return",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }


        public async Task<(Results result, object personnels)> LoadPersonnels(string id)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_GETPERSONNELS", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmuserid", account.USR_ID},
                {"parmdeptid", id }
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, string message)> ResolveTicket(string ticketNo)
        {
            string supportAccount = _fd.String("Company:Support");
            var splitAccount = supportAccount.Split(':');
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_RESOLVETICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmuserid", account.USR_ID},
                {"parmticketno", ticketNo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                //Email Service
                Timeout.Set(()=>EmailServices.PrepareSendingToGmail("resolve", $"Request for Acknowledgement Ticket No. #{row["ticketNo"].Str()}", row, splitAccount[0], splitAccount[1], row["forwardEmail"].Str()),275);
                await sendApprovalTicket(results, row["requestId"].Str());
                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<(Results result, string message)> HDResolveTicket(string ticketNo)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_HDRESOLVETICKET", new Dictionary<string, object>()
            {
                {"parmuserid", account.USR_ID},
                {"parmticketno", ticketNo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                //await sendApprovalTicket(results, row["requestId"].Str());
                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<bool> sendApprovalTicket(IDictionary<string, object> data, string forwardto)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{forwardto}/approval",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<(Results result, string message)> DeclineTicket(string ticketNo)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_CANCELTICKET", new Dictionary<string, object>()
            {
                {"parmuserid", account.USR_ID},
                {"parmticketno", ticketNo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                await sendDeclinedTicket(results, row["requestId"].Str());
                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<bool> sendDeclinedTicket(IDictionary<string, object> data, string forwardto)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{forwardto}/decline",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<(Results result, string message)> CancelTicket(CancelInfo cancelInfo)
        {
            string supportAccount = _fd.String("Company:Support");
            var splitAccount = supportAccount.Split(':');
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_DISMISSTICKET", new Dictionary<string, object>()
            {
                {"parmuserid", account.USR_ID},
                {"parmticketno", cancelInfo.ticketNo },
                {"parmrfc", cancelInfo.RFC }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
            {
                //Email Service
                //EmailServices.PrepareSendingToGmail(account.FLL_NM, $"Forward this ticket no. #{row["ticketNo"].Str()} to {row["departmentName"].Str()}", row, splitAccount[0], splitAccount[1], row["forwardEmail"].Str());
                await sendCanceledTicket(results, row["requestId"].Str());
                return (Results.Success, "Success");
            }
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<bool> sendCanceledTicket(IDictionary<string, object> data, string forwardto)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{forwardto}/cancel",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<(Results result, string message)> ForwardTicket(TicketInfo ticket)
        {
            string supportAccount = _fd.String("Company:Support");
            var splitAccount = supportAccount.Split(':');
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_FORWARDTICKET", new Dictionary<string, object>()
            {
                //{"parmplid", "0002"},
                //{"parmpgrpid","001" },
                //{"parmticketno",ticket.ticketNo },
                //{"parmassigneddepartment",ticket.assignedDepartment },
                //{"parmforwarddepartment",ticket.forwardDepartment },
                //{"parmforwardto",ticket.forwardTo },
                //{"parmremarks",ticket.forwardRemarks },
                //{"parmstatus",ticket.status },
                //{"parmforwardedby", "00020010000001"}

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
                Timeout.Set(()=>EmailServices.PrepareSendingToGmail("forward", $"Forward this ticket no. #{row["ticketNo"].Str()} to {row["departmentName"].Str()}", row, splitAccount[0], splitAccount[1], row["forwardEmail"].Str()),275);

                //Call 
                await PostForwardTicket(results, row["forwardTo"].Str());
                await RequestorTicket(results, row["requestId"].Str());
                return (Results.Success, "Success");
            }
                
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<bool> PostForwardTicket(IDictionary<string, object> data, string forwardto)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{forwardto}/forwardticket",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }
        public async Task<bool> RequestorTicket(IDictionary<string, object> data, string requestorid)
        {
            await Pusher.PushAsync($"{account.PL_ID}/{account.PGRP_ID}/{requestorid}/requestor",
                new { type = "forwardticket-notification", content = SubscriberDto.RequestTicketNotification(data), notification = SubscriberDto.RequestNotification(data) });
            return true;
        }

        public async Task<(Results result, object comments)> GetComments(string transactionNo)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_HEADTICKETCOMMENTS", new Dictionary<string, object>()
            {
                {"parmuserid", "00020010000001"},
                {"parmtransactionno", transactionNo}
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, object cntticket)> LoadCntTicketAsync(string id)
        {
            var results = _repo.DSpQueryMultiple($"dbo.spfn_AEARP0F", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdepartmentid", id},
            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.LoadCountDepartmentHeadAssignedTicket(results.ReadSingleOrDefault()));
            return (Results.Null, null);
        }

        public async Task<(Results result, object count)> UnseenCountAssync(string id)
        {
            var result = _repo.DSpQueryMultiple("dbo.spfn_AEARP0G", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmdepartmentid", id },
                { "parmuserid", account.USR_ID },
            }).ReadSingleOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                return (Results.Success, row["UN_OPN"].Str());
            }
            return (Results.Null, null);
        }
    }
}
