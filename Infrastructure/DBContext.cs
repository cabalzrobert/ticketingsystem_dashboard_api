using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure.Commons;


namespace Infrastructure
{
    public class DBContext : DbContext  
    {
        private readonly ILoggerFactory _loggerFactory;

        public DBContext(ILoggerFactory loggerFactory, DbContextOptions<DBContext> options)
                            : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseLoggerFactory(_loggerFactory);
            base.OnConfiguring(builder);
        }

        //public DbSet<Area> dsArea {get; set;}
        //public DbSet<Branch> dsBranch {get; set;}
        //public DbSet<Department> dsDepartment {get; set;}
        //public DbSet<Department> dsDepartment {get; set;}
        //DbSet<Area> area;
        //DbSet<Branch> branch;

        /*public async Task<int> SaveChangesAsync(int userID)
        {
            var modEntities = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();
            var changeEvent = new ChangeEvent { UserId = userID };
            Set<ChangeEvent>().Add(changeEvent);

            foreach (var modEntity in modEntities)
            {
                string tableName = modEntity.Metadata.Relational().TableName;
                string rowId = String.Join(",", modEntity.Properties.Where(p => p.Metadata.IsPrimaryKey()).Select(p => p.CurrentValue.ToString()));

                foreach (var prop in modEntity.Properties.Where(p => p.IsModified))
                {
                    // Log the change
                    var log = new ChangeLog
                    {
                        ChangeEvent = changeEvent,
                        OldValue = prop.OriginalValue?.ToString(),
                        NewValue = prop.CurrentValue?.ToString(),
                        TableName = tableName,
                        ColumnName = prop.Metadata.Relational().ColumnName,
                        RowId = rowId
                    };
                    Set<ChangeLog>().Add(log);
                }
            }

            return await base.SaveChangesAsync().ConfigureAwait(false);
        }*/
        protected override void OnModelCreating(ModelBuilder builder)
        {
            ModelEntityFinder.Init(builder);
        }
    }
}