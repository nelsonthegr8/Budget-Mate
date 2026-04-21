using Financial_ForeCast.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace Financial_ForeCast.Services
{
    /// <summary>
    /// Server-based database service that communicates via HTTP REST API.
    /// Implements IDbService interface for seamless swapping with LocalDbService.
    /// </summary>
    public class ServerDbService : BaseDbService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ServerDbService(string serverUrl)
        {
            _baseUrl = serverUrl.TrimEnd('/');
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        #region Main Menu Cards Section
        public override async Task<List<MainMenuCards>> GetMainMenuCards()
            => await GetAsync<List<MainMenuCards>>("/api/cards");

        public override async Task AddMainMenuCard(MainMenuCards card)
            => await _httpClient.PostAsJsonAsync("/api/cards", card);

        public override async Task UpdateMainMenuCard(MainMenuCards card)
            => await _httpClient.PutAsJsonAsync("/api/cards", card);

        public override async Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards)
        {
            // Server handles calculations; return as-is or call recalculation endpoint if needed
            return cards;
        }
        #endregion

        #region Income And Expense Section
        public override async Task<List<IncomeExpense>> GetAllIncomesAndExpenses()
            => await GetAsync<List<IncomeExpense>>("/api/incomeexpense");

        public override async Task<List<IncomeExpense>> GetIncome()
            => await GetAsync<List<IncomeExpense>>("/api/incomeexpense/income");

        public override async Task<List<IncomeExpense>> GetExpenses()
            => await GetAsync<List<IncomeExpense>>("/api/incomeexpense/expense");

        public override async Task<double> GetRoadMapSum(int rMapID)
            => await GetAsync<double>($"/api/roaddata/sum/{rMapID}");

        public override async Task<double> GetRoadMapNetWorthSum(int rMapID)
            => await GetAsync<double>($"/api/roaddata/networth/{rMapID}");

        public override async Task AddIncomeExpense(IncomeExpense incomeExpense)
        {
            await _httpClient.PostAsJsonAsync("/api/incomeexpense", incomeExpense);
            // Server should handle RoadMapAmount updates automatically
        }

        public override async Task<IncomeExpense> RemoveByIncomeExpenseByID(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/incomeexpense/{id}");
            if (response.IsSuccessStatusCode)
            {
                // Server handles RoadMapAmount updates automatically
                return null;
            }
            throw new HttpRequestException($"Failed to delete income/expense with ID {id}");
        }

        public override async Task UpdateIncomeExpense(IncomeExpense incomeExpense)
            => await _httpClient.PutAsJsonAsync("/api/incomeexpense", incomeExpense);
        #endregion

        #region RoadMap Section
        public override async Task<List<RoadMaps>> GetRoadMaps()
            => await GetAsync<List<RoadMaps>>("/api/roads");

        public override async Task AddRoadMap(RoadMaps roadMap)
            => await _httpClient.PostAsJsonAsync("/api/roads", roadMap);

        public override async Task<RoadMaps> RemoveRoadMap(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/roads/{id}");
            if (response.IsSuccessStatusCode)
                return null;
            throw new HttpRequestException($"Failed to delete road map with ID {id}");
        }

        public override async Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount)
            => await _httpClient.PutAsJsonAsync($"/api/roads/amount/{roadMapID}", new { roadMapAmount });

        public override async Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount)
            => await _httpClient.PutAsJsonAsync($"/api/roads/networth/{roadMapID}", new { roadMapNetWorthAmount });

        public override async Task<int> GetSelectedRoadMapID()
            => await GetAsync<int>("/api/roads/selected");
        #endregion

        #region Account Section
        public override async Task<List<Accnts>> GetAccounts()
            => await GetAsync<List<Accnts>>("/api/accounts");

        public override async Task AddAccount(Accnts accnt)
        {
            await _httpClient.PostAsJsonAsync("/api/accounts", accnt);
            // Server handles RoadMapNetWorth updates automatically
        }

        public override async Task RemoveAccount(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/accounts/{id}");
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to delete account with ID {id}");
        }
        #endregion

        #region Forecast Section
        public override async Task<List<Forecasts>> GetForecasts()
            => await GetAsync<List<Forecasts>>("/api/forecasts");

        public override async Task AddForecasts(List<Forecasts> forecast)
            => await _httpClient.PostAsJsonAsync("/api/forecasts", forecast);

        public override async Task RemoveForecasts(string forecastName)
        {
            var response = await _httpClient.DeleteAsync($"/api/forecasts?name={Uri.EscapeDataString(forecastName)}");
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Failed to delete forecast '{forecastName}'");
        }

        public override async Task UpdateForecasts(Forecasts forecast)
            => await _httpClient.PutAsJsonAsync("/api/forecasts", forecast);

        public override async Task<List<Forecasts>> GetForecastsByName(string forecastName)
            => await GetAsync<List<Forecasts>>($"/api/forecasts/name?name={Uri.EscapeDataString(forecastName)}");

        public override async Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear()
        {
            DateTime today = DateTime.Today;
            string month = monthAbbreviations[today.Month - 1];
            int year = today.Year;
            return await GetAsync<List<Forecasts>>($"/api/forecasts/current?month={month}&year={year}");
        }
        #endregion

        #region Helper Methods
        private async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(content, GetJsonOptions());
        }

        private static JsonSerializerOptions GetJsonOptions()
            => new() { PropertyNameCaseInsensitive = true };
        #endregion
    }
}
