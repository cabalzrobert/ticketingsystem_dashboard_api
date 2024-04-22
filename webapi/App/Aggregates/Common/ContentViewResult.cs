using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace webapi.App.Aggregates.Common
{
    public class ContentViewResult 
    {
        public static ContentResult InvalidConfigurationResult(){
            return new ContentResult { 
                Content = "{\"Status\":\"error\",\"Message\":\"Invalid configuration\"}", 
                StatusCode = (int) HttpStatusCode.OK,
                ContentType = "application/json; charset=utf-8",
            };
        }

        public static ContentResult UnauthorizedAccountResult(){
            return new ContentResult { 
                Content = "{\"Status\":\"error\",\"Message\":\"Unauthorized\"}", 
                StatusCode = (int) HttpStatusCode.OK,
                ContentType = "application/json; charset=utf-8",
            };
        }
        public static ContentResult SessionExpiredResult(){
            return new ContentResult { 
                Content = "{\"Status\":\"error\",\"Type\":\"device.session-end\",\"Message\":\"Session expired!\"}",
                StatusCode = (int) HttpStatusCode.OK,
                ContentType = "application/json; charset=utf-8",
            };
        }
        public static ContentResult FailedLogInAccountResult(){
            return new ContentResult { 
                Content = "{\"Status\":\"error\",\"Message\":\"Please account is inactive, please contact to your admin support\"}",
                StatusCode = (int) HttpStatusCode.OK,
                ContentType = "application/json; charset=utf-8",
            };
        }
        public static ContentResult BlockedAccountResult(){
            return new ContentResult { 
                Content = "{\"Status\":\"error\",\"Message\":\"Your account is blocked, please contact to your admin support\"}",
                StatusCode = (int) HttpStatusCode.OK,
                ContentType = "application/json; charset=utf-8",
            };
        }
    }
}