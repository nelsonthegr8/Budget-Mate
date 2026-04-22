namespace Financial_ForeCast.Services
{
    public class DbServiceFactory
    {
        public const string ConnectionStringKey = "RemoteDbConnectionString";
        public const string DbTypeKey = "RemoteDbType";

        public IDbService Create()
        {
            var connectionString = Preferences.Get(ConnectionStringKey, string.Empty);
            var dbTypeStr = Preferences.Get(DbTypeKey, string.Empty);

            if (!string.IsNullOrWhiteSpace(connectionString) && !string.IsNullOrWhiteSpace(dbTypeStr))
            {
                try
                {
                    var dbType = Enum.Parse<RemoteDbType>(dbTypeStr);
                    return new RemoteDbService(connectionString, dbType);
                }
                catch
                {
                    // Fall back to local if the remote connection fails
                    return new LocalDbService();
                }
            }

            return new LocalDbService();
        }

        public static bool HasRemoteConnectionString()
        {
            return !string.IsNullOrWhiteSpace(Preferences.Get(ConnectionStringKey, string.Empty));
        }

        public static void SetRemoteConnectionString(string connectionString, RemoteDbType dbType)
        {
            Preferences.Set(ConnectionStringKey, connectionString);
            Preferences.Set(DbTypeKey, dbType.ToString());
        }

        public static void ClearRemoteConnectionString()
        {
            Preferences.Remove(ConnectionStringKey);
            Preferences.Remove(DbTypeKey);
        }

        public static string GetRemoteConnectionString()
        {
            return Preferences.Get(ConnectionStringKey, string.Empty);
        }

        public static RemoteDbType GetRemoteDbType()
        {
            var dbTypeStr = Preferences.Get(DbTypeKey, nameof(RemoteDbType.PostgreSQL));
            return Enum.TryParse<RemoteDbType>(dbTypeStr, out var result) ? result : RemoteDbType.PostgreSQL;
        }
    }
}
