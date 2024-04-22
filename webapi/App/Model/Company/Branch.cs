using System;

namespace webapi.App.Model.Company
{
    public class Branch 
    {
        public String BranchName {get; set;}
        public String BranchAddress {get; set;}
        public String LicenseNo {get; set;}
        public String TinNo {get; set;}
        public String ShortName {get; set;}
        public String AreaName {get; set;}
        public String TelephoneNumber {get; set;}
        public String TechSupportNumber {get; set;}
        public String EmailAddress {get; set;}
        public String WebsiteUrl {get; set;}
        public String ZipCode {get; set;}
        public object Agent {get; set;}
    }
}
