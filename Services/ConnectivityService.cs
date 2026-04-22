using Microsoft.EntityFrameworkCore;

namespace Financial_ForeCast.Services
{
    public class ConnectivityService : IDisposable
    {
        private readonly DbServiceFactory _factory;
        private Timer _healthCheckTimer;
        private bool _isOnline;
        private bool _disposed;
        private bool _initialCheckDone;

        public bool IsOnline => _isOnline;
        public bool HasRemoteConfig => DbServiceFactory.HasRemoteConnectionString();

        public event Action<bool> OnConnectivityChanged;

        public ConnectivityService(DbServiceFactory factory)
        {
            _factory = factory;
            // Don't assume online — we haven't checked yet
            _isOnline = false;
            _initialCheckDone = false;
        }

        public void StartMonitoring(int intervalSeconds = 30)
        {
            StopMonitoring();
            if (!HasRemoteConfig) return;

            _healthCheckTimer = new Timer(OnTimerCallback,
                null, TimeSpan.Zero, TimeSpan.FromSeconds(intervalSeconds));
        }

        private async void OnTimerCallback(object state)
        {
            try
            {
                await CheckConnectivity();
            }
            catch
            {
                // Prevent unobserved exceptions from crashing the app
            }
        }

        public void StopMonitoring()
        {
            _healthCheckTimer?.Dispose();
            _healthCheckTimer = null;
        }

        public async Task<bool> CheckConnectivity()
        {
            if (!HasRemoteConfig) return false;

            var wasOnline = _isOnline;
            try
            {
                var connectionString = DbServiceFactory.GetRemoteConnectionString();
                var dbType = DbServiceFactory.GetRemoteDbType();
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

                using var testDb = new AppDbContext(optionsBuilder.Options);
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                var canConnect = await testDb.Database.CanConnectAsync(cts.Token);
                _isOnline = canConnect;
            }
            catch
            {
                _isOnline = false;
            }

            // Always fire on the first check so the provider knows the real state,
            // and fire on any subsequent state change.
            if (!_initialCheckDone || wasOnline != _isOnline)
            {
                _initialCheckDone = true;
                OnConnectivityChanged?.Invoke(_isOnline);
            }

            return _isOnline;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopMonitoring();
        }
    }
}
