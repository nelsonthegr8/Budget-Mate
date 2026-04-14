using Financial_ForeCast.Models;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Financial_ForeCast.Services
{
    public class ServerDbService : BaseDbService
    {
    private readonly string _connectionString;
    public ServerDbService(string connectionString)
    {
        _connectionString = connectionString;
    }

    private SQLiteConnection CreateConnection()
    {
        var conn = new SQLiteConnection(_connectionString);
        conn.Open();
        return conn;
    }

    private async Task<T> QueryAsync<T>(string sql, Func<SQLiteDataReader, T> readerFunc)
    {
        using var conn = CreateConnection();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = await cmd.ExecuteReaderAsync();
        var results = new List<object>();
        while (await reader.ReadAsync())
        {
            results.Add(readerFunc(reader));
        }
        // assuming T is List<...>
        return (T)(object)results;
    }

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
