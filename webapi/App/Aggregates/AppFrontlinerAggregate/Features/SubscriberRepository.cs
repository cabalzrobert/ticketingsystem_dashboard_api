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

namespace webapi.App.Aggregates.AppFrontlinerAggregate.Features
{
    [Service.ITransient(typeof(SubscriberRepository))] 
    public interface ISubscriberRepository
    {
        Task<(Results result, object items)> SearchAccountAsync(String AccountID);
    } 

    public class SubscriberRepository : ISubscriberRepository
    {
        private readonly ICompany _company;
        private readonly IRepository _repo;
        public SubscriberRepository(ICompany company, IRepository repo){
            _repo = repo; 
            _company = company;
        }
        public async Task<(Results result, object items)> SearchAccountAsync(String AccountID){
            var results = _repo.DSpQueryMultiple("dbo.spfn_BDBBEA0B", new Dictionary<string, object>(){
                { "parmcompid", _company.CompanyID() },
                { "parmbrcd", _company.BranchID() },
                { "parmactid", AccountID },
            });
            if(results != null)
                return (Results.Success, results.ReadSingleOrDefault()); 
            return (Results.Null, null); 
        }
    }
}