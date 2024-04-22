using System;
using System.ComponentModel.DataAnnotations;
using Comm.Commons.Extensions;
using System.Collections.Generic;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Linq;
using webapi.App.Aggregates.Common;
namespace webapi.App.RequestModel.SubscriberApp.Features.LotteryGame
{
    public class PlaceBetRequest
    {
        public List<PlaceBet> Bets;
        public Dictionary<string, PlaceBetEntry> PlaceBets;
        public string MobileNumber;

        public static bool validity(PlaceBetRequest request){
            if(request==null) return false;
            if(request.Bets==null || request.Bets.Count==0) return false;

            var placebets = (request.PlaceBets =  new Dictionary<string, PlaceBetEntry>());
            var bets = request.Bets;
            foreach (var bet in bets){
                if(!placebets.ContainsKey(bet.BetType))
                    placebets[bet.BetType] = new PlaceBetEntry(); 
                var placebet = placebets[bet.BetType];
                placebet.Bets.Add(bet);
                bet.NumberBet = bet.Combination.Str().Trim().Replace(" ", "");
                placebet.GameType = bet.BetType;
            }
            request.MobileNumber = (request.MobileNumber.Str().Length>10?request.MobileNumber:"");
            return true;
        }
        /*
        
            var draws = request.Draws.To<List<PlaceBet>>();
            if(draws==null || draws.Count==0) return false;
        */
    }

    public class PlaceBetEntry
    {
        public List<PlaceBet> Bets = new List<PlaceBet>();
        public string Inputs;
        public string GameType;
        public string TransactionNo;
        public object Ticket;
        //
        public string Message;
        public string Status = "ok";
    }


    public class PlaceBet
    {
        public string Combination;
        public string BetType; 
        public string Straight;
        public string Rumble;
        public string NumberBet;
        public string GameType;
        public Xml ToXml(){
            return JsonConvert.DeserializeObject<Xml>(JsonConvert.SerializeObject(this));
        }

        [XmlRoot(ElementName = "item")]
        public class Xml {
            [XmlAttribute("STRGHT_AMT")]
            public string Straight;
            [XmlAttribute("RMB_AMT")]
            public string Rumble;
            [XmlAttribute("NUM_BET")]
            public string NumberBet;
        }
    }
}


/*

    public class PlaceBetRequest
    {
        public List<PlaceBet> Draws;
        public Dictionary<string, PlaceBetEntry> PlaceBets;

        public static bool validity(PlaceBetRequest request){
            if(request==null) return false;
            if(request.Draws==null || request.Draws.Count==0) return false;

            var placebets = (request.PlaceBets =  new Dictionary<string, PlaceBetEntry>());
            var draws = request.Draws;
            foreach (var draw in draws){
                if(!placebets.ContainsKey(draw.BetType))
                    placebets[draw.BetType] = new PlaceBetEntry(); 
                var placebet = placebets[draw.BetType];
                placebet.DrawsInBetType.Add(draw);

                var gametype = (draw.GameType = LotteryGameDto.Static.BetType.GetKey(draw.BetType)).Str();
                if(gametype.Equals("01")) draw.NumberBet = $"{draw.Ball1.Str().Trim()}-{draw.Ball2.Str().Trim()}-{draw.Ball3.Str().Trim()}"; 
                else if(gametype.Equals("02")||gametype.Equals("03")) draw.NumberBet = $"{draw.Ball1.Str().Trim()}-{draw.Ball2.Str().Trim()}";
                else draw.NumberBet = null;

                placebet.GameType = gametype;
            }
            return true;
        }
        / *
        
            var draws = request.Draws.To<List<PlaceBet>>();
            if(draws==null || draws.Count==0) return false;
        * /
    }

    public class PlaceBetEntry
    {
        public List<PlaceBet> DrawsInBetType = new List<PlaceBet>();
        public string Inputs;
        public string GameType;
        public string TransactionNo;
        public object Ticket;
        //
        public string Message;
        public string Status = "ok";
    }


    public class PlaceBet
    {
        public string Ball1;
        public string Ball2;
        public string Ball3;

        public string BetType; 
        public string DrawType;
        public string StraightAmount;
        public string RumbleAmount;
        public string NumberBet;
        public string GameType;
        public Xml ToXml(){
            return JsonConvert.DeserializeObject<Xml>(JsonConvert.SerializeObject(this));
        }

        [XmlRoot(ElementName = "item")]
        public class Xml {
            [XmlAttribute("STRGHT_AMT")]
            public string StraightAmount;
            [XmlAttribute("RMB_AMT")]
            public string RumbleAmount;
            [XmlAttribute("NUM_BET")]
            public string NumberBet;
        }
    }
*/