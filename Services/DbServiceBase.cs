using Financial_ForeCast.Models;
using Microsoft.EntityFrameworkCore;

namespace Financial_ForeCast.Services
{
    public abstract class DbServiceBase : IDbService
    {
        protected AppDbContext _db;

        public Dictionary<string, string> Cards { get; } = new Dictionary<string, string> { { "Net Worth", "/accountvalues" },
                                                                               { "Income", "/income" },
                                                                               { "Expenses", "/expense" },
                                                                               { "Financial Forecast", "/forecast" },};
        public string[] MonthAbbreviations { get; } = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };

        protected void InitializeDatabase()
        {
            _db.Database.EnsureCreated();
        }

        // Main Menu Cards Section
        public async Task<List<MainMenuCards>> GetMainMenuCards()
        {
            var result = await _db.MainMenuCards.OrderBy(c => c.Name == "Net Worth" ? 0 : 1).ToListAsync();
            if (result.Count == 0)
            {
                foreach (var card in Cards)
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
            _db.MainMenuCards.Add(card);
            await _db.SaveChangesAsync();
        }
        public async Task UpdateMainMenuCard(MainMenuCards card)
        {
            _db.MainMenuCards.Update(card);
            await _db.SaveChangesAsync();
        }
        public async Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards)
        {
            foreach (var card in cards)
            {

                if (card.Name == "Net Worth")
                {
                    var netWorth = await GetRoadMapNetWorthSum(await GetSelectedRoadMapID());
                    card.Amount = netWorth;
                    var history = await GetNetWorthHistory();
                    if (history.Count <= 0)
                    {
                        await RecordNetWorthSnapshot(netWorth);
                    }
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
                else if (card.Name == "Financial Forecast")
                {
                    var income = await GetIncome();
                    double totalIncome = income.Sum(x => x.Amount);
                    var expenses = await GetExpenses();
                    double totalExpenses = expenses.Sum(x => x.Amount);
                    card.Amount = totalIncome - totalExpenses;
                    await UpdateMainMenuCard(card);
                }
            }

            return cards;
        }
        // End of Main Menu Cards Section
        // Income And Expense 
        public async Task<List<IncomeExpense>> GetAllIncomesAndExpenses()
        {
            return await _db.IncomeExpenses.ToListAsync();
        }
        public async Task<List<IncomeExpense>> GetIncome()
        {
            return await _db.IncomeExpenses.Where(x => x.Type == "Income").ToListAsync();
        }

        public async Task<List<IncomeExpense>> GetExpenses()
        {
            return await _db.IncomeExpenses.Where(x => x.Type == "Expense").ToListAsync();
        }
        public async Task<double> GetRoadMapSum(int rMapID)
        {
            var items = await _db.IncomeExpenses.Where(x => x.RoadMapID == rMapID).ToListAsync();
            return items.Sum(x => x.Type == "Income" ? x.Amount : x.Amount * -1);
        }

        public async Task<double> GetRoadMapNetWorthSum(int rMapID)
        {
            var positiveTypes = new[] { "Savings", "Checkings", "Asset Value" };
            var items = await _db.Accounts.Where(x => x.RoadMapID == rMapID).ToListAsync();
            return items.Sum(x => positiveTypes.Contains(x.Type) ? x.Amount : x.Amount * -1);
        }

        public async Task AddIncomeExpense(IncomeExpense incomeExpense)
        {
            _db.IncomeExpenses.Add(incomeExpense);
            await _db.SaveChangesAsync();
            double roadMapAmount = await GetRoadMapSum(incomeExpense.RoadMapID);
            await UpdateRoadMapAmount(incomeExpense.RoadMapID, roadMapAmount);
        }

        public async Task<IncomeExpense> RemoveByIncomeExpenseByID(int id)
        {
            var incomeExpense = await _db.IncomeExpenses.FirstOrDefaultAsync(x => x.Id == id);
            if (incomeExpense != null)
            {
                _db.IncomeExpenses.Remove(incomeExpense);
                await _db.SaveChangesAsync();
                double roadMapAmount = await GetRoadMapSum(incomeExpense.RoadMapID);
                await UpdateRoadMapAmount(incomeExpense.RoadMapID, roadMapAmount);
            }
            return incomeExpense;
        }
        public async Task UpdateIncomeExpense(IncomeExpense incomeExpense)
        {
            _db.IncomeExpenses.Update(incomeExpense);
            await _db.SaveChangesAsync();
        }
        // End of Income And Expense Section
        // RoadMap 
        public async Task<List<RoadMaps>> GetRoadMaps()
        {
            var result = await _db.RoadMaps.ToListAsync();
            if (result.Count == 0)
            {
                var nRoadMap = new RoadMaps { RoadMapName = "Default", RoadMapSavingAmount = 0, isSelected = true };
                await AddRoadMap(nRoadMap);
                result.Add(nRoadMap);
            }
            return result;
        }

        public async Task AddRoadMap(RoadMaps roadMap)
        {
            _db.RoadMaps.Add(roadMap);
            await _db.SaveChangesAsync();
        }

        public async Task<RoadMaps> RemoveRoadMap(int id)
        {
            var rMaps = await _db.RoadMaps.FirstOrDefaultAsync(x => x.Id == id);
            if (rMaps != null)
            {
                var relatedItems = await _db.IncomeExpenses.Where(x => x.RoadMapID == id).ToListAsync();
                _db.IncomeExpenses.RemoveRange(relatedItems);
                _db.RoadMaps.Remove(rMaps);
                await _db.SaveChangesAsync();
            }
            return rMaps;
        }
        public async Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount)
        {
            var rMaps = await _db.RoadMaps.FirstOrDefaultAsync(x => x.Id == roadMapID);
            if (rMaps != null)
            {
                rMaps.RoadMapPrevSavingAmount = rMaps.RoadMapSavingAmount;
                rMaps.RoadMapSavingAmount = roadMapAmount;
                _db.RoadMaps.Update(rMaps);
                await _db.SaveChangesAsync();
            }
        }
        public async Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount)
        {
            var rMaps = await _db.RoadMaps.FirstOrDefaultAsync(x => x.Id == roadMapID);
            if (rMaps != null)
            {
                rMaps.PrevNetWorth = rMaps.NetWorth;
                rMaps.NetWorth = roadMapNetWorthAmount;
                _db.RoadMaps.Update(rMaps);
                await _db.SaveChangesAsync();
                await RecordNetWorthSnapshot(roadMapNetWorthAmount);
            }
        }
        public async Task<int> GetSelectedRoadMapID()
        {
            var result = await _db.RoadMaps.FirstOrDefaultAsync(x => x.isSelected == true);
            return result == null ? 0 : result.Id;
        }
        //End of RoadMap Section
        // Account 
        public async Task<List<Accnts>> GetAccounts()
        {
            return await _db.Accounts.ToListAsync();
        }

        public async Task AddAccount(Accnts accnt)
        {
            _db.Accounts.Add(accnt);
            await _db.SaveChangesAsync();
            double roadMapNetWorthAmount = await GetRoadMapNetWorthSum(accnt.RoadMapID);
            await UpdateRoadNetWorthAmount(accnt.RoadMapID, roadMapNetWorthAmount);
        }

        public async Task UpdateAccount(Accnts accnt)
        {
            _db.Accounts.Update(accnt);
            await _db.SaveChangesAsync();
        }

        public async Task RemoveAccount(int id)
        {
            var accnt = await _db.Accounts.FirstOrDefaultAsync(x => x.Id == id);
            if (accnt != null)
            {
                _db.Accounts.Remove(accnt);
                await _db.SaveChangesAsync();
                double roadMapNetWorthAmount = await GetRoadMapNetWorthSum(accnt.RoadMapID);
                await UpdateRoadNetWorthAmount(accnt.RoadMapID, roadMapNetWorthAmount);
            }
        }
        //End of Account Section
        //Forecast Section
        public async Task<List<Forecasts>> GetForecasts()
        {
            return await _db.Forecasts.ToListAsync();
        }
        public async Task AddForecasts(List<Forecasts> forecast)
        {
            _db.Forecasts.AddRange(forecast);
            await _db.SaveChangesAsync();
        }
        public async Task RemoveForecasts(string forecastName)
        {
            var forecast = await _db.Forecasts.Where(x => x.ForcastName == forecastName).ToListAsync();
            if (forecast != null && forecast.Count > 0)
            {
                _db.Forecasts.RemoveRange(forecast);
                await _db.SaveChangesAsync();
            }
        }
        public async Task UpdateForecasts(Forecasts forecast)
        {
            _db.Forecasts.Update(forecast);
            await _db.SaveChangesAsync();
        }
        public async Task<List<Forecasts>> GetForecastsByName(string forecastName)
        {
            return await _db.Forecasts.Where(x => x.ForcastName == forecastName).ToListAsync();
        }
        public async Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear()
        {
            DateTime today = DateTime.Today;
            string month = MonthAbbreviations[today.Month - 1];
            int Year = today.Year;
            return await _db.Forecasts.Where(x => x.Month == month && x.Year == Year).ToListAsync();
        }
        public async Task<List<string>> GetSavedForecastNames()
        {
            return await _db.Forecasts.Select(x => x.ForcastName).Distinct().ToListAsync();
        }
        //End of Forecast Section
        // Net Worth History
        public async Task RecordNetWorthSnapshot(double amount)
        {
            var today = DateTime.Today;
            var todayString = DateTime.Today.ToString();
            var existing = await _db.NetWorthSnapshots
                .FirstOrDefaultAsync(x => x.RecordedDate == todayString);

            if (existing != null)
            {
                existing.Amount = amount;
                _db.NetWorthSnapshots.Update(existing);
            }
            else
            {
                _db.NetWorthSnapshots.Add(new NetWorthSnapshot
                {
                    RecordedDate = today.ToString(),
                    Amount = amount
                });
            }
            await _db.SaveChangesAsync();
        }

        public async Task<List<NetWorthSnapshot>> GetNetWorthHistory()
        {
            return await _db.NetWorthSnapshots
                .OrderBy(x => x.RecordedDate)
                .ToListAsync();
        }
        //End of Net Worth History Section
    }
}
