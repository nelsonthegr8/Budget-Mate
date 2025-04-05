using Financial_ForeCast.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Financial_ForeCast.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "FinancialForeCast.db";
        private readonly SQLiteAsyncConnection _connection;

        public LocalDbService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
            _connection.CreateTableAsync<IncomeExpense>();
            _connection.CreateTableAsync<RoadMaps>();
            _connection.CreateTableAsync<Accnts>();
            _connection.CreateTableAsync<MainMenuCards>();
           initializeMainMenuCards();
        }
        // Main Menu Cards
        private async void initializeMainMenuCards() 
        {
            // Check if table is empty before inserting
            var count = await _connection.Table<MainMenuCards>().CountAsync();
            if (count == 0)
            {
                var initialData = new List<MainMenuCards>
                {
                    new MainMenuCards { CardName = "Income", MenuLink = "/income", TotalAmount = 0 },
                    new MainMenuCards { CardName = "Expenses", MenuLink = "/expense", TotalAmount = 0 },
                    new MainMenuCards { CardName = "Financial Forecast", MenuLink = "/forecast", TotalAmount = 0 },
                    new MainMenuCards {CardName = "Budget Mate", MenuLink = "Go to home screen", TotalAmount = 0}
                };

                await _connection.InsertAllAsync(initialData);
            }
        }
        // Income And Expense 
        public async Task<List<IncomeExpense>> GetAllIncomesAndExpenses()
        {
            var result = await _connection.Table<IncomeExpense>().ToListAsync();
            return result;
        }
        public async Task<List<IncomeExpense>> GetIncome()
        {
            var customQuery = "SELECT * FROM IncomeExpense WHERE Type = 'Income'";
            var result = await _connection.QueryAsync<IncomeExpense>(customQuery);

            return result;
        }

        public async Task<List<IncomeExpense>> GetExpenses()
        {
            var customQuery = "SELECT * FROM IncomeExpense WHERE Type = 'Expense'";
            var result = await _connection.QueryAsync<IncomeExpense>(customQuery);

            return result;
        }
        public async Task<double> GetRoadMapSum(int rMapID)
        {
            var customQuery = "SELECT SUM(CASE WHEN Type = 'Income' THEN Amount ELSE Amount * -1 END) FROM IncomeExpense WHERE RoadMapID = ?";
            var queryResult = await _connection.ExecuteScalarAsync<double>(customQuery, rMapID);
            return queryResult;
        }

        public async Task<double> GetRoadMapNetWorthSum(int rMapID)
        {
            var customQuery = "SELECT SUM(CASE WHEN Type IN ('Savings','Checkings','Asset Value') THEN Amount ELSE Amount * -1 END) FROM Accounts WHERE RoadMapID = ?";
            var queryResult = await _connection.ExecuteScalarAsync<double>(customQuery, rMapID);
            return queryResult;
        }

        public async Task AddIncomeExpense(IncomeExpense incomeExpense)
        {
            await _connection.InsertAsync(incomeExpense);
            double roadMapAmount = await GetRoadMapSum(incomeExpense.RoadMapID);
            await UpdateRoadMapAmount(incomeExpense.RoadMapID, roadMapAmount);
        }

        public async Task<IncomeExpense> RemoveByIncomeExpenseByID(int id)
        {
            var incomeExpense = await _connection.Table<IncomeExpense>().Where(x => x.Id == id).FirstOrDefaultAsync();
            if (incomeExpense != null)
            {
                await _connection.DeleteAsync(incomeExpense);
                double roadMapAmount = await GetRoadMapSum(incomeExpense.RoadMapID);
                await UpdateRoadMapAmount(incomeExpense.RoadMapID, roadMapAmount);
            }
            return incomeExpense;
        }
        public async Task UpdateIncomeExpense(IncomeExpense incomeExpense)
        {
            await _connection.UpdateAsync(incomeExpense);
        }
        // RoadMap 
        public async Task<List<RoadMaps>> GetRoadMaps()
        {
            var result = await _connection.Table<RoadMaps>().ToListAsync();
            if (result.Count == 0)
            {
                var nRoadMap = new RoadMaps { RoadMapName = "Default", RoadMapSavingAmount = 0 };
                await AddRoadMap(nRoadMap);
                result.Add(nRoadMap);
            }
            return result;
        }

        public async Task AddRoadMap(RoadMaps roadMap)
        {
            await _connection.InsertAsync(roadMap);
        }

        public async Task<RoadMaps> RemoveRoadMap(int id)
        {
            var rMaps = await _connection.Table<RoadMaps>().Where(x => x.Id == id).FirstOrDefaultAsync();
            if (rMaps != null)
            {
                var customQuery = "DELETE FROM IncomeExpense WHERE RoadMapID = " + id;
                var result = await _connection.QueryAsync<IncomeExpense>(customQuery);

                await _connection.DeleteAsync(rMaps);
            }
            return rMaps;
        }
        public async Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount)
        {
            var rMaps = await _connection.Table<RoadMaps>().Where(x => x.Id == roadMapID).FirstOrDefaultAsync();
            if (rMaps != null)
            {
                var customQuery = "UPDATE RoadMaps SET RoadMapPrevSavingAmount = RoadMapSavingAmount," +
                    "                                  RoadMapSavingAmount = ? WHERE RoadMapID = " + roadMapID;
                var result = await _connection.QueryAsync<RoadMaps>(customQuery, roadMapAmount);

            }
        }
        public async Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount)
        {
            var rMaps = await _connection.Table<RoadMaps>().Where(x => x.Id == roadMapID).FirstOrDefaultAsync();
            if (rMaps != null)
            {
                var customQuery = "UPDATE RoadMaps SET PrevNetWorth = NetWorth," +
                    "                                  NetWorth = ? WHERE RoadMapID = " + roadMapID;
                var result = await _connection.QueryAsync<RoadMaps>(customQuery, roadMapNetWorthAmount);

            }
        }
        // Account 
        public async Task<List<Accnts>> GetAccounts()
        {
            var result = await _connection.Table<Accnts>().ToListAsync();
            return result;
        }

        public async Task AddAccount(Accnts accnt)
        {
            await _connection.InsertAsync(accnt);
            double roadMapNetWorthAmount = await GetRoadMapNetWorthSum(accnt.RoadMapID);
            await UpdateRoadNetWorthAmount(accnt.RoadMapID, roadMapNetWorthAmount);
        }

        public async Task RemoveAccount(int id)
        {
            var accnt = await _connection.Table<Accnts>().Where(x => x.Id == id).FirstOrDefaultAsync();
            if (accnt != null)
            {
                await _connection.DeleteAsync(accnt);
                double roadMapNetWorthAmount = await GetRoadMapNetWorthSum(accnt.RoadMapID);
                await UpdateRoadNetWorthAmount(accnt.RoadMapID, roadMapNetWorthAmount);
            }
        }

    }
}
