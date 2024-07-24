using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using webapi.Commons.AutoRegister;
using webapi.App.Aggregates.SubscriberAppAggregate.Common;
using webapi.App.Aggregates.Common;
using Infrastructure.Repositories;
using webapi.App.Model.User;
using Comm.Commons.Extensions;
using webapi.App.RequestModel.Common;
using webapi.App.Aggregates.Common.Dto;

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.Department
{
    [Service.ITransient(typeof(DepartmentRepository))]
    public interface IDepartmentRepository
    {
        Task<(Results result, String message)> SaveDepartmentAsyn(DepartmentModel request);
        Task<(Results result, String message)> UpdateDepartmentAsyn(DepartmentModel request);
        Task<(Results result, object dept)> LoadDepartmentAsync(FilterRequest req);
    }
    public class DepartmentRepository:IDepartmentRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public DepartmentRepository(ISubscriber identity, IRepository repo)
        {
            _identity = identity;
            _repo = repo;
        }

        public async Task<(Results result, string message)> SaveDepartmentAsyn(DepartmentModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_LAE0001", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdepartmentname", request.DepartmentName},
                {"parmdepartmenthead", request.DepartmentHeadID},
                {"parmcategorylist", request.CategoryList},
                {"parmcategorylistremove", request.CategoryListRemove},
                {"parmstafflist", request.StaffList},
                {"parmstafflistremove", request.StaffListRemove},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                request.DepartmentID = row["DEPT_ID"].Str();
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "New Department successfully add!");
                }
                else if (ResultCode == "2")
                    return (Results.Success, "Already Exist");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, object dept)> LoadDepartmentAsync(FilterRequest req)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_LAE0002", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmrownum", req.num_row },
                {"parmsearch", req.Search }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetDepartmentList(results));
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> UpdateDepartmentAsyn(DepartmentModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_LAE0004", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmdepartmentid", request.DepartmentID},
                {"parmdepartmentname", request.DepartmentName},
                {"parmdepartmenthead", request.DepartmentHeadID},
                {"parmcategorylist", request.CategoryList},
                {"parmcategorylistremove", request.CategoryListRemove},
                {"parmstafflist", request.StaffList},
                {"parmstafflistremove", request.StaffListRemove},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                //request.DepartmentID = row["DEPT_ID"].Str();
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "Department successfully update!");
                }
                else if (ResultCode == "2")
                    return (Results.Success, "Already Exist");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }
    }
}
