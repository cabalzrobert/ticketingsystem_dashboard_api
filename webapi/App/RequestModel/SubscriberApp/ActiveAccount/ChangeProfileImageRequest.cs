using System;
using System.ComponentModel.DataAnnotations;

namespace webapi.App.RequestModel.SubscriberApp.ActiveAccount
{
    public class ChangeProfileImageRequest
    {
        [Required]
        public String Img;
        public String ImageUrl;
    }
}