using System;

namespace webapi.App.Model.Company
{
    public class Company 
    {
        public String CompanyName {get; set;}
        public String CompanyAddress {get; set;}
        public String LicenseNo {get; set;}
        public String TinNo {get; set;}
        public String ShortName {get; set;}
        public String AreaName {get; set;}
        public String TelephoneNumber {get; set;}
        public String TechSupportNumber {get; set;}
        public String EmailAddress {get; set;}
        public String WebsiteUrl {get; set;}
        public object MainOffice {get; set;}
    }
}
