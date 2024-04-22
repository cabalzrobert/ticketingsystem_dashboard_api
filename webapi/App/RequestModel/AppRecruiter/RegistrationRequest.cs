using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace webapi.App.RequestModel.AppRecruiter
{
    public class RegistrationRequest
    {
        public string RegisterID;
        public string Type;
        public string Role;
        public string GeneralCoordinator;
        public string Coordinator;

        public string Firstname;
        public string Middlename;
        public string Lastname;
        public string Fullname;

        public string BirthDate;
        public string Gender;
        public string BloodType;
        public string Nationality;
        public string Citizenship;
        public string MaritalStatus;

        public string MobileNumber;
        public string EmailAddress;
        public string Address;
        public string Occupation;
        public string Skills;

        public string Region;
        public string Province;
        public string Municipality;
        public string Barangay;

        //
        public string Img;
        public string ImageUrl;
    }
    public class STLMembership
    {
        public string PLID;
        public string PGRPID;
        public string Userid;
        public string Acctid;

        public string TitleName;
        public string Firstname;
        public string Lastname;
        public string Middlename;
        public string Nickname;
        public string ExtensionName;
        public string Religion;

        public string BirthDate;
        public string PlaceOfBirth;
        public string Gender;
        public string BloodType;
        public string Nationality;
        public string Citizenship;
        public string MaritalStatus;
        public string PartnerID;

        public string MobileNumber;
        public string EmailAddress;
        public string PrecentNumber;
        public string ClusterNumber;
        public string SequnceNumber;
        public string HomeAddress;
        public string PresentAddress;
        public string Profession;
        public string Occupation;
        public string Skills;
        public string Height;
        public string Weight;
        public string LivingWParent;


        public string HouseNo;
        public string HouseholdNo;
        public string Family;

        public string Region;
        public string Province;
        public string Municipality;
        public string Barangay;
        public string Sitio;
        public string LocationSite;

        public int Leadertype;
        public string AccountType;
        public string Username;
        public string Userpassword;


        public int isProfilePictureChange;
        public string Img;
        public string Signature;
        public string ImageUrl;
        public int isSignatureChange;
        public string SignatureURL;
        //public string psncd;

        public string RefGroupID;
        public string RefLDRID;
        public int isEmployed;
        public int WDisability;
        public string TypeDisability;
        public int RegisterVoter;
        public int WChildren;
        public int ParentResideBrgy;

        //Parent
        public string FR_ID;
        public string MO_ID;
        public string FrFirstName;
        public string FrMiddleInitial;
        public string FrLastname;
        public string FrFullName;
        public string FrContactNo;
        public string FrEmail;
        public string FrAddress;

        public string MoFirstname;
        public string MoMiddleInitial;
        public string MoLastname;
        public string MoFullName;
        public string MoContactNo;
        public string MoEmail;
        public string MoAddress;

        public string MonthlyIncome;
        public int PermanentResidence;
        public int SeniorCitizenMember;
        public int SingleParent;
        public int IndigentFamily;
        public string ResidentType;
        public string DateReside;
        public string CompleteAddress;
        public int SubscriberType;

        public string iChildren;
        public string iValidGovernmentID;
        public string iOrganization;
        public string iEducationalAttainment;
        public string iEmployement;
        public string Json;
        public int Verified;
        public List<FingerImage> ImageList;
        public List<GovAttachment> GovIDList;
        public string ExportedDocument;
        public string URLDocument;
        public class FingerImage
        {
            public int Id;
            public string Index;
            public string Image;
        }
        public class GovAttachment
        {
            public int id;
            public string govvalid;
            public string govvalid_no;
            public string attachment;
            public string base64stringattachment;
            public string newupload;
        }
    }
    public class DateRangeModel
    {
        public string DateFrom { get; set; }
        public string DateTo { get; set; }
    }
    public class ValidityAccount : DateRangeModel
    {
        public string iUSR_ID { get; set; }
    }
    public class ResetGroupPassword
    {
        public string PL_ID;
        public string PGRP_ID;
        public string isAgent;
        public string NewPassword;
        public string ACT_ID;
        public string USR_ID;

    }
    public class ResetPassword
    {
        public string PL_ID;
        public string PGRP_ID;
        public string MobileNumber;
        public string ACT_ID;
        public string USR_ID;
    }
    public class TransferMember
    {
        public string PL_ID;
        public string PGRP_ID;
        public string Ref_Group_ID;
        public string Ref_LDR_ID;
        public string USR_ID;
        public string ACT_ID;
    }
    public class BlockAccount
    {
        public string PL_ID;
        public string PGRP_ID;
        public string ACT_ID;
    }
    public class ChangeAccountType
    {
        public string PL_ID;
        public string PGRP_ID;
        public string USR_ID;
        public string USR_NM;
        public int AccountType;
    }
    public class TotalRegisterVoter
    {
        public string PLID;
        public string PGRPID;
        public string Reg;
        public string Prov;
        public string Mun;
        public string Brgy;
        public string BrgyProfID;
        public string TotalVoter;
    }
    public class APK
    {
        public string PL_ID;
        public string PGRP_ID;
        public string TRNNo;
        public string APKVerno;
        public string APKName;
        public string APKPath;
        public string APKPathCBA;
        public byte[] APK_File;
    }
    public class GroupLeader
    {
        public string plid;
        public string pgrpid;
        public string usrid;
    }
    public class MasterListRequest
    {
        public string num_row;
        public string search;
        public string reg;
        public string prov;
        public string mun;
        public string brgy;
        public string sitio;
        public string verified;
        public string userid;
    }
    public class LicenseFilterRequest
    {
        public string num_row;
        public string search;
        public string reg;
        public string prov;
        public string mun;
        public string brgy;
    }
    public class BarangayList
    {
        public string num_row;
        public string islandcode;
        public string reg;
        public string prov;
        public string mun;
        public string brgy;
    }
    public class BRGYProfileRequest
    {
        public string plid;
        public string pgrpid;
        public string num_row;
        public string reg;
        public string prov;
        public string mun;
        public string brgy;
    }
    public class BrgySignatory
    {
        public string Brgy_Captain;
        public string Brgy_Captain_sig;
        public string Brgy_Captain_sig_URL;
        public string Capt_Events;
        public string Brgy_FirstCouncilor;
        public string Brgy_FirstCouncilor_sig;
        public string Brgy_FirstCouncilor_sig_URL;
        public string First_Events;
        public string Brgy_SecondCouncilor;
        public string Brgy_SecondCouncilor_sig;
        public string Brgy_SecondCouncilor_sig_URL;
        public string Second_Events;
        public string Brgy_ThirdCouncilor;
        public string Brgy_ThirdCouncilor_sig;
        public string Brgy_ThirdCouncilor_sig_URL;
        public string Third_Events;
        public string Brgy_FourthCouncilor;
        public string Brgy_FourthCouncilor_sig;
        public string Brgy_FourthCouncilor_sig_URL;
        public string Fourth_Events;
        public string Brgy_FifthCouncilor;
        public string Brgy_FifthCouncilor_sig;
        public string Brgy_FifthCouncilor_sig_URL;
        public string Fifth_Events;
        public string Brgy_SixthCouncilor;
        public string Brgy_SixthCouncilor_sig;
        public string Brgy_SixthCouncilor_sig_URL;
        public string Sixth_Events;
        public string Brgy_SeventhCouncilor;
        public string Brgy_SeventhCouncilor_sig;
        public string Brgy_SeventhCouncilor_sig_URL;
        public string Seventh_Events;
        public string SK_Chairman;
        public string SK_Chairman_sig;
        public string SK_Chairman_sig_URL;
        public string SKChairman_Events;
        public string Brgy_Secretary;
        public string Brgy_Secretary_sig;
        public string Brgy_Secretary_sig_URL;
        public string Secretary_Events;
        public string Brgy_Treasurer;
        public string Brgy_Treasurer_sig;
        public string Brgy_Treasurer_sig_URL;
        public string Treasurer_Events;
        public string Brgy_Chief_Tanod;
        public string Brgy_Chief_Tanod_sig;
        public string Brgy_Chief_Tanod_sig_URL;
        public string Tanod_Events;
    }
    public class FilterRequestDoc
    {
        public string plid;
        public string pgrpid;
        public string userid;
        public string reqdoc;
        public string docid;
        public string id;
        public string controlno;
        public string num_row;
        public string search;
        public string status;
        public string from;
        public string to;
        public Nullable<int> cancelled;
        public Nullable<int> release;
    }

    public class BarangaySignatures
    {
        public List<Signatory> signatories = new List<Signatory>();
        public string isignatories;
        public class Signatory
        {
            public string BRGY_POSITION;
            public string ELECTED_OFFICIAL;
            public string ELECTED_OFFICIAL_ID;
            public string COMMITTEE;
            public string SIGNATURE_URL;
            public string base64stringattachment;
            public string NEW_UPLOAD;
        }
    }
    public class BarangaySignatory
    {
        public string ColName;
        public string ColSignature;
    }
    public class BusinesseRequest
    {
        public string BussinessID;
        public string ControlNo;
        public string CTCNo;
        public string InitialCapital;
        public string RegisteredNo;
        public string NatureofBusiness;
        public string BusinessName;
        public string Email;
        public string ContactNo;
        public string BusinessAddress;
        public string DateOperate;
        public string BusinessOwnershipTypeID;
        public string FirstName;
        public string MiddleInitial;
        public string LastName;
        public string FullName;
        public string OwnerAddress;
        public string OwnerEmail;
        public string OwnerContactNo;
        public string OwnerID;
        public string BizStatus;

    }

    public class Emergency_Type 
    {
        public string PL_ID;
        public string PGRP_ID;
        public string EmgyTypID;
        public string EmgyType;
        public string Message;
    }
    public class Emergency_Contact_Person
    {
        public string PL_ID;
        public string PGRP_ID;
        public string EmgyTypID;
        public string EmgyContactID;
        public string ContactPerson;
        public string MobileNumber;
    }
    public class EmergencyAlert
    {
        public string PL_ID;
        public string PGRP_ID;
        public string EmgyID;
        public string EmgyTypID;
        public string UserID;
        public string FUllName;
        public string GeolocationLat;
        public string GeolocatonLong;
        public string Closed_Details;
        public string ClosedDate;
        public int Closed_Type;
        public string Closed_TypeName;
    }

    public class LicenseKey
    {
        public string plid;
        public string pgrpid;
        public string licensekey;
        public string licenseval;
        public string registereddevice;
        public string licenseexpiry;
        public string mcaddress;
        public string licmod;
        public string location;
        public string userid;
        public string localno;
    }
    public class LicenseKeyAvailable
    {
        public string plid;
        public string pgrpid;
        public string licensekey;
        public string licenseval;
        public string registereddevice;
        public string licenseexpiry;
        public string licmod;
        public string licnum;
        public string productid;
        public string mcaddress;

    }
    public class LicenseKeyCertificate
    {
        public string certificatecontent;
        public string UserAccount;

    }
    public class Generate_License_Key
    {
        public string ID;
        public string PL_ID;
        public string PGRP_ID;
        public string ProductID;
        public string LicenseType;
        public string Expiry_Days;
        public string LicenseKey;
        public int Lic_Mod;
        public string Location;
        public string UserAccount;
        public string Date_Register;
        public string Date_Generated;
        public string Extension;
        public string prevExtension;
        public string licenserenew;
    }
    public class OfficialHeader
    {
        public string iBrgyOfficialLogo;
        public string iMunicipalLogo;
        public int BrgyLogoChange;
        public string BrgyOfficialLogo;
        public int MunLogoChange;
        public string MunicipalLogo;
        public string ContactNo;
        public string Country;
        public string Province;
        public string Municipality;
        public string Barangay;
        public string IssuedLocation;
        public int DefaultValidity;
        public int MonthValidity;
    }

    public class Organization
    {
        public string OrganizationID;
        public string OrganizationNM;
        public string OrganizatioAbbr;
        public string Estabalished;
    }
    public class Establishment
    {
        public string PL_ID;
        public string PGRP_ID;
        public string Est_ID;
        public string Est_Name;
        public string Est_Type;
        public string ContactDetails;
        public string Address;
        public string EmailAddress;
        public string CompanyLogo;
        public string Company_Logo;
        public string EstablishmentLocation;
    }
    public class Establishment_Request
    {
        public string num_row;
        public string search;
        public string type;
    }

    public class LegalDocument
    {
        public string Tagline;
        public string FormTabID;
        public string FormTabDescription;
    }
    public class LegalDocument_Transaction
    {
        public string PLID;
        public string PGRPID;
        public string LegalFormsID;
        public string LegalFormssControlNo;
        public string TypeofTemplateID;
        public string TypeofTemplateNM;
        public string TemplateID;
        public string TemplateNM;
        public string DeathCertificateID;
        public string ORNumber;
        public string AmountPaid;

        public string UserID;
        public string Requestor;
        public string URLProfPic;
        public string Taken_ProfPic;
        public string ProfilePic;
        public string Gender;
        public string GenderNM;
        public string MaritalStatus;
        public string MaritalStatusNM;
        public string ResidentAddress;
        public string BirthDate;
        public string BirthPlace;
        public string ContactNumber;
        public int Age;

        public string IssuedDate;
        public string ExpiryDate;
        public string GivenDay;
        public string GivenMonth;
        public string GivenYear;
        public string Barangay;
        public int MosValidity;
        public string itagline;
        public string Search;
        public string DateFrom;
        public string DateTo;
        public string Status;
        public string AppointmentDate;
        public string VerifiedBy;
        public string CertifiedBy;
        public string ExportedDocument;
        public string URLDocument;
    }

    public class GovernmentValidID
    {
        public string ID;
        public string GovernmentID;
    }
    public class TemplateType
    {
        public string PLID;
        public string PGRPID;
        public string TemplateTypeID;
        public string Description;
    }
    public class FormTemplate
    {
        public string PLID;
        public string PGRPID;
        public string FormID;
        public string BarangayCode;
        public string FormContent;
        public string FormName;
        public string iTagline;
    }
    public class ResidentType
    {
        public string RestTypId;
        public string Description;
    }
    public class TemplateDocument
    {
        public string TemplateTypeID;
        public string TemplateDocID;
        public int TemplateMod;
        public string Description;
        public string TemplateDocContent;
        public string iTagline;
    }
    public class ItemDescription
    {
        public string DescriptionID;
        public string TPLID;
        public string TP_ItemDescription;
        public string Item_Description;
    }

    public class SystemUser
    {
        public string plid { get; set; }
        public string pgrpid { get; set; }
        public string userid { get; set; }

        public string username { get; set; }
        public int subscribertype { get; set; }
        public int accounttype { get; set; }

        public string profileid { get; set; }
        public string profilename { get; set; }

        public string mobno { get; set; }

        public string usercreator { get; set; }
    }

    public class SelectUser
    {
        public string plid { get; set; }
        public string pgrpid { get; set; }

        public int subtype { get; set; }

        public string locmun { get; set; }
        public int? locprov { get; set; }
        public string locreg { get; set; }
    }

    public class AccessProfile
    {
        public string plid { get; set; }
        public string pgrpid { get; set; }
        public string profileid { get; set; }

        public string profilename { get; set; }
        public string creatorid { get; set; }

        public string accessstring_superadmin { get; set; }
        public string accessstring_admin { get; set; }
        public string accessstring_operation { get; set; }
        public string accessstring_reporting { get; set; }
        public string accessstring_appearance { get; set; }
        public string accessstring_configuration { get; set; }

        public int usercreation { get; set; }
        public int usermodification { get; set; }
        public int userremoval { get; set; }
        public int profileaccess { get; set; }
}

}