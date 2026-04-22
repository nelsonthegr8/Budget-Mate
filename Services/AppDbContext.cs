using Financial_ForeCast.Models;
using Microsoft.EntityFrameworkCore;

namespace Financial_ForeCast.Services
{
    public class AppDbContext : DbContext
    {
        public DbSet<IncomeExpense> IncomeExpenses { get; set; }
        public DbSet<RoadMaps> RoadMaps { get; set; }
        public DbSet<Accnts> Accounts { get; set; }
        public DbSet<MainMenuCards> MainMenuCards { get; set; }
        public DbSet<Forecasts> Forecasts { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
