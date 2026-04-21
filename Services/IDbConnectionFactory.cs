using System.Data.Common;

namespace Financial_ForeCast.Services
{
    /// <summary>
    /// Factory interface for creating database connections.
    /// Allows swapping between SQLite (local) and server databases (SQL Server, PostgreSQL, MySQL).
    /// </summary>
    public interface IDbConnectionFactory
    {
        /// <summary>
        /// Creates a new database connection.
        /// </summary>
        DbConnection CreateConnection();

        /// <summary>
        /// Gets the database type identifier.
        /// </summary>
        string DatabaseType { get; }
    }

    /// <summary>
    /// Creates SQLite connections for local database.
    /// </summary>
    public class LocalDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _dbPath;

        public LocalDbConnectionFactory()
        {
            _dbPath = Path.Combine(FileSystem.AppDataDirectory, "FinancialForeCast.db");
        }

        public string DatabaseType => "SQLite";

        public DbConnection CreateConnection()
        {
            // For SQLite-net compatibility, we'll use the async connection in the service
            // This factory is mainly for type identification
            return null; 
        }
    }

    /// <summary>
    /// Creates connections for server databases based on connection string.
    /// Supports SQL Server, PostgreSQL, MySQL, and SQLite.
    /// </summary>
    public class ServerDbConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public ServerDbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public string DatabaseType { get; private set; }

        public DbConnection CreateConnection()
        {
            // Auto-detect database type from connection string
            if (_connectionString.Contains("Data Source=", StringComparison.OrdinalIgnoreCase) ||
                _connectionString.Contains("DataSource=", StringComparison.OrdinalIgnoreCase))
            {
                DatabaseType = "SQLite";
                return new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
            }
            else if (_connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase) &&
                     _connectionString.Contains("Database=", StringComparison.OrdinalIgnoreCase))
            {
                // Check for SQL Server specific keywords
                if (_connectionString.Contains("Integrated Security") || 
                    _connectionString.Contains("User Id") ||
                    _connectionString.Contains("UID"))
                {
                    DatabaseType = "SQLServer";
                    return new System.Data.SqlClient.SqlConnection(_connectionString);
                }
                else
                {
                    // Assume PostgreSQL
                    DatabaseType = "PostgreSQL";
                    return new Npgsql.NpgsqlConnection(_connectionString);
                }
            }
            else if (_connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase) ||
                     _connectionString.Contains("Port=", StringComparison.OrdinalIgnoreCase))
            {
                DatabaseType = "MySQL";
                return new MySql.Data.MySqlClient.MySqlConnection(_connectionString);
            }

            // Default to SQLite
            DatabaseType = "SQLite";
            return new Microsoft.Data.Sqlite.SqliteConnection(_connectionString);
        }
    }
}
