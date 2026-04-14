using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Financial_ForeCast.Models;

namespace Financial_ForeCast.Services
{
    public abstract class BaseDbService : IDbService
    {
        // Common table info
        protected readonly Dictionary<string, string> cards = new Dictionary<string, string>
        {
            {"Net Worth", "/accountvalues"},
            {"Income", "/income"},
            {"Expenses", "/expense"},
            {"Financial Forecast", "/forecast"}
        };

        protected readonly string[] monthAbbreviations =
        {
            "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"
        };

        // SQL query templates (used by both implementations)
        protected const string QueryIncome = "SELECT * FROM IncomeExpense WHERE Type = 'Income'";
        protected const string QueryExpense = "SELECT * FROM IncomeExpense WHERE Type = 'Expense'";
        protected const string QueryRoadMapSum = "SELECT SUM(CASE WHEN Type = 'Income' THEN Amount ELSE Amount * -1 END) FROM IncomeExpense WHERE RoadMapID = ?";
        protected const string QueryRoadMapNetWorth = "SELECT SUM(CASE WHEN Type IN ('Savings','Checkings','Asset Value') THEN Amount ELSE Amount * -1 END) FROM Accounts WHERE RoadMapID = ?";

        // IDbService members declarations to be overridden
        public abstract Task<List<MainMenuCards>> GetMainMenuCards();
        public abstract Task AddMainMenuCard(MainMenuCards card);
        public abstract Task UpdateMainMenuCard(MainMenuCards card);
        public abstract Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards);
        public abstract Task<List<IncomeExpense>> GetAllIncomesAndExpenses();
        public abstract Task<List<IncomeExpense>> GetIncome();
        public abstract Task<List<IncomeExpense>> GetExpenses();
        public abstract Task<double> GetRoadMapSum(int rMapID);
        public abstract Task<double> GetRoadMapNetWorthSum(int rMapID);
        public abstract Task AddIncomeExpense(IncomeExpense incomeExpense);
        public abstract Task<IncomeExpense> RemoveByIncomeExpenseByID(int id);
        public abstract Task UpdateIncomeExpense(IncomeExpense incomeExpense);
        public abstract Task<List<RoadMaps>> GetRoadMaps();
        public abstract Task AddRoadMap(RoadMaps roadMap);
        public abstract Task<RoadMaps> RemoveRoadMap(int id);
        public abstract Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount);
        public abstract Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount);
        public abstract Task<int> GetSelectedRoadMapID();
        public abstract Task<List<Accnts>> GetAccounts();
        public abstract Task AddAccount(Accnts accnt);
        public abstract Task RemoveAccount(int id);
        public abstract Task<List<Forecasts>> GetForecasts();
        public abstract Task AddForecasts(List<Forecasts> forecast);
        public abstract Task RemoveForecasts(string forecastName);
        public abstract Task UpdateForecasts(Forecasts forecast);
        public abstract Task<List<Forecasts>> GetForecastsByName(string forecastName);
        public abstract Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear();
    }
}
