using System;
using webapi.App.RequestModel.SubscriberApp.Features.Winning.Common;
using Comm.Commons.Extensions;

namespace webapi.App.RequestModel.SubscriberApp.Features.Winning
{
    public class ClaimingRequest
    {
        public String Type;
        public String Remittance;
        public String ReferenceID;
        public double Amount;
        public ClaimReceiver Receiver;
        public ClaimUsinghBank Bank;
        //
        public String TypeCode;
        public String Name;
        public String Number;
        public String Address;
        public bool IsConvertToGameCredit;
        public static bool validity0a(ClaimingRequest request){
            if(request==null) return false;
            request.TypeCode = GetTypeCode(request.Type); 
            if(request.TypeCode.IsEmpty())
                return false;        

            bool HasRemittance = !request.Remittance.IsEmpty();
            bool IsBankTransfer = (request.Bank!=null && request.Type.Equals("BTB") && HasRemittance);
            bool IsCashDelivery = (request.Receiver!=null && request.Type.Equals("CD") && HasRemittance);
            bool IsMoneyTransfer = (request.Receiver!=null && request.Type.Equals("MRC") && HasRemittance);
            request.IsConvertToGameCredit = ((request.Receiver==null&&request.Bank==null) && request.Type.Equals("CGC") && !HasRemittance);
            request.Name = request.Number = request.Address = "";
            if(IsBankTransfer){
                request.Name = request.Bank.AccountName;
                request.Number = request.Bank.AccountNo;
            } else if(IsCashDelivery||IsMoneyTransfer){
                request.Name = request.Receiver.Fullname;
                request.Number = request.Receiver.MobileNumber;
                if(IsCashDelivery)
                    request.Address = request.Receiver.Address;
            }
            return (request.IsConvertToGameCredit || !(request.Name.IsEmpty() && request.Number.IsEmpty()));
        }

        private static String GetTypeCode(String Type){
            if(Type==null) return null;
            else if(Type.Equals("BTB")) return "01";
            else if(Type.Equals("CD")) return "02";
            else if(Type.Equals("MRC")) return "03";
            else if(Type.Equals("CGC")) return "04";
            return null;
        }
    }
}