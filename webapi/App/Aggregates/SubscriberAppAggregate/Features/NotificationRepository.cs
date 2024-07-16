using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using webapi.App.Globalize.Company;

using Newtonsoft.Json;
using webapi.App.RequestModel.Common;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;

namespace webapi.App.Aggregates.SubscriberAppAggregate.Features
{
    [Service.ITransient(typeof(NotificationRepository))] 
    public interface INotificationRepository
    {
        Task<(Results result, object items)> NotificationAsync(FilterRequest filter);
        Task<(Results result, object obj)> SeenAsync(String NotificationID);
        Task<(Results result, object count)> UnseenCountAsync();
        Task<(Results result, object count)> RequestUnseenCountAsync(FilterRequest filter);
        Task<(Results result, object count)> LastTransactionNo();
        Task<(Results result, String message)> ReadAllAsync(NotificationList request);
    }

    public class NotificationRepository : INotificationRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get{ return _identity.AccountIdentity(); } } 
        public NotificationRepository(ISubscriber identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }

        public async Task<(Results result, object items)> NotificationAsync(FilterRequest filter){
            var results = _repo.DSpQueryMultiple("dbo.spfn_0AA0AB0A", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmisall", filter.isRead },
                { "parmiscom", filter.isCom },
                { "parmisdept", filter.isDept },
                { "parmftrns", filter.BaseFilter },
                { "parmdepartment", filter.DepartmentID },
            });
            if(results != null)
                return (Results.Success, NotificationDto.FilterNotifications(results.Read(), 100));
            return (Results.Null, null); 
        }
        public async Task<(Results result, object obj)> SeenAsync(String NotificationID){
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_0AA0AB0B", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmnotifid", NotificationID }, 
            });
            return (Results.Success, null); 
        }
        public async Task<(Results result, object count)> UnseenCountAsync(){
            var result = _repo.DSpQueryMultiple("dbo.spfn_0AA0AB0C", new Dictionary<string, object>(){
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
            }).ReadSingleOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>) result);
                return (Results.Success, row["UN_OPN"].Str());
            }
            return (Results.Null, null);
        }
        public async Task<(Results result, object count)> RequestUnseenCountAsync(FilterRequest filter)
        {
            var result = _repo.DSpQueryMultiple("dbo.spfn_0AA00B", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmiscom", filter.isCom },
                { "parmisdept", filter.isDept },
                { "parmftrns", filter.BaseFilter },
                { "parmdepartment", filter.DepartmentID },
            }).ReadSingleOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                return (Results.Success, row["UN_OPN"].Str());
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, object count)> LastTransactionNo()
        {
            var result = _repo.DSpQueryMultiple("dbo.spfn_0AA0AB0D", new Dictionary<string, object>(){
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
            }).ReadSingleOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                return (Results.Success, row["TRN_NO"].Str());
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> ReadAllAsync(NotificationList request)
        {
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_0AA0AB0E", new Dictionary<string, object>(){
                { "parmplid", account.PL_ID },
                { "parmpgrpid", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                { "parmjson", request.Notificationlist },
            });
            return (Results.Success, null);
        }
    }
}