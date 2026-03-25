using COMICZONE.Models;

namespace COMICZONE.Services
{
    public interface IProductSearchService
    {
        Task<string> GetStoreContextAsync(string message);
        Task<string> ExecuteDatabaseQueryAsync(ChatbotIntent intent);
    }
}
