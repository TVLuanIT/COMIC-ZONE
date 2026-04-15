using COMICZONE.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace COMICZONE.Services
{
    public interface IChatService
    {
        Task<List<MarketplaceMessage>> GetConversationsAsync(int userId);
        Task<List<MarketplaceMessage>> GetMessagesAsync(int currentUserId, int otherUserId);
        Task<MarketplaceMessage> SaveMessageAsync(int senderId, int receiverId, string message, int? postId = null);
        Task MarkAsReadAsync(int userId, int otherUserId);
        Task<int> GetTotalUnreadCountAsync(int userId);
        Task<bool> RecallMessageAsync(int messageId, int userId);
    }
}
