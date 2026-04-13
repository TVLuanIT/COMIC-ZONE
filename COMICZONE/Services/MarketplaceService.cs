using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace COMICZONE.Services
{
    public class MarketplaceService : IMarketplaceService
    {
        private readonly ComiczoneContext _context;

        public MarketplaceService(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<(List<MarketplacePost> Items, int TotalCount)> GetAllPostsAsync(string status = "Approved", string sortOrder = "date_desc", string? searchTerm = null, string? category = null, string? condition = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 12)
        {
            var query = _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
                .Where(p => p.Status == status && p.Isdeleted == false);

            // Filtering
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Title.Contains(searchTerm) || (p.Description != null && p.Description.Contains(searchTerm)));
            }

            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(p => p.Category == category);
            }

            if (!string.IsNullOrEmpty(condition))
            {
                query = query.Where(p => p.Condition == condition);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Count total before pagination
            int totalCount = await query.CountAsync();

            // Sorting
            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(p => p.Createdat),
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.Createdat),
            };

            // Pagination
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return (items, totalCount);
        }

        public async Task<MarketplacePost?> GetPostByIdAsync(int id)
        {
            return await _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
                .FirstOrDefaultAsync(p => p.Id == id && p.Isdeleted == false);
        }

        public async Task<MarketplacePost> CreatePostAsync(MarketplacePost post)
        {
            post.Createdat = DateTime.Now;
            post.Status = "Pending";
            post.Isdeleted = false;
            _context.MarketplacePosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> UpdatePostStatusAsync(int postId, string status)
        {
            var post = await _context.MarketplacePosts.FindAsync(postId);
            if (post == null) return false;

            post.Status = status;
            post.Updatedat = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeletePostAsync(int postId)
        {
            var post = await _context.MarketplacePosts.FindAsync(postId);
            if (post == null) return false;

            post.Isdeleted = true;
            post.Updatedat = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task AddPostImageAsync(MarketplacePostImage image)
        {
            _context.MarketplacePostImages.Add(image);
            await _context.SaveChangesAsync();
        }

        public async Task<MarketplaceOrder> PlaceOrderAsync(MarketplaceOrder order)
        {
            order.Createdat = DateTime.Now;
            order.Status = "Pending";
            _context.MarketplaceOrders.Add(order);
            
            var post = await _context.MarketplacePosts.FindAsync(order.Postid);
            if (post != null)
            {
                post.Status = "Sold";
            }
            
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var order = await _context.MarketplaceOrders.FindAsync(orderId);
            if (order == null) return false;

            order.Status = status;
            order.Updatedat = DateTime.Now;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<MarketplaceOrder>> GetOrdersByBuyerAsync(int buyerId)
        {
            return await _context.MarketplaceOrders
                .Include(o => o.Post)
                    .ThenInclude(p => p.MarketplacePostImages)
                .Include(o => o.Seller)
                .Where(o => o.Buyerid == buyerId)
                .OrderByDescending(o => o.Createdat)
                .ToListAsync();
        }

        public async Task<List<MarketplaceOrder>> GetOrdersBySellerAsync(int sellerId)
        {
            return await _context.MarketplaceOrders
                .Include(o => o.Post)
                .Include(o => o.Buyer)
                .Where(o => o.Sellerid == sellerId)
                .OrderByDescending(o => o.Createdat)
                .ToListAsync();
        }

        public async Task<MarketplaceReview> AddReviewAsync(MarketplaceReview review)
        {
            review.Createdat = DateTime.Now;
            _context.MarketplaceReviews.Add(review);
            await _context.SaveChangesAsync();
            return review;
        }

        public async Task<List<MarketplaceReview>> GetSellerReviewsAsync(int sellerId)
        {
            var orders = await _context.MarketplaceOrders
                .Where(o => o.Sellerid == sellerId)
                .Select(o => o.Id)
                .ToListAsync();

            if (!orders.Any()) return new List<MarketplaceReview>();

            return await _context.MarketplaceReviews
                .Include(r => r.Reviewer)
                .Where(r => orders.Contains(r.Orderid))
                .OrderByDescending(r => r.Createdat)
                .ToListAsync();
        }
        public async Task<bool> IsFavoritedAsync(int userId, int postId)
        {
            return await _context.MarketplaceFavorites
                .AnyAsync(f => f.Userid == userId && f.Postid == postId);
        }

        public async Task<bool> ToggleFavoriteAsync(int userId, int postId)
        {
            var existing = await _context.MarketplaceFavorites
                .FirstOrDefaultAsync(f => f.Userid == userId && f.Postid == postId);

            if (existing != null)
            {
                _context.MarketplaceFavorites.Remove(existing);
                await _context.SaveChangesAsync();
                return false; // removed => not favorited
            }
            else
            {
                _context.MarketplaceFavorites.Add(new MarketplaceFavorite
                {
                    Userid = userId,
                    Postid = postId,
                    Createdat = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return true; // added => is favorited
            }
        }

        public async Task<(List<MarketplacePost> Items, int TotalCount)> GetUserFavoritesAsync(int userId, int page = 1, int pageSize = 8)
        {
            var query = _context.MarketplaceFavorites
                .Where(f => f.Userid == userId && f.Post.Isdeleted == false);

            int totalCount = await query.CountAsync();

            var items = await query
                .Include(f => f.Post)
                    .ThenInclude(p => p.MarketplacePostImages)
                .Include(f => f.Post)
                    .ThenInclude(p => p.Seller)
                .OrderByDescending(f => f.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(f => f.Post)
                .ToListAsync();

            return (items, totalCount);
        }

        // Messages
        public async Task<MarketplaceMessage> SendMessageAsync(MarketplaceMessage message)
        {
            message.Createdat = DateTime.Now;
            message.Isread = false;
            _context.MarketplaceMessages.Add(message);
            await _context.SaveChangesAsync();
            
            // Optionally, load sender info to return for immediate display
            await _context.Entry(message).Reference(m => m.Sender).LoadAsync();
            return message;
        }

        public async Task<List<MarketplaceMessage>> GetConversationAsync(int userId, int otherUserId, int postId)
        {
            return await _context.MarketplaceMessages
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .Where(m => m.Postid == postId && 
                            ((m.Senderid == userId && m.Receiverid == otherUserId) || 
                             (m.Senderid == otherUserId && m.Receiverid == userId)))
                .OrderBy(m => m.Createdat)
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId, int postId)
        {
            return await _context.MarketplaceMessages
                .CountAsync(m => m.Receiverid == userId && m.Postid == postId && m.Isread == false);
        }

        public async Task MarkMessagesAsReadAsync(int receiverId, int senderId, int postId)
        {
            var unreadMessages = await _context.MarketplaceMessages
                .Where(m => m.Receiverid == receiverId && m.Senderid == senderId && m.Postid == postId && m.Isread == false)
                .ToListAsync();

            if (unreadMessages.Any())
            {
                foreach (var msg in unreadMessages)
                {
                    msg.Isread = true;
                }
                await _context.SaveChangesAsync();
            }
        }
    }
}
