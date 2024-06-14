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

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features
{
    [Service.ITransient(typeof(DepartmentHeadRepository))]
    public interface IDepartmentHeadRepository
    {
        Task<(Results result, string message)> CreateTicket(TicketInfo ticket);
        Task<(Results result, object tickets)> GetTickets(string id, int tab);
        Task<(Results result, string message)> AssignedTicket(TicketInfo ticket);
        Task<(Results result, string message)> ReturnTicket(TicketInfo ticket);
        Task<(Results result, object personnels)> LoadPersonnels(string id);
        Task<(Results result, string message)> ForwardTicket(TicketInfo ticket);
        Task<(Results result, string message)> ResolveTicket(string ticketNo);
        Task<(Results result, string message)> CancelTicket(string ticketNo);
        Task<(Results result, object comments)> GetComments(string transactionNo);
        Task<(Results result, object cntticket)> LoadCntTicketAsync(string id);
        Task<(Results result, object count)> UnseenCountAssync(string id);
    }
    public class DepartmentHeadRepository : IDepartmentHeadRepository
    {
        private readonly ISubscriber _account;
        private readonly IRepository _repo;
        private TicketingUser account { get { return _account.AccountIdentity(); } }
        public DepartmentHeadRepository(IRepository repo, ISubscriber account) 
        {
            _account = account;
            _repo= repo;
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

        public async Task<(Results result, object tickets)> GetTickets(string id, int tab)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_HEADTICKETS", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdepartmentid", id },
                {"parmtab", tab }
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, string message)> AssignedTicket(TicketInfo ticket)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_ASSIGNTICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid",account.PGRP_ID},
                {"parmticketno",ticket.ticketNo },
                {"parmassignedto",ticket.assignedTo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<(Results result, string message)> ReturnTicket(TicketInfo ticket)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_RETURNTICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmticketno",ticket.ticketNo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<(Results result, object personnels)> LoadPersonnels(string id)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_GETPERSONNELS", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdeptid", id }
            });
            if (results != null)
                return (Results.Success, results);
            return (Results.Failed, null);

        }

        public async Task<(Results result, string message)> ResolveTicket(string ticketNo)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_RESOLVETICKET", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmticketno", ticketNo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<(Results result, string message)> CancelTicket(string ticketNo)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_CANCELTICKET", new Dictionary<string, object>()
            {
                {"parmticketno", ticketNo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }

        public async Task<(Results result, string message)> ForwardTicket(TicketInfo ticket)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_FORWARDTICKET", new Dictionary<string, object>()
            {
                {"parmplid", "0002"},
                {"parmpgrpid","001" },
                {"parmticketno",ticket.ticketNo },
                {"parmassigneddepartment",ticket.assignedDepartment },
                {"parmforwarddepartment",ticket.forwardDepartment },
                {"parmforwardto",ticket.forwardTo },
                {"parmremarks",ticket.forwardRemarks },
                {"parmstatus",ticket.status },
                {"parmforwardedby", "00020010000001"}
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

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
