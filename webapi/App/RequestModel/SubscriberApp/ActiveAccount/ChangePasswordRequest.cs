using System;
using System.ComponentModel.DataAnnotations;

namespace webapi.App.RequestModel.SubscriberApp.ActiveAccount
{
    public class ChangePasswordRequest
    {
        [Required]
        public String OldPassword;
        [Required]
        public String Password;
        [Required]
        public String ConfirmPassword;
    }
}