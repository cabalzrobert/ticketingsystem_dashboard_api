using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.RequestModel.SubscriberApp;
using webapi.App.Model.User;
using webapi.App.Globalize.Company;

using Newtonsoft.Json;
using System.IO;
using System.Net;

namespace webapi.App.Aggregates.SubscriberAppAggregate
{
    [Service.ITransient(typeof(ForgotAccountRepository))] 
    public interface IForgotAccountRepository 
    {
        Task<(Results result, String message)> RequestForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(Results result, String message)> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<(Results result, String message)> RequiredChangePasswordAsync(ForgotPasswordRequest request);
    }

    public class ForgotAccountRepository : IForgotAccountRepository
    {
        private readonly ICompany _company;
        private readonly IRepository _repo;
        public ForgotAccountRepository(ICompany company, IRepository repo){
            _repo = repo; 
            _company = company;
        }
        public async Task<(Results result, String message)> RequestForgotPasswordAsync(ForgotPasswordRequest request){
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BDAS0RFP", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmusrmobno", request.MobileNumber },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (Results.Success, "Code is successfully sent");
                return (Results.Failed, "Mobile number doesn't exist");
            }
            return (Results.Null, null);
        }
        public async Task<(Results result, String message)> ForgotPasswordAsync(ForgotPasswordRequest request){
            var form = request.RequestForm;
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BDAS0RFP1CP", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmusrmobno", form.MobileNumber  },
                { "parmotp", form.OTPCode  },
                { "parmnwpsswrd", form.Password },
                { "parmcnfrmpsswrd", form.ConfirmPassword },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (Results.Success, "Change successfull! You can now use your new password.");
                else if(ResultCode == "61")
                    return (Results.Failed, "Password did not match");
                else if(ResultCode == "21")
                    return (Results.Failed, "Your account has blocked by admin");
                return (Results.Failed, "Failed to Change! your request is already done");
            }
            return (Results.Null, null);
        }
        public async Task<(Results result, String message)> RequiredChangePasswordAsync(ForgotPasswordRequest request){
            var form = request.RequestForm;
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BDAS0FCP", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmusernm", form.Username  },   
                { "parmusrpsswrd", form.OldPassword  },
                { "parmnwpsswrd", form.Password },
                { "parmcnfrmpsswrd", form.ConfirmPassword },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (Results.Success, "Change Successfull! you can now use your new password");
                else if(ResultCode == "61")
                    return (Results.Failed, "Password did not match");
                else if(ResultCode == "21")
                    return (Results.Failed, "Your account has blocked by admin");
                return (Results.Failed, "Failed to Change! your request is already done");
            }
            return (Results.Null, null);
        }
    }
}