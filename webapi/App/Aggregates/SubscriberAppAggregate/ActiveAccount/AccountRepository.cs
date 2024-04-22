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

using Newtonsoft.Json;
using webapi.Services.Firebase;
using Comm.Commons.Advance;

namespace webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount
{
    [Service.ITransient(typeof(AccountRepository))] 
    public interface IAccountRepository
    {
        Task<(Results result, object balance)> AccountBalanceAsync();
        //Task<Results> FirebaseTokenUpdateAsync(String firebaseToken);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get{ return _identity.AccountIdentity(); } } 
        public AccountRepository(ISubscriber identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }

        public async Task<(Results result, object balance)> AccountBalanceAsync(){
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BFA0A", new Dictionary<string, object>(){
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
            }).FirstOrDefault();
            if(result != null)
                return (Results.Success, SubscriberDto.AccountBalance(account, result));
            return (Results.Null, null); 
        }
    }
}