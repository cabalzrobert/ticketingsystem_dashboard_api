using System;

namespace webapi.App.Model.User
{
    public class Account
    {
        public String GUID;
        public String CompanyID;
        public String BranchID;
        public String GroupID; 
        public String SubscriberID;
        public String OperatorID;
        public String AccountNo;
        public String AccountID;
        public String LastLogIn;
        public String BranchZipCode;
        public String DeviceID;
        public String DeviceName;
        public String ReferenceID;

        public bool IsLogin;
        public bool IsBlocked;
        public String SessionID;
        public bool IsCurrentDevice;
        public bool IsSessionExpired;
        public bool IsTerminal;
        //
        public bool IsGeneralCoordinator;
        public bool IsCoordinator;
        public bool IsPlayer;
        //
        public bool IsCompanyAgent;
        public bool IsBranchAgent;
        public bool IsInTreasury;
        public bool IsInOperations;
        public bool IsInAccounting;
        public bool IsInAccounts;
        public bool IsInOperationHead;

        public String GeneralCoordinatorID;
        public String CoordinatorID;
        public String GeneralCoordinator;
        public String Coordinator;
        public String GeneralCoordinatorAccountID;
        public String CoordinatorAccountID;
    }
}
