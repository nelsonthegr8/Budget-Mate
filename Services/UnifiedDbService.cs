using Financial_ForeCast.Models;
using SQLite;
using System.Data.Common;
using System.Data;

namespace Financial_ForeCast.Services
{
    /// <summary>
    /// Unified database service that works with both local SQLite and server databases.
    /// Uses the same business logic regardless of connection source.
    /// </summary>
    public class UnifiedDbService : IDbService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly SQLiteAsyncConnection? _sqliteConnection; // For local SQLite only
        
        // Common table info
        protected readonly Dictionary<string, string> cards = new Dictionary<string, string>
        {
            { "Net Worth", "/accountvalues" },
            { "Income", "/income" },
            { "Expenses", "/expense" },
            { "Financial Forecast", "/forecast" }
        };

        protected readonly string[] monthAbbreviations =
        {
            "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
        };

        // Constructor for local SQLite
        public UnifiedDbService()
        {
            _connectionFactory = new LocalDbConnectionFactory();
            _sqliteConnection = new SQLiteAsyncConnection(
                Path.Combine(FileSystem.AppDataDirectory, "FinancialForeCast.db"));
            InitializeTables();
        }

        // Constructor for server database
        public UnifiedDbService(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        private void InitializeTables()
        {
            if (_sqliteConnection != null)
            {
                _sqliteConnection.CreateTableAsync<IncomeExpense>().Wait();
                _sqliteConnection.CreateTableAsync<RoadMaps>().Wait();
                _sqliteConnection.CreateTableAsync<Accnts>().Wait();
                _sqliteConnection.CreateTableAsync<MainMenuCards>().Wait();
                _sqliteConnection.CreateTableAsync<Forecasts>().Wait();
            }
        }

        #region Main Menu Cards Section
        public async Task<List<MainMenuCards>> GetMainMenuCards()
        {
            var result = await GetMainMenuCardsInternal();
            
            if (result.Count == 0 && _sqliteConnection != null)
            {
                foreach (var card in cards)
                {
                    var nCard = new MainMenuCards 
                    { 
                        Name = card.Key, 
                        Link = card.Value, 
                        Amount = 0, 
                        Passthrough = "{}" 
                    };
                    await AddMainMenuCard(nCard);
                    result.Add(nCard);
                }
            }
            
            var updatedCards = await GenerateCardCalculations(result);
            return updatedCards;
        }

        private async Task<List<MainMenuCards>> GetMainMenuCardsInternal()
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.Table<MainMenuCards>().ToListAsync();
            }
            else
            {
                return await ExecuteQueryAsync<MainMenuCards>(
                    "SELECT * FROM MainMenuCards", 
                    r => new MainMenuCards 
                    { 
                        Id = (int)r["Id"],
                        Name = (string)r["Name"],
                        Link = (string)r["Link"],
                        Amount = (double)r["Amount"],
                        Passthrough = (string)r["Passthrough"]
                    });
            }
        }

        public async Task AddMainMenuCard(MainMenuCards card)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.InsertAsync(card);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "INSERT INTO MainMenuCards (Name, Link, Amount, Passthrough) VALUES (@Name, @Link, @Amount, @Passthrough)",
                    new { card.Name, card.Link, card.Amount, card.Passthrough });
            }
        }

        public async Task UpdateMainMenuCard(MainMenuCards card)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.UpdateAsync(card);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "UPDATE MainMenuCards SET Name=@Name, Link=@Link, Amount=@Amount, Passthrough=@Passthrough WHERE Id=@Id",
                    new { card.Name, card.Link, card.Amount, card.Passthrough, card.Id });
            }
        }

        public async Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards)
        {
            foreach (var card in cards) 
            {
                if (card.Name == "Net Worth")
                {
                    var netWorth = await GetRoadMapNetWorthSum(await GetSelectedRoadMapID());
                    card.Amount = netWorth;
                    await UpdateMainMenuCard(card);
                }
                else if (card.Name == "Income")
                {
                    var income = await GetIncome();
                    double totalIncome = income.Sum(x => x.Amount);
                    card.Amount = totalIncome;
                    await UpdateMainMenuCard(card);
                }
                else if (card.Name == "Expenses")
                {
                    var expenses = await GetExpenses();
                    double totalExpenses = expenses.Sum(x => x.Amount);
                    card.Amount = totalExpenses;
                    await UpdateMainMenuCard(card);
                }
            }
            return cards;
        }
        #endregion

        #region Income And Expense Section
        public async Task<List<IncomeExpense>> GetAllIncomesAndExpenses()
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.Table<IncomeExpense>().ToListAsync();
            }
            else
            {
                return await ExecuteQueryAsync<IncomeExpense>(
                    "SELECT * FROM IncomeExpense",
                    r => MapIncomeExpense(r));
            }
        }

        public async Task<List<IncomeExpense>> GetIncome()
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.QueryAsync<IncomeExpense>("SELECT * FROM IncomeExpense WHERE Type = 'Income'");
            }
            else
            {
                return await ExecuteQueryAsync<IncomeExpense>(
                    "SELECT * FROM IncomeExpense WHERE Type = @Type",
                    r => MapIncomeExpense(r),
                    new { Type = "Income" });
            }
        }

        public async Task<List<IncomeExpense>> GetExpenses()
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.QueryAsync<IncomeExpense>("SELECT * FROM IncomeExpense WHERE Type = 'Expense'");
            }
            else
            {
                return await ExecuteQueryAsync<IncomeExpense>(
                    "SELECT * FROM IncomeExpense WHERE Type = @Type",
                    r => MapIncomeExpense(r),
                    new { Type = "Expense" });
            }
        }

        public async Task<double> GetRoadMapSum(int rMapID)
        {
            if (_sqliteConnection != null)
            {
                var queryResult = await _sqliteConnection.ExecuteScalarAsync<double>(
                    "SELECT SUM(CASE WHEN Type = 'Income' THEN Amount ELSE Amount * -1 END) FROM IncomeExpense WHERE RoadMapID = ?", rMapID);
                return queryResult ?? 0;
            }
            else
            {
                var result = await ExecuteScalarAsync<double>(
                    "SELECT SUM(CASE WHEN Type = 'Income' THEN Amount ELSE Amount * -1 END) FROM IncomeExpense WHERE RoadMapID = @RoadMapID",
                    new { RoadMapID = rMapID });
                return result ?? 0;
            }
        }

        public async Task<double> GetRoadMapNetWorthSum(int rMapID)
        {
            if (_sqliteConnection != null)
            {
                var queryResult = await _sqliteConnection.ExecuteScalarAsync<double>(
                    "SELECT SUM(CASE WHEN Type IN ('Savings','Checkings','Asset Value') THEN Amount ELSE Amount * -1 END) FROM Accounts WHERE RoadMapID = ?", rMapID);
                return queryResult ?? 0;
            }
            else
            {
                var result = await ExecuteScalarAsync<double>(
                    "SELECT SUM(CASE WHEN Type IN ('Savings','Checkings','Asset Value') THEN Amount ELSE Amount * -1 END) FROM Accounts WHERE RoadMapID = @RoadMapID",
                    new { RoadMapID = rMapID });
                return result ?? 0;
            }
        }

        public async Task AddIncomeExpense(IncomeExpense incomeExpense)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.InsertAsync(incomeExpense);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "INSERT INTO IncomeExpense (Type, Amount, Description, Date, RoadMapID, Category) " +
                    "VALUES (@Type, @Amount, @Description, @Date, @RoadMapID, @Category)",
                    new { 
                        incomeExpense.Type, 
                        incomeExpense.Amount, 
                        incomeExpense.Description, 
                        incomeExpense.Date, 
                        incomeExpense.RoadMapID,
                        incomeExpense.Category 
                    });
            }
            
            double roadMapAmount = await GetRoadMapSum(incomeExpense.RoadMapID);
            await UpdateRoadMapAmount(incomeExpense.RoadMapID, roadMapAmount);
        }

        public async Task<IncomeExpense> RemoveByIncomeExpenseByID(int id)
        {
            IncomeExpense? incomeExpense = null;
            
            if (_sqliteConnection != null)
            {
                incomeExpense = await _sqliteConnection.Table<IncomeExpense>().Where(x => x.Id == id).FirstOrDefaultAsync();
                if (incomeExpense != null)
                {
                    await _sqliteConnection.DeleteAsync(incomeExpense);
                }
            }
            else
            {
                incomeExpense = (await ExecuteQueryAsync<IncomeExpense>(
                    "SELECT * FROM IncomeExpense WHERE Id = @Id",
                    r => MapIncomeExpense(r),
                    new { Id = id })).FirstOrDefault();
                    
                if (incomeExpense != null)
                {
                    await ExecuteNonQueryAsync(
                        "DELETE FROM IncomeExpense WHERE Id = @Id",
                        new { Id = id });
                }
            }
            
            if (incomeExpense != null)
            {
                double roadMapAmount = await GetRoadMapSum(incomeExpense.RoadMapID);
                await UpdateRoadMapAmount(incomeExpense.RoadMapID, roadMapAmount);
            }
            
            return incomeExpense;
        }

        public async Task UpdateIncomeExpense(IncomeExpense incomeExpense)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.UpdateAsync(incomeExpense);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "UPDATE IncomeExpense SET Type=@Type, Amount=@Amount, Description=@Description, " +
                    "Date=@Date, RoadMapID=@RoadMapID, Category=@Category WHERE Id=@Id",
                    new { 
                        incomeExpense.Type, 
                        incomeExpense.Amount, 
                        incomeExpense.Description, 
                        incomeExpense.Date, 
                        incomeExpense.RoadMapID,
                        incomeExpense.Category,
                        incomeExpense.Id 
                    });
            }
        }
        #endregion

        #region RoadMap Section
        public async Task<List<RoadMaps>> GetRoadMaps()
        {
            var result = await GetRoadMapsInternal();
            
            if (result.Count == 0 && _sqliteConnection != null)
            {
                var nRoadMap = new RoadMaps 
                { 
                    RoadMapName = "Default", 
                    RoadMapSavingAmount = 0, 
                    isSelected = true 
                };
                await AddRoadMap(nRoadMap);
                result.Add(nRoadMap);
            }
            
            return result;
        }

        private async Task<List<RoadMaps>> GetRoadMapsInternal()
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.Table<RoadMaps>().ToListAsync();
            }
            else
            {
                return await ExecuteQueryAsync<RoadMaps>(
                    "SELECT * FROM RoadMaps",
                    r => MapRoadMap(r));
            }
        }

        public async Task AddRoadMap(RoadMaps roadMap)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.InsertAsync(roadMap);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "INSERT INTO RoadMaps (RoadMapName, RoadMapSavingAmount, isSelected, RoadMapPrevSavingAmount, NetWorth, PrevNetWorth) " +
                    "VALUES (@RoadMapName, @RoadMapSavingAmount, @isSelected, @RoadMapPrevSavingAmount, @NetWorth, @PrevNetWorth)",
                    new { 
                        roadMap.RoadMapName, 
                        roadMap.RoadMapSavingAmount, 
                        roadMap.isSelected,
                        roadMap.RoadMapPrevSavingAmount ?? 0,
                        roadMap.NetWorth ?? 0,
                        roadMap.PrevNetWorth ?? 0
                    });
            }
        }

        public async Task<RoadMaps> RemoveRoadMap(int id)
        {
            var rMaps = await GetRoadMapsInternal().Then(r => r.Where(x => x.Id == id).FirstOrDefaultAsync()).Result;
            
            if (rMaps != null)
            {
                if (_sqliteConnection != null)
                {
                    await _sqliteConnection.ExecuteAsync("DELETE FROM IncomeExpense WHERE RoadMapID = ?", id);
                    await _sqliteConnection.DeleteAsync(rMaps);
                }
                else
                {
                    await ExecuteNonQueryAsync("DELETE FROM IncomeExpense WHERE RoadMapID = @Id", new { Id = id });
                    await ExecuteNonQueryAsync("DELETE FROM RoadMaps WHERE Id = @Id", new { Id = id });
                }
            }
            
            return rMaps;
        }

        public async Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.ExecuteAsync(
                    "UPDATE RoadMaps SET RoadMapPrevSavingAmount = RoadMapSavingAmount, RoadMapSavingAmount = ? WHERE Id = ?",
                    roadMapAmount, roadMapID);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "UPDATE RoadMaps SET RoadMapPrevSavingAmount = RoadMapSavingAmount, RoadMapSavingAmount = @RoadMapAmount WHERE Id = @Id",
                    new { RoadMapAmount = roadMapAmount, Id = roadMapID });
            }
        }

        public async Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.ExecuteAsync(
                    "UPDATE RoadMaps SET PrevNetWorth = NetWorth, NetWorth = ? WHERE Id = ?",
                    roadMapNetWorthAmount, roadMapID);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "UPDATE RoadMaps SET PrevNetWorth = NetWorth, NetWorth = @NetWorth WHERE Id = @Id",
                    new { NetWorth = roadMapNetWorthAmount, Id = roadMapID });
            }
        }

        public async Task<int> GetSelectedRoadMapID()
        {
            if (_sqliteConnection != null)
            {
                var result = await _sqliteConnection.Table<RoadMaps>().Where(x => x.isSelected == true).FirstOrDefaultAsync();
                return result?.Id ?? 0;
            }
            else
            {
                var result = await ExecuteQueryAsync<RoadMaps>(
                    "SELECT * FROM RoadMaps WHERE isSelected = 1",
                    r => MapRoadMap(r));
                return result.FirstOrDefault()?.Id ?? 0;
            }
        }
        #endregion

        #region Account Section
        public async Task<List<Accnts>> GetAccounts()
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.Table<Accnts>().ToListAsync();
            }
            else
            {
                return await ExecuteQueryAsync<Accnts>(
                    "SELECT * FROM Accounts",
                    r => MapAccount(r));
            }
        }

        public async Task AddAccount(Accnts accnt)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.InsertAsync(accnt);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "INSERT INTO Accounts (Type, Amount, RoadMapID, Name) VALUES (@Type, @Amount, @RoadMapID, @Name)",
                    new { accnt.Type, accnt.Amount, accnt.RoadMapID, accnt.Name });
            }
            
            double roadMapNetWorthAmount = await GetRoadMapNetWorthSum(accnt.RoadMapID);
            await UpdateRoadNetWorthAmount(accnt.RoadMapID, roadMapNetWorthAmount);
        }

        public async Task RemoveAccount(int id)
        {
            Accnts? accnt = null;
            
            if (_sqliteConnection != null)
            {
                accnt = await _sqliteConnection.Table<Accnts>().Where(x => x.Id == id).FirstOrDefaultAsync();
                if (accnt != null)
                {
                    await _sqliteConnection.DeleteAsync(accnt);
                }
            }
            else
            {
                accnt = (await ExecuteQueryAsync<Accnts>(
                    "SELECT * FROM Accounts WHERE Id = @Id",
                    r => MapAccount(r),
                    new { Id = id })).FirstOrDefault();
                    
                if (accnt != null)
                {
                    await ExecuteNonQueryAsync("DELETE FROM Accounts WHERE Id = @Id", new { Id = id });
                }
            }
            
            if (accnt != null)
            {
                double roadMapNetWorthAmount = await GetRoadMapNetWorthSum(accnt.RoadMapID);
                await UpdateRoadNetWorthAmount(accnt.RoadMapID, roadMapNetWorthAmount);
            }
        }
        #endregion

        #region Forecast Section
        public async Task<List<Forecasts>> GetForecasts()
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.Table<Forecasts>().ToListAsync();
            }
            else
            {
                return await ExecuteQueryAsync<Forecasts>(
                    "SELECT * FROM Forecasts",
                    r => MapForecast(r));
            }
        }

        public async Task AddForecasts(List<Forecasts> forecast)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.InsertAllAsync(forecast);
            }
            else
            {
                foreach (var f in forecast)
                {
                    await ExecuteNonQueryAsync(
                        "INSERT INTO Forecasts (ForcastName, Month, Year, Amount, Type) " +
                        "VALUES (@ForcastName, @Month, @Year, @Amount, @Type)",
                        new { f.ForcastName, f.Month, f.Year, f.Amount, f.Type });
                }
            }
        }

        public async Task RemoveForecasts(string forecastName)
        {
            if (_sqliteConnection != null)
            {
                var forecasts = await _sqliteConnection.Table<Forecasts>().Where(x => x.ForcastName == forecastName).ToListAsync();
                if (forecasts != null && forecasts.Count > 0)
                {
                    await _sqliteConnection.DeleteAllAsync(forecasts);
                }
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "DELETE FROM Forecasts WHERE ForcastName = @Name",
                    new { Name = forecastName });
            }
        }

        public async Task UpdateForecasts(Forecasts forecast)
        {
            if (_sqliteConnection != null)
            {
                await _sqliteConnection.UpdateAsync(forecast);
            }
            else
            {
                await ExecuteNonQueryAsync(
                    "UPDATE Forecasts SET ForcastName=@ForcastName, Month=@Month, Year=@Year, Amount=@Amount, Type=@Type WHERE Id=@Id",
                    new { forecast.ForcastName, forecast.Month, forecast.Year, forecast.Amount, forecast.Type, forecast.Id });
            }
        }

        public async Task<List<Forecasts>> GetForecastsByName(string forecastName)
        {
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.Table<Forecasts>().Where(x => x.ForcastName == forecastName).ToListAsync();
            }
            else
            {
                return await ExecuteQueryAsync<Forecasts>(
                    "SELECT * FROM Forecasts WHERE ForcastName = @Name",
                    r => MapForecast(r),
                    new { Name = forecastName });
            }
        }

        public async Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear()
        {
            DateTime today = DateTime.Today;
            string month = monthAbbreviations[today.Month - 1];
            int year = today.Year;
            
            if (_sqliteConnection != null)
            {
                return await _sqliteConnection.Table<Forecasts>().Where(x => x.Month == month && x.Year == year).ToListAsync();
            }
            else
            {
                return await ExecuteQueryAsync<Forecasts>(
                    "SELECT * FROM Forecasts WHERE Month = @Month AND Year = @Year",
                    r => MapForecast(r),
                    new { Month = month, Year = year });
            }
        }
        #endregion

        #region Helper Methods for Server Database
        private async Task<List<T>> ExecuteQueryAsync<T>(string sql, Func<IDataReader, T> mapper, object? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (parameters != null)
            {
                AddParameters(command, parameters);
            }
            
            using var reader = await command.ExecuteReaderAsync();
            var results = new List<T>();
            
            while (await reader.ReadAsync())
            {
                results.Add(mapper(reader));
            }
            
            return results;
        }

        private async Task<T?> ExecuteScalarAsync<T>(string sql, object? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (parameters != null)
            {
                AddParameters(command, parameters);
            }
            
            var result = await command.ExecuteScalarAsync();
            return result == DBNull.Value ? default : (T?)Convert.ChangeType(result, typeof(T));
        }

        private async Task ExecuteNonQueryAsync(string sql, object? parameters = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            await connection.OpenAsync();
            
            using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (parameters != null)
            {
                AddParameters(command, parameters);
            }
            
            await command.ExecuteNonQueryAsync();
        }

        private void AddParameters(IDbCommand command, object parameters)
        {
            var properties = parameters.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(parameters);
                var param = command.CreateParameter();
                param.ParameterName = "@" + prop.Name;
                param.Value = value ?? DBNull.Value;
                command.Parameters.Add(param);
            }
        }

        private IncomeExpense MapIncomeExpense(IDataReader reader)
        {
            return new IncomeExpense
            {
                Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                Type = reader["Type"]?.ToString(),
                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDouble(reader["Amount"]) : 0,
                Description = reader["Description"]?.ToString(),
                Date = reader["Date"] != DBNull.Value ? Convert.ToDateTime(reader["Date"]) : DateTime.Now,
                RoadMapID = reader["RoadMapID"] != DBNull.Value ? Convert.ToInt32(reader["RoadMapID"]) : 0,
                Category = reader["Category"]?.ToString()
            };
        }

        private RoadMaps MapRoadMap(IDataReader reader)
        {
            return new RoadMaps
            {
                Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                RoadMapName = reader["RoadMapName"]?.ToString(),
                RoadMapSavingAmount = reader["RoadMapSavingAmount"] != DBNull.Value ? Convert.ToDouble(reader["RoadMapSavingAmount"]) : 0,
                isSelected = reader["isSelected"] != DBNull.Value && Convert.ToBoolean(reader["isSelected"]),
                RoadMapPrevSavingAmount = reader["RoadMapPrevSavingAmount"] != DBNull.Value ? Convert.ToDouble(reader["RoadMapPrevSavingAmount"]) : null,
                NetWorth = reader["NetWorth"] != DBNull.Value ? Convert.ToDouble(reader["NetWorth"]) : null,
                PrevNetWorth = reader["PrevNetWorth"] != DBNull.Value ? Convert.ToDouble(reader["PrevNetWorth"]) : null
            };
        }

        private Accnts MapAccount(IDataReader reader)
        {
            return new Accnts
            {
                Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                Type = reader["Type"]?.ToString(),
                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDouble(reader["Amount"]) : 0,
                RoadMapID = reader["RoadMapID"] != DBNull.Value ? Convert.ToInt32(reader["RoadMapID"]) : 0,
                Name = reader["Name"]?.ToString()
            };
        }

        private Forecasts MapForecast(IDataReader reader)
        {
            return new Forecasts
            {
                Id = reader["Id"] != DBNull.Value ? Convert.ToInt32(reader["Id"]) : 0,
                ForcastName = reader["ForcastName"]?.ToString(),
                Month = reader["Month"]?.ToString(),
                Year = reader["Year"] != DBNull.Value ? Convert.ToInt32(reader["Year"]) : 0,
                Amount = reader["Amount"] != DBNull.Value ? Convert.ToDouble(reader["Amount"]) : 0,
                Type = reader["Type"]?.ToString()
            };
        }
        #endregion
    }
}
