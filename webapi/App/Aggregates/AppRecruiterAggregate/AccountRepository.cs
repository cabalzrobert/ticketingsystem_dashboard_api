using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.RequestModel.AppRecruiter;
using webapi.App.Model.User;
using webapi.App.Globalize.Company;

using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace webapi.App.Aggregates.AppRecruiterAggregate
{
    [Service.ITransient(typeof(AccountRepository))] 
    public interface IAccountRepository 
    {
        Task<(Results result, String message, Recruiter user)> SignInAsync(SignInRequest request);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly ICompany _company;
        private readonly IRepository _repo;
        public AccountRepository(ICompany company, IRepository repo){
            _repo = repo; 
            _company = company;
        }
        public async Task<(Results result, String message, Recruiter user)> SignInAsync(SignInRequest request){
            var results = _repo.DSpQueryMultiple("dbo.spfn_ABAOL", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmusrnm", request.Username },
                { "parmpsswrd", request.Password },
            });
            if(results != null){
                var row = ((IDictionary<string, object>)results.ReadSingleOrDefault());
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (Results.Success, null, new Recruiter(){
                        CompanyID = row["COMP_ID"].Str(),
                        BranchID = row["BR_CD"].Str(),
                        UserID = row["USR_ID"].Str(),
                        Fullname = row["FLL_NM"].Str(),
                    });
                else if(ResultCode == "21")
                    return (Results.Failed, "Your account has blocked by admin", null);
                return (Results.Failed, "Invalid mobile number and password! Please try again", null);
            }
            return (Results.Null, null, null);
        }
    }
}