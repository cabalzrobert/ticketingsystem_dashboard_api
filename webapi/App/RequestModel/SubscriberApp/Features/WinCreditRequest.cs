using System;
using System.ComponentModel.DataAnnotations;
namespace webapi.App.RequestModel.SubscriberApp.Features
{
    public class WinCreditRequest
    {
        [Required]
        public double Amount;
    }
}