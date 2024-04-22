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
using webapi.App.Aggregates.AppRecruiterAggregate.Common;

namespace webapi.App.Aggregates.AppRecruiterAggregate.Features
{
    [Service.ITransient(typeof(SubscriberRepository))] 
    public interface ISubscriberRepository
    {
        Task<(Results result, object items)> SubscribersAsync(FilterRequest filter);
    } 

    public class SubscriberRepository : ISubscriberRepository
    {
        private readonly IRecruiter _identity;
        private readonly IRepository _repo;
        public Recruiter account { get{ return _identity.AccountIdentity(); } } 
        public SubscriberRepository(IRecruiter identity, IRepository repo){
            _identity = identity; 
            _repo = repo; 
        }
        public async Task<(Results result, object items)> SubscribersAsync(FilterRequest filter){
            var results = _repo.DSpQueryMultiple("dbo.spfn_BDBBEA0A", new Dictionary<string, object>(){
                { "parmcompid", account.CompanyID },
                { "parmbrcd", account.BranchID },
                { "parmusrtyp", filter.Type },
                { "parmactid", filter.ID },
                { "parmsrch", filter.Search },
                { "parmfid", filter.BaseFilter },
            });
            if(results != null)
                return (Results.Success, SubscriberDto.SearchSubscribers(results.Read(), 25)); 
            return (Results.Null, null); 
        }
    }
}