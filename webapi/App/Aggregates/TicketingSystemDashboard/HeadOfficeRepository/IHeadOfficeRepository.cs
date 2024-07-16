using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Comm.Commons.Extensions;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.Common;
using webapi.App.TSDashboardModel;
using Infrastructure.Repositories;

namespace webapi.App.Aggregates.TicketingSystemDashboard.HeadOfficeRepository
{
    [Service.ITransient(typeof(HeadOfficeRepository))]
    public interface IHeadOfficeRepository
    {
        Task<(Results result, String message)> SaveHeadOffice(AgentHeadOffice ho);
    }

    public class HeadOfficeRepository : IHeadOfficeRepository
    {
        private readonly IRepository _repo;
        public HeadOfficeRepository(IRepository repo)
        {
            _repo = repo;
        }

        public async Task<(Results result, string message)> SaveHeadOffice(AgentHeadOffice ho)
        {
            try
            {
                var result = _repo.DSpQuery<dynamic>($"dbo.spfn_ESATCAABDABDB", new Dictionary<string, object>()
                {
                    {"parmplid", ho.parmplid},
                    {"parmgrpid", ho.parmpgrpid},
                    {"parmlnm", ho.HeadOfficeName},

                    {"parmadd", ho.HeadOfficeAddress},
                    {"@parmtelno", ho.HeadOfficeTelephoneNumber},
                    {"@parmemail", ho.HeadOfficeEmailAddress},

                    {"parmuserfnm", ho.FirstName},
                    {"parmuserlnm", ho.LastName},
                    {"parmusermnm", ho.MiddleInitial},
                    {"parmemailadd", ho.EmailAddress},


                    {"parmusername", ho.Username},
                    {"parmuserpassword", ho.Password},
                    {"parmusermobno", ho.MobileNumber},
                    {"parmuserid", ho.UserID},
                }).FirstOrDefault();
                if (result != null)
                {
                    var row = ((IDictionary<string, object>)result);
                    string ResultCode = row["RESULT"].Str();
                    if (ResultCode == "1")
                        return (Results.Success, "Successfully Save");
                    return (Results.Failed, "Something wrong in your data. Please try again");
                }
                return (Results.Null, null);
            }
            catch (Exception) { throw; }
        }
    }
}
