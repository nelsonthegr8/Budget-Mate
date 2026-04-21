using Microsoft.Extensions.Configuration;

namespace Financial_ForeCast.Services
{
    /// <summary>
    /// Factory pattern to switch between LocalDbService and ServerDbService at runtime.
    /// Configuration driven via appsettings or environment variables.
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
        /// Returns LocalDbService by default, or ServerDbService if configured.
        /// </summary>
        public IDbService GetService()
        {
            if (_isInitialized && _dbService != null)
                return _dbService;

            var useServer = _configuration.GetValue<bool>("UseServerDatabase", false);
            var serverUrl = _configuration.GetValue<string>("ServerDatabaseUrl", "");

            if (useServer && !string.IsNullOrEmpty(serverUrl))
            {
                _dbService = new ServerDbService(serverUrl);
            }
            else
            {
                _dbService = new LocalDbService();
            }

            _isInitialized = true;
            return _dbService;
        }

        /// <summary>
        /// Switches the active database service at runtime.
        /// Call this when user changes database connection settings.
        /// </summary>
        public void SwitchToServer(string serverUrl)
        {
            _dbService = new ServerDbService(serverUrl);
            _isInitialized = true;
        }

        /// <summary>
        /// Switches back to local database.
        /// </summary>
        public void SwitchToLocal()
        {
            _dbService = new LocalDbService();
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
    }
}
