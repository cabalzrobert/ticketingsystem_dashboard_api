using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.RequestModel.SubscriberApp;
using webapi.App.RequestModel.SubscriberApp.Common;
using webapi.App.Model.User;
using webapi.App.Globalize.Company;

using Newtonsoft.Json;
using System.IO;
using System.Net;
using webapi.App.Features.UserFeature;

namespace webapi.App.Aggregates.SubscriberAppAggregate
{
    [Service.ITransient(typeof(AccountRepository))] 
    public interface IAccountRepository 
    {
        Task<(SignInResults result, String message, String apkVersion, String apkUrl)> ApkUpdateCheckerAsync(SignInRequest request);
        Task<(SignInResults result, String message, Subscriber user, object data)> SignInAsync(SignInRequest request);
        Task<(SignUpResults result, Subscriber user, String message)> SignUpAsync(SignUpRequest request);
        Task<(SignUpResults result, String message)> SignUpWithMobileAsync(SignUpWithMobileRequest request);
        Task<(VerifyOtpResults result, Subscriber user, String message)> VerifyOtpAsync(AuthFormRequest request);
    }

    public class AccountRepository : IAccountRepository
    {
        private readonly ICompany _company;
        private readonly IRepository _repo;
        public AccountRepository(ICompany company, IRepository repo){
            _repo = repo; 
            _company = company;
        }
        public async Task<(SignInResults result, String message, String apkVersion, String apkUrl)> ApkUpdateCheckerAsync(SignInRequest request){
            var results = _repo.DSpQueryMultiple("dbo.spfn_BAA0A", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmapkvrsn", request.ApkVersion },
                { "parmstrmnl", (request.Terminal?"1":"0") }, 
            });
            if(results!=null){
                var result = ((IDictionary<string, object>)results.ReadSingleOrDefault());
                if(result==null)
                    return (SignInResults.Success, null, null, null);
                return (SignInResults.ApkUpdate, "New version available", result["APP_VRSN"].Str(), result["APK_UPDT_URL"].Str());
            }
            return (SignInResults.Null, null, null, null);
        }
        public async Task<(SignInResults result, String message, Subscriber user, object data)> SignInAsync(SignInRequest request){
            string SignatureID = DateTime.Now.ToTimeMillisecond().ToString("X"); 
            var results = _repo.DSpQueryMultiple("dbo.spfn_BDASL69MV0A", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmusernm", request.Username },
                { "parmusrpsswrd", request.Password },
                { "parmdvcnm", request.DeviceName }, 
                { "parmdvcmdlid", $"{ request.DeviceID }:{ SignatureID }" },
                { "parmtrmnal", (request.Terminal?"1":"0") }, 
            });
            if(results != null){
                var row = ((IDictionary<string, object>)results.ReadSingleOrDefault());
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1" || ResultCode == "52"){
                    Subscriber user = UserDto.Subscriber(row);
                    if(ResultCode == "1"){
                        var company = LotteryGameDto.CompanySettings(results.ReadSingleOrDefault());
                        if(user.IsPlayer||user.IsCoordinator||user.IsGeneralCoordinator){
                            //if(user.BranchZipCode.Str() == PostalCode){
                                if(!user.IsBlocked){
                                    user.DeviceID = request.DeviceID;
                                    user.DeviceName = request.DeviceName;
                                    user.SignatureID = SignatureID;
                                    user.IsTerminal = request.Terminal;
                                    pusherSubscriberSessionEnd(user);
                                    return (SignInResults.Success, null, user, new {
                                        Company = company,
                                    }); //*logs
                                }
                                return (SignInResults.Failed, "Your account has blocked by admin", null, null); //*logs
                            //}
                            //return (SignInResults.Failed, null, "Your account can't sign in. Make sure current location belongs to your account"); //*logs
                        }
                    } else {
                        //if(subscriber.BranchZipCode.Str() == PostalCode) //*logs
                            return (SignInResults.PreRegister, "Verified Successfull!", user, null); //*logs
                        //return (SignInResults.Failed, null, "Unable to proceed to registration form. Make sure current location belongs to branch"); //*logs
                    }
                }else if(ResultCode == "31")
                    return (SignInResults.ChangePassword, "You're required to change your password.", null, null);
                else if(ResultCode == "32")
                    return (SignInResults.Failed, String.Format("Your password was changed {0} ago",row["LST_CHNG"].Str()), null, null);  //8 months
                    //return (SignInResults.ChangePassword, null, $"By default password, Your account required to change password");
                else if(ResultCode == "20")
                    return (SignInResults.Failed, $"Your account has been locked! Please wait for {row["RM_LCK"].Str()} minute(s) and try again", null, null);
                else if(ResultCode == "21")
                    return (SignInResults.Failed, "Your account has blocked by admin", null, null);
                else if(ResultCode == "22")
                    return (SignInResults.Failed, "Your account is not assign to this terminal", null, null);
                else if(ResultCode == "23")
                    return (SignInResults.Failed, "Your account has been assigned only for terminal", null, null);
                else if(ResultCode == "60")
                    return (SignInResults.Failed, "Your registration request is already expired! Please ask new account registration again", null, null);
                return (SignInResults.Failed, "Invalid mobile number and password! Please try again", null, null);
            }
            return (SignInResults.Null, null, null, null);
        }

        public async Task<(SignUpResults result, Subscriber user, String message)> SignUpAsync(SignUpRequest request){
            var form = request.RequestForm;
            string SignatureID = DateTime.Now.ToTimeMillisecond().ToString("X");
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BCAS0RCC", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmusrmobno", form.MobileNumber },
                { "parmotp", form.OTPCode },
                { "parmnwpsswrd", form.Password },
                { "parmcnfrmpsswrd", form.ConfirmPassword },
                //
                { "parmusrfnm", form.Firstname },
                { "parmusrlnm", form.Lastname },
                { "parmusrncknm", "" },
                { "parmemladd", form.EmailAddress },
                //
                { "parmgndr", "" },
                { "parmmtrlstat", "" },
                //{ "parmntnlty", "" },
                { "parmctznshp", "" },
                //{ "parmbrloczp", "" },
                //
                { "parmdvcnm", form.DeviceName },
                { "parmdvcmdlid", $"{ form.DeviceID }:{ SignatureID }" },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1"){
                    Subscriber user = UserDto.Subscriber(row);
                    user.DeviceID = form.DeviceID;
                    user.DeviceName = form.DeviceName;
                    user.SignatureID = SignatureID;
                    return (SignUpResults.Success, user, String.Format("Your account successfully registered with Account#{0}! You can now proceed to your account", user.AccountID)); //*logs
                }else if(ResultCode=="60") 
                    return (SignUpResults.Failed, null, "Your registration request is already expired! Please ask new account registration again");
                else if(ResultCode=="0") 
                    return (SignUpResults.Failed, null, "Failed to Register!");
                return (SignUpResults.Failed, null, "An error encountered while processing your request. please try again");
            }
            return (SignUpResults.Null, null, null);
        }

        public async Task<(SignUpResults result, String message)> SignUpWithMobileAsync(SignUpWithMobileRequest request){
            //string BranchID = getBranchID(request);
			string BranchID = "001"; //repoResult.BranchID;
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BCAS0RC", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmbrcd", BranchID },
                { "parmusrmobno", request.MobileNumber },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1")
                    return (SignUpResults.Success, "Code is successfully sent");
                return (SignUpResults.Failed, String.Format("Mobile number {0} is already registered!", request.MobileNumber));
            }
			return (SignUpResults.Null, null);
        }
        public async Task<(VerifyOtpResults result, Subscriber user, String message)> VerifyOtpAsync(AuthFormRequest request){
            var result = _repo.DSpQuery<dynamic>("dbo.spfn_BCAS0MV", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmusrmobno", request.MobileNumber },
                { "parmotp", request.OTPCode },
            }).FirstOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(!request.IsChangePassword && ResultCode == "52")
                    return (VerifyOtpResults.VerifiedUser, UserDto.Subscriber(row), null);
                else if(request.IsChangePassword && ResultCode == "1")
                    return (VerifyOtpResults.ChangePassword, null, "Verified Successfull!");
                return (VerifyOtpResults.Failed, null, "Invalid one time pin!");
            }
            return (VerifyOtpResults.Null, null, null);
        }
        //Widget
        public static async void pusherSubscriberSessionEnd(Subscriber user){
            await Pusher.PushAsync($"/{user.CompanyID}/{user.BranchID}/{user.SubscriberID}/notify", 
                new{ type = "device.session-end", message = "Your session has been expired! some one are accessing your account. please change your password immediately.", });
        }
    }
}