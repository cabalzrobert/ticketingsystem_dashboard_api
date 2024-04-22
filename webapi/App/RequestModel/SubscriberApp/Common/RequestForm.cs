using System;
using webapi.App.Model.User;

namespace webapi.App.RequestModel.SubscriberApp.Common
{
    public class RequestForm: Subscriber 
    {
        public new String AccountID {get; set;}
        public String Username {get; set;}
        public String OldPassword {get; set;}
        public String Password {get; set;}
        public String ConfirmPassword {get; set;}
        public String DepartmentID {get; set;}
        //
        public String DeviceID { get; set; }
        public String DeviceName { get; set; }
        public String OTPCode { get; set; }
        public String SerialNo { get; set; }
    }
}