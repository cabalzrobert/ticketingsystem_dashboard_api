using System;
using System.Collections.Generic;

namespace webapi.App.RequestModel.Feature
{
    public class ReportAProblemRequest
    {
        public String TicketNo;
        public string TransactionNo;
        public String Sitio;
        public String Subject;
        public String Body;
        public String CorrectiveAction;
        public List<String> Attachments;
        public String iAttachments;
        public String SenderAccount;
        public String SenderName;
        public String AddressLocation;
        public String PermanentAddress;
        public String DeviceID;
        public String DeviceName;
        public String Manufacturer;
        public String Serial;
        public String Brand;
        public String DeviceOS;
        public String DeviceVersion;
    }
}