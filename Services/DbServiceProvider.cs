namespace Financial_ForeCast.Services
{
    public class DbServiceProvider
    {
        private readonly DbServiceFactory _factory;
        private readonly ConnectivityService _connectivity;
        private readonly BackupSyncService _backupSync;
        private IDbService _current;
        private bool _isOfflineMode;

        public bool IsOfflineMode => _isOfflineMode;

        /// <summary>
        /// Raised when the provider switches between online/offline mode.
        /// Parameter: true = online, false = offline.
        /// </summary>
        public event Action<bool> OnModeChanged;

        public DbServiceProvider(DbServiceFactory factory, ConnectivityService connectivity, BackupSyncService backupSync)
        {
            _factory = factory;
            _connectivity = connectivity;
            _backupSync = backupSync;

            // Subscribe BEFORE starting monitoring so we catch the first check result
            _connectivity.OnConnectivityChanged += HandleConnectivityChanged;

            if (DbServiceFactory.HasRemoteConnectionString())
            {
                // Start with local as a safe default until connectivity is confirmed
                _current = new LocalDbService();
                _isOfflineMode = true;
                _connectivity.StartMonitoring();
                _backupSync.StartBackupSchedule();
            }
            else
            {
                _current = _factory.Create();
            }
        }

        public IDbService Current => _current;

        public void Refresh()
        {
            _current = _factory.Create();
            _isOfflineMode = false;

            if (DbServiceFactory.HasRemoteConnectionString())
            {
                _connectivity.StartMonitoring();
                _backupSync.StartBackupSchedule();
            }
            else
            {
                _connectivity.StopMonitoring();
                _backupSync.StopBackupSchedule();
            }
        }

        private async void HandleConnectivityChanged(bool isOnline)
        {
            if (isOnline && _isOfflineMode)
            {
                // Server is back — check if user wants to push local data
                if (BackupSyncService.IsPushOnReconnect())
                {
                    await _backupSync.PushLocalToRemote();
                }

                _current = _factory.Create();
                _isOfflineMode = false;
                OnModeChanged?.Invoke(true);
            }
            else if (!isOnline && !_isOfflineMode)
            {
                // Server went down — fall back to local
                _current = new LocalDbService();
                _isOfflineMode = true;
                OnModeChanged?.Invoke(false);
            }
        }
    }
}
