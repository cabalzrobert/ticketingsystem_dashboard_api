using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Infrastructure.Commons.AutoRegister;
namespace Infrastructure.Repositories
{
    [Service.ITransient(typeof(ReadOnlyRepository<DBContext>))] //Service.Scope
    public interface IReadOnlyRepository
    {
        Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> filter = null,
                                          Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                          string includeProperties = null,
                                          int? skip = null,
                                          int? take = null,
                                          bool asNoTracking = false) where T : class;

        Task<T> GetAsync<T>(object id) where T : class;

        Task<bool> EntityExistsAsync<T>(Expression<Func<T, bool>> filter = null) where T : class;

        Task<int> CountAsync<T>(Expression<Func<T, bool>> filter = null) where T : class;

        IQueryable<T> Query<T>(string includeProperties = null, bool asNoTracking = false) where T : class;
        Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> filter, string includeProperties = null, bool asNoTracking = false) where T : class;
        IQueryable<T> Get<T>(Expression<Func<T, bool>> filter, string includeProperties = null, bool asNoTracking = false) where T : class;
    }
}