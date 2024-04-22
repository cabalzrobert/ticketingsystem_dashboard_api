using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

using Comm.Commons.Extensions;
using Infrastructure.Repositories;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.Model.User;
using webapi.App.Globalize.Company;

using Newtonsoft.Json;
using Comm.Commons.Advance;

using webapi.App.RequestModel.Common;
using webapi.App.RequestModel.AppRecruiter;
using webapi.App.Aggregates.AppRecruiterAggregate.Common;

namespace webapi.App.Aggregates.AppRecruiterAggregate.Features
{
    [Service.ITransient(typeof(RegistrationRepository))] 
    public interface IRegistrationRepository
    {
        Task<(Results result, object items)> PendingCustomersAsync(FilterRequest filter);
        Task<(Results result, String message)> RegistrationAsync(RegistrationRequest request, bool isUpdate = false);
    } 

    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly IRecruiter _identity;
        private readonly IRepository _repo;
        public Recruiter account { get{ return _identity.AccountIdentity(); } } 
        public RegistrationRepository(IRecruiter identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }
        public async Task<(Results result, object items)> PendingCustomersAsync(FilterRequest filter){
            var results = _repo.DSpQueryMultiple("dbo.spfn_BCC0C", new Dictionary<string, object>(){
                { "parmcompid", account.CompanyID },
                { "parmbrcd", account.BranchID },
                { "parmrctrid", account.UserID },
                { "parmsrch", filter.Search },
                { "parmfid", filter.BaseFilter },
            });
            if(results != null)
                return (Results.Success, SubscriberDto.SearchRegisters(results.Read<dynamic>(), 25)); 
            return (Results.Null, null); 
        }

        public async Task<(Results result, String message)> RegistrationAsync(RegistrationRequest request, bool isUpdate = false){
            int type = (int)request.Type.ToString().ToDecimalDouble();
            if(type==1) return await this.ManagerRegistrationAsync(request, isUpdate);
            else if(type==2) return await this.SupervisorRegistrationAsync(request, isUpdate);
            else if(type==3) return await this.PlayerRegistrationAsync(request, isUpdate);
            return (Results.Null, null); 
        } 

