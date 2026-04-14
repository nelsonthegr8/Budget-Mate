using Financial_ForeCast.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Financial_ForeCast.Services
{
    public class ServerDbService : BaseDbService
    {
        private readonly HttpClient _client;
        public ServerDbService(HttpClient client)
        {
            _client = client;
        }

        // Helper to perform GET and deserialize
        private async Task<T> GetAsync<T>(string url)
        {
            var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions{PropertyNameCaseInsensitive=true});
        }

        private async Task PostAsync<T>(string url, T payload)
        {
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        // Implement all IDbService methods using server endpoints
        public override Task<List<MainMenuCards>> GetMainMenuCards()
            => GetAsync<List<MainMenuCards>>("/api/mainmenu");

        public override async Task AddMainMenuCard(MainMenuCards card)
            => await PostAsync("/api/mainmenu", card);

        public override async Task UpdateMainMenuCard(MainMenuCards card)
            => await PostAsync($"/api/mainmenu/{card.Id}", card);

        public override Task<List<MainMenuCards>> GenerateCardCalculations(List<MainMenuCards> cards)
            { throw new NotImplementedException(); }

        public override Task<List<IncomeExpense>> GetAllIncomesAndExpenses()
            => GetAsync<List<IncomeExpense>>("/api/incomeexpense/all");

        public override Task<List<IncomeExpense>> GetIncome()
            => GetAsync<List<IncomeExpense>>("/api/incomeexpense/income");

        public override Task<List<IncomeExpense>> GetExpenses()
            => GetAsync<List<IncomeExpense>>("/api/incomeexpense/expense");

        public override Task<double> GetRoadMapSum(int rMapID)
            => GetAsync<double>($"/api/roaddata/sum/{rMapID}");

        public override Task<double> GetRoadMapNetWorthSum(int rMapID)
            => GetAsync<double>($"/api/roaddata/networth/{rMapID}");

        public override async Task AddIncomeExpense(IncomeExpense incomeExpense)
            => await PostAsync("/api/incomeexpense", incomeExpense);

        public override async Task<IncomeExpense> RemoveByIncomeExpenseByID(int id)
            { throw new NotImplementedException(); }

        public override async Task UpdateIncomeExpense(IncomeExpense incomeExpense)
            { throw new NotImplementedException(); }

        public override Task<List<RoadMaps>> GetRoadMaps()
            => GetAsync<List<RoadMaps>>("/api/roads");

        public override async Task AddRoadMap(RoadMaps roadMap)
            => await PostAsync("/api/roads", roadMap);

        public override async Task<RoadMaps> RemoveRoadMap(int id)
            { throw new NotImplementedException(); }

        public override async Task UpdateRoadMapAmount(int roadMapID, double roadMapAmount)
            { throw new NotImplementedException(); }

        public override async Task UpdateRoadNetWorthAmount(int roadMapID, double roadMapNetWorthAmount)
            { throw new NotImplementedException(); }

        public override Task<int> GetSelectedRoadMapID()
            => GetAsync<int>("/api/roads/selected");

        public override Task<List<Accnts>> GetAccounts()
            => GetAsync<List<Accnts>>("/api/accounts");

        public override async Task AddAccount(Accnts accnt)
            => await PostAsync("/api/accounts", accnt);

        public override async Task RemoveAccount(int id)
            { throw new NotImplementedException(); }

        public override Task<List<Forecasts>> GetForecasts()
            => GetAsync<List<Forecasts>>("/api/forecasts");

        public override async Task AddForecasts(List<Forecasts> forecast)
            => await PostAsync("/api/forecasts", forecast);

        public override async Task RemoveForecasts(string forecastName)
            { throw new NotImplementedException(); }

        public override Task UpdateForecasts(Forecasts forecast)
            { throw new NotImplementedException(); }

        public override Task<List<Forecasts>> GetForecastsByName(string forecastName)
            { throw new NotImplementedException(); }

        public override Task<List<Forecasts>> GetForecastsByCurrentMonthAndYear()
            { throw new NotImplementedException(); }
    }
}
