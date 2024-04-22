using System;
using System.Collections.Generic;
using Comm.Commons.Extensions;
using System.Linq;

namespace webapi.App.Aggregates.Common
{
    public class OperatorDto
    {
        public static IEnumerable<dynamic> GetOperators(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> GetOperator(e));
        }
        public static IDictionary<string, object> GetOperator(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;

            string usertype = data["USR_TYP"].Str().Trim();

            o.OperatorID = $"{ data["COMP_ID"].Str() }{ data["CUST_ID"].Str() }"; 
            o.BranchID =  data["BR_CD"].Str();
            //o.UserTypeID = usertype;//((int)usertype.ToDecimalDouble() - 5).ToString();

            o.Fullname =  data["USR_FLLNME"].Str();
            o.Firstname =  data["USR_FRST_NM"].Str();
            o.Lastname =  data["USR_LST_NM"].Str();
            o.DateRegistered =  data["RGS_TRN_TS"]; 
            o.DateRegisteredDisplay =  data["RGS_TRN_TS"].To<DateTime>().ToString("MMMM dd, yyyy hh:mm:ss tt");

            o.PresentAddress =  data["PRSNT_ADD"].Str();
            o.MobileNumber =  data["USR_MOB_NO"].Str();
            o.EmailAddress =  data["EML_ADD"].Str(); 

            string username = data["USR_NM"].Str(); 
            if(username.IsEmpty()) username =  o.OperatorID;
            o.Username =  username;

            string isblock = data["IS_BLCK"].Str().ToLower();
            o.IsBlocked = (isblock.Equals("1") || isblock.Equals("true"));
            o.Blocked =  (o.IsBlocked?"YES":"NO");

            o.Department = ""; o.IsAgent = false;
            if(usertype.Equals("6")) o.Department = "Accounting";
            else if(usertype.Equals("7")) o.Department = "Treasury";
            else if(usertype.Equals("8")) o.Department = "Operations";
            else if(usertype.Equals("9")) o.Department = "Accounts";
            else if(usertype.Equals("11")) o.Department = "Operation Head";
            else if(usertype.Equals("10")) {
                o.Department = "Branch Agent";
                o.IsAgent = true;
            }
            return o;
        }
        public static IDictionary<string, object> GetBranchProfile(IDictionary<string, object> data, bool includeAgent = false){
            dynamic o = Dynamic.Object;
            o.BranchID =  data["BR_CD"].Str();
            o.BranchName =  data["BR_NME"].Str();
            o.BranchAddress =  data["BR_ADDRS"].Str();
            o.LicenseNo =  data["LCNSD_NO"].Str();
            o.TinNo =  data["TIN_NO"].Str();
            o.ShortName =  data["SHRT_NM"].Str();
            o.AreaName =  data["AREA_NM"].Str();
            o.TelephoneNumber =  data["BR_TEL_NO"].Str();
            o.TechSupportNumber =  data["BR_TCHSUPNO"].Str();
            o.EmailAddress =  data["BR_EML"].Str();
            o.WebsiteUrl =  data["BR_WBST"].Str();
            o.ZipCode =  data["BR_LOC_ZP"].Str();
            o.DateRegistered =  data["RGS_TRN_TS"]; 
            // BR_BRGY_CD
            if(includeAgent){
                o.Agent = GetOperator(data); 
            }
            return o;
        }
        public static IDictionary<string, object> GetCompanyProfile(IDictionary<string, object> data, bool fullInfo = true){
            dynamic o = Dynamic.Object;
            o.CompanyID =  data["COMP_ID"].Str();
            o.CompanyName =  data["COMP_NM"].Str();
            o.CompanyAddress =  data["COMP_ADD"].Str();
            if(!fullInfo) return o;

            o.LicenseNo =  data["LCNSD_NO"].Str();
            o.TinNo =  data["TIN_NO"].Str();
            o.ShortName =  data["SHRT_NM"].Str();
            o.AreaName =  data["AREA_NM"].Str();
            o.TelephoneNumber =  data["COMP_TEL_NO"].Str();
            o.TechSupportNumber =  data["TCHSUPNO"].Str();
            o.EmailAddress =  data["EMLADD"].Str();
            o.WebsiteUrl =  data["WBST"].Str();
            //o.ZipCode =  data["BR_LOC_ZP"].Str();
            o.DateRegistered =  data["RGS_TRN_TS"]; 

            o.LiveStreamUrl =  data["URL_STRMNG"].Str();
            o.LiveStreamName =  data["URL_STRMNG_NM"].Str();
            o.YoutubeID =  AggrUtils.YouTube.GetVideoId(o.LiveStreamUrl);
            return o;
        }

        
        public static IDictionary<string, object> GetSubscriber(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.AccountID = data["ACT_ID"].Str();
            o.Firstname = data["USR_FRST_NM"].Str();
            o.Lastname = data["USR_LST_NM"].Str();
            o.Fullname = ($"{o.Firstname} {o.Lastname}").Trim();

            string commissionBalance = data["ACT_COM_BAL"].Str();
            if(!commissionBalance.IsEmpty())
                o.CommissionBalance = commissionBalance.ToDecimalDouble();
                
            return o;
        }  
        public static IEnumerable<dynamic> GetBranches(IEnumerable<dynamic> list){
            if(list==null) return null;
            return list.Select(e=> GetBranch(e));
        }
        public static IDictionary<string, object> GetBranch(IDictionary<string, object> data){
            dynamic o = Dynamic.Object;
            o.BranchID =  data["BR_CD"].Str();
            o.BranchName = data["BR_NME"].Str();
            o.TotalGeneralCoordinator = (int)data["TOT_GEN_COORD"].Str().ToDecimalDouble();
            return o;
        }
        //
        public static IDictionary<string, object> SeparateSubscribers(IEnumerable<dynamic> list){
            if(list==null) return null;
            dynamic types = Dynamic.Object;
            List<object> generalCoordinators = (types.GeneralCoordinators = new List<object>());
            List<object> coordinators = (types.Coordinators = new List<object>());
            List<object> players = (types.Players = new List<object>());

            foreach(IDictionary<string, object> data in list){
                dynamic subscriber = GetSubscriber(data, true);
                List<object> subscribers = (types.Players as List<object>);
                if(subscriber.IsGeneralCoordinator)
                    subscribers = (types.GeneralCoordinators as List<object>);
                else if(subscriber.IsCoordinator)
                    subscribers = (types.Coordinators as List<object>);
                subscribers.Add(subscriber);

                //subscriber.IsTerminal = false;
                //subscriber.SerialNo = "";

                subscriber.UserType = "";
                if(subscriber.IsPlayer) subscriber.UserType = "Player";
                else if(subscriber.IsCoordinator) subscriber.UserType = "Coordinator";
                else if(subscriber.IsGeneralCoordinator) subscriber.UserType = "General Coordinator";
                //
                subscriber.Status = "";
                if(subscriber.IsBlocked) subscriber.Status="BLOCKED";
                else if(subscriber.IsActive) subscriber.Status="ACTIVE";

                if(subscriber.IsGeneralCoordinator) 
                    subscriber.TotalCoordinator = 0;
                else if(subscriber.IsCoordinator) 
                    subscriber.TotalPlayer = 0;
            }
            coordinators.ForEach((dynamic subscriber)=>{
                dynamic genCoord = generalCoordinators.Where((dynamic s)=>s.BaseID==subscriber.GeneralCoordinatorID).SingleOrDefault();
                if(genCoord!=null){
                    subscriber.GeneralCoordinatorID = genCoord.AccountID;
                    subscriber.GeneralCoordinator = genCoord.Fullname;
                    genCoord.TotalCoordinator += 1;
                }
            });
            players.ForEach((dynamic subscriber)=>{
                dynamic coord = coordinators.Where((dynamic s)=>s.BaseID==subscriber.CoordinatorID).SingleOrDefault();
                if(coord!=null){
                    subscriber.CoordinatorID = coord.AccountID;
                    subscriber.Coordinator = coord.Fullname;
                    subscriber.GeneralCoordinatorID = coord.GeneralCoordinatorID;
                    subscriber.GeneralCoordinator = coord.GeneralCoordinator;

                    coord.TotalPlayer += 1;
                }
            });
            return types;
        }
        public static IEnumerable<dynamic> GetSubscribers(IEnumerable<dynamic> list){
            if(list==null) return null;
            dynamic types = SeparateSubscribers(list); list = null;
            List<dynamic> subscribers = new List<dynamic>();
            subscribers.AddRange(types.GeneralCoordinators as IEnumerable<dynamic>);
            subscribers.AddRange(types.Coordinators as IEnumerable<dynamic>);
            subscribers.AddRange(types.Players as IEnumerable<dynamic>);
            return subscribers;
        }

        public static IDictionary<string, object> GetSubscriber(IDictionary<string, object> data, bool includeCoords = false){
            dynamic o = Dynamic.Object;
            
            o.AccountID = data["ACT_ID"].Str();
            o.BaseID = $"{ data["COMP_ID"].Str() }{ data["CUST_ID"].Str() }";
            o.BranchID =  data["BR_CD"].Str();
            //
            o.Firstname = data["USR_FRST_NM"].Str().ToUpper();
            o.Lastname = data["USR_LST_NM"].Str().ToUpper();
            o.Fullname =  $"{ o.Firstname } { o.Lastname }".Trim();
            o.DisplayName =  data["USR_NCK_NM"].Str();
            o.EmailAddress =  data["EML_ADD"].Str();
            o.MobileNumber = data["USR_MOB_NO"].Str();
            o.PresentAddress = data["PRSNT_ADD"].Str();
            o.ImageUrl =  data["IMG_URL"].Str();
            //
            o.CreditBalance = data["ACT_CRDT_BAL"].Str().ToDecimalDouble();
            o.CommissionBalance = data["ACT_COM_BAL"].Str().ToDecimalDouble();
            
            string usertype = data["USR_TYP"].Str();
            o.IsPlayer = (usertype.Equals("5"));
            o.IsCoordinator = (usertype.Equals("4"));
            o.IsGeneralCoordinator = (usertype.Equals("3"));
            if(includeCoords){
                o.GeneralCoordinatorID = data["REF_CUST_ID2"].Str();
                o.CoordinatorID = data["REF_CUST_ID"].Str();
            }else{
                o.ReferenceID =  data["RGS_EMP_ID"].Str();
            }

            string serailNo =  data["SRIAL_ID"].Str();
            o.IsTerminal = !serailNo.IsEmpty();
            o.SerialNo = serailNo;

            string userstatus = data["USR_STAT"].Str().ToLower();
            o.IsActive = (userstatus.Equals("1") || userstatus.Equals("true"));
            string isblock = data["IS_BLCK"].Str().ToLower();
            o.IsBlocked = (isblock.Equals("1") || isblock.Equals("true"));

            if(o.IsCoordinator || o.IsGeneralCoordinator){
                o.SharedCommission = data["COM_RT"].Str().ToDecimalDouble();
            }
            return o;
        }
        
    
    }

}