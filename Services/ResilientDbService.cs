using System.Data.Common;
using Financial_ForeCast.Models;

namespace Financial_ForeCast.Services
{
    /// <summary>
    /// A wrapper around IDbService that catches remote DB connection failures
    /// and automatically triggers a fallback to the local database.
    /// </summary>
    public class ResilientDbService : IDbService
    {
        private readonly IDbService _inner;
        private readonly Action _onConnectionFailed;
        private readonly Func<IDbService> _fallbackResolver;

        public ResilientDbService(IDbService inner, Action onConnectionFailed, Func<IDbService> fallbackResolver)
        {
            _inner = inner;
            _onConnectionFailed = onConnectionFailed;
            _fallbackResolver = fallbackResolver;
        }

        public Dictionary<string, string> Cards => _inner.Cards;
        public string[] MonthAbbreviations => _inner.MonthAbbreviations;

        // Main Menu Cards
        public Task<List<MainMenuCards>> GetMainMenuCards() => Execute(() => _inner.GetMainMenuCards(), fb => fb.GetMainMenuCards());
        public Task AddMainMenuCard(MainMenuCards card) => Execute(() => _inner.AddMainMenuCard(card), fb => fb.AddMainMenuCard(card));
        public Task UpdateMainMenuCard(MainMenuCards card) => Execute(() => _inner.UpdateMainMenuCard(card), fb => fb.UpdateMainMenuCard(card));
        public Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards) => Execute(() => _inner.GenerateCardCalculations(cards), fb => fb.GenerateCardCalculations(cards));

        // Income And Expense
        public Task<List<IncomeExpense>> GetAllIncomesAndExpenses() => Execute(() => _inner.GetAllIncomesAndExpenses(), fb => fb.GetAllIncomesAndExpenses());
        public Task<List<IncomeExpense>> GetIncome() => Execute(() => _inner.GetIncome(), fb => fb.GetIncome());
        public Task<List<IncomeExpense>> GetExpenses() => Execute(() => _inner.GetExpenses(), fb => fb.GetExpenses());
        public Task<double> GetRoadMapSum(int rMapID) => Execute(() => _inner.GetRoadMapSum(rMapID), fb => fb.GetRoadMapSum(rMapID));
        public Task<double> GetRoadMapNetWorthSum(int rMapID) => Execute(() => _inner.GetRoadMapNetWorthSum(rMapID), fb => fb.GetRoadMapNetWorthSum(rMapID));
        public Task AddIncomeExpense(IncomeExpense incomeExpense) => Execute(() => _inner.AddIncomeExpense(incomeExpense), fb => fb.AddIncomeExpense(incomeExpense));
        public Task<IncomeExpense> RemoveByIncomeExpenseByID(int id) => Execute(() => _inner.RemoveByIncomeExpenseByID(id), fb => fb.RemoveByIncomeExpenseByID(id));
        public Task UpdateIncomeExpense(IncomeExpense incomeExpense) => Execute(() => _inner.UpdateIncomeExpense(incomeExpense), fb => fb.UpdateIncomeExpense(incomeExpense));

        // RoadMap
        public Task<List<RoadMaps>> GetRoadMaps() => Execute(() => _inner.GetRoadMaps(), fb => fb.GetRoadMaps());
        public Task AddRoadMap(RoadMaps roadMap) => Execute(() => _inner.AddRoadMap(roadMap), fb => fb.AddRoadMap(roadMap));
        public Task<RoadMaps> RemoveRoadMap(int id) => Execute(() => _inner.RemoveRoadMap(id), fb => fb.RemoveRoadMap(id));
        public Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount) => Execute(() => _inner.UpdateRoadMapAmount(roadMapID, roadMapAmount), fb => fb.UpdateRoadMapAmount(roadMapID, roadMapAmount));
        public Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount) => Execute(() => _inner.UpdateRoadNetWorthAmount(roadMapID, roadMapNetWorthAmount), fb => fb.UpdateRoadNetWorthAmount(roadMapID, roadMapNetWorthAmount));
        public Task<int> GetSelectedRoadMapID() => Execute(() => _inner.GetSelectedRoadMapID(), fb => fb.GetSelectedRoadMapID());

        // Account
        public Task<List<Accnts>> GetAccounts() => Execute(() => _inner.GetAccounts(), fb => fb.GetAccounts());
        public Task AddAccount(Accnts accnt) => Execute(() => _inner.AddAccount(accnt), fb => fb.AddAccount(accnt));
        public Task RemoveAccount(int id) => Execute(() => _inner.RemoveAccount(id), fb => fb.RemoveAccount(id));

        // Forecast
        public Task<List<Forecasts>> GetForecasts() => Execute(() => _inner.GetForecasts(), fb => fb.GetForecasts());
        public Task AddForecasts(List<Forecasts> forecast) => Execute(() => _inner.AddForecasts(forecast), fb => fb.AddForecasts(forecast));
        public Task RemoveForecasts(string forecastName) => Execute(() => _inner.RemoveForecasts(forecastName), fb => fb.RemoveForecasts(forecastName));
        public Task UpdateForecasts(Forecasts forecast) => Execute(() => _inner.UpdateForecasts(forecast), fb => fb.UpdateForecasts(forecast));
        public Task<List<Forecasts>> GetForecastsByName(string forecastName) => Execute(() => _inner.GetForecastsByName(forecastName), fb => fb.GetForecastsByName(forecastName));
        public Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear() => Execute(() => _inner.GetForecastsByCurrentMonthAndYear(), fb => fb.GetForecastsByCurrentMonthAndYear());
        public Task<List<string>> GetSavedForecastNames() => Execute(() => _inner.GetSavedForecastNames(), fb => fb.GetSavedForecastNames());

        private async Task<T> Execute<T>(Func<Task<T>> operation, Func<IDbService, Task<T>> fallbackOperation)
        {
            try
            {
                return await operation();
            }
            catch (Exception ex) when (IsConnectionException(ex))
            {
                _onConnectionFailed();
                var fallback = _fallbackResolver();
                return await fallbackOperation(fallback);
            }
        }

        private Task<T> Execute<T>(Func<Task<T>> operation)
        {
            // Overload for calls that don't need retry (will still throw)
            return Execute(operation, _ => throw new InvalidOperationException("No fallback provided."));
        }

        private async Task Execute(Func<Task> operation, Func<IDbService, Task> fallbackOperation)
        {
            try
            {
                await operation();
            }
            catch (Exception ex) when (IsConnectionException(ex))
            {
                _onConnectionFailed();
                var fallback = _fallbackResolver();
                await fallbackOperation(fallback);
            }
        }

        private Task Execute(Func<Task> operation)
        {
            return Execute(operation, _ => throw new InvalidOperationException("No fallback provided."));
        }

        private static bool IsConnectionException(Exception ex)
        {
            // Walk the exception chain for known DB connectivity errors
            var current = ex;
            while (current != null)
            {
                // Common connection-related exception types
                if (current is DbException
                    || current is TimeoutException
                    || current is System.Net.Sockets.SocketException)
                    return true;

                // Check message for common connectivity indicators
                var msg = current.Message?.ToLowerInvariant() ?? "";
                if (msg.Contains("connection")
                    || msg.Contains("timeout")
                    || msg.Contains("network")
                    || msg.Contains("unreachable")
                    || msg.Contains("refused"))
                    return true;

                current = current.InnerException;
            }
            return false;
        }
    }
}
