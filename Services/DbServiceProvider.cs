namespace Financial_ForeCast.Services
{
    public class DbServiceProvider
    {
        private readonly DbServiceFactory _factory;
        private readonly ConnectivityService _connectivity;
        private readonly BackupSyncService _backupSync;
        private IDbService _current;
        private IDbService _resilientCurrent;
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

            WrapCurrent();
        }

        /// <summary>
        /// Pages use this to access the DB. When running remote, calls are wrapped
        /// so that a mid-operation connection failure triggers an immediate fallback
        /// to the local database.
        /// </summary>
        public IDbService Current => _resilientCurrent;

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

            WrapCurrent();
        }

        private void WrapCurrent()
        {
            if (_current is RemoteDbService)
            {
                _resilientCurrent = new ResilientDbService(_current, OnRemoteConnectionFailed, () => _current);
            }
            else
            {
                _resilientCurrent = _current;
            }
        }

        private void OnRemoteConnectionFailed()
        {
            if (_isOfflineMode) return;

            // Immediately swap to local so the next call from the page succeeds
            _current = new LocalDbService();
            _isOfflineMode = true;
            WrapCurrent();
            OnModeChanged?.Invoke(false);
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
                WrapCurrent();
                OnModeChanged?.Invoke(true);
            }
            else if (!isOnline && !_isOfflineMode)
            {
                // Server went down — fall back to local
                _current = new LocalDbService();
                _isOfflineMode = true;
                WrapCurrent();
                OnModeChanged?.Invoke(false);
            }
        }
    }
}
