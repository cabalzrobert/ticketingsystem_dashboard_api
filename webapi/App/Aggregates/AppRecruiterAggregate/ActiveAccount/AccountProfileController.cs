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
using webapi.App.RequestModel.AppRecruiter;
using webapi.App.Aggregates.AppRecruiterAggregate.Common;

namespace webapi.App.Aggregates.AppRecruiterAggregate.ActiveAccount
{
    [Service.ITransient(typeof(AccountProfileRepository))] 
    public interface IAccountProfileRepository 
    {
        Task<(Results result, String message)> ChangePasswordAsync(ChangePasswordRequest request);
    }

    public class AccountProfileRepository : IAccountProfileRepository
    {
        private readonly IRecruiter _identity;
        private readonly IRepository _repo;
        public Recruiter account { get{ return _identity.AccountIdentity(); } } 
        public AccountProfileRepository(IRecruiter identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }
        public async Task<(Results result, String message)> ChangePasswordAsync(ChangePasswordRequest request){
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_ABASI0CP1U", new Dictionary<string, object>(){
                //{ "parmsssid", account.SessionID },
                { "parmcompid", account.CompanyID },
                { "parmbrcd", account.BranchID },
                { "parmrctrid", account.UserID },
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
    }
}