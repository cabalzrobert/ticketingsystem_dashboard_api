using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Data;
using Dapper;
using System.Text;
using System;
using static System.Collections.Specialized.BitVector32;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Infrastructure.Repositories
{

    public class Repository<TContext> : ReadOnlyRepository<TContext>, IRepository where TContext : DBContext
    {
        public Repository(TContext context)
            : base(context)
        {
        }

        public void Delete<T>(object id) where T : class
        {
            T entity = _context.Set<T>().Find(id);
            Delete(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            DbSet<T> dbSet = _context.Set<T>();
            if (_context.Entry(entity).State == EntityState.Detached)
            {
                dbSet.Attach(entity);
            }
            dbSet.Remove(entity);
        }

        public void DeleteRange<T>(List<T> entities) where T : class
        {
            DbSet<T> dbSet = _context.Set<T>();
            List<T> AttachRanges = new List<T>(), RemoveRanges = new List<T>();
            foreach(T entity in entities)
                (_context.Entry(entity).State==EntityState.Detached? AttachRanges:RemoveRanges)
                    .Add(entity);
            if(AttachRanges.Count != 0) dbSet.AttachRange(AttachRanges);
            if(RemoveRanges.Count != 0) dbSet.RemoveRange(RemoveRanges);
        }

        public T Insert<T>(T entity) where T : class
        {
            return _context.Set<T>().Add(entity).Entity;
        }

        public void InsertRange<T>(List<T> entities) where T : class
        {
            _context.Set<T>().AddRange(entities);
        }

        public void Update<T>(T entity) where T : class
        {
            _context.Set<T>().Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<int> SaveAsync(int userID)
        {
            return await _context.SaveChangesAsync().ConfigureAwait(false);
            //return await _context.SaveChangesAsync(userID).ConfigureAwait(false);
        }

        public IList<T> DQuery<T>(string SqlString) where T : class
        {
            var connection = _context.Database.GetDbConnection();
            return connection.Query<T>(SqlString).ToList(); //$"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n{ SqlString }"
        }
        
        public IList<T> DQuery<T>(string SqlString, Dictionary<string, object> parameters) where T : class
        {
            var connection = _context.Database.GetDbConnection();
            return connection.Query<T>(SqlString,
                param: new DynamicParameters(parameters)).ToList();
        }
        public SqlMapper.GridReader DQueryMultiple(string SqlString, Dictionary<string, object> parameters)
        {
            var connection = _context.Database.GetDbConnection();
            
            return connection.QueryMultiple(SqlString,
                param: new DynamicParameters(parameters));
        }
        
        public SqlMapper.GridReader DQueryMultiple(string SqlString)
        {
            var connection = _context.Database.GetDbConnection();
            return connection.QueryMultiple(SqlString);
        }
        
        
        public IList<T> DSpQuery<T>(string SpName, Dictionary<string, object> parameters) where T : class
        {
            var connection = _context.Database.GetDbConnection();
            return connection.Query<T>(SpName,
                param: new DynamicParameters(parameters),
                commandType: CommandType.StoredProcedure).ToList();
        }
        
        public SqlMapper.GridReader DSpQueryMultiple(string SpName, Dictionary<string, object> parameters)
        {
            var connection = _context.Database.GetDbConnection();
            //Dapper.SqlMapper.TypeMapProvider = type =>
            //{
            //    // create fallback default type map
            //    var fallback = new DefaultTypeMap(type);
            //    return new CustomPropertyTypeMap(type, (t, column) =>
            //    {
            //        var property = t.GetProperties().FirstOrDefault(prop =>
            //            prop.GetCustomAttributes(typeof(ColumnAttribute))
            //                .Cast<ColumnAttribute>()
            //                .Any(attr => attr.Name == column));

            //        // if no property matched - fall back to default type map
            //        if (property == null)
            //        {
            //            property = fallback.GetMember(column)?.Property;
            //        }

            //        return property;
            //    });
            //};
            return connection.QueryMultiple(SpName,
                param: new DynamicParameters(parameters),
                commandType: CommandType.StoredProcedure);
        }

        public DataTable SpEntry(string SpName, Dictionary<string, string> parameters)
        {
            var datatable=new DataTable();
            var connection = _context.Database.GetDbConnection();
            using(var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = SpName;
                try
                {
                    connection.Open();
                    foreach (var param in parameters)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = ("@" + param.Key);
                        parameter.Value = (param.Value ?? "");
                        command.Parameters.Add(parameter);
                    }
                    using(var reader = command.ExecuteReader())
                    {
                        if(reader.HasRows)
                            datatable.Load(reader);
                    }
                }
                finally
                { 
                    connection.Close(); 
                }
            }
            return datatable;
        }


        public DataTable QueryOnTable(string SqlString)
        {
            var datatable=new DataTable();
            var connection = _context.Database.GetDbConnection();
            using(var command = connection.CreateCommand())
            {
                command.CommandText = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n{ SqlString }";
                try
                {
                    connection.Open();
                    using(var reader = command.ExecuteReader())
                        datatable.Load(reader);
                }
                finally
                { 
                    connection.Close(); 
                }
            }
            return datatable;
        }
        public IList<DataTable> QueryOnTables(string SqlString)
        {
            IList<DataTable> collections = null;
            var connection = _context.Database.GetDbConnection();
            using(var command = connection.CreateCommand())
            {
                command.CommandText = $"SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;\n{ SqlString }";
                try
                {
                    connection.Open();
                    using(var reader = command.ExecuteReader())
                    {
                        collections = new List<DataTable>();
                        if(reader.HasRows){
                            DataTable datatable = null;    
                            while (!reader.IsClosed){
                                collections.Add(datatable=new DataTable());
                                datatable.Load(reader);
                            }
                        } 
                    }
                }
                finally
                { 
                    connection.Close(); 
                }
            }
            return collections;
        }
    }
}