using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COMICZONE.Services
{
    public class ChatService : IChatService
    {
        private readonly ComiczoneContext _context;

        public ChatService(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<List<MarketplaceMessage>> GetConversationsAsync(int userId)
        {
            // Tìm tất cả các tin nhắn liên quan đến người dùng hiện tại
            var query = _context.MarketplaceMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Post)
                    .ThenInclude(p => p.MarketplacePostImages)
                .Where(m => m.Senderid == userId || m.Receiverid == userId);

            // Nhóm theo OtherUserId (Người dùng phía bên kia cuộc hội thoại)
            var messages = await query.ToListAsync();
            
            var conversations = messages
                .GroupBy(m => m.Senderid == userId ? m.Receiverid : m.Senderid)
                .Select(g => g.OrderByDescending(m => m.Createdat).First())
                .OrderByDescending(m => m.Createdat)
                .ToList();

            return conversations;
        }

        public async Task<List<MarketplaceMessage>> GetMessagesAsync(int currentUserId, int otherUserId)
        {
            return await _context.MarketplaceMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Include(m => m.Post)
                    .ThenInclude(p => p.MarketplacePostImages)
                .Where(m => (m.Senderid == currentUserId && m.Receiverid == otherUserId) ||
                            (m.Senderid == otherUserId && m.Receiverid == currentUserId))
                .OrderBy(m => m.Createdat)
                .ToListAsync();
        }

        public async Task<MarketplaceMessage> SaveMessageAsync(int senderId, int receiverId, string message, int? postId = null)
        {
            var msg = new MarketplaceMessage
            {
                Senderid = senderId,
                Receiverid = receiverId,
                Message = message,
                Postid = postId,
                Createdat = DateTime.Now,
                Isread = false
            };

            _context.MarketplaceMessages.Add(msg);
            await _context.SaveChangesAsync();

            // Load navigation properties
            await _context.Entry(msg).Reference(m => m.Sender).LoadAsync();
            await _context.Entry(msg).Reference(m => m.Receiver).LoadAsync();
            
            return msg;
        }

        public async Task MarkAsReadAsync(int userId, int otherUserId)
        {
            var unread = await _context.MarketplaceMessages
                .Where(m => m.Receiverid == userId && m.Senderid == otherUserId && (m.Isread == false || m.Isread == null))
                .ToListAsync();

            if (unread.Any())
            {
                foreach (var m in unread) m.Isread = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetTotalUnreadCountAsync(int userId)
        {
            return await _context.MarketplaceMessages
                .CountAsync(m => m.Receiverid == userId && (m.Isread == false || m.Isread == null));
        }

        public async Task<bool> RecallMessageAsync(int messageId, int userId)
        {
            var msg = await _context.MarketplaceMessages.FindAsync(messageId);
            if (msg == null || msg.Senderid != userId) return false;

            // Đánh dấu thu hồi bằng mã đặc biệt
            msg.Message = "[MESSAGE_RECALLED]";
            msg.Postid = null; // Xóa đính kèm sản phẩm nếu có
            
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
