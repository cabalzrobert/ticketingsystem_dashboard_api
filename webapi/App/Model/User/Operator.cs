using System;

namespace webapi.App.Model.User
{
    public class Operator : User
    {
        public String SignatureID; //
        public String PasswordExpired {get; set;}
        public String DepartmentID {get; set;}
        public bool IsCompanyAgent {get; set;}
        public bool IsBranchAgent {get; set;}
        public bool IsInTreasury {get; set;}
        public bool IsInOperations {get; set;}
        public bool IsInOperationHead {get; set;}
        public bool IsInAccounting {get; set;}
        public bool IsInAccounts {get; set;}
        public String Department {get; set;}
        public String Username {get; set;}
        
        public DateTime DateLogin {get; set;}
        public String DateLoginDisplay {get; set;}

        public String Environment {get; set;}
        public int ExpiryCount {get; set;}
        public bool IsExpired {get; set;}
    }
}