        private async Task<(Results result, String message)> ManagerRegistrationAsync(RegistrationRequest request, bool isUpdate){
            var result = _repo.DSpQueryMultiple("dbo.spfn_BCCBS0M", new Dictionary<string, object>(){
                { "parmcompid", account.CompanyID },
                { "parmbrcd", account.BranchID },
                { "parmrctrid", account.UserID },
                { "parmrgsid", (isUpdate?request.RegisterID:"") },
                { "parmmobno", request.MobileNumber },
                { "parmgrpcd", request.Role },
                //{ "parmusrtyp", request.Firstname },
                { "parmfnm", request.Firstname },
                { "parmmnm", request.Middlename },
                { "parmlnm", request.Lastname },

                { "parmaddrss", request.Address },
                { "parmemladd", request.EmailAddress },

                { "parmbrtdt", request.BirthDate },
                { "parmbldtyp", request.BloodType },
                { "parmgndr", request.Gender },
                { "parmmtrlstat", request.MaritalStatus },
                { "parmctznshp", "" },
                { "parmnatnlty", request.Nationality },

                { "parmoccptn", request.Occupation },
                { "parmsklls", request.Skills },

                { "parmreg", request.Region },
                { "parmprov", request.Province },
                { "parmmun", request.Municipality },
                { "parmbrgy", request.Barangay },
                { "parmimgurl", request.ImageUrl },
            }).ReadSingleOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1"){
                    if(isUpdate) return (Results.Success, "Customer successfully updated."); 
                    return (Results.Success, "Customer successfully added to registration queue. please wait for approval to complete registration"); 
                }else if(ResultCode=="2") 
                    return (Results.Failed, String.Format("Mobile number {0} is already registered!", request.MobileNumber));
                else if (ResultCode == "3")
                    return (Results.Failed, "Invalid customer!\nMake sure the registration entry is existing");
                else if (ResultCode == "4")
                    return (Results.Failed, "Registration entry already done process");
                return (Results.Failed, "An error encountered while processing your request. please try again");

            }
            return (Results.Null, null); 
        }
        private async Task<(Results result, String message)> SupervisorRegistrationAsync(RegistrationRequest request, bool isUpdate){
            var result = _repo.DSpQueryMultiple("dbo.spfn_BCCBS0S", new Dictionary<string, object>(){
                { "parmcompid", account.CompanyID },
                { "parmbrcd", account.BranchID },
                { "parmrctrid", account.UserID },
                { "parmrgsid", (isUpdate?request.RegisterID:"") },
                { "parmmanageractid", request.GeneralCoordinator },

                { "parmmobno", request.MobileNumber },
                { "parmgrpcd", request.Role },
                { "parmfnm", request.Firstname },
                { "parmmnm", request.Middlename },
                { "parmlnm", request.Lastname },

                { "parmaddrss", request.Address },
                { "parmemladd", request.EmailAddress },

                { "parmbrtdt", request.BirthDate },
                { "parmbldtyp", request.BloodType },
                { "parmgndr", request.Gender },
                { "parmmtrlstat", request.MaritalStatus },
                { "parmctznshp", request.Citizenship },
                { "parmnatnlty", request.Nationality },

                { "parmoccptn", request.Occupation },
                { "parmsklls", request.Skills },

                { "parmreg", request.Region },
                { "parmprov", request.Province },
                { "parmmun", request.Municipality },
                { "parmbrgy", request.Barangay },
                { "parmimgurl", request.ImageUrl },
            }).ReadSingleOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1"){
                    if(isUpdate) return (Results.Success, "Customer successfully updated."); 
                    return (Results.Success, "Customer successfully added to registration queue. please wait for approval to complete registration"); //inform your player to complete registration within 30 minutes
                }else if(ResultCode=="2") 
                    return (Results.Failed, String.Format("Mobile number {0} is already registered!", request.MobileNumber));
                else if (ResultCode == "3")
                    return (Results.Failed, "Invalid customer!\nMake sure the registration entry is existing");
                else if (ResultCode == "4")
                    return (Results.Failed, "Registration entry already done process");
                return (Results.Failed, "An error encountered while processing your request. please try again");
            }
            return (Results.Null, null); 
        }
        private async Task<(Results result, String message)> PlayerRegistrationAsync(RegistrationRequest request, bool isUpdate){
            var result = _repo.DSpQueryMultiple("dbo.spfn_BCCBS0P", new Dictionary<string, object>(){
                { "parmcompid", account.CompanyID },
                { "parmbrcd", account.BranchID },
                { "parmrctrid", account.UserID },
                { "parmrgsid", (isUpdate?request.RegisterID:"") },
                { "parmmanageractid", request.GeneralCoordinator },
                { "parmsupervisoractid", request.Coordinator },

                { "parmmobno", request.MobileNumber },
                { "parmgrpcd", request.Role },
                { "parmfnm", request.Firstname },
                { "parmmnm", request.Middlename },
                { "parmlnm", request.Lastname },

                { "parmaddrss", request.Address },
                { "parmemladd", request.EmailAddress },

                { "parmbrtdt", request.BirthDate },
                { "parmbldtyp", request.BloodType },
                { "parmgndr", request.Gender },
                { "parmmtrlstat", request.MaritalStatus },
                { "parmctznshp", "" },
                { "parmnatnlty", request.Nationality },

                { "parmoccptn", request.Occupation },
                { "parmsklls", request.Skills },

                { "parmreg", request.Region },
                { "parmprov", request.Province },
                { "parmmun", request.Municipality },
                { "parmbrgy", request.Barangay },
                { "parmimgurl", request.ImageUrl },
            }).ReadSingleOrDefault();
            if(result != null){
                var row = ((IDictionary<string, object>)result);
                string ResultCode = row["RESULT"].Str();
                if(ResultCode == "1"){
                    if(isUpdate) return (Results.Success, "Customer successfully updated."); 
                    return (Results.Success, "Customer successfully added to registration queue. please wait for approval to complete registration"); 
                }else if(ResultCode=="2") 
                    return (Results.Failed, String.Format("Mobile number {0} is already registered!", request.MobileNumber));
                else if (ResultCode == "3")
                    return (Results.Failed, "Invalid customer!\nMake sure the registration entry is existing");
                else if (ResultCode == "4")
                    return (Results.Failed, "Registration entry already done process");
                return (Results.Failed, "An error encountered while processing your request. please try again");
            }
            return (Results.Null, null); 
        }
    }
}