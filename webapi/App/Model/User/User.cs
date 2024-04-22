using System;
using System.Text;
using Comm.Commons.Extensions;

namespace webapi.App.Model.User
{
    public class User 
    {
        public String CompanyID;
        public String BranchID;
        public String GroupID;
        public String AccountID;
        //
        public String OperatorID;
        public String SubscriberID;
        //
        public String ReferenceID;
        public bool IsBlocked;
        public bool IsReseller;
        public String LastLogIn;
        public String SessionID;
        public String BranchZipCode;
        //
        public double CreditBalance;
        public String PurchaseOrderNo;
        public double WinningBalance;
        public double CommissionBalance;
        public double CommissionRate;
        public String SharedCommission;
        //Info
        public String ImageUrl;
        public String Firstname;
        public String Lastname;
        public String Fullname;
        public String DisplayName;
        public String BirthDate;
        public String PresentAddress;
        public String Address;
        public String EmailAddress;
        //
        public String OTPCode;
        public String MobileNumber;
        //
        public String GeneralCoordinator;
        public String GeneralCoordinatorID;
        public String Coordinator;
        public String CoordinatorID;
        //
        public bool IsProduction;
        public String DeviceID;
        public String DeviceName;
    }
}
