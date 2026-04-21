# Database Connection Guide

## Overview
Budget-Mate supports both local SQLite and server databases. The same business logic works regardless of which database you choose.

## Configuration

Edit `appsettings.json` to configure your database:

### Local SQLite (Default)
```json
{
  "UseServerDatabase": false,
  "ServerDatabaseConnectionString": ""
}
```
No additional configuration needed. Data is stored on the device.

### SQL Server
```json
{
  "UseServerDatabase": true,
  "ServerDatabaseConnectionString": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

### PostgreSQL
```json
{
  "UseServerDatabase": true,
  "ServerDatabaseConnectionString": "Host=YOUR_HOST;Port=5432;Database=YOUR_DB;Username=YOUR_USER;Password=YOUR_PASSWORD;"
}
```

### MySQL
```json
{
  "UseServerDatabase": true,
  "ServerDatabaseConnectionString": "Server=YOUR_SERVER;Database=YOUR_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;"
}
```

### Remote SQLite
```json
{
  "UseServerDatabase": true,
  "ServerDatabaseConnectionString": "Data Source=/path/to/remote/database.db"
}
```

## Runtime Switching

You can switch databases at runtime using the `DbServiceFactory`:

```csharp
// Inject DbServiceFactory into your service/page
private readonly DbServiceFactory _dbFactory;

// Switch to server database
_dbFactory.SwitchToServer("Server=...;Database=...;");

// Switch back to local
_dbFactory.SwitchToLocal();
```

## Required NuGet Packages

For server databases, install the appropriate package:

- **SQL Server**: `System.Data.SqlClient` or `Microsoft.Data.SqlClient`
- **PostgreSQL**: `Npgsql`
- **MySQL**: `MySql.Data` or `Pomelo.EntityFrameworkCore.MySql`
- **SQLite**: `Microsoft.Data.Sqlite` (already included)

## Security Notes

- Connection strings contain credentials - protect your `appsettings.json`
- Consider using environment variables for production
- Use SSL/TLS for remote database connections
- Implement proper authentication before allowing database access
