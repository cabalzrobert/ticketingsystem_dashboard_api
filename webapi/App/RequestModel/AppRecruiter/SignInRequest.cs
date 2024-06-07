using System;
using System.ComponentModel.DataAnnotations;
namespace webapi.App.RequestModel.AppRecruiter
{
    public class SignInRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }

        public string DeviceID { get; set; }
        public string DeviceName { get; set; }
        public bool Terminal { get; set; }

        //[Required]
        public string ApkVersion { get; set; }

        //[Required]
        public string CoordinateLocation { get; set; }
        //[Required]
        public string AddressLocation { get; set; }
    }
    public class RequiredChangePassword
    {
        public string PLID;
        public string PGRPID;
        public string Username;
        public string OldPassword;
        public string Password;
        public string ConfirmPassword;
    }
    public class FetchLoadNews
    {
        public string category;
        public string jsonnews;
    }

    public class TicketingSignInRequest
    {
        public string Username;
        public string Password;
        public string plid;
        public string groupid;
    }

    public class SendOtp
    {
        public string UserId;
        public string MobileNumber;
    }

    public class SetPassword
    {
        public string UserId;
        public string MobileNumber;
        public string Password;
    }
}
