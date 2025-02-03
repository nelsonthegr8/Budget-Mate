using System.Text.Json;

namespace Financial_ForeCast.Services
{
    public interface IConnectionService
    {
        Task AddConnection(AIConnection connection);
        Task DeleteConnection(string name);
        Task<List<AIConnection>> GetAIConnections();
    }
}