using Microsoft.EntityFrameworkCore;

namespace Financial_ForeCast.Services
{
    public class BackupSyncService : IDisposable
    {
        // Preferences keys
        public const string BackupEnabledKey = "BackupEnabled";
        public const string BackupIntervalHoursKey = "BackupIntervalHours";
        public const string PushOnReconnectKey = "PushOnReconnect";

        private readonly DbServiceFactory _factory;
        private Timer _backupTimer;
        private bool _disposed;
        private DateTime _lastBackupTime = DateTime.MinValue;

        public event Action<string> OnBackupStatusChanged;

        public BackupSyncService(DbServiceFactory factory)
        {
            _factory = factory;
        }

        // Settings helpers
        public static bool IsBackupEnabled() => Preferences.Get(BackupEnabledKey, false);
        public static void SetBackupEnabled(bool enabled) => Preferences.Set(BackupEnabledKey, enabled);

        public static int GetBackupIntervalHours() => Preferences.Get(BackupIntervalHoursKey, 0);
        public static void SetBackupIntervalHours(int hours) => Preferences.Set(BackupIntervalHoursKey, hours);

        public static bool IsPushOnReconnect() => Preferences.Get(PushOnReconnectKey, false);
        public static void SetPushOnReconnect(bool enabled) => Preferences.Set(PushOnReconnectKey, enabled);

        public void StartBackupSchedule()
        {
            StopBackupSchedule();

            if (!IsBackupEnabled() || !DbServiceFactory.HasRemoteConnectionString()) return;

            int intervalHours = GetBackupIntervalHours();
            if (intervalHours <= 0) return;

            var interval = TimeSpan.FromHours(intervalHours);
            _backupTimer = new Timer(async _ => await BackupRemoteToLocal(),
                null, interval, interval);
        }

        public void StopBackupSchedule()
        {
            _backupTimer?.Dispose();
            _backupTimer = null;
        }

        /// <summary>
        /// Copy all data from the remote database to the local database.
        /// </summary>
        public async Task BackupRemoteToLocal()
        {
            try
            {
                OnBackupStatusChanged?.Invoke("Backing up remote data to local device...");

                using var remoteDb = CreateRemoteContext();
                using var localDb = CreateLocalContext();

                if (remoteDb == null || localDb == null) return;

                // Check if there is any remote data
                bool hasData = await remoteDb.RoadMaps.AnyAsync() ||
                               await remoteDb.IncomeExpenses.AnyAsync() ||
                               await remoteDb.Accounts.AnyAsync() ||
                               await remoteDb.MainMenuCards.AnyAsync() ||
                               await remoteDb.Forecasts.AnyAsync();

                if (!hasData)
                {
                    OnBackupStatusChanged?.Invoke("No remote data to back up.");
                    return;
                }

                await CopyData(remoteDb, localDb);
                _lastBackupTime = DateTime.Now;

                OnBackupStatusChanged?.Invoke($"Backup completed at {_lastBackupTime:g}");
            }
            catch (Exception ex)
            {
                OnBackupStatusChanged?.Invoke($"Backup failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Push all local data to the remote database, overwriting remote data.
        /// </summary>
        public async Task PushLocalToRemote()
        {
            try
            {
                OnBackupStatusChanged?.Invoke("Pushing local data to remote server...");

                using var remoteDb = CreateRemoteContext();
                using var localDb = CreateLocalContext();

                if (remoteDb == null || localDb == null) return;

                await CopyData(localDb, remoteDb);

                OnBackupStatusChanged?.Invoke("Local data pushed to remote server successfully.");
            }
            catch (Exception ex)
            {
                OnBackupStatusChanged?.Invoke($"Push to remote failed: {ex.Message}");
            }
        }

        private async Task CopyData(AppDbContext source, AppDbContext destination)
        {
            // Clear destination tables (order matters for FK constraints)
            destination.Forecasts.RemoveRange(await destination.Forecasts.ToListAsync());
            destination.Accounts.RemoveRange(await destination.Accounts.ToListAsync());
            destination.IncomeExpenses.RemoveRange(await destination.IncomeExpenses.ToListAsync());
            destination.MainMenuCards.RemoveRange(await destination.MainMenuCards.ToListAsync());
            destination.RoadMaps.RemoveRange(await destination.RoadMaps.ToListAsync());
            await destination.SaveChangesAsync();

            // Copy RoadMaps first and track old ID -> new ID for FK remapping
            var roadMapIdMap = new Dictionary<int, int>();
            var sourceRoadMaps = await source.RoadMaps.AsNoTracking().ToListAsync();
            foreach (var src in sourceRoadMaps)
            {
                var oldId = src.Id;
                var newItem = CloneWithoutId(src);
                destination.RoadMaps.Add(newItem);
                await destination.SaveChangesAsync();
                roadMapIdMap[oldId] = newItem.Id;
            }

            // Copy MainMenuCards (no FK dependency)
            var sourceCards = await source.MainMenuCards.AsNoTracking().ToListAsync();
            foreach (var src in sourceCards)
            {
                destination.MainMenuCards.Add(CloneWithoutId(src));
            }
            if (sourceCards.Count > 0) await destination.SaveChangesAsync();

            // Copy IncomeExpenses (remap RoadMapID)
            var sourceIncome = await source.IncomeExpenses.AsNoTracking().ToListAsync();
            foreach (var src in sourceIncome)
            {
                var newItem = CloneWithoutId(src);
                if (roadMapIdMap.TryGetValue(src.RoadMapID, out var newRoadMapId))
                    newItem.RoadMapID = newRoadMapId;
                destination.IncomeExpenses.Add(newItem);
            }
            if (sourceIncome.Count > 0) await destination.SaveChangesAsync();

            // Copy Accounts (remap RoadMapID)
            var sourceAccounts = await source.Accounts.AsNoTracking().ToListAsync();
            foreach (var src in sourceAccounts)
            {
                var newItem = CloneWithoutId(src);
                if (roadMapIdMap.TryGetValue(src.RoadMapID, out var newRoadMapId))
                    newItem.RoadMapID = newRoadMapId;
                destination.Accounts.Add(newItem);
            }
            if (sourceAccounts.Count > 0) await destination.SaveChangesAsync();

            // Copy Forecasts (no FK dependency)
            var sourceForecasts = await source.Forecasts.AsNoTracking().ToListAsync();
            foreach (var src in sourceForecasts)
            {
                destination.Forecasts.Add(CloneWithoutId(src));
            }
            if (sourceForecasts.Count > 0) await destination.SaveChangesAsync();
        }

        /// <summary>
        /// Creates a shallow copy of an entity with the Id property reset to 0,
        /// so EF Core treats it as a new entity and lets the DB generate the key.
        /// All other properties are preserved.
        /// </summary>
        private static T CloneWithoutId<T>(T source) where T : new()
        {
            var clone = new T();
            foreach (var prop in typeof(T).GetProperties())
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                if (prop.Name == "Id") continue;
                prop.SetValue(clone, prop.GetValue(source));
            }
            return clone;
        }

        private AppDbContext CreateRemoteContext()
        {
            var connectionString = DbServiceFactory.GetRemoteConnectionString();
            var dbType = DbServiceFactory.GetRemoteDbType();
            if (string.IsNullOrWhiteSpace(connectionString)) return null;

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

            var ctx = new AppDbContext(optionsBuilder.Options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        private AppDbContext CreateLocalContext()
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "FinancialForeCast.db");
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;
            var ctx = new AppDbContext(options);
            ctx.Database.EnsureCreated();
            return ctx;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            StopBackupSchedule();
        }
    }
}
