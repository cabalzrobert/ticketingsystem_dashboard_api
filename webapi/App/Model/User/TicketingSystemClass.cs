using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.App.Model.User
{
    public class TicketingSystemClass
    {
    }
    public class DepartmentModel
    {
        public string PL_ID;
        public string PGRP_ID;
        public string DepartmentID;
        public string DepartmentName;
    }
    public class CategoryModel
    {
        public string PL_ID;
        public string PGRP_ID;
        public string CategoryID;
        public string Categoryname;
    }
    public class PositionModel
    {
        public string PL_ID;
        public string PGRP_ID;
        public string PositionID;
        public string Positionname;
    }
    public class RolesModel
    {
        public string PL_ID;
        public string PGRP_ID;
        public string RolesID;
        public string Rolesname;
    }
    public class UserAccountModel
    {
        public string UserAccountID;
        public string AccountID;
        public string DepartmentID;
        public string RolesID;
        public string PositionID;
        public string Firstname;
        public string Middlename;
        public string Lastname;
        public string Name;
        public string Gender;
        public string Birthdate;
        public string MobileNumber;
        public string Address;
        public string ProfilePicture;
        public string IMGURL;
        public int isCommunicator;
        public int isDeptartmentHead;
    }
    public class TicketModel 
    {
        public string TicketNo;
        public string TransactionNo;
        public string Category;
        public string Categoryname;
        public string TitleTicket;
        public string TicketDescription;
        public string PriorityLevel;
        public string iTicketAttachment;
        public List<String> TicketAttachment;
        public string IssuedDate;
        public string Status;
        public string Statusname;
        public string CreatedDate;
        public string TicketStatusname;
        public string TicketStatus;
    }
    public class TicketCommentModel
    {
        public string Company_ID;
        public string Branch_ID;
        public string CommentID;
        public string TransactionNo;
        public string SenderID;
        public string DisplayName;
        public string ProfilePicture;
        public string Message;
        public bool isImage;
        public bool isFile;
        public string CommentDate;
        public bool isRead;
        public bool IsYou;
        public string ImageAttachment;
        public string FileAttachment;
        public bool isMessage;
    }

}
