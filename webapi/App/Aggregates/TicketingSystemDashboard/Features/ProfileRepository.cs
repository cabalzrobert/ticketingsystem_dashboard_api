using Infrastructure.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.App.Aggregates.Common;
using webapi.Commons.AutoRegister;
using Comm.Commons.Extensions;
using webapi.App.TSDashboardModel;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Model.User;
using webapi.App.Aggregates.Common.Dto;

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features
{
    [Service.ITransient(typeof(ProfileRepository))]
    public interface IProfileRepository
    {
        Task<(Results result, string message)> UpdateProfile(Profile user);

    }
    public class ProfileRepository : IProfileRepository
    {
        private readonly ISubscriber _account;
        private readonly IRepository _repo;
        private TicketingUser account { get { return _account.AccountIdentity(); } }
        public ProfileRepository(IRepository repo, ISubscriber account) 
        {
            _account = account;
            _repo= repo;
        }

        public async Task<(Results result, string message)> UpdateProfile(Profile profile)
        {
            var results = _repo.DSpQuery<dynamic>("dbo.spfn_UPDATEPROFILE", new Dictionary<string, object>()
            {
                {"parmmobileno", profile.mobileNumber },
                {"parmdisplayname", profile.displayName },
                {"parmuserid", profile.userId },
                {"parmfirstname", profile.firstName },
                {"parmmiddlename", profile.middleName },
                {"parmlastname", profile.lastName },
                {"parmgender", profile.gender },
                {"parmbirthday", profile.birthday },
                {"parmaddress", profile.address },
                {"parmisbasicinfo", profile.isBasicInfo }
            }).FirstOrDefault();

            var row = (IDictionary<string, object>)results;
            string resultCode = row["RESULT"].Str();
            if (resultCode == "1")
                return (Results.Success, "Success");
            else if (resultCode == "0")
                return (Results.Failed, "Failed");
            return (Results.Null, null);

        }
    }
}
