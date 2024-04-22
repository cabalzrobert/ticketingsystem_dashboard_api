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
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.RequestModel.Common;
using webapi.App.Features.UserFeature;

namespace webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount
{
    [Service.ITransient(typeof(PingRepository))] 
    public interface IPingRepository
    {
        Task<(Results result, object test)> SelfCheckAsync();
    }

    public class PingRepository : IPingRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get{ return _identity.AccountIdentity(); } } 
        public PingRepository(ISubscriber identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }

        public async Task<(Results result, object test)> SelfCheckAsync(){
            var results = _repo.DSpQueryMultiple("dbo.spfn_AAASN0A", new Dictionary<string, object>(){
                //{ "parmsssid", account.SessionID },
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
            });
            if(results != null){
                var row = ((IDictionary<string, object>)results.ReadSingleOrDefault());
                string ResultCode = row["RESULT"].Str(); 
                if(ResultCode == "1"){
                    if(row["HAS_NOTIFY"].Str().Equals("1")){
                        await notifySelfNotification(account, results.Read());
                    }
                    return (Results.Success, null); 
                }
            }
            return (Results.Null, null); 
        }

        public static async Task<bool> notifySelfNotification(TicketingUser account,IEnumerable<dynamic> rows)
        {
            if (rows == null || rows.Count() < 1) return false;
            List<object> list = new List<object>();
            foreach (IDictionary<string, object> row in rows)
            {
                list.Clear(); list.Add(row);
                await Pusher.PushAsync($"/{account.PL_ID}/{account.PGRP_ID}/{account.USR_ID}/notify"
                    , new { type = "notification", content = NotificationDto.Notifications(list) });
            }
            return true;
        }
    }
}