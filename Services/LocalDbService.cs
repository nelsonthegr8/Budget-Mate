using Financial_ForeCast.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Financial_ForeCast.Services
{
    public class LocalDbService
    {
        private const string DB_NAME = "FinancialForeCast.db";
        private readonly SQLiteAsyncConnection _connection;
        public Dictionary<string, string> cards = new Dictionary<string, string> { { "Net Worth", "/accountvalues" },
                                                                               { "Income", "/income" },
                                                                               { "Expenses", "/expense" },
                                                                               { "Financial Forecast", "/forecast" },};
        public string[] monthAbbreviations = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        public LocalDbService()
        {
            _connection = new SQLiteAsyncConnection(Path.Combine(FileSystem.AppDataDirectory, DB_NAME));
            _connection.CreateTableAsync<IncomeExpense>();
            _connection.CreateTableAsync<RoadMaps>();
            _connection.CreateTableAsync<Accnts>();
            _connection.CreateTableAsync<MainMenuCards>();
        }
        // Main Menu Cards Section
        public async Task<List<MainMenuCards>> GetMainMenuCards()
        {
            var result = await _connection.Table<MainMenuCards>().ToListAsync();
            if (result.Count == 0)
            {
                foreach (var card in cards)
                {
                    var nCard = new MainMenuCards { Name = card.Key, Link = card.Value, Amount = 0, Passthrough = "{}" };
                    await AddMainMenuCard(nCard);
                    result.Add(nCard);
                }
            }
            var updatedCards = await GenerateCardCalculations(result);

            return updatedCards;
        }
        public async Task AddMainMenuCard(MainMenuCards card)
        {
            await _connection.InsertAsync(card);
        }
        public async Task UpdateMainMenuCard(MainMenuCards card)
        {
            await _connection.UpdateAsync(card);
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
        // End of Main Menu Cards Section
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
        // End of Income And Expense Section
        // RoadMap 
        public async Task<List<RoadMaps>> GetRoadMaps()
        {
            var result = await _connection.Table<RoadMaps>().ToListAsync();
            if (result.Count == 0)
            {
                var nRoadMap = new RoadMaps { RoadMapName = "Default", RoadMapSavingAmount = 0 , isSelected = true};
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
        public async Task<int> GetSelectedRoadMapID()
        {
            var result = await _connection.Table<RoadMaps>().Where(x => x.isSelected == true).FirstOrDefaultAsync();
            int roadMapID = result == null ? 0 : result.Id;

            return roadMapID;
        }
        //End of RoadMap Section
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
        //End of Account Section
        //Forecast Section
        public async Task<List<Forecasts>> GetForecasts()
        {
            var result = await _connection.Table<Forecasts>().ToListAsync();
            return result;
        }
        public async Task AddForecasts(List<Forecasts> forecast)
        {
            await _connection.InsertAsync(forecast);
        }
        public async Task RemoveForecasts(string forecastName)
        {
            var forecast = await _connection.Table<Forecasts>().Where(x => x.ForcastName == forecastName).ToListAsync();
            if (forecast != null)
            {
                await _connection.DeleteAsync(forecast);
            }
        }
        public async Task UpdateForecasts(Forecasts forecast)
        {
            await _connection.UpdateAsync(forecast);
        }
        public async Task<List<Forecasts>> GetForecastsByName(string forecastName)
        {
            var result = await _connection.Table<Forecasts>().Where(x => x.ForcastName == forecastName).ToListAsync();
            return result;
        }
        public async Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear()
        {
            DateTime today = DateTime.Today;
            string month = monthAbbreviations[today.Month - 1];
            int Year = today.Year;
            var result = await _connection.Table<Forecasts>().Where(x => x.Month == month && x.Year == Year).ToListAsync();
            return result;
        }
        //End of Forecast Section
    }
}
