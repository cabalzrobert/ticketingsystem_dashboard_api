using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using Infrastructure.Commons.AutoRegister;
using Dapper;

namespace Infrastructure.Repositories
{
    [Service.ITransient(typeof(Repository<DBContext>))] // Service. Scope
    public interface IRepository : IReadOnlyRepository
    {
        T Insert<T>(T entity) where T : class;
        void InsertRange<T>(List<T> entities) where T: class;
        void Update<T>(T entity) where T : class;
        void Delete<T>(object id) where T : class;
        void Delete<T>(T entity) where T : class;
        void DeleteRange<T>(List<T> entities) where T : class;
        Task<int> SaveAsync();
        Task<int> SaveAsync(int userID);
        IList<T> DQuery<T>(string SqlString) where T : class;
        IList<T> DQuery<T>(string SqlString, Dictionary<string, object> parameters) where T : class;
        SqlMapper.GridReader DQueryMultiple(string SqlString);
        SqlMapper.GridReader DQueryMultiple(string SqlString, Dictionary<string, object> parameters);
        IList<T> DSpQuery<T>(string SpName, Dictionary<string, object> parameters) where T : class;
        SqlMapper.GridReader DSpQueryMultiple(string SpName, Dictionary<string, object> parameters);
        DataTable QueryOnTable(string SqlString);
        IList<DataTable> QueryOnTables(string SqlString);
        DataTable SpEntry(string SpName, Dictionary<string, string> parameters);
    }
}
