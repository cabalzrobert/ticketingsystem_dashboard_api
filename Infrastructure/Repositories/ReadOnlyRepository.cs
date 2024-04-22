using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    
    public class ReadOnlyRepository<TContext> : IReadOnlyRepository where TContext : DBContext
    {
        protected readonly TContext _context;

        public ReadOnlyRepository(TContext context)
        {
            _context = context;
        }

        public async Task<int> CountAsync<T>(Expression<Func<T, bool>> filter = null) where T : class
        {
            return await GetQueryable(filter).CountAsync().ConfigureAwait(false);
        }

        public async Task<bool> EntityExistsAsync<T>(Expression<Func<T, bool>> filter = null) where T : class
        {
            return await GetQueryable(filter).AnyAsync().ConfigureAwait(false);
        }

        public async Task<IEnumerable<T>> FindAsync<T>(Expression<Func<T, bool>> filter = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null, string includeProperties = null, int? skip = null, int? take = null, bool asNoTracking = false) where T : class
        {
            return await GetQueryable(filter, orderBy, includeProperties, skip, take, asNoTracking).ToListAsync().ConfigureAwait(false);
        }

        public async Task<T> GetAsync<T>(object id) where T : class
        {
            return await _context.Set<T>().FindAsync(id).ConfigureAwait(false);
        }

        public async Task<T> GetFirstAsync<T>(Expression<Func<T, bool>> filter, string includeProperties = null, bool asNoTracking = false) where T : class
        {
            return await GetQueryable<T>(filter, null, includeProperties, null, null, asNoTracking).FirstOrDefaultAsync();
        }

        public IQueryable<T> Get<T>(Expression<Func<T, bool>> filter, string includeProperties = null, bool asNoTracking = false) where T : class
        {
            return GetQueryable<T>(filter, null, includeProperties, null, null, asNoTracking);
        }

        public IQueryable<T> Query<T>(string includeProperties = null, bool asNoTracking = false) where T : class
        {
            return GetQueryable<T>(null, null, includeProperties, null, null, asNoTracking);
        }

        protected virtual IQueryable<T> GetQueryable<T>(Expression<Func<T, bool>> filter = null,
                                                        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
                                                        string includeProperties = null,
                                                        int? skip = null,
                                                        int? take = null,
                                                        bool asNoTracking = false) where T : class
        {
            includeProperties = includeProperties ?? "";
            IQueryable<T> query = _context.Set<T>();

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            string[] splitProps = includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var prop in splitProps)
            {
                query = query.Include(prop);
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (skip.HasValue)
            {
                query = query.Skip(skip.Value);
            }

            if (take.HasValue)
            {
                query = query.Take(take.Value);
            }

            return query;
        }
    }
}