using System.Collections.Generic;
using System.Threading.Tasks;

namespace Financial_ForeCast.Services
{
    public interface IDbService
    {
        Task<List<MainMenuCards>> GetMainMenuCards();
        Task AddMainMenuCard(MainMenuCards card);
        Task UpdateMainMenuCard(MainMenuCards card);
        Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards);
        Task<List<IncomeExpense>> GetAllIncomesAndExpenses();
        Task<List<IncomeExpense>> GetIncome();
        Task<List<IncomeExpense>> GetExpenses();
        Task<double> GetRoadMapSum(int rMapID);
        Task<double> GetRoadMapNetWorthSum(int rMapID);
        Task AddIncomeExpense(IncomeExpense incomeExpense);
        Task<IncomeExpense> RemoveByIncomeExpenseByID(int id);
        Task UpdateIncomeExpense(IncomeExpense incomeExpense);
        Task<List<RoadMaps>> GetRoadMaps();
        Task AddRoadMap(RoadMaps roadMap);
        Task<RoadMaps> RemoveRoadMap(int id);
        Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount);
        Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount);
        Task<int> GetSelectedRoadMapID();
        Task<List<Accnts>> GetAccounts();
        Task AddAccount(Accnts accnt);
        Task RemoveAccount(int id);
        Task<List<Forecasts>> GetForecasts();
        Task AddForecasts(List<Forecasts> forecast);
        Task RemoveForecasts(string forecastName);
        Task UpdateForecasts(Forecasts forecast);
        Task<List<Forecasts>> GetForecastsByName(string forecastName);
        Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear();
    }
}
