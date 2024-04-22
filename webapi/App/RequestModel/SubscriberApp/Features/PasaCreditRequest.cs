using System;
using System.ComponentModel.DataAnnotations;
namespace webapi.App.RequestModel.SubscriberApp.Features
{
    public class PasaCreditRequest
    {
        [Required]
        public double Amount;
        [Required]
        public String Subscriber;
    }
}