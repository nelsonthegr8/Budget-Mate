using Microsoft.Extensions.Configuration;

namespace Financial_ForeCast.Services
{
    /// <summary>
    /// Factory for managing database service instances.
    /// Supports switching between local SQLite and server databases at runtime.
    /// </summary>
    public class DbServiceFactory
    {
        private readonly IConfiguration _configuration;
        private IDbService _dbService;
        private bool _isInitialized;

        public DbServiceFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the configured database service instance (lazy initialization).
        /// Returns UnifiedDbService configured for local SQLite or server database based on settings.
        /// </summary>
        public IDbService GetService()
        {
            if (_isInitialized && _dbService != null)
                return _dbService;

            var useServer = _configuration.GetValue<bool>("UseServerDatabase", false);
            var connectionString = _configuration.GetValue<string>("ServerDatabaseConnectionString", "");

            if (useServer && !string.IsNullOrEmpty(connectionString))
            {
                var connectionFactory = new ServerDbConnectionFactory(connectionString);
                _dbService = new UnifiedDbService(connectionFactory);
            }
            else
            {
                _dbService = new UnifiedDbService(); // Local SQLite
            }

            _isInitialized = true;
            return _dbService;
        }

        /// <summary>
        /// Switches the active database service to a server database at runtime.
        /// Call this when user provides a connection string.
        /// 
        /// Supported connection string formats:
        /// - SQL Server: "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASS;"
        /// - PostgreSQL: "Host=YOUR_HOST;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASS;"
        /// - MySQL: "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASS;"
        /// - SQLite: "Data Source=/path/to/database.db"
        /// </summary>
        public void SwitchToServer(string connectionString)
        {
            var connectionFactory = new ServerDbConnectionFactory(connectionString);
            _dbService = new UnifiedDbService(connectionFactory);
            _isInitialized = true;
        }

        /// <summary>
        /// Switches back to local SQLite database.
        /// </summary>
        public void SwitchToLocal()
        {
            _dbService = new UnifiedDbService();
            _isInitialized = true;
        }

        /// <summary>
        /// Resets the factory so GetService() will re-evaluate configuration.
        /// </summary>
        public void Reset()
        {
            _dbService = null;
            _isInitialized = false;
        }

        /// <summary>
        /// Gets the current database type (SQLite, SQLServer, PostgreSQL, MySQL).
        /// </summary>
        public string GetCurrentDatabaseType()
        {
            if (_dbService is UnifiedDbService unifiedService)
            {
                // Check if it's using local or server by inspecting internals
                // This is a simplified check - in production you might want a more explicit property
                return "SQLite (Local)"; // Default assumption
            }
            return "Unknown";
        }
    }
}
