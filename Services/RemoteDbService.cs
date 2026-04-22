using Microsoft.EntityFrameworkCore;

namespace Financial_ForeCast.Services
{
    public enum RemoteDbType
    {
        PostgreSQL,
        SqlServer
    }

    public class RemoteDbService : DbServiceBase
    {
        public RemoteDbService(string connectionString, RemoteDbType dbType)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            switch (dbType)
            {
                case RemoteDbType.PostgreSQL:
                    optionsBuilder.UseNpgsql(connectionString);
                    break;
                case RemoteDbType.SqlServer:
                    optionsBuilder.UseSqlServer(connectionString);
                    break;
            }

            _db = new AppDbContext(optionsBuilder.Options);
            InitializeDatabase();
        }
    }
}
