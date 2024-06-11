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
using webapi.App.Aggregates.Common.Dto;

namespace webapi.App.Aggregates.TicketingSystemDashboard
{
    [Service.ITransient(typeof(AccountRepository))]
    public interface IAccountRepository
    {
        Task<(Results result, String message, TicketingUser account)> DashboardSignInAsync(TicketingSignInRequest request);
        Task<(Results result, string message, string userId)> NewUserLogin(string username);
        Task<(Results result, string message, string otp)> SendOTP(SendOtp sendOtp);
        Task<(Results result, string message)> SetPassword(SetPassword setPassword);
    }
    public class AccountRepository:IAccountRepository
    {
        private readonly IRepository _repo;
        public AccountRepository(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<(Results result, string message, TicketingUser account)> DashboardSignInAsync(TicketingSignInRequest request)
        {
            var results = _repo.DSpQueryMultiple("dbo.spfn_ESATBDAOL", new Dictionary<string, object>(){
                {"parmusername", request.Username},
                {"parmuserpassword", request.Password},
            });
            if (results != null)
            {
                var row = ((IDictionary<string, object>)results.ReadFirstOrDefault());
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "Success", new TicketingUser()
                    {
                        PL_ID = row["PL_ID"].Str(),
                        PGRP_ID = row["PGRP_ID"].Str(),
                        USR_ID = row["USR_ID"].Str(),
                        ACT_ID = row["ACT_ID"].Str(),
                        FRST_NM = row["FRST_NM"].Str(),
                        LST_NM = row["LST_NM"].Str(),
                        MDL_NM = row["MDL_NM"].Str(),
                        FLL_NM = row["FLL_NM"].Str(),
                        DEPT_ID = row["DEPT_ID"].Str(),
                        MOB_NO = row["MOB_NO"].Str(),
                        EML_ADD = row["EML_ADD"].Str(),
                        HM_ADDR = row["HM_ADDR"].Str(),
                        PRSNT_ADDR = row["PRSNT_ADDR"].Str(),
                        GNDR = row["GNDR"].Str(),
                        MRTL_STAT = row["MRTL_STAT"].Str(),
                        ImageUrl = row["IMR_URL"].Str(),
                        BRT_DT = row["BRT_DT"].Str(),
                        OCCPTN = row["OCCPTN"].Str(),
                        SKLLS = row["SKLLS"].Str(),
                        PRF_PIC = row["PRF_PIC"].Str(),
                        SIGNATUREID = row["SIGNATUREID"].Str(),
                        SessionID = row["SSSN_ID"].Str(),
                        ACT_TYP = row["ACT_TYP"].Str(),
                        isCommunicator = Convert.ToBoolean(row["isCommunicator"].Str()),
                        isDeptartmentHead = Convert.ToBoolean(row["isDeptartmentHead"].Str())

                    });
                }
                else if (ResultCode == "22")
                    return (Results.Failed, "Your account was in-active", null);
                else if (ResultCode == "21")
                    return (Results.Failed, "Your account has no rigth to access this system", null);
                else if (ResultCode == "0")
                    return (Results.Failed, "Invalid username and password! Please try again", null);
                return (Results.Failed, "Invalid username and password! Please try again", null);
            }
            return (Results.Null, null, null);
        }

        public async Task<(Results result, string message, string userId)> NewUserLogin(string username)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_FIRSTLOGIN", new Dictionary<string, object>()
            {
                {"parmusername", username},
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success", row["USR_ID"].Str());
            else if (resultCode == "0")
                return (Results.Failed, "Failed", null);
            return (Results.Null, null, null);

        }

        public async Task<(Results result, string message, string otp)> SendOTP(SendOtp sendOtp)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_SENDOTP", new Dictionary<string, object>()
            {
                {"parmusrid", sendOtp.UserId},
                {"parmmobno", sendOtp.MobileNumber}
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success", row["OTP"].Str());
            else if (resultCode == "0")
                return (Results.Failed, "Failed", null);
            return (Results.Null, null, null);

        }

        public async Task<(Results result, string message)> SetPassword(SetPassword setPassword)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_SETPASSWORD", new Dictionary<string, object>()
            {
                {"parmuserid", setPassword.UserId},
                {"parmmobno", setPassword.MobileNumber},
                {"parmpassword", setPassword.Password}
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }
    }
}
