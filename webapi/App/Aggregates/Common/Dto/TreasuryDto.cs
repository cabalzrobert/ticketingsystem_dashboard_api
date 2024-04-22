using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class TreasuryDto 
    {
        public static IEnumerable<dynamic> PayableCommissions(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> PayableCommission(e));
        }
        public static IDictionary<string, object> PayableCommission(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.AccountID = data["ACT_ID"].Str();
            o.AccountName = data["USR_FLL_NM"].Str().Trim();
            o.CommissionBalance = data["ACT_COM_BAL"].Str().ToDecimalDouble();
            o.CommissionBalanceRange = data["ACT_COM_BAL_RNG"].Str().ToDecimalDouble();

            string usertype = data["USR_TYP"].Str();
            o.UserType = "";
            if(o.IsCoordinator = (usertype.Equals("4"))) o.UserType = "Coordinator";
            if(o.IsGeneralCoordinator = (usertype.Equals("3"))) o.UserType = "General Coordinator";
            return o;
        }

        public static IEnumerable<dynamic> ClaimCommissions(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> ClaimCommission(e));
        }
        public static IDictionary<string, object> ClaimCommission(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.TransactionNo = data["TRN_NO"].Str();
            o.AccountID = data["ACT_ID"].Str();
            o.AccountName = data["USR_FLL_NM"].Str().Trim();
            o.ClaimAmount = data["CLM_AMT"].Str().ToDecimalDouble();

            string usertype = data["USR_TYP"].Str();
            o.UserType = "";
            if(o.IsCoordinator = (usertype.Equals("4"))) o.UserType = "Coordinator";
            if(o.IsGeneralCoordinator = (usertype.Equals("3"))) o.UserType = "General Coordinator";

            o.DateClaim = o.TimeClaim = "";
            try{
                DateTime trndt = data["RGS_TRN_TS"].To<DateTime>();
                o.DateClaim = trndt.ToString("MMMM dd, yyyy");
                o.TimeClaim = trndt.ToString("hh:mm:ss tt");
            }catch{}
            return o;
        }

    }
    
}