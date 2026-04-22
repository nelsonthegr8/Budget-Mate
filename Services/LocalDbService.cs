using Microsoft.EntityFrameworkCore;

namespace Financial_ForeCast.Services
{
    public class LocalDbService : DbServiceBase
    {
        private const string DB_NAME = "FinancialForeCast.db";

        public LocalDbService()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, DB_NAME);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
            _db = new AppDbContext(options);
            InitializeDatabase();
        }
    }
}
