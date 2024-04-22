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
using webapi.App.RequestModel.SubscriberApp.ActiveAccount;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;

namespace webapi.App.Aggregates.SubscriberAppAggregate.ActiveAccount
{
    [Service.ITransient(typeof(AccountProfileRepository))] 
    public interface IAccountProfileRepository 
    {
        Task<(Results result, String message)> ChangePasswordAsync(ChangePasswordRequest request);
        Task<(Results result, String message)> ChangeProfileDetailsAsync(Subscriber request);
        Task<(Results result, String message)> ChangeProfileImageAsync(String urlPath);
    }

    public class AccountProfileRepository : IAccountProfileRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get{ return _identity.AccountIdentity(); } } 
        public AccountProfileRepository(ISubscriber identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }
        public async Task<(Results result, String message)> ChangePasswordAsync(ChangePasswordRequest request){
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BDASI0CP1U", new Dictionary<string, object>(){
                { "parmsssid", account.SessionID },
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                //
                { "parmoldpsswrd", request.OldPassword },
                { "parmnwpsswrd", request.Password },
                { "parmcnfrmpsswrd", request.ConfirmPassword },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (Results.Success, "Change Successfull!");
                else if(ResultCode == "65")
                    return (Results.Failed, "Incorrect old password");
                return (Results.Failed, "An error encountered while processing your request. please try again");
            }
            return (Results.Null, null);
        }
        public async Task<(Results result, String message)> ChangeProfileDetailsAsync(Subscriber request){ 
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BDBSI0CPD1U", new Dictionary<string, object>(){
                { "parmsssid", account.SessionID },
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                //
                { "parmusrfnm", request.Firstname },
                { "parmusrlnm", request.Lastname },
                { "parmusrncknm", request.DisplayName },
                { "parmprsntadd", request.PresentAddress },
                { "parmbrtdt", request.BirthDate },
                { "parmemladd", request.EmailAddress },
                { "parmgndr", "" },
                { "parmmtrlstat", "" },
                { "parmctznshp", "" },
                { "parmbrloczp", "" },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (Results.Success, "Change Successfull!");
                else if(ResultCode == "66")
                    return (Results.Failed, String.Format("Email address {0} is already associated on existing customer account!", request.EmailAddress));
                return (Results.Failed, "An error encountered while processing your request. please try again");
            }
            return (Results.Null, null);
        }
        public async Task<(Results result, String message)> ChangeProfileImageAsync(String urlPath){
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BDBSI0CPI1U", new Dictionary<string, object>(){
                { "parmsssid", account.SessionID },
                { "parmcompid", account.PL_ID },
                { "parmbrcd", account.PGRP_ID },
                { "parmuserid", account.USR_ID },
                //
                { "parmimgurl", urlPath },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (Results.Success, "Change Successfull!");
                return (Results.Failed, "An error encountered while processing your request. please try again");
            }
            return (Results.Null, null);
        }

    }
}