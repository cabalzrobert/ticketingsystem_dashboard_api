using System;

namespace webapi.App.Model.User
{
    public class Recruiter
    {
        public String CompanyID;
        public String BranchID;
        public String UserID;
        public string Fullname;
        public string DeviceID;
        public string DeviceName;
        public string MacAddress;
    }

    public class STLAccount
    {
        public String PL_ID;
        public String PGRP_ID;
        //public String PSN_CD;
        public String USR_ID;
        public String ACT_ID;
        public string ACT_TYP;

        public string FRST_NM;
        public string LST_NM;
        public string MDL_NM;
        public string FLL_NM;
        public string NCK_NM;

        public string MOB_NO;
        public string EML_ADD;
        public string HM_ADDR;
        public string PRSNT_ADDR;
        public string LOC_REG;
        public string LOC_PROV;
        public string LOC_MUN;
        public string LOC_BRGY;
        public string LOC_BRGY_NM;

        public string GNDR;
        public string MRTL_STAT;
        public string CTZNSHP;
        public string ImageUrl;
        public string BRT_DT;
        public string BLD_TYP;
        public string NATNLTY;
        public string OCCPTN;
        public string SKLLS;
        public string PRF_PIC;
        public string SIGNATUREID;
        public string SKIN;
        public string S_ACTV;
        public string SUB_TYP;

        public String SessionID;
        public bool sActive;
        public bool IsSessionExpired;
        public bool IsLogin;

        public string PROFILE_ID;
        public string CAN_CREATE;
        public string CAN_UPDATE;
        public string CAN_DELETE;
        public string PROFILE_ACCESS;
        public string SUBSCRIBER_ID;
        public string RQRD_CHNG_PSSWRD;
        public string TRGR_CHNG_PSSWRD;
    }

    public class TicketingUser
    {
        public String PL_ID;
        public String PGRP_ID;
        public String USR_ID;
        public String ACT_ID;
        public String ACT_TYP;
        public String DEPT_ID;
        public string Department;

        public string FRST_NM;
        public string LST_NM;
        public string MDL_NM;
        public string FLL_NM;
        public string NCK_NM;

        public string MOB_NO;
        public string EML_ADD;
        public string HM_ADDR;
        public string PRSNT_ADDR;

        public string GNDR;
        public string MRTL_STAT;
        public string ImageUrl;
        public string BRT_DT;   
        public string OCCPTN;
        public string SKLLS;
        public string PRF_PIC;
        public string SIGNATUREID;
        public string S_ACTV;
        public string SUB_TYP;

        public String SessionID;
        public bool sActive;
        public bool IsSessionExpired;
        public bool IsLogin;
        public bool isCommunicator;
        public bool isDeptartmentHead;

        public string PROFILE_ID;
        public string CAN_CREATE;
        public string CAN_UPDATE;
        public string CAN_DELETE;
        public string SUBSCRIBER_ID;
        public string RQRD_CHNG_PSSWRD;
        public string TRGR_CHNG_PSSWRD;
    }
    
}
