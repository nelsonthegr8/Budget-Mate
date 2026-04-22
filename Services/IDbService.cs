using Financial_ForeCast.Models;

namespace Financial_ForeCast.Services
{
    public interface IDbService
    {
        Dictionary<string, string> Cards { get; }
        string[] MonthAbbreviations { get; }

        // Main Menu Cards
        Task<List<MainMenuCards>> GetMainMenuCards();
        Task AddMainMenuCard(MainMenuCards card);
        Task UpdateMainMenuCard(MainMenuCards card);
        Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards);

        // Income And Expense
        Task<List<IncomeExpense>> GetAllIncomesAndExpenses();
        Task<List<IncomeExpense>> GetIncome();
        Task<List<IncomeExpense>> GetExpenses();
        Task<double> GetRoadMapSum(int rMapID);
        Task<double> GetRoadMapNetWorthSum(int rMapID);
        Task AddIncomeExpense(IncomeExpense incomeExpense);
        Task<IncomeExpense> RemoveByIncomeExpenseByID(int id);
        Task UpdateIncomeExpense(IncomeExpense incomeExpense);

        // RoadMap
        Task<List<RoadMaps>> GetRoadMaps();
        Task AddRoadMap(RoadMaps roadMap);
        Task<RoadMaps> RemoveRoadMap(int id);
        Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount);
        Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount);
        Task<int> GetSelectedRoadMapID();

        // Account
        Task<List<Accnts>> GetAccounts();
        Task AddAccount(Accnts accnt);
        Task RemoveAccount(int id);

        // Forecast
        Task<List<Forecasts>> GetForecasts();
        Task AddForecasts(List<Forecasts> forecast);
        Task RemoveForecasts(string forecastName);
        Task UpdateForecasts(Forecasts forecast);
        Task<List<Forecasts>> GetForecastsByName(string forecastName);
        Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear();
        Task<List<string>> GetSavedForecastNames();
    }
}
