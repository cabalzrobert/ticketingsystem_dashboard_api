using System;
using System.Collections.Generic;
using webapi.App.Model.User;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class CreditDto
    {
        public static IEnumerable<dynamic> FilterRequestingCredits(STLAccount account, IEnumerable<dynamic> list, bool isCompletionBase=false, int limit = 50){
            if(list==null) return null;
            var items = RequestingCredits(account, list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                if(!isCompletionBase) filter.BaseFilter = o.DateRequested;
                else  filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> RequestingCredits(STLAccount account, IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> RequestingCredit(account, e));
        }
        public static IDictionary<string, object> RequestingCredit(STLAccount account, IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            string status = data["REQ_STAT"].Str();
            string usertype = data["USR_TYP"].Str();
            bool IsSendCredit = (data["PAY_STAT"].Str().Equals("0") && status.Equals("1"));
            o.AccountID = data["ACT_ID"].Str();
            o.ReferenceID = data["PO_NO"].Str();
            o.Firstname = data["FRST_NM"].Str();
            o.Lastname = data["LST_NM"].Str();
            o.Fullname = data["FLL_NM"].Str();
            //o.DisplayName = data["NCK_NM"].Str();
            o.ImageUrl =  data["IMG_URL"].Str();
            //
            o.IsPending = (status.Equals("0"));
            o.IsApproved = (status.Equals("1"));
            o.IsCancelled = (status.Equals("2"));

            //o.IsGeneralCoordinator = (usertype.Equals("3"));
            //if(account.IsGeneralCoordinator)
            //    o.IsCoordinator = (usertype.Equals("4"));
            //o.IsPlayer = (usertype.Equals("5"));

            o.DateRequested = data["RGS_TRN_TS"];
            if(o.IsApproved || o.IsCancelled){
                o.DateTransaction = data["UPD_TRN_TS"];
            }
            o.RequestCredit = data["TOT_AMT"].Str().ToDecimalDouble();

            string paymethod = data["PAY_MTHD"].Str();
            string paystatus = data["PAY_STAT"].Str();
            o.IsCash = (paymethod.Equals("1"));
            o.IsCredit = (paymethod.Equals("2"));
            o.IsPaid = (paystatus.Equals("1"));
            o.IsUnpaid = (!o.IsPaid || paystatus.Equals("2"));

            //
            try{
                DateTime datetime = data["RGS_TRN_TS"].To<DateTime>();
                o.FulldateRequested = datetime.ToString("MMM dd, yyyy hh:mm:ss tt");
            }catch{}
            if(o.IsApproved || o.IsCancelled){
                try{
                    DateTime datetime = data["UPD_TRN_TS"].To<DateTime>();
                    o.FulldateTransaction = datetime.ToString("MMM dd, yyyy hh:mm:ss tt");
                }catch{}
            }
            return o;
        }
        
        public static IDictionary<string, object> ApprovedSummary(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.Paid = data["PD_AMT"].Str().ToDecimalDouble();
            o.Unpaid = data["NPD_AMT"].Str().ToDecimalDouble();
            return o;
        }

        public static IDictionary<string, object> RequestCreditApproval(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.DateTransaction = data["TRN_TS"];
            string status = data["TRN_STAT"].Str();
            o.IsPending = (status.Equals("0"));
            o.IsApproved = (status.Equals("1"));
            o.IsCancelled = (status.Equals("2"));
            try{
                DateTime datetime = data["TRN_TS"].To<DateTime>();
                o.FulldateTransaction = datetime.ToString("MMM dd, yyyy hh:mm:ss tt");
            }catch{}
            return o;
        }
    }
}