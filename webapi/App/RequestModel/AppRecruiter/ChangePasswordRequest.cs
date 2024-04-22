using System;
using System.ComponentModel.DataAnnotations;
namespace webapi.App.RequestModel.AppRecruiter
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