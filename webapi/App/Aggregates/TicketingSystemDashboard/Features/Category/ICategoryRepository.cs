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

namespace webapi.App.Aggregates.TicketingSystemDashboard.Features.Category
{
    [Service.ITransient(typeof(CategoryRepository))]
    public interface ICategoryRepository
    {
        Task<(Results result, String message)> SaveCategoryAsyn(CategoryModel request);
        Task<(Results result, String message)> UpdateCategoryAsyn(CategoryModel request);
        Task<(Results result, object cat)> LoadCategoryAsync(FilterRequest req);
    }
    public class CategoryRepository:ICategoryRepository
    {
        private readonly ISubscriber _identity;
        private readonly IRepository _repo;
        public TicketingUser account { get { return _identity.AccountIdentity(); } }
        public CategoryRepository(ISubscriber identity, IRepository repo)
        {
            _identity = identity;
            _repo = repo;
        }

        public async Task<(Results result, string message)> SaveCategoryAsyn(CategoryModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_LAC0001", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmcategoryname", request.Categoryname},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                request.CategoryID = row["CATID"].Str();
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "2")
                    return (Results.Success, "Already Exist");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, string message)> UpdateCategoryAsyn(CategoryModel request)
        {
            var result = _repo.DSpQuery<dynamic>($"dbo.spfn_LAC0003", new Dictionary<string, object>()
            {
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmcategoryid", request.CategoryID},
                {"parmcategoryname", request.Categoryname},
                {"parmuserid", account.USR_ID}
            }).FirstOrDefault();
            if (result != null)
            {
                var row = ((IDictionary<string, object>)result);
                //request.DepartmentID = row["DEPT_ID"].Str();
                string ResultCode = row["RESULT"].Str();
                if (ResultCode == "1")
                {
                    return (Results.Success, "Successfully save.");
                }
                else if (ResultCode == "2")
                    return (Results.Success, "Already Exist");
                else if (ResultCode == "0")
                    return (Results.Failed, "Please check data. Try again");
            }
            return (Results.Null, null);
        }

        public async Task<(Results result, object cat)> LoadCategoryAsync(FilterRequest req)
        {
            var results = _repo.DSpQuery<dynamic>($"dbo.spfn_LAC0002", new Dictionary<string, object>()
            {
                //{"parmplid",account.PL_ID },
                {"parmplid", account.PL_ID},
                {"parmpgrpid", account.PGRP_ID},
                {"parmrownum", req.num_row },
                {"parmsearch", req.Search }

            });
            if (results != null)
                return (Results.Success, TickectingSubscriberDto.GetCategoryList(results));
            return (Results.Null, null);
        }
    }
}
