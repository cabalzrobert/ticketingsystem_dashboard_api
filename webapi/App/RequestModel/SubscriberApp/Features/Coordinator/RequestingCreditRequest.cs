using System;
using System.ComponentModel.DataAnnotations;
using webapi.App.Model.User;
using Comm.Commons.Extensions;

namespace webapi.App.RequestModel.SubscriberApp.Features.Coordinator
{
    public class RequestingCreditRequest
    {
        [Required]
        public Account Transaction;
        public String Status;
        public String Type;

        public bool IsApproved;
        public bool IsCash;

        public static bool validity0a(RequestingCreditRequest request){
            if(request==null || request.Transaction==null || request.Status.Str().IsEmpty())
                return false;
            request.IsApproved = (request.Status.Equals("1"));
            request.IsCash = (request.Type.Str().ToLower().Equals("cash"));
            return true;
        }
        public static bool validity0b(RequestingCreditRequest request){
            if(request==null || request.Transaction==null)
                return false;
            return true;
        }

    }
}