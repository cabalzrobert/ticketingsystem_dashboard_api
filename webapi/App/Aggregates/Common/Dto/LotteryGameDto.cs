using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using Comm.Commons.Advance;
using System.Linq;
using webapi.App.RequestModel.Common;

namespace webapi.App.Aggregates.Common
{
    public class LotteryGameDto
    {

        public static IEnumerable<dynamic> FilterGameWinningsWithSummaryDetails(IEnumerable<dynamic> winnings, IEnumerable<dynamic> summaries, IEnumerable<dynamic> details, int limit = 50){
            if(winnings==null) return null;
            var items = GameWinningsWithSummaryDetails(winnings, summaries, details);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }

        public static IEnumerable<dynamic> GameWinningsWithSummaryDetails(IEnumerable<dynamic> winnings, IEnumerable<dynamic> summaries, IEnumerable<dynamic> details){
            winnings = WinResults(winnings).ToList();
            summaries = GameSummariesWithDetails(summaries, details); 
            Dictionary<string, dynamic> summariesPerTransaction = new Dictionary<string, dynamic>();
            foreach(dynamic summary in summaries){
                string TransNo = (summary.TransactionNo as object).Str();
                summariesPerTransaction[TransNo] = summary;
            }
            foreach(dynamic winning in winnings){
                string TransNo = (winning.TransactionNo as object).Str();
                winning.Ticket = summariesPerTransaction.GetValue(TransNo);
            }
            return winnings; 
        }
    
        public static IEnumerable<dynamic> WinResults(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> WinResult(e));
        }

        public static IDictionary<string, object> WinResult(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.TransactionNo = data["TRN_NO"];
            o.DateTransaction = data["RGS_TRN_TS"];
            //--
            o.GameName = data["GM_TYP_NM"].Str(); 
            o.GameID = data["GM_TYP"].Str(); 
            o.DrawType = data["DRW_TYP"].Str(); 
            string bettype = (o.BetType = data["GM_TYP_CD"].Str());
            string numbers = (o.Numbers = data["NUM_BET"].Str().Trim());
            if(bettype.Equals("PARES"))
                o.Numbers = String.Join('-', numbers.Split('-').Select(e=> e.Str().Trim().PadLeft(2, '0')));
            o.Is2Ball = (bettype.Equals("SWER2")||bettype.Equals("PARES"));
            o.Is3Ball = (bettype.Equals("SWER3")||bettype.Equals("3D"));
            o.Is6Ball = false;

            //
            try{
                DateTime DateTransaction = (DateTime)data["DRW_TRN_TS"];
                o.DateTransactionDrawed = DateTransaction;
                o.DateoPosted = DateTransaction.ToString("MMM dd, yyyy");
                o.DateDrawed = $"{ o.DateoPosted } ({ o.DrawType })";
                o.TimeoPosted = DateTransaction.ToString("hh:mm:ss tt");
            }catch{}
            o.TotalBetStraightAmount = data["TOT_BET_STRGHT_AMT"].Str().ToDecimalDouble();
            o.TotalBetRumbleAmount = data["TOT_BET_RMB_AMT"].Str().ToDecimalDouble();
            o.TotalWinStraightAmount = data["TOT_WIN_STRGHT_AMT"].Str().ToDecimalDouble();
            o.TotalWinRumbleAmount= data["TOT_WIN_RMB_AMT"].Str().ToDecimalDouble();
            o.TotalWinAmount= data["TOT_WIN_AMT"].Str().ToDecimalDouble();
            return o;
        }

        public static IEnumerable<dynamic> FilterGameSummariesWithDetails(IEnumerable<dynamic> summaries, IEnumerable<dynamic> details, int limit = 50){
            if(summaries==null) return null;
            var items = GameSummariesWithDetails(summaries, details);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }

        public static IEnumerable<dynamic> GameSummariesWithDetails(IEnumerable<dynamic> summaries, IEnumerable<dynamic> details){
            summaries = GameSummaries(summaries).ToList();
            if(summaries.Count() != 0){
                details = GameDetails(details);
                Dictionary<string, object> detailsPerTransaction = new Dictionary<string, object>();
                foreach(dynamic detail in details){
                    string TransNo = (detail.TransactionNo as object).Str();
                    if(!detailsPerTransaction.ContainsKey(TransNo))
                        detailsPerTransaction[TransNo] = new List<dynamic>();
                    (detailsPerTransaction[TransNo] as List<dynamic>).Add(detail); 
                }
                foreach(dynamic summary in summaries){
                    string TransNo = (summary.TransactionNo as object).Str();
                    summary.Draws = detailsPerTransaction.GetValue(TransNo);
                }
            }
            return summaries; 
        }

        public static IEnumerable<dynamic> GameSummaries(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> GameSummary(e));
        }
    
        public static IDictionary<string, object> GameSummary(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.DateTransaction = data["RGS_TRN_TS"];
            o.TransactionNo = data["TRN_NO"].Str();
            o.TotalBetCount = (int)data["TOT_CNT_BET"].Str().ToDecimalDouble();
            o.TotalStraightCount = (int)data["TOT_CNT_STRGHT_BET"].Str().ToDecimalDouble();
            o.TotalRumbleCount = (int)data["TOT_CNT_RMBL_BET"].Str().ToDecimalDouble();
            o.TotalStraightAmount = data["TOT_AMT_STRGHT"].Str().ToDecimalDouble();
            o.TotalRumbleAmount = data["TOT_AMT_RMBL"].Str().ToDecimalDouble();
            o.TotalBetAmount = data["TOT_AMT_BET"].Str().ToDecimalDouble();
            o.DrawType = data["DRW_TYP"].Str();
            o.BetType = data["GM_CD"].Str();
            string customer = data["REF_CSTMR"].Str();
            if(!customer.IsEmpty()) 
                o.Customer = customer; 
            try{
                object obj = (data["RGS_TRN_TS"]??null);
                if(obj != null){
                    DateTime datetime = (DateTime)obj;
                    o.DateoPosted = datetime.ToString("MMM dd, yyyy");
                    o.DateDrawed = $"{ o.DateoPosted } ({ o.DrawType })";
                    o.TimeoPosted = datetime.ToString("hh:mm:ss tt");
                }
            }catch{}
            try{
                object obj = (data["DRW_TRN_DT"]??null);
                if(obj != null){
                    DateTime datetime = (DateTime)obj;
                    o.DateDrawed = $"{ datetime.ToString("MMM dd, yyyy") } ({ o.DrawType })";
                }
            }catch{}
            try{
                object obj = (data["RQT_TS"]??null);
                if(obj != null){
                    DateTime datetime = (DateTime)obj;
                    o.RequestDate = datetime.ToString("MMM dd, yyyy hh:mm:ss tt");
                }
            }catch{}
            return o;
        }
        public static IEnumerable<dynamic> GameDetails(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> GameDetail(e));
        }
        public static IDictionary<string, object> GameDetail(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            string stat = data["S_DNE"].Str();
            o.TransactionNo = data["TRN_NO"].Str();
            o.SequenceNo = (int)data["SEQ_NO"].Str().ToDecimalDouble();
            o.StraightAmount = data["STRGHT_AMT"].Str().ToDecimalDouble();
            o.RumbleAmount = data["RMB_AMT"].Str().ToDecimalDouble();
            o.DrawType = data["DRW_TYP"].Str();
            //o.IsDone = (stat.Equals("1");
            string bettype = (o.BetType = o.BetType = data["GM_CD"].Str());
            string numbers = (o.Numbers = data["NUM_BET"].Str().Trim());
            if(bettype.Equals("PARES"))
                o.Numbers = String.Join('-', numbers.Split('-').Select(e=> e.Str().Trim().PadLeft(2, '0')));
            double ReturnStraightAmount = data["RET_STRGHT_AMT"].Str().ToDecimalDouble(); 
            double ReturnRumbleAmount = data["RET_RMB_AMT"].Str().ToDecimalDouble();
            bool HasReturn = ((ReturnStraightAmount+ReturnRumbleAmount)!=0);
            bool IsReturn  = HasReturn && ((o.StraightAmount+o.RumbleAmount)==0);
            if(HasReturn){
                if(stat.Equals("0"))
                    o.IsSoldOut = true;
                o.IsReturn = IsReturn;
                o.HasReturn = HasReturn;
                o.ReturnStraightAmount = ReturnStraightAmount;
                o.ReturnRumbleAmount = ReturnRumbleAmount;
            }
            if(data["S_WIN"].To<bool>(false)){
                string type = data["WIN_TYP"].Str();
                if(type.Equals("A")) o.WinType = "S";
                else if(type.Equals("B")) o.WinType = "R";
                else if(type.Equals("C")) o.WinType = "S/R";
            }
            return o;
        }
        
        public static IEnumerable<dynamic> FilterGameResults(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = GameResults(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }

        public static IEnumerable<dynamic> GameResults(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> GameResult(e));
        }

        public static IDictionary<string, object> GameResult(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.DateTransaction = data["RGS_TRN_TS"];
            o.DrawType = data["DRW_TYP"].Str(); //;Static.DrawType.GetValue(.Trim());
            string bettype = (o.BetType = data["GM_CD"].Str()); //Static.BetType.GetValue(data["GM_TYP"].Str())
            string numbers = (o.Numbers = data["NUM_RES"].Str().Trim());
            if(bettype.Equals("PARES"))
                o.Numbers = String.Join('-', numbers.Split('-').Select(e=> e.Str().Trim().PadLeft(2, '0')));
            o.Is2Ball = (bettype.Equals("SWER2")||bettype.Equals("PARES"));
            o.Is3Ball = (bettype.Equals("SWER3")||bettype.Equals("3D"));
            o.Is6Ball = false;
            try{
                DateTime DateTransaction = (DateTime)data["RGS_TRN_TS"];
                o.DateoPosted = DateTransaction.ToString("MMM dd, yyyy");
                o.DateDrawed = $"{ o.DateoPosted } ({ o.DrawType })";
                o.TimeoPosted = DateTransaction.ToString("hh:mm:ss tt");
                o.TransactionNo = DateTransaction.ToString("yyyyMMddHHmmss");
            }catch{}
            return o;
        }
        
        public static IEnumerable<dynamic> WinTickets(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>WinTicket(e));
        } 
        public static IDictionary<string, object> WinTicket(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.TransactionNo = data["TRN_NO"].Str();
            o.AccountID = data["ACT_ID"].Str();
            string gametype = data["GM_TYP"].Str();
            ////string gameid = (o.GameID = Static.BetType.GetValue(gametype));
            o.GameID = data["GM_CD"].Str();
            o.GameName = data["GM_NM"].Str(); //$"STL { gameid.ToUpper() }";

            //string drawtype = ((int)data["DRW_TYP"].Str().ToDecimalDouble()).Str();
            o.DrawSchedule =  data["DRW_TYP"].Str(); //Static.DrawType.GetValue(drawtype);

            o.TotalWinStraight = data["TOT_WIN_STRGHT_AMT"].Str().ToDecimalDouble();
            o.TotalWinRumble = data["TOT_WIN_RMB_AMT"].Str().ToDecimalDouble();
            o.TotalWinPrize = data["TOT_WIN_AMT"].Str().ToDecimalDouble();
            
            try{
                DateTime drawDT = data["DRW_TRN_DT"].To<DateTime>();
                o.DateoDraw = drawDT.ToString("MMMM dd, yyyy");
            }catch{}
            try{ 
                DateTime postDT = data["DRWN_TRN_TS"].To<DateTime>();
                o.DatePosted = postDT.ToString("MMMM dd, yyyy hh:mm:ss tt");
                o.DateoPosted = postDT.ToString("MMMM dd, yyyy");
                o.TimeoPosted = postDT.ToString("hh:mm:ss tt");
            }catch{}
            return o;
        }


        public static IEnumerable<dynamic> UnclaimWins(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>UnclaimWin(e));
        } 
        public static IDictionary<string, object> UnclaimWin(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.TransactionNo = data["TRN_NO"].Str();
            o.AccountID = data["ACT_ID"].Str();
            o.AccountName = data["USR_FLL_NM"].Str();

            ////string gameid = (o.GameID = Static.BetType.GetValue(gametype));
            o.GameID = data["GM_CD"].Str(); 
            o.GameName = data["GM_NM"].Str(); //$"STL { gameid.ToUpper() }";

            //string drawtype = ((int)data["DRW_TYP"].Str().ToDecimalDouble()).Str();
            o.DrawSchedule =  data["DRW_TYP"].Str(); //Static.DrawType.GetValue(drawtype);

            o.DateDraw = data["DRW_TRN_DATE"].To<DateTime>().ToString("MMMM dd, yyyy");
            o.Result = data["NUM_RES"].Str();
            o.Combination = data["NUM_BET"].Str();
            o.BetStraight = data["STRGHT_AMT"].Str().ToDecimalDouble();
            o.BetRumble = data["RMB_AMT"].Str().ToDecimalDouble();
            o.TotalBet = (o.BetStraight + o.BetRumble);
            o.WinStraight = data["WIN_STRGHT_AMT"].Str().ToDecimalDouble();
            o.WinRumble = data["WIN_RMB_AMT"].Str().ToDecimalDouble();
            o.TotalWinAmount = data["WIN_TOT_AMT"].Str().ToDecimalDouble();
            return o;
        }

        public static IEnumerable<dynamic> PreviousBetTickets(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>PreviousBetTicket(e));
        } 
        public static IDictionary<string, object> PreviousBetTicket(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.AccountID = data["ACT_ID"].Str();
            o.TransactionNo = data["TRN_NO"].Str();
            o.ReferenceID = Cipher.MD5Hash($"{o.AccountID}:{o.TransactionNo}");
            o.TransactionSeries = $"{o.TransactionNo}-{ data["SEQ_NO"].Str()}";
            o.IsPrinted = "0";
            o.IsSendedSms = "0";
            o.IsEmailed = "0";
            o.BetStraight = data["STRGHT_AMT"].Str().ToDecimalDouble();
            o.BetRumble = data["RMB_AMT"].Str().ToDecimalDouble();
            o.TotalBet = (o.BetStraight + o.BetRumble);
            //string gametype = data["GM_TYP"].Str();
            //string gameid = (o.GameID = Static.BetType.GetValue(gametype));
            o.GameID = data["GM_TYP_CD"].Str(); 
            o.GameName = data["GM_TYP_NM"].Str(); // 

            //string drawtype = ((int)data["DRW_TYP"].Str().ToDecimalDouble()).Str();
            o.DrawSchedule = data["DRW_TYP"].Str(); //Static.DrawType.GetValue(drawtype);
            o.Combination = data["NUM_BET"].Str();
            try{
                DateTime postDT = data["RGS_TRN_TS"].To<DateTime>();
                o.DatePosted = postDT.ToString("MMMM dd, yyyy hh:mm:ss tt");
                o.DateoPosted = postDT.ToString("MMMM dd, yyyy");
                o.TimeoPosted = postDT.ToString("hh:mm:ss tt");
            }catch{}
            try{
                o.DateDraw = data["DRW_TRN_DATE"].To<DateTime>().ToString("yyyy-MM-dd");
            }catch{}
            return o;
        }

        public static IEnumerable<dynamic> LotteryTypes(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>LotteryType(e));
        } 
        public static IDictionary<string, object> LotteryType(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.GameID = data["ID"].Str();
            o.GameCode = data["NAME"].Str();
            o.GameName = data["DESCR"].Str();
            o.DigitCount = data["BLL_CNT"].Str();
            o.MinDigit = data["MN_DGT"].Str();
            o.MaxDigit = data["MX_DGT"].Str();
            return o;
        }
        public static IEnumerable<dynamic> LotteryDrawSchedules(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>LotteryDrawSchedule(e));
        } 
        public static IDictionary<string, object> LotteryDrawSchedule(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.GameID = data["GM_TYP"].Str();
            o.SequenceNo = data["SEQ_NO"].Str();
            o.DrawName = data["DRW_TYP"].Str().Trim();
            o.CutOffStart = data["CUT_STRT"].Str();
            o.CutOffEnd = data["CUT_END"].Str();
            return o;
        }

        public static IEnumerable<dynamic> LotterySoldOutCombinations(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>LotterySoldOutCombination(e));
        } 
        public static IDictionary<string, object> LotterySoldOutCombination(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.GameID = data["GM_TYP"].Str();
            o.Combination = data["NUM_COMB"].Str();
            o.No = data["RW_NO"].Str();
            return o;
        }

        public static IEnumerable<dynamic> LotteryLimitBetCombinations(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>LotteryLimitBetCombination(e));
        } 
        public static IDictionary<string, object> LotteryLimitBetCombination(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.GameID = data["GM_TYP"].Str();
            o.Combination = data["NUM_COMB"].Str();
            o.Straight = data["STRGHT_AMT_LMT"].Str();
            o.Rumble = data["RMBL_AMT_LMT"].Str();
            return o;
        }


        public static IEnumerable<dynamic> GameProfiles(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>GameProfile(e));
        } 
        public static IDictionary<string, object> GameProfile(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;

            o.WinPrice = data["PRC_WNNG_AMNT"].Str().ToDecimalDouble();
            o.StraightLimitAmount = data["STRGHT_AMT_LMT"].Str().ToDecimalDouble();
            o.RumbleLimitAmount = data["RMBL_AMT_LMT"].Str().ToDecimalDouble();
            o.FixedRate = false;
            //
            o.GameID = data["GM_TYP"].Str();
            o.GameName = data["GM_TYP_NM"].Str();
            o.DateDraw = data["DRW_TRN_DATE"].To<DateTime>().ToString("yyyy-MM-dd");
            o.DrawSchedule = data["DRW_TYP"].Str();
            o.CutOffStart = data["CUT_STRT"].Str();
            o.CutOffEnd = data["CUT_END"].Str();

            return o;
        }

        
        public static IEnumerable<dynamic> PostedResults(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>PostedResult(e));
        } 
        public static IDictionary<string, object> PostedResult(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            //string gametype = data["GM_TYP"].Str();
            //var gameid = (o.GameID = Static.BetType.GetValue(gametype));
            o.GameID = data["GM_TYP"].Str(); //GM_TYP_CD
            o.GameCode = data["GM_TYP_CD"].Str();
            o.GameName = data["GM_TYP_NM"].Str();//$"STL { gameid.ToUpper() }";
            //string drawtype = ((int)data["DRW_TYP"].Str().ToDecimalDouble()).Str();
            o.DrawSchedule = data["DRW_TYP"].Str();//Static.DrawType.GetValue(drawtype);

            o.DateDraw = data["DRW_TRN_DATE"].To<DateTime>().ToString("yyyy-MM-dd");
            o.Result = data["NUM_RES"].Str();
            o.DatePosted = data["RGS_TRN_TS"].To<DateTime>().ToString("yyyy-MM-dd hh:mm:ss tt");
            o.TotalStraight = data["TOT_AMT_STRGHT_WNRS"].Str().ToDecimalDouble();
            o.TotalRumble = data["TOT_AMT_RMBL_WNRS"].Str().ToDecimalDouble();
            o.TotalBet = data["TOT_AMT_WNRS"].Str().ToDecimalDouble();
            o.PostedBy = "";
            o.Status = "";
            return o;
        }

        public static IEnumerable<dynamic> RequestPostResults(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>RequestPostResult(e));
        } 
        public static IDictionary<string, object> RequestPostResult(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.GameID = data["GM_TYP"].Str();
            o.GameCode = data["GM_TYP_CD"].Str();
            o.GameName = data["GM_TYP_NM"].Str();
            o.DrawSchedule = data["DRW_TYP"].Str();

            o.DateDraw = data["DRW_TRN_DATE"].To<DateTime>().ToString("MMMM dd, yyyy");
            o.DatePost = data["RGS_TRN_TS"].To<DateTime>().ToString("MMMM dd, yyyy hh:mm:ss tt");
            o.PostedBy = data["USR_FLL_NM"].Str().Trim();
            o.Result = data["NUM_RES"].Str();
            o.TotalStraight = data["TOT_AMT_STRGHT"].Str().ToDecimalDouble();
            o.TotalRumble = data["TOT_AMT_RMBL"].Str().ToDecimalDouble();
            o.TotalBet = (o.TotalStraight + o.TotalRumble);
            o.Status = "FOR APPROVAL";
            return o;
        }


        public static IDictionary<string, object> GameSettings(IEnumerable<dynamic> list){
            dynamic o = Dynamic.Object;
            foreach(IDictionary<string, object> data in list){
                string type = data["GM_TYP"].Str();
                if(type.Equals("01")){
                    o.LimitStraightAmountBetSWER3 =  data["STRGHT_AMT_LMT"].Str().ToDecimalDouble();
                    o.LimitRumbleAmountBetSWER3 =  data["RMBL_AMT_LMT"].Str().ToDecimalDouble();
                }else if(type.Equals("02")){
                    o.LimitStraightAmountBetSWER2 =  data["STRGHT_AMT_LMT"].Str().ToDecimalDouble();
                    o.LimitRumbleAmountBetSWER2 =  data["RMBL_AMT_LMT"].Str().ToDecimalDouble();
                }else if(type.Equals("03")){
                    o.LimitStraightAmountBetPARES =  data["STRGHT_AMT_LMT"].Str().ToDecimalDouble();
                    o.LimitRumbleAmountBetPARES =  data["RMBL_AMT_LMT"].Str().ToDecimalDouble();
                }
            }
            return o;
        }
        public static IDictionary<string, object> CompanySettings(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            string compid = data["COMP_ID"].Str();
            if(!compid.IsEmpty()) o.CompanyID =  compid; 
            o.CompanyName =  data["COMP_NM"].Str();
            o.CompanyAddress =  data["COMP_ADD"].Str();
            o.LiveStreamUrl = data["URL_STRMNG"].Str();
            o.LiveStreamName = data["URL_STRMNG_NM"].Str();
            o.AppSharingDescription = data["APP_SHRING_DESC"].Str();

            return o;
        }

        public static IDictionary<string, object> PartyListSettings(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            string compid = data["PL_ID"].Str();
            if (!compid.IsEmpty()) o.CompanyID = compid;
            o.CompanyName = data["PL_NM"].Str();
            o.CompanyAddress = data["PL_ADD"].Str();
            o.LiveStreamUrl = data["URL_STRMNG"].Str();
            o.LiveStreamName = data["URL_STRMNG_NM"].Str();
            o.AppSharingDescription = data["APP_SHRING_DESC"].Str();

            return o;
        }

        public static IEnumerable<dynamic> SalesDetails(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> SalesDetail(e));
        }
        public static IDictionary<string, object> SalesDetail(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            string stat = data["S_DNE"].Str();
            o.TransactionDate = data["DRW_TRN_DT_NM"].Str();
            o.TotalStraight = data["TOT_AMT_STRGHT"].Str().ToDecimalDouble();
            o.TotalRumble = data["TOT_AMT_RMBL"].Str().ToDecimalDouble();
            o.TotalSales = (o.TotalStraight + o.TotalRumble);
            return o;
        }

        public static IEnumerable<dynamic> Games(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> Game(e));
        }

        public static IDictionary<string, object> Game(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.GameName = data["GM_NM"].Str(); 
            o.GameID = data["GM_ID"].Str(); 
            o.GameCode = data["GM_CD"].Str(); 
            return o;
        }
    }
}