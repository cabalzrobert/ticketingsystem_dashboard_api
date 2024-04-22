using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class LedgerDto
    {
        public static IEnumerable<dynamic> FilterCreditLedgers(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = CreditLedgers(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> CreditLedgers(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> CreditLedger(e));
        }
        public static IDictionary<string, object> CreditLedger(IDictionary<string, object> data){
            return Ledger(data, isCredit:true);
        }
        
        public static IEnumerable<dynamic> FilterCommissionLedgers(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = CommissionLedgers(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> CommissionLedgers(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> CommissionLedger(e));
        }
        public static IDictionary<string, object> CommissionLedger(IDictionary<string, object> data){
            return Ledger(data, isCommission:true);
        }
        public static IDictionary<string, object> Ledger(IDictionary<string, object> data, bool isCommission=false, bool isCredit=false){
            dynamic o = Dynamic.Object;
            string transtype = data["TRN_TYP"].Str().Trim();
            o.TransactionNo = data["TRN_NO"].Str();
            o.SequenceNo = data["SEQ_NO"].Str();
            o.DateTransaction = data["RGS_TRN_TS"];
            o.BeginningBalance = data["BEG_BAL"].Str().ToDecimalDouble();
            o.Debit = data["DBT"].Str().ToDecimalDouble();
            o.Credit = data["CRDT"].Str().ToDecimalDouble();
            o.EndBalance = data["END_BAL"].Str().ToDecimalDouble();
            if(transtype.Equals("11")){
                o.IsReturn = true;
                o.AccountID=data["REF_ACT_ID"].Str();
                if(isCommission && !isCredit) o.IsCommission = true;
                else{
                    string bettype = data["FGHT_BET"].Str();
                    string status = data["FGHT_STAT"].Str();
                    if(!bettype.IsEmpty() && (status.Equals("0")||status.Equals("3"))){
                        o.BetType = (bettype.Equals("1")?"Meron":"Wala");
                        o.FightNo = data["FGHT_NO"].Str();
                        o.ReferenceID = data["REF_NO"].Str();
                        o.IsCancelled = (status.Equals("0"));
                        o.IsDraw = (status.Equals("3"));
                    }
                }
            }else if(transtype.Equals("3")||transtype.Equals("10")){
                //o.SubscriberID=data["FRM_CUST_NO"].Str();
                o.IsReceived=(transtype.Equals("3"));
                var referenceID = data["REF_NO"].Str();
                if(!referenceID.IsEmpty()){
                    o.ReferenceID = data["REF_NO"].Str();
                    o.FightNo = data["REF_SEQ_NO"].Str();
                    o.IsFromArena = true;
                }else if(transtype.Equals("3")) 
                    o.AccountID=data["REF_ACT_ID"].Str();
                o.IsCommission = true;
            }else if(transtype.Equals("6")||transtype.Equals("7")||transtype.Equals("8")){
                o.AccountID=data["REF_ACT_ID"].Str();
                if(transtype.Equals("7")) o.IsSold = true;
                else if(transtype.Equals("8")||transtype.Equals("6")) 
                    o.IsReceived = true;
            }else if(transtype.Equals("1")){
                o.IsPlay = true;
                string bettype = data["FGHT_BET"].Str();
                if(!bettype.IsEmpty()){
                    o.BetType = (bettype.Equals("1")?"Meron":"Wala");
                    o.FightNo = data["FGHT_NO"].Str();
                    o.ReferenceID = data["REF_NO"].Str();
                }
            }else if(transtype.Equals("12")){
                o.IsConvertToGameCredit = true;
            }else if(transtype.Equals("13")){
                o.IsConvertToWinCredit = true;
            }else if(transtype.Equals("14")){
                o.IsTranferCredit = true;
                o.IsReceived = data["S_RCVD"].Str().Equals("1");
                o.AccountID = data["REF_ACT_ID"].Str();
            }

            try{
                DateTime DateTransaction = (DateTime)data["RGS_TRN_TS"];
                o.TimeTransaction = DateTransaction.ToString("hh:mm:ss tt");
                o.DateDisplay = DateTransaction.ToString("MMM dd, yyyy");
            }catch{}
            return o;
        }
    }

}