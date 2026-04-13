using COMICZONE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace COMICZONE.Services
{
    public interface IMarketplaceService
    {
        // Posts
        Task<(List<MarketplacePost> Items, int TotalCount)> GetAllPostsAsync(string status = "Approved", string sortOrder = "date_desc", string? searchTerm = null, string? category = null, string? condition = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 12);
        Task<MarketplacePost?> GetPostByIdAsync(int id);
        Task<MarketplacePost> CreatePostAsync(MarketplacePost post);
        Task<bool> UpdatePostStatusAsync(int postId, string status);
        Task<bool> DeletePostAsync(int postId);
        Task AddPostImageAsync(MarketplacePostImage image);

        // Orders
        Task<MarketplaceOrder> PlaceOrderAsync(MarketplaceOrder order);
        Task<bool> UpdateOrderStatusAsync(int orderId, string status);
        Task<List<MarketplaceOrder>> GetOrdersByBuyerAsync(int buyerId);
        Task<List<MarketplaceOrder>> GetOrdersBySellerAsync(int sellerId);

        // Reviews
        Task<MarketplaceReview> AddReviewAsync(MarketplaceReview review);
        Task<List<MarketplaceReview>> GetSellerReviewsAsync(int sellerId);

        // Favorites
        Task<bool> IsFavoritedAsync(int userId, int postId);
        Task<bool> ToggleFavoriteAsync(int userId, int postId);
        Task<(List<MarketplacePost> Items, int TotalCount)> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 8);

        // Messages
        Task<MarketplaceMessage> SendMessageAsync(MarketplaceMessage message);
        Task<List<MarketplaceMessage>> GetConversationAsync(int userId, int otherUserId, int postId);
        Task<int> GetUnreadCountAsync(int userId, int postId);
        Task MarkMessagesAsReadAsync(int receiverId, int senderId, int postId);

        // User Profile Check
        Task<Customer?> GetCustomerByUserIdAsync(int userId);
    }
}
