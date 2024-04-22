using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using webapi.App.Model.User;

using webapi.Commons.AutoRegister;
using Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using Comm.Commons.Extensions;
using Comm.Commons.Advance;

using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using webapi.App.Aggregates.Common;
using Microsoft.Extensions.Configuration;
using System.Collections.Specialized;
using webapi.Services.Authentication;


namespace  webapi.App.Aggregates.AppRecruiterAggregate.Common
{
    [Service.Transient] 
    public class RecruiterAuthenticationAttribute : ActionFilterAttribute
    { 
        private readonly IRecruiter _identity; 
        public Recruiter account { get{ return _identity.AccountIdentity(); } } 
        public RecruiterAuthenticationAttribute(IRecruiter identity){  
            _identity = identity;  
        } 


        public override void OnActionExecuting(ActionExecutingContext filterContext){
            /*
                check is maintenance here
            */
            _identity.Controller(filterContext.Controller as ControllerBase);
            if(!_identity.IsAuthorized()){
                filterContext.Result = ContentViewResult.UnauthorizedAccountResult();
                return;
            }
            //
            /*if(!account.IsLogin){
                filterContext.Result = ContentViewResult.FailedLogInAccountResult();
                return;
            }
            if(account.IsBlocked){
                filterContext.Result = ContentViewResult.BlockedAccountResult();
                return;
            }
            if(account.IsSessionExpired){
                filterContext.Result = ContentViewResult.SessionExpiredResult();
                return;
            }*/
        }
    }
    
    [Service.Transient] 
    public class RecruiterAuthenticationGETAttribute : ActionFilterAttribute
    { 
        private readonly IRecruiter _identity; 
        public RecruiterAuthenticationGETAttribute(IRecruiter identity){  
            _identity = identity;  
        } 
        public Recruiter account { get{ return _identity.AccountIdentity(); } } 

        public override void OnActionExecuting(ActionExecutingContext filterContext){
            /*
                check is maintenance here
            */
            _identity.Controller(filterContext.Controller as ControllerBase, true);
            if(!_identity.IsAuthorized()){
                filterContext.Result = ContentViewResult.UnauthorizedAccountResult();
                return;
            }
            //
            /*if(!account.IsLogin){
                filterContext.Result = ContentViewResult.FailedLogInAccountResult();
                return;
            }
            if(account.IsBlocked){
                filterContext.Result = ContentViewResult.BlockedAccountResult();
                return;
            }
            if(filterContext.HttpContext.WebSockets.IsWebSocketRequest) return;
            if(account.IsSessionExpired){
                filterContext.Result = ContentViewResult.SessionExpiredResult();
                return;
            }*/
        }
    }

    
    [Service.IScope(typeof(RecruiterUser))] 
    public interface IRecruiter
    {
        bool Controller(ControllerBase controller, bool IsGETBase = false);
        Recruiter AccountIdentity();
        bool IsAuthorized();
    }
    public class RecruiterUser : IRecruiter
    {  
        private readonly static Recruiter Empty = new Recruiter();
        private readonly IQueryStringAuthenticationMiddleware _authMv;
        private readonly IRepository _repo;
        public RecruiterUser(IQueryStringAuthenticationMiddleware authMv, IRepository repo){
            _authMv = authMv;
            _repo = repo;
        }

        private Recruiter accountIdentity = RecruiterUser.Empty;
        public bool Controller(ControllerBase Controller, bool IsGETBase = false){ 
            var Request = Controller.Request;
            var User = Controller.User;
            if(IsGETBase){
                if(Request.Method != "GET")
                    return false;
                var queryString = HttpUtility.ParseQueryString(Request.QueryString.Value);
                if(!queryString.AllKeys.Contains("token"))
                    return false;
                if(!_authMv.Validate(User, queryString))
                    return false;
            }

            if(UserIdentity(User.Identities.Last() as ClaimsIdentity)){
                return true;
            }
            return false;
        }

        public bool IsAuthorized(){
            return (this.accountIdentity != Empty);
        }
        public Recruiter AccountIdentity(){
            return accountIdentity;
        }

        private bool UserIdentity(ClaimsIdentity claimsIdentity){
            Recruiter identity = null;
            var token = claimsIdentity.FindFirst("token");
            var guid = claimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti);
            if(token != null && guid != null){
                try{
                    var stringify = Cipher.Decrypt(token.Value.Str(), guid.Value.Str());
                    if(!stringify.Str().IsEmpty()){
                        identity = JsonConvert.DeserializeObject<Recruiter>(stringify);
                        if(identity != null){
                            this.accountIdentity = identity;
                            /*
                            try{
                                var result = _repo.DSpQuery<dynamic>($@"dbo.spfn_BDABEA0A", new Dictionary<string, object>(){
                                    { "parmcompid", identity.CompanyID },
                                    { "parmbrcd", identity.BranchID },
                                    { "parmuserid", identity.SubscriberID },
                                    { "parmdvcnm", identity.DeviceName },
                                    { "parmdvcmdlid", identity.DeviceID },
                                }).FirstOrDefault();
                                if(result != null){
                                    var row = ((IDictionary<string, object>)result);
                                    if(row["RESULT"].Str() == "1"){
                                        identity.IsLogin = true;
                                        identity.IsBlocked = (row["S_BLCK"].Str().Equals("1"));
                                        identity.IsCurrentDevice = (row["CUR_DVC"].Str().Equals("1"));
                                        identity.IsSessionExpired = !(row["SSSN_ID"].Str().ToLower().Equals(identity.SessionID.Str().ToLower()));
                                        this.accountIdentity = identity;
                                        return true;
                                    }
                                }
                            }catch{
                                this.accountIdentity = identity;
                            }
                            */
                        }
                    }
                }catch{}
            }
            return false;
        }
    }
}