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


namespace  webapi.App.Aggregates.SubscriberAppAggregate.Common
{
    [Service.Transient] 
    public class SubscriberAuthenticationAttribute : ActionFilterAttribute
    { 
        private readonly ISubscriber _identity; 
        public TicketingUser account { get{ return _identity.AccountIdentity(); } } 
        public SubscriberAuthenticationAttribute(ISubscriber identity){  
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
            if(!account.IsLogin){
                filterContext.Result = ContentViewResult.FailedLogInAccountResult();
                return;
            }
            //if(account.IsBlocked){
            //    filterContext.Result = ContentViewResult.BlockedAccountResult();
            //    return;
            //}
            //if(account.IsSessionExpired){
            //    filterContext.Result = ContentViewResult.SessionExpiredResult();
            //    return;
            //}
        }
    }
    
    [Service.Transient] 
    public class SubscriberAuthenticationGETAttribute : ActionFilterAttribute
    { 
        private readonly ISubscriber _identity; 
        public SubscriberAuthenticationGETAttribute(ISubscriber identity){  
            _identity = identity;  
        } 
        public TicketingUser account { get{ return _identity.AccountIdentity(); } } 

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
            if(!account.IsLogin){
                filterContext.Result = ContentViewResult.FailedLogInAccountResult();
                return;
            }
            //if(account.IsBlocked){
            //    filterContext.Result = ContentViewResult.BlockedAccountResult();
            //    return;
            //}
            //if(filterContext.HttpContext.WebSockets.IsWebSocketRequest) return;
            //if(account.IsSessionExpired){
            //    filterContext.Result = ContentViewResult.SessionExpiredResult();
            //    return;
            //}
        }
    }

    
    [Service.IScope(typeof(SubscriberUser))] 
    public interface ISubscriber
    {
        bool Controller(ControllerBase controller, bool IsGETBase = false);
        TicketingUser AccountIdentity();
        bool IsAuthorized();
    }
    public class SubscriberUser : ISubscriber
    {  
        private readonly static TicketingUser Empty = new TicketingUser();
        private readonly IQueryStringAuthenticationMiddleware _authMv;
        private readonly IRepository _repo;
        public SubscriberUser(IQueryStringAuthenticationMiddleware authMv, IRepository repo){
            _authMv = authMv;
            _repo = repo;
        }

        private TicketingUser accountIdentity = SubscriberUser.Empty;
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
        public TicketingUser AccountIdentity(){
            return accountIdentity;
        }

        private bool UserIdentity(ClaimsIdentity claimsIdentity){
            TicketingUser identity = null;
            var token = claimsIdentity.FindFirst("token");
            var guid = claimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti);
            if(token != null && guid != null){
                try{
                    var stringify = Cipher.Decrypt(token.Value.Str(), guid.Value.Str());
                    if(!stringify.Str().IsEmpty()){
                        identity = JsonConvert.DeserializeObject<TicketingUser>(stringify);
                        if(identity != null)
                        {
                            identity.IsLogin = true;
                            identity.sActive = true;
                            identity.IsSessionExpired = false;
                            this.accountIdentity = identity;
                            //try{
                            //    var result = _repo.DSpQuery<dynamic>($@"dbo.spfn_BDABDB01", new Dictionary<string, object>(){
                            //        { "parmplid", identity.PL_ID },
                            //        { "parmpgrpid", identity.PGRP_ID },
                            //        { "parmuserid", identity.USR_ID },
                            //        //{ "parmdvcnm", identity.DeviceName },
                            //        //{ "parmdvcmdlid", identity.DeviceID },
                            //    }).FirstOrDefault();
                            //    if(result != null){
                            //        var row = ((IDictionary<string, object>)result);
                            //        if(row["RESULT"].Str() == "1"){
                            //            identity.IsLogin = true;
                            //            identity.sActive = (row["S_ACTV"].Str().Equals("1"));
                            //            //identity.IsCurrentDevice = (row["CUR_DVC"].Str().Equals("1"));
                            //            identity.IsSessionExpired = !(row["
                            //            "].Str().ToLower().Equals(identity.SessionID.Str().ToLower()));
                            //            this.accountIdentity = identity;
                            //            return true;
                            //        }
                            //    }
                            //}catch{
                            //    this.accountIdentity = identity;
                            //}
                        }
                    }
                }catch{}
            }
            return false;
        }
    }
}