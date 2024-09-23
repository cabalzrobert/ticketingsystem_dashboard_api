using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Comm.Commons.Extensions;

namespace webapi.App.Aggregates.Common.Dto
{
    public class TickectingSubscriberDto
    {
        public static IDictionary<string, object> GetSubscriberList(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            o.PL_ID = data["PL_ID"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetDepartmentList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetDepartment_ist(data);
            return items;
        }
        public static IEnumerable<dynamic> GetDepartment_ist(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_Department_ist(e));
        }
        public static IDictionary<string, object> Get_Department_ist(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.DepartmentID = data["DEPTID"].Str();
            o.DepartmentName = data["DEPT_DESCR"].Str();
            o.DepartmentHead = data["DepartmentHead"].Str();
            o.DepartmentHeadID = data["USR_ID"].Str();
            o.NoOfStaff = data["NoOfStaff"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetCategoryList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetCategory_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetCategory_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_Category_List(e));
        }
        public static IDictionary<string, object> Get_Category_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.DepartmentID = data["DEPTID"].Str();
            o.CategoryID = data["CATID"].Str();
            o.Categoryname = data["CAT_DESCR"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetPositionList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetPosition_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetPosition_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_Position_List(e));
        }
        public static IDictionary<string, object> Get_Position_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.PositionID = data["POSID"].Str();
            o.Positionname = data["POS_DESCR"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetReportList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetReport_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetReport_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_Report_List(e));
        }
        public static IDictionary<string, object> Get_Report_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Department = data["DEPT_DESCR"].Str();
            o.DepartmentID = data["DEPT_ID"].Str();
            o.Pending = data["Pending"].Str();
            o.Resolve = data["Resolve"].Str();
            o.Cancell = data["Cancell"].Str();
            o.Ongoing = data["Ongoing"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetTicketElapsedReportList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetTicketElapsedReport_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetTicketElapsedReport_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_TicketElapsedReport_List(e));
        }
        public static IDictionary<string, object> Get_TicketElapsedReport_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Row_Num = (data["Row_Num"].Str() == "") ? 0 : Convert.ToInt32(data["Row_Num"].Str());
            o.USR_ID = data["USR_ID"].Str();
            o.RequestorName = data["Requestor"].Str();
            o.PL_ID = data["PL_ID"].Str();
            o.PGRP_ID = data["PGRP_ID"].Str();
            o.TransactionNo = data["TRN_NO"].Str();
            o.TicketNo = data["TCKT_NO"].Str();
            o.TitleTicket = data["SBJCT"].Str();
            o.CreatedDate = Convert.ToDateTime(data["RGS_TRN_TS"].Str()).ToString("MMM dd, yyyy hh:mm tt");
            o.ElapsedTime = data["ElapsedTime"].Str();
            o.Status = data["Status"].Str();
            o.TicketRequestEvent = data["TicketRequestEvent"].Str();
            o.ViewEvent = true;
            return o;
        }

        public static IEnumerable<dynamic> GetRolesList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetRoles_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetRoles_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_Roles_List(e));
        }
        public static IDictionary<string, object> Get_Roles_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.RolesID = data["ROLEID"].Str();
            o.Rolesname = data["ROLE_DESCR"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetUserAccessList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetUserAccess_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetUserAccess_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_UserAccess_List(e));
        }
        public static IDictionary<string, object> Get_UserAccess_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.UserAccount = data["FLL_NM"].Str();
            o.AccountTypeName = data["AccountTypeName"].Str();
            o.DepartmentName = data["DEPT_DESCR"].Str();
            o.USR_ID = data["USR_ID"].Str();
            o.MenuTab = data["PGS"].Str();
            o.ViewMenu = (data["PGS"].Str() == "") ? false : true;
            return o;
        }

        public static IEnumerable<dynamic> GetUsserAccountList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetUsserAccount_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetUsserAccount_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_UsserAccount_List(e));
        }
        public static IDictionary<string, object> Get_UsserAccount_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.GroupID = data["PGRP_ID"].Str();
            o.UserAccountID = data["USR_ID"].Str();
            o.AccountID = data["ACT_ID"].Str();
            o.ProfilePicture = data["PRF_PIC"].Str();
            o.ImageURL = data["IMG_URL"].Str();
            o.Firstname = data["FRST_NM"].Str();
            o.Middlename = data["MDL_NM"].Str();
            o.Lastname = data["LST_NM"].Str();
            o.Name = data["FLL_NM"].Str();
            o.Birthdate = data["BRT_DT"].Str();
            o.MobileNumber = data["MOB_NO"].Str();
            o.Email = data["EML_ADD"].Str();
            o.DepartmentID = data["DEPT_ID"].Str();
            o.Department = data["DEPT_DESCR"].Str();
            o.RolesID = data["ROLES_ID"].Str();
            o.Role = data["ROLE_DESCR"].Str();
            o.PositionID = data["POS_ID"].Str();
            o.Position = data["POS_DESCR"].Str();
            o.Gender = data["GNDR"].Str();
            o.Gendername = data["GNDR_NM"].Str();
            o.HomeAddress = data["HM_ADDR"].Str();
            o.Address = data["PRSNT_ADDR"].Str();
            o.LastSeen = data["LST_LOG_IN"].Str();
            o.isCommunicator = data["isCommunicator"].Str();
            o.isDepartmentHead = data["isDeptHead"].Str();
            o.AccountType = data["AccountType"].Str();
            o.AccountTypeName = data["AccountTypeName"].Str();
            o.RegisteredDate = Convert.ToDateTime(data["RGS_TRN_TS"].Str()).ToString("MMM dd, yyyy hh:mm tt");
            return o;
        }

        public static IEnumerable<dynamic> GetRequestTicketList(IEnumerable<dynamic> data, int limit = 100, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetRequestTicket_List(data);
            var count = items.Count();
            if (count >= limit)
            {
                var o = items.Last();
                var filter = (o.NextFilter = Dynamic.Object);
                items = items.Take(count - 1).Concat(new[] { o });
                filter.NextFilter = o.Num_Row;
            }
            return items;
        }
        public static IEnumerable<dynamic> GetRequestTicket_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_RequestTicket_List(e));
        }
        public static IDictionary<string, object> Get_RequestTicket_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.Branch_ID = data["PGRP_ID"].Str();
            o.Requestorname = data["FLL_NM"].Str();
            o.RequestorEmail = data["EML_ADD"].Str();
            o.RequestorMobileNumber = data["MOB_NO"].Str();
            o.RequestorProfPic = data["IMG_URL"].Str();
            o.TransactionNo = data["TRN_NO"].Str();
            o.TicketNo = data["TCKT_NO"].Str();
            o.TitleTicket = data["SBJCT"].Str();
            o.TicketDescription = data["BODY"].Str();
            o.Status = data["STAT"].Str();
            o.Statusname = data["STAT_NM"].Str();
            o.TicketStatus = data["TCKT_STAT"].Str();
            o.TicketStatusname = data["TCKT_STAT_NM"].Str();
            o.Category = data["CATEGORY"].Str();
            o.Categoryname = data["CAT_DESCR"].Str();
            o.PriorityLevel = data["PRIORITY_LVL"].Str();
            o.PriorityLevelname = data["PRIORITY_LVL_NM"].Str();
            o.CreatedDate = (data["RGS_TRN_TS"].Str() == "") ? "" : Convert.ToDateTime(data["RGS_TRN_TS"].Str()).ToString("dd MMM yyyy hh:mm");
            o.AssignedAccount = data["AssignedAccount"].Str();
            o.AssignedAccountname = data["AssignedAccountname"].Str();
            o.AssignedAccountEmail = data["AssignedAccountEmail"].Str();
            o.AssignedAccountProfilePicture = data["AssignedAccountProfilePicture"].Str();
            //o.isAssigned = textInfo.ToLower(data["isAssigned"].Str());
            o.isAssigned = Convert.ToBoolean(data["isAssigned"]);
            o.Attachment = data["ATTCHMNT"].Str();
            o.Department = data["DepartmentID"].Str();
            o.DepartmentName = data["DepartmentName"].Str();
            o.ElapsedTime = data["elapsedTime"].Str();
            o.isRead = Convert.ToBoolean(data["S_OPN"]);
            return o;
        }

        public static IDictionary<string, object> LoadCountComment(IDictionary<string, object> data, bool fullinfo = true)
        {
            dynamic o = Dynamic.Object;
            o.Pending = data["Pending"].Str();
            o.Resolve = data["Resolve"].Str();
            o.AllTicketCount = data["AllTicket"].Str();
            return o;
        }

        public static IDictionary<string, object> LoadCountDepartmentHeadAssignedTicket(IDictionary<string, object> data, bool fullinfo = true)
        {
            dynamic o = Dynamic.Object;
            o.Unassigned = data["Unassigned"].Str();
            o.Assigned = data["Assigned"].Str();
            o.UnsolvedTickets = data["UnsolvedTickets"].Str();
            return o;
        }

        public static IDictionary<string, object> LoadCountTicketCommunicator(IDictionary<string, object> data, bool fullinfo = true)
        {
            dynamic o = Dynamic.Object;
            o.UnAssigned = data["UN_ASSIGNED"].Str();
            o.Assigned = data["ASSIGNED"].Str();
            o.Ressolved = data["RESSOLVED"].Str();
            o.AllTicketCount = data["ALL_TICKET"].Str();
            o.UnsolvedTickets = data["TOTAL_UNSOLVED"].Str();
            return o;
        }

        public static IEnumerable<dynamic> GetTicketCommentList(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            var items = GetTicketComment_List(data);
            return items;
        }
        public static IEnumerable<dynamic> GetTicketComment_List(IEnumerable<dynamic> data, bool fullinfo = true)
        {
            if (data == null) return null;
            return data.Select(e => Get_TicketComment_List(e));
        }
        public static IDictionary<string, object> Get_TicketComment_List(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            //o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.Branch_ID = data["PGRP_ID"].Str();
            o.CommentID = data["COMMNT_ID"].Str();
            o.TransactionNo = data["TRN_NO"].Str();
            o.SenderID = data["SNDR_ID"].Str();
            o.DisplayName = data["DSPLY_NM"].Str();
            o.Department = data["DEPT_DESCR"].Str();
            o.ProfilePicture = data["PROF_IMG_URL"].Str();
            o.Message = data["MSG"].Str();
            o.isImage = (data["S_IMG"].Str() == "") ? false : Convert.ToBoolean(data["S_IMG"].Str());
            o.isFile = (data["S_FILE"].Str() == "") ? false : Convert.ToBoolean(data["S_FILE"].Str());
            o.CommentDate = (data["RGS_TRN_TS"].Str() == "") ? "" : Convert.ToDateTime(data["RGS_TRN_TS"].Str()).ToString("MMM dd yyy hh:mm tt");
            o.isRead = (data["isRead"].Str() == "") ? false : Convert.ToBoolean(data["isRead"].Str());
            o.IsYou = (data["isYou"].Str() == "") ? false : Convert.ToBoolean(data["isYou"].Str());
            o.ImageAttachment = data["IMG_Atachment"].Str();
            o.FileAttachment = data["FILE_Atachment"].Str();
            o.isMessage = (data["isMessage"].Str() == "") ? false : Convert.ToBoolean(data["isMessage"].Str());
            return o;
        }

        public static IDictionary<string, object> SendCommentNotification(IDictionary<string, object> data)
        {
            dynamic o = Dynamic.Object;
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            //o.Num_Row = (data["Num_Row"].Str() == "") ? 0 : Convert.ToInt32(data["Num_Row"].Str());
            o.Company_ID = data["PL_ID"].Str();
            o.Branch_ID = data["PGRP_ID"].Str();
            o.CommentID = data["COMMNT_ID"].Str();
            o.TransactionNo = data["TRN_NO"].Str();
            o.SenderID = data["SNDR_ID"].Str();
            o.DisplayName = data["DSPLY_NM"].Str();
            o.Department = data["DEPT_DESCR"].Str();
            o.ProfilePicture = data["PROF_IMG_URL"].Str();
            o.Message = data["MSG"].Str();
            o.isImage = (data["S_IMG"].Str() == "") ? false : Convert.ToBoolean(data["S_IMG"].Str());
            o.isFile = (data["S_FILE"].Str() == "") ? false : Convert.ToBoolean(data["S_FILE"].Str());
            o.CommentDate = (data["RGS_TRN_TS"].Str() == "") ? "" : Convert.ToDateTime(data["RGS_TRN_TS"].Str()).ToString("MMM dd yyy hh:mm tt");
            o.isRead = (data["isRead"].Str() == "") ? false : Convert.ToBoolean(data["isRead"].Str());
            o.IsYou = (data["isYou"].Str() == "") ? false : Convert.ToBoolean(data["isYou"].Str());
            o.ImageAttachment = data["IMG_Atachment"].Str();
            o.FileAttachment = data["FILE_Atachment"].Str();
            o.isMessage = (data["isMessage"].Str() == "") ? false : Convert.ToBoolean(data["isMessage"].Str());
            return o;
        }


    }
}
