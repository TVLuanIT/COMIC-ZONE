using COMICZONE.Models;

namespace COMICZONE.Services
{
    public interface IRecommendationService
    {
        Task<List<Product>> GetRecommendedProductsAsync(string userId);
    }
}
