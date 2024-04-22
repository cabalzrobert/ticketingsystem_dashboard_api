using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class ControlDto 
    {
        
        public static IEnumerable<dynamic> Remittances(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> Remittance(e));
        }
        public static IDictionary<string, object> Remittance(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.RemittanceID = data["REM_ID"].Str();
            o.RemittanceCode = data["REM_CD"].Str();
            o.RemittanceName = data["REM_NM"].Str();

            o.EncashmentTypeID = o.EncashmentType = "";
            int encashmenttype = (int)data["ENC_TYP"].Str().ToDecimalDouble();
            if(encashmenttype==1){
                o.EncashmentTypeID = "BTB";
                o.EncashmentType = "Bank to Bank Transfer";
            }else if(encashmenttype==2){
                o.EncashmentTypeID = "CD";
                o.EncashmentType = "Door to Door Delivery";
            } else if(encashmenttype==3){
                o.EncashmentTypeID = "MRC";
                o.EncashmentType = "Money Remittance Center";
            }

            o.LogoUrl = data["REM_LOGO"].Str();
            o.TermsUrl = data["REM_TRM"].Str();
            return o;
        }

        public static IEnumerable<dynamic> EncashmentTypes(){
            var list = new List<object>();
            list.Add(new { ID = "BTB", Name = "Bank to Bank Transfer" });
            list.Add(new { ID = "CD", Name = "Door to Door Delivery" });
            list.Add(new { ID = "MRC", Name = "Money Remittance Center" });
            return list;
        }

    }
    
}