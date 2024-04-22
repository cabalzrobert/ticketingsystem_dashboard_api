using System;

namespace webapi.App.RequestModel.SubscriberApp.Common
{
    public class AuthFormRequest
    {
        public String MobileNumber;
        public String AddressLocation;
        public String CoordinateLocation;
        public String OTPCode;
        public bool IsPin;
        public bool IsChangePassword;
        public bool Required;
    }
}