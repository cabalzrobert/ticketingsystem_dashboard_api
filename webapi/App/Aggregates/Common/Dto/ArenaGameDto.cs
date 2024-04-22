using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class ArenaGameDto
    {
        public static dynamic ArenaDetails(IDictionary<string, object> data, bool includeResult = true){
            dynamic o = Dynamic.Object;
            o.FightID = data["GM_ID"].Str().Trim();
            o.FightNo = data["FGHT_NO"].Str().Trim();

            string eventDescription = data["EVNT_TTLE"].Str().Trim();
            if(!eventDescription.IsEmpty()) o.EventDescription = eventDescription;
            string fightDescription = data["FGHT_DESC"].Str().Trim();
            if(!fightDescription.IsEmpty()) o.FightDescription = fightDescription;

            string arenaStreamUrl = data["FGHT_STRM_URL"].Str().Trim();
            if(!arenaStreamUrl.IsEmpty()) o.ArenaLiveStreamUrl = arenaStreamUrl;
            //--stream
            string streamtype = data["STRM_TYP"].Str().Trim();
            if(!streamtype.IsEmpty()){
                if(streamtype.Equals("1")) o.IsLiveStream = true;
                else if(streamtype.Equals("2")) o.IsAdsStream = true;
                else o.IsDefaultStream = true;
                if(streamtype.Equals("1")){
                    string streamUrl = data["FGHT_STRM_URL"].Str().Trim();
                    if(!streamUrl.IsEmpty()) o.LiveStreamUrl = streamUrl;
                }else if(streamtype.Equals("2")){
                    string streamUrl = data["STRM_ADS_URL"].Str().Trim();
                    if(!streamUrl.IsEmpty()) o.LiveStreamUrl = streamUrl;
                }else{
                    string streamUrl = data["STRM_DEFLT_URL"].Str().Trim();
                    if(!streamUrl.IsEmpty()) o.LiveStreamUrl = streamUrl;
                }
            }

            o.TotalBetMeron = data["LFT_AMT"].Str().Trim().ToDecimalDouble();
            o.TotalBetWala = data["RGHT_AMT"].Str().Trim().ToDecimalDouble();
            o.PayoutPercentageBetMeron = AggrUtils.Number.Floor(data["LFT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble()); 
            o.PayoutPercentageBetWala = AggrUtils.Number.Floor(data["RGHT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble());

            string leftname = data["LFT_NM"].Str();
            string rightname = data["RGHT_NM"].Str();
            if(!leftname.IsEmpty() && !rightname.IsEmpty()){
                o.MeronTallyName = leftname;
                o.WalaTallyName = rightname;
                o.MeronWeight = data["LFT_WGHT"].Str();
                o.WalaWeight = data["RGHT_WGHT"].Str();
                o.HasTally = true;
            }

            string status = data["GM_STAT"].Str().Trim();
            o.IsOpened = o.IsLastCalled = o.IsClosed = o.IsQueueing = false;
            if("489".Contains(status)){
                o.IsOpened = status.Equals("4");
                o.IsLastCalled = status.Equals("8");
                o.IsClosed = status.Equals("9");
            }else if(!status.Equals("5") && includeResult){
                o.IsClosed = true;
                o.IsMeronWin = status.Equals("1");
                o.IsWalaWin = status.Equals("2");
                o.IsWin = (o.IsWalaWin||o.IsMeronWin);
                o.IsDraw = status.Equals("3");
                o.IsCancelled = status.Equals("0");
                o.IsDeclaredResult = true;
            }else{
                o.IsQueueing = status.Equals("5");
                o.IsClosed = true;
                o.TotalBetMeron = 0.00;
                o.TotalBetWala = 0.00;
            }

            string toStartTimer = data["FGHT_TMR"].Str(); 
            var timerTs = data["FGHT_TMR_STRT"].To<DateTime?>();
            string stat = data["FGHT_TMR_STAT"].Str().Trim();
            if(timerTs!=null && !toStartTimer.IsEmpty() && stat.Equals("1")){
                var serverTs = data["CURT_TS"].To<DateTime>();
                string[] split = toStartTimer.Split(':');
                double dMs = 16.66667;
                int min = (int)AggrUtils.Text.Get(split, 0).ToDecimalDouble();
                int sec = (int)AggrUtils.Text.Get(split, 1).ToDecimalDouble();
                int msec = (int)AggrUtils.Text.Get(split, 2).ToDecimalDouble();
                var endTimerTs = (timerTs??serverTs).AddMinutes(min).AddSeconds(sec).AddMilliseconds(msec*dMs);
                o.IsTimerRunning = true;
                if(serverTs<endTimerTs){
                    TimeSpan ts = endTimerTs.Subtract(serverTs);
                    int milliseconds = (int)ts.TotalMilliseconds;
                    int seconds = (milliseconds / 1000);
                    milliseconds = (int)((milliseconds % 1000) / dMs);
                    int totalMilliseconds = (seconds * 60) + (int)(milliseconds/dMs);
                    int minutes = (seconds/60);
                    seconds = (seconds % 60);
                    //--
                    o.RunningTimer = totalMilliseconds;
                    o.RunningTimerInFormat = $"{ minutes.Str().PadLeft(2, '0') }:{ seconds.Str().PadLeft(2, '0') }:{ milliseconds.Str().PadLeft(2, '0') }";
                }else{
                    o.RunningTimer = 0;
                    o.RunningTimerInFormat = "00:00:00";
                }
            } 
            return o;
        }

        public static IEnumerable<dynamic> FilterTickets(IDictionary<string, object> arena, IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = Tickets(arena, list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> Tickets(IEnumerable<dynamic> list){
            return Tickets(null, list);
        } 
        public static IEnumerable<dynamic> Tickets(IDictionary<string, object> arena, IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>Ticket(arena, e));
        } 
        public static IDictionary<string, object> Ticket(IDictionary<string, object> arena, IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            if(arena!=null){
                o.GameTypeDescription = "SABONG ARENA";
                o.CockpitArena = arena["BR_NME"].Str();
                o.CockpitArenaLocation = arena["BR_ADDRS"].Str();
            }
            o.TransactionNo = data["TRN_NO"].Str();
            o.FightID = data["FGHT_ID"].Str();
            o.FightNo = data["FGHT_NO"].Str();
            o.EventDescription = data["EVNT_TTLE"].Str();
            o.FightDescription = data["FGHT_DESC"].Str();
            o.DateTransaction = data["RGS_TRN_TS"];
            o.BetAmount = data["BET_AMT"].Str().ToDecimalDouble();
            o.IsCancelled = false;
            string status = data["STAT"].Str().Trim();
            o.IsOngoing = status.Equals("4");
            o.BetType = (data["BET_TYP"].Str().Equals("left")?"Meron":"Wala");
            if(!o.IsOngoing){
                o.IsCancelled = status.Equals("0");
                o.IsDraw = status.Equals("3");
                if(!(o.IsCancelled || o.IsDraw)){
                    o.IsWin = data["S_WIN"].To<bool>(false);
                    if(o.IsWin){
                        o.WinningAmount = o.WinningPercentageAmount = o.WinningPercentage = 0;
                        double winningAmount = data["WIN_AMT"].Str().ToDecimalDouble();
                        o.WinningPercentageAmount = o.WinningAmount = winningAmount;
                        o.WinningPercentage = AggrUtils.Number.Floor(data["WIN_PRCNTG_AMT"].Str().ToDecimalDouble());
                    }
                }
            }

            string fightNoZ3 = data["FGHT_NO"].Str().PadLeft(3, '0');
            string dateplace = "000000/0000";
            try{ 
                DateTime placeDT = data["RGS_TRN_TS"].To<DateTime>();
                o.DatePlaced  = placeDT.ToString("MMMM dd, yyyy hh:mm:ss tt");
                o.DateoPlaced  = placeDT.ToString("MMMM dd, yyyy");
                o.TimeoPlaced = placeDT.ToString("hh:mm:ss tt");
                dateplace = placeDT.ToString("MMddyy/HHmm");
            }catch{}
            o.BetReferenceCode = $"{fightNoZ3}/{dateplace}";
            try{
                DateTime postDT = data["UPD_TRN_TS"].To<DateTime>();
                o.DatePosted = postDT.ToString("MMMM dd, yyyy hh:mm:ss tt");
                o.DateoPosted = postDT.ToString("MMMM dd, yyyy");
                o.TimeoPosted = postDT.ToString("hh:mm:ss tt");
            }catch{}
            return o;
        }

        public static IDictionary<string, object> LastPlaceBet(IEnumerable<dynamic> rows){
            dynamic o = Dynamic.Object;
            foreach(IDictionary<string, object> row in rows){
                string fightID = row["FGHT_ID"].Str();
                string betType = row["FGHT_BET"].Str();
                string fightNo = row["FGHT_NO"].Str();
                string fightNoZ3 = fightNo.PadLeft(3, '0');
                string dateplace = "000000/0000";
                try{
                    DateTime placeDT = row["RGS_TRN_TS"].To<DateTime>();
                    dateplace = placeDT.ToString("MMddyy/HHmm");
                }catch{}
                if(betType.Equals("1")){
                    o.TotalBetMeron = row["TOT_LFT_AMT"].Str().ToDecimalDouble();
                    o.MeronFightID = fightID;
                    o.MeronFightNo = fightNo;
                    o.MeronReferenceCode = $"{fightNoZ3}/{dateplace}";
                    o.MeronAccepted = true;
                }else if(betType.Equals("2")){
                    o.TotalBetWala = row["TOT_RGHT_AMT"].Str().ToDecimalDouble();
                    o.WalaFightID = fightID;
                    o.WalaFightNo = fightNo;
                    o.WalaReferenceCode = $"{fightNoZ3}/{dateplace}";
                    o.WalaAccepted = true;
                }
            }
            return o;
        }
        
        public static IEnumerable<dynamic> FilterTrends(IEnumerable<dynamic> list, int limit = 50){
            if(list==null) return null;
            var items = Trends(list);
            var count = items.Count();
            if(count>=limit){
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count-1).Concat(new[]{ o });
                filter.BaseFilter = o.DateTransaction;
            }
            return items;
        }
        public static IEnumerable<dynamic> Trends(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>Trend(e));
        } 

        public static IDictionary<string, object> Trend(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.FightID = data["FGHT_ID"].Str();
            o.FightNo = data["FGHT_NO"].Str();
            o.DateTransaction = data["RGS_TRN_TS"];
            
            string status = data["GM_STAT"].Str().Trim();
            o.IsMeronWin = status.Equals("1");
            o.IsWalaWin = status.Equals("2");
            o.IsWin = (o.IsWalaWin||o.IsMeronWin);
            o.Status = ""; 
            if(o.IsMeronWin) o.BetTypeWin = "Meron";
            if(o.IsWalaWin) o.BetTypeWin = "Wala";

            o.IsDraw = status.Equals("3");
            o.IsCancelled = status.Equals("0");
            o.IsReturned = (o.IsDraw||o.IsCancelled);
            
            DateTime transDT = data["RGS_TRN_TS"].To<DateTime>();
            o.DateCreated = transDT.ToString("yyyy-MM-dd hh:mm:ss tt");
            o.TimeoCreated = transDT.ToString("hh:mm:ss tt");

            o.Status = "-"; //"Queueing";
            if(o.IsCancelled) o.Status="Cancelled";
            else if(o.IsDraw) o.Status="Draw";
            else if(o.IsMeronWin) o.Status="Meron Win";
            else if(o.IsWalaWin) o.Status="Wala Win";
            return o;
        }

        public static List<List<dynamic>> GroupTrends(IEnumerable<dynamic> rows){
            List<List<dynamic>> trends = new List<List<dynamic>>();
            if(rows!=null && rows.Count()!=0){
                List<dynamic> trends6 = new List<dynamic>();
                string lastStatus = "";
                foreach(IDictionary<string, object> row in rows){
                    dynamic o = Dynamic.Object;
                    string status = row["GM_STAT"].Str().Trim();
                    if(status.Equals("0")){
                        o.Status = (status = "cancelled");
                        o.IsCancelled = true;
                    }else if(status.Equals("1")){
                        o.Status = (status = "win-meron");
                        o.IsWin = true;
                        o.Win = "MERON";
                    }else if(status.Equals("2")){
                        o.Status = (status = "win-wala");
                        o.IsWin = true;
                        o.Win = "WALA";
                    }else if(status.Equals("3")){
                        o.Status = (status = "draw");
                        o.IsDraw = true;
                    }else{
                        o.Status = (status = "");
                        o.IsQueueing = true;
                    }
                    if(!lastStatus.IsEmpty() && (!status.Equals(lastStatus) || (status.Equals(lastStatus) && trends6.Count>6)))
                        trends.Add(trends6 = new List<dynamic>());
                    trends6.Add(o);
                    lastStatus = status;
                }
            }
            return trends;
        }
        public static List<object> GroupTrendsV2(IEnumerable<dynamic> rows, bool includeHold=true){
            List<object> trends = new List<object>();
            if(rows!=null && rows.Count()!=0){
                List<int> trends5 = null;
                string lastStatus = "";
                int hold = 0;
                foreach(IDictionary<string, object> row in rows){
                    string status = row["GM_STAT"].Str().Trim();
                    int iStatus = 0;
                    bool jump = false, insertJump = false;
                    if(row["S_HLD"].Str().Trim().Equals("1")) {
                        if(!includeHold){
                            hold++;
                            continue;
                        } 
                        iStatus = -3;
                    }else if(row["ACTV_FGHT_NO"].Str().Trim().Equals("1")) iStatus = 1;
                    else if(status.Equals("0")) iStatus = -1;
                    else if(status.Equals("1")) iStatus = 2;
                    else if(status.Equals("2")) iStatus = 3;
                    else if(status.Equals("3")) iStatus = -2;
                    status = iStatus.Str(); 

                    if(lastStatus.IsEmpty()) jump = true;
                    else if(iStatus == 0 || iStatus == 1) jump = true;
                    else if(trends5.Count>4) jump = true;
                    else if(iStatus == 0){}
                    else if(!status.Equals(lastStatus)){
                        if(lastStatus.Equals("-1") || lastStatus.Equals("-2")){
                            if(iStatus == 2 || iStatus == 3 || iStatus == -3)
                                jump = true;
                        }else if(iStatus == 2 || iStatus == 3 || iStatus == -3){
                            jump = true;
                        }else{
                            insertJump = true;
                            if(lastStatus.Equals("-3"))
                                jump = !(insertJump = false);
                        }
                        if(jump && trends5.Count==0)
                            jump = false;
                    }
                    if(insertJump){
                        trends5.Add(iStatus);
                        trends.Add(trends5 = new List<int>());
                    }else{
                        if(jump) trends.Add(trends5 = new List<int>());
                        trends5.Add(iStatus);
                    }
                    lastStatus = status;
                }
                if(includeHold && hold>0){
                    for(int i=0;i<hold;i++){
                        if(trends5.Count>4)
                            trends.Add(trends5 = new List<int>());
                        trends5.Add(0);
                    }
                }
            }
            return trends;
        }
    
        public static IEnumerable<dynamic> WinTickets(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>WinTicket(e));
        } 
        public static IDictionary<string, object> WinTicket(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.TransactionNo = data["TRN_NO"].Str();
            o.FightID = data["FGHT_ID"].Str();
            o.ReferenceID = data["REF_NO"].Str();
            o.FightNo = data["FGHT_NO"].Str();
            o.EventDescription = data["EVNT_TTLE"].Str();
            o.FightDescription = data["FGHT_DESC"].Str();
            o.DateTransaction = data["RGS_TRN_TS"];
            o.BetAmount = data["BET_AMT"].Str().ToDecimalDouble();
            o.IsCancelled = false;
            string status = data["STAT"].Str().Trim();
            o.IsOngoing = status.Equals("4");
            o.BetType = (data["BET_TYP"].Str().Equals("left")?"Meron":"Wala");
            if(!o.IsOngoing){
                o.IsCancelled = status.Equals("0");
                o.IsDraw = status.Equals("3");
                if(!(o.IsCancelled || o.IsDraw)){
                    o.IsWin = data["S_WIN"].To<bool>(false);
                    if(o.IsWin){
                        o.WinningAmount = o.WinningPercentageAmount = o.WinningPercentage = 0;
                        double winningAmount = data["WIN_AMT"].Str().ToDecimalDouble();
                        o.WinningPercentageAmount = o.WinningAmount = winningAmount;
                        o.WinningPercentage = AggrUtils.Number.Floor(data["WIN_PRCNTG_AMT"].Str().ToDecimalDouble());
                    } 
                }
            }

            string fightNoZ3 = data["FGHT_NO"].Str().PadLeft(3, '0');
            string dateplace = "000000/0000";
            try{ 
                DateTime placeDT = data["RGS_TRN_TS"].To<DateTime>();
                o.DatePlaced  = placeDT.ToString("MMMM dd, yyyy hh:mm:ss tt");
                o.DateoPlaced  = placeDT.ToString("MMMM dd, yyyy");
                o.TimeoPlaced = placeDT.ToString("hh:mm:ss tt");
                dateplace = placeDT.ToString("MMddyy/HHmm");
            }catch{}
            o.BetReferenceCode = $"{fightNoZ3}/{dateplace}";
            try{
                DateTime postDT = data["UPD_TRN_TS"].To<DateTime>();
                o.DatePosted = postDT.ToString("MMMM dd, yyyy hh:mm:ss tt");
                o.DateoPosted = postDT.ToString("MMMM dd, yyyy");
                o.TimeoPosted = postDT.ToString("hh:mm:ss tt");
            }catch{}
            return o;
        }
    
        public static IEnumerable<dynamic> ArenaPostResults(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>ArenaPostResult(e));
        } 
        public static IDictionary<string, object> ArenaPostResult(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.FightID = data["GM_ID"].Str();
            o.ReferenceID = data["REF_NO"].Str();
            o.FightNo = (int)data["FGHT_NO"].Str().ToDecimalDouble();
            o.DateDraw = data["DRW_TRN_DT"].Str();
            o.EventDescription = data["EVNT_TTLE"].Str();
            o.FightDescription =  data["FGHT_DESC"].Str();
            //o.LiveStreamUrl =  data["FGHT_STRM_URL"].Str();
            //o.DefaultStreamUrl = data["STRM_DEFLT_URL"].Str(); 
            //
            o.TotalBetMeron = data["LFT_AMT"].Str().ToDecimalDouble();
            o.TotalBetWala= data["RGHT_AMT"].Str().ToDecimalDouble();
            o.PayoutPercentageBetMeron = AggrUtils.Number.Floor(data["LFT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble());
            o.PayoutPercentageBetWala = AggrUtils.Number.Floor(data["RGHT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble());
            o.TotalPayout= data["PY_OUT"].Str().ToDecimalDouble();
            o.TotalProfit= data["PRFT"].Str().ToDecimalDouble();

            o.PostedBy = data["USR_FLL_NM_PST"].Str().Trim().ToUpper();
            o.ProcessedBy = data["USR_FLL_NM_PRCSS"].Str().Trim().ToUpper();
            //

            string status = data["GM_STAT"].Str().Trim();
            //bool isActive = false;
            //if(!IsActive && "489|5".Contains(status))
            //    IsActive = isActive = true;
            //
            o.IsQueueing = false; //status.Equals("5");
            o.IsOpened = false; //status.Equals("4");
            o.IsLastCalled = false; //status.Equals("8");
            o.IsClosed = true; //status.Equals("9");
            //--
            o.IsMeronWin = status.Equals("1");
            o.IsWalaWin = status.Equals("2");
            o.IsWin = (o.IsWalaWin||o.IsMeronWin);
            o.IsDraw = status.Equals("3");
            o.IsCancelled = status.Equals("0");
            o.IsReturned = (o.IsDraw||o.IsCancelled);
            //
            //o.IsStarted = (o.IsOpened||o.IsLastCalled) && !o.IsClosed;
            o.IsActive = data["ACTV_FGHT"].Str().Equals("1"); //
            o.IsOngoing = (o.IsActive||o.IsOpened||o.IsLastCalled||o.IsClosed);
            o.IsDone = (o.IsWin||o.IsReturned);
            
            DateTime drawDT = data["DRW_TRN_DATE"].To<DateTime>();
            o.DateDraw = drawDT.ToString("yyyy-MM-dd");
            o.DateSchedule = drawDT.ToString("MMMM dd, yyyy");
            DateTime transDT = data["RGS_TRN_TS"].To<DateTime>();
            o.DateCreated = transDT.ToString("yyyy-MM-dd hh:mm:ss tt");
            o.TimeCreated = transDT.ToString("hh:mm:ss tt");

            o.Status = "Unknown"; //"Queueing";
            if(o.IsCancelled) o.Status = "Cancelled";
            else if(o.IsDraw) o.Status = "Draw";
            else if(o.IsMeronWin) o.Status = "Meron Win";
            else if(o.IsWalaWin) o.Status = "Wala Win";
            return o;
        }



        public static IEnumerable<dynamic> ArenaEvents(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>ArenaEvent(e));
        } 
        public static IDictionary<string, object> ArenaEvent(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.FightID = data["GM_ID"].Str();
            o.ReferenceID = data["REF_NO"].Str();
            o.EventDescription = data["EVNT_TTLE"].Str();
            o.TotalFights = data["TOT_NUM_FGHTS"].Str().ToDecimalDouble();
            o.FromFightNo = data["STRT_FGHT_NO"].Str().ToDecimalDouble();
            o.ToFightNo = data["END_FGHT_NO"].Str().ToDecimalDouble();

            o.IsActive = data["S_OPN"].Str().Equals("1");
            o.IsClosed = data["S_CLSE"].Str().Equals("1");
            o.IsHold = data["S_HOLD"].Str().Equals("1");
            o.IsOpened = (o.IsActive && !o.IsClosed);
            //
            o.CanOpen =  (!o.IsClosed && !o.IsOpened);   //data["ALLW_OPN"].Str().Equals("1");//
            o.CanClose = ((o.IsOpened || o.IsHold) && !o.IsClosed);  //(o.IsOpened && !o.IsClosed);
            o.CanAddFight = (!o.IsClosed);
            //
            o.GameID = o.GameName = "";
            string gametype = data["GM_TYP"].Str();
            if(gametype.Equals("01")){
                o.GameID = "cf01";
                o.GameName = "Chicken Fight";
            }

            o.EventDescription = data["EVNT_TTLE"].Str();
            o.FightDescription =  data["FGHT_DESC"].Str();
            o.LiveStreamUrl =  data["FGHT_STRM_URL"].Str();
            o.DefaultStreamUrl = data["STRM_DEFLT_URL"].Str(); 
            o.ToStartTimer = data["FGHT_TMR"].Str(); 

            DateTime drawDT = data["DRW_TRN_DATE"].To<DateTime>();
            o.DateDraw = drawDT.ToString("yyyy-MM-dd");
            o.DateSchedule = drawDT.ToString("MMMM dd, yyyy");
            DateTime transDT = data["RGS_TRN_TS"].To<DateTime>();
            o.DateCreated = transDT.ToString("yyyy-MM-dd hh:mm:ss tt");
            o.TimeCreated = transDT.ToString("hh:mm:ss tt");
            return o;
        }

        public static IEnumerable<dynamic> ArenaFights(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=>ArenaFight(e));
        } 

        public static IDictionary<string, object> ArenaFight(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.FightID = data["FGHT_ID"].Str();
            o.ReferenceID = data["REF_NO"].Str();
            o.FightNo = (int)data["FGHT_NO"].Str().ToDecimalDouble();
            //i.DateDraw = row["DRW_TRN_DT"].Str();
            o.EventDescription = data["EVNT_TTLE"].Str();
            o.FightDescription =  data["FGHT_DESC"].Str();
            o.LiveStreamUrl =  data["FGHT_STRM_URL"].Str();
            o.DefaultStreamUrl = data["STRM_DEFLT_URL"].Str(); 
            o.ToStartTimer = data["FGHT_TMR"].Str(); 
            //
            o.TotalBetMeron = data["LFT_AMT"].Str().ToDecimalDouble();
            o.TotalBetWala= data["RGHT_AMT"].Str().ToDecimalDouble();
            o.PayoutPercentageBetMeron = AggrUtils.Number.Floor(data["LFT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble()); 
            o.PayoutPercentageBetWala = AggrUtils.Number.Floor(data["RGHT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble());
            o.TotalPayout= data["PY_OUT"].Str().ToDecimalDouble();
            o.TotalProfit= data["PRFT"].Str().ToDecimalDouble();

            string status = data["GM_STAT"].Str().Trim();
            //
            o.IsQueueing = status.Equals("5");
            o.IsOpened = status.Equals("4");
            o.IsLastCalled = status.Equals("8");
            o.IsClosed = status.Equals("9");
            //--
            o.IsMeronWin = status.Equals("1");
            o.IsWalaWin = status.Equals("2");
            o.IsWin = (o.IsWalaWin||o.IsMeronWin);
            o.IsDraw = status.Equals("3");
            o.IsCancelled = status.Equals("0");
            o.IsReturned = (o.IsDraw||o.IsCancelled);
            o.IsHold = data["S_HLD"].Str().Equals("1"); 
            //
            o.IsActive = data["ACTV_FGHT"].Str().Equals("1"); //
            o.IsOngoing = (o.IsActive||o.IsOpened||o.IsLastCalled||o.IsClosed);
            o.IsDone = (o.IsWin||o.IsReturned);
            
            DateTime drawDT = data["DRW_TRN_DATE"].To<DateTime>();
            o.DateDraw = drawDT.ToString("yyyy-MM-dd");
            o.DateSchedule = drawDT.ToString("MMMM dd, yyyy");
            DateTime transDT = data["RGS_TRN_TS"].To<DateTime>();
            o.DateCreated = transDT.ToString("yyyy-MM-dd hh:mm:ss tt");
            o.TimeCreated = transDT.ToString("hh:mm:ss tt");

            o.Status = "Queueing";
            if(o.IsCancelled) o.Status = "Cancelled";
            else if(o.IsDone) o.Status = "Done";
            else if(o.IsHold) o.Status = "Hold";
            else if(o.IsOngoing) o.Status = "Ongoing";
            
            string leftname = data["LFT_NM"].Str();
            string rightname = data["RGHT_NM"].Str();
            if(!leftname.IsEmpty() && !rightname.IsEmpty()){
                o.LeftName = leftname;
                o.RightName = rightname;
                o.LeftWeight = data["LFT_WGHT"].Str();
                o.RightWeight = data["RGHT_WGHT"].Str();
            }

            return o;
        }
    
    
        
        public static IEnumerable<dynamic> ArenaActiveFights(IEnumerable<dynamic> list){
            if(list==null) return (new List<object>());
            //return list.Select(e=>ArenaActiveFight(e));
            dynamic active = null;
            return list.Select(e=>{
                dynamic o = ArenaActiveFight(e);
                if(o.IsActive && !o.IsDone)
                    active = o;
                return o;
            }).Select(o=>{
                o.CanActivate = (!o.IsActive && !o.IsDone);
                o.CanHold = (o.CanActivate && !o.IsHold && !o.IsDone);
                o.CanCancel = (!o.IsDone); //(!o.IsClosed && );
                if(active!=null && !o.IsActive){
                    if(active.FightNo!=o.FightNo){
                        o.CanActivate = false;
                    }
                }
                return o;
            });
        } 
        public static IDictionary<string, object> ArenaActiveFight(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.FightID = data["FGHT_ID"].Str();
            o.ReferenceID = data["REF_NO"].Str();
            o.FightNo = (int)data["FGHT_NO"].Str().ToDecimalDouble();
            //i.DateDraw = row["DRW_TRN_DT"].Str();
            o.EventDescription = data["EVNT_TTLE"].Str();
            o.FightDescription =  data["FGHT_DESC"].Str();
            o.LiveStreamUrl =  data["FGHT_STRM_URL"].Str();
            o.DefaultStreamUrl = data["STRM_DEFLT_URL"].Str(); 
            o.ToStartTimer = data["FGHT_TMR_BS"].Str(); 
            string streamtype = data["STRM_TYP"].Str().Trim();
            if(!streamtype.IsEmpty()){
                if(streamtype.Equals("1")){
                    string streamUrl = data["FGHT_STRM_URL"].Str().Trim();
                    if(!streamUrl.IsEmpty()) o.PlayedStreamUrl = streamUrl;
                }else if(streamtype.Equals("2")){
                    string streamUrl = data["STRM_ADS_URL"].Str().Trim();
                    if(!streamUrl.IsEmpty()) o.PlayedStreamUrl = streamUrl;
                }else{
                    string streamUrl = data["STRM_DEFLT_URL"].Str().Trim();
                    if(!streamUrl.IsEmpty()) o.PlayedStreamUrl = streamUrl;
                }
            }

            //
            o.TotalBetMeron = data["LFT_AMT"].Str().ToDecimalDouble();
            o.TotalBetWala= data["RGHT_AMT"].Str().ToDecimalDouble();
            o.PayoutPercentageBetMeron = AggrUtils.Number.Floor(data["LFT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble()); 
            o.PayoutPercentageBetWala = AggrUtils.Number.Floor(data["RGHT_WNNG_PRCNTG_AMT"].Str().ToDecimalDouble()); 
            o.TotalPayout= data["PY_OUT"].Str().ToDecimalDouble();
            o.TotalProfit= data["PRFT"].Str().ToDecimalDouble();

            string status = data["GM_STAT"].Str().Trim();
            //
            o.IsQueueing = status.Equals("5");
            o.IsOpened = status.Equals("4");
            o.IsLastCalled = status.Equals("8");
            o.IsClosed = status.Equals("9");
            //--
            o.IsMeronWin = status.Equals("1");
            o.IsWalaWin = status.Equals("2");
            o.IsWin = (o.IsWalaWin||o.IsMeronWin);
            o.IsDraw = status.Equals("3");
            o.IsCancelled = status.Equals("0");
            o.IsReturned = (o.IsDraw||o.IsCancelled);
            o.IsHold = data["S_HLD"].Str().Equals("1"); 
            //
            //i.IsStarted = (i.IsOpened||i.IsLastCalled) && !i.IsClosed;
            o.IsActive = data["ACTV_FGHT"].Str().Equals("1"); //
            o.IsOngoing = (o.IsActive||o.IsOpened||o.IsLastCalled||o.IsClosed);
            o.IsDone = (o.IsWin||o.IsReturned);
            
            DateTime drawDT = data["DRW_TRN_DATE"].To<DateTime>();
            o.DateDraw = drawDT.ToString("yyyy-MM-dd");
            o.DateSchedule = drawDT.ToString("MMMM dd, yyyy");
            DateTime transDT = data["RGS_TRN_TS"].To<DateTime>();
            o.DateCreated = transDT.ToString("yyyy-MM-dd hh:mm:ss tt");
            o.TimeCreated = transDT.ToString("hh:mm:ss tt");

            o.Status = "Queueing";
            if(o.IsActive && o.IsQueueing) o.Status = "Active Fight";
            else if(o.IsCancelled) o.Status = "Cancelled";
            else if(o.IsDraw) o.Status = "Draw";
            else if(o.IsMeronWin) o.Status = "Meron Win";
            else if(o.IsWalaWin) o.Status = "Wala Win";
            else if(o.IsHold) o.Status = "HOLD";
            else if(o.IsOpened) o.Status = "OPEN";
            else if(o.IsLastCalled) o.Status = "LAST CALL";
            else if(o.IsClosed) o.Status = "CLOSE|Waiting Result";

            string leftname = data["LFT_NM"].Str();
            string rightname = data["RGHT_NM"].Str();
            if(!leftname.IsEmpty() && !rightname.IsEmpty()){
                o.LeftName = leftname;
                o.RightName = rightname;
                o.LeftWeight = data["LFT_WGHT"].Str();
                o.RightWeight = data["RGHT_WGHT"].Str();
            }

            if(o.IsOngoing){
                string toStartTimer = data["FGHT_TMR"].Str(); 
                var timerTs = data["FGHT_TMR_STRT"].To<DateTime?>();
                string stat = data["FGHT_TMR_STAT"].Str().Trim();
                if(timerTs!=null && !toStartTimer.IsEmpty() && stat.Equals("1")){
                    var serverTs = data["CURT_TS"].To<DateTime>();
                    string[] split = toStartTimer.Split(':');
                    double dMs = 16.66667;
                    int min = (int)AggrUtils.Text.Get(split, 0).ToDecimalDouble();
                    int sec = (int)AggrUtils.Text.Get(split, 1).ToDecimalDouble();
                    int msec = (int)AggrUtils.Text.Get(split, 2).ToDecimalDouble();
                    var endTimerTs = (timerTs??serverTs).AddMinutes(min).AddSeconds(sec).AddMilliseconds(msec*dMs);
                    o.IsTimerRunning = true;
                    if(serverTs<endTimerTs){
                        TimeSpan ts = endTimerTs.Subtract(serverTs);
                        int milliseconds = (int)ts.TotalMilliseconds;
                        int seconds = (milliseconds / 1000);
                        milliseconds = (int)((milliseconds % 1000) / dMs);
                        int totalMilliseconds = (seconds * 60) + (int)(milliseconds/dMs);
                        int minutes = (seconds/60);
                        seconds = (seconds % 60);
                        //--
                        o.RunningTimer = totalMilliseconds;
                        o.RunningTimerInFormat = $"{ minutes.Str().PadLeft(2, '0') }:{ seconds.Str().PadLeft(2, '0') }:{ milliseconds.Str().PadLeft(2, '0') }";
                    }else{
                        o.RunningTimer = 0;
                        o.RunningTimerInFormat = "00:00:00";
                    }
                }
            }
            return o;
        }

    
    }

}