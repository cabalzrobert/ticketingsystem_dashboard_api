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
        public string HeadOfficeEmailAddress;
        //6
        public string MobileNumber;
        public string EmailAddress;
        public string UserID;
    }
    
}

