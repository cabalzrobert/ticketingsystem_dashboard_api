using Org.BouncyCastle.Bcpg;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace webapi.App.TSDashboardModel
{
    public class TSDashboard
    {
    }

    public class HeadOffice
    {
        //Party List Information
        public string parmplid { get; set; }
        public string HeadOfficeName;
        public string parmplnm { get; set; }
        public string parmpladd { get; set; }
        public string parmtelno { get; set; }

        public string parmplemladd { get; set; }

        //Group Information
        public string parmpgrpid { get; set; }
        public string parmpsncd { get; set; }
        public string parmpltclid { get; set; }


    }
    public class AgentHeadOffice : HeadOffice
    {

        public string FirstName;
        public string LastName;
        public string MiddleInitial;
        public string Username;
        public string Password;
        public string HeadOfficeAddress;
        public string HeadOfficeTelephoneNumber;
        public string HeadOfficeEmailAddress;
        //6
        public string MobileNumber;
        public string EmailAddress;
        public string UserID;
    }

    public class TicketInfo
    {
        public string ticketNo;
        public string requestId;
        public string categoryId;
        public string title;
        public string description;
        public string[] attachment;
        public Int16 priorityLevel;
        public string forwardDepartment;
        public string forwardTo;
        public string forwardRemarks;
        public string assignedDepartment;
        public string assignedTo;
        public Int16 status;
        public string commentId;
        public Int16 permission;
    }

    public class FilterTickets
    {
        public int tab;
        public string departmentId;
        public int row;
        public string search;
    }

}

