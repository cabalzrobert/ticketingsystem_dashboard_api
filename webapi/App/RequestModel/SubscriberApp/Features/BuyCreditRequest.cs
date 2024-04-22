using System;
using System.ComponentModel.DataAnnotations;
namespace webapi.App.RequestModel.SubscriberApp.Features
{
    public class BuyCreditRequest
    {
        [Required]
        public double Amount;
    }
}