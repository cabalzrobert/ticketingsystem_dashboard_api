using System;
using System.ComponentModel.DataAnnotations;
using Comm.Commons.Extensions;
namespace webapi.App.RequestModel.SubscriberApp.Features.ArenaGame
{
    public class PlaceBetRequest
    {
        public String GameType;
        public double Amount;
        public String BetType;
        public String ReferenceID;
        public String CoordinateLocation;
        public String AddressLocation;

        public static bool validity0a(PlaceBetRequest request){
            if(request==null) return false;
            if(request.BetType.IsEmpty() || request.Amount<10)return false; 
            var betType = request.BetType.Str().ToLower();
            if(betType.Equals("meron")) request.BetType = "1";
            else if(betType.Equals("wala")) request.BetType = "2";
            else return false;
            request.GameType = "01";
            return true;
        }
    }
}