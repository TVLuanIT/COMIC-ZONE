using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
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
                .Include(p => p.MarketplacePostPromotions)
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
                "date_asc" => query.OrderByDescending(p => p.MarketplacePostPromotions.Any(mp => mp.Status == "Active" && mp.EndDate > DateTime.Now)).ThenBy(p => p.Createdat),
                "price_asc" => query.OrderByDescending(p => p.MarketplacePostPromotions.Any(mp => mp.Status == "Active" && mp.EndDate > DateTime.Now)).ThenBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.MarketplacePostPromotions.Any(mp => mp.Status == "Active" && mp.EndDate > DateTime.Now)).ThenByDescending(p => p.Price),
                "title_asc" => query.OrderByDescending(p => p.MarketplacePostPromotions.Any(mp => mp.Status == "Active" && mp.EndDate > DateTime.Now)).ThenBy(p => p.Title),
                "title_desc" => query.OrderByDescending(p => p.MarketplacePostPromotions.Any(mp => mp.Status == "Active" && mp.EndDate > DateTime.Now)).ThenByDescending(p => p.Title),
                _ => query.OrderByDescending(p => p.MarketplacePostPromotions.Any(mp => mp.Status == "Active" && mp.EndDate > DateTime.Now)).ThenByDescending(p => p.Createdat),
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
            post.StatusEnum = MarketplacePostStatus.Pending;
            post.Isdeleted = false;
            _context.MarketplacePosts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> UpdatePostAsync(MarketplacePost post)
        {
            var existingPost = await _context.MarketplacePosts.FindAsync(post.Id);
            if (existingPost == null || existingPost.Isdeleted == true) return false;

            existingPost.Title = post.Title;
            existingPost.Price = post.Price;
            existingPost.Condition = post.Condition;
            existingPost.ConditionEnum = post.ConditionEnum;
            existingPost.Category = post.Category;
            existingPost.CategoryEnum = post.CategoryEnum;
            existingPost.Description = post.Description;
            existingPost.Updatedat = DateTime.Now;
            existingPost.StatusEnum = MarketplacePostStatus.Pending; // Requires re-approval
            existingPost.Status = MarketplacePostStatus.Pending.ToString();

            await _context.SaveChangesAsync();
            return true;
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

        public async Task<(List<MarketplacePost> Items, int TotalCount)> GetPostsBySellerAsync(int sellerId, int page = 1, int pageSize = 10)
        {
            var query = _context.MarketplacePosts
                .Include(p => p.MarketplacePostImages)
                .Include(p => p.MarketplacePostPromotions)
                .Where(p => p.Sellerid == sellerId && p.Isdeleted == false);

            int totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
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

        public async Task<Customer?> GetCustomerByUserIdAsync(int userId)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Userid == userId);
        }

        // Promotions
        public async Task<MarketplacePostPromotion> PromotePostAsync(int postId, int userId, int days, decimal totalAmount, string paymentMethod)
        {
            var promotion = new MarketplacePostPromotion
            {
                Postid = postId,
                Userid = userId,
                PromotionType = paymentMethod, // Store payment method here for tracking
                Price = totalAmount,
                Status = "Pending",
                CreatedAt = DateTime.Now,
                // EndDate & StartDate are left null for Pending, initialized upon Activation
            };
            
            // We temporarily store the requested duration inside EndDate until it activates, or we can just infer it later?
            // Actually, we can store StartDate = Now, EndDate = Now + days, but keep Status = "Pending".
            // Then on Activate, we re-stamp StartDate and EndDate.
            var now = DateTime.Now;
            promotion.StartDate = now;
            promotion.EndDate = now.AddDays(days);

            _context.MarketplacePostPromotions.Add(promotion);
            await _context.SaveChangesAsync();

            return promotion;
        }

        public async Task<MarketplacePostPromotion?> GetPromotionByIdAsync(int promotionId)
        {
            return await _context.MarketplacePostPromotions.FindAsync(promotionId);
        }

        public async Task<bool> ActivatePromotionAsync(int promotionId)
        {
            var promotion = await _context.MarketplacePostPromotions.FindAsync(promotionId);
            if (promotion == null || promotion.Status == "Active") return false;

            // Recalculate dates based on the differential from Creation
            int days = (promotion.EndDate.Value - promotion.StartDate.Value).Days;
            if (days <= 0) days = 1;

            var now = DateTime.Now;
            promotion.StartDate = now;
            promotion.EndDate = now.AddDays(days);
            promotion.Status = "Active";

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelPromotionAsync(int promotionId)
        {
            var promotion = await _context.MarketplacePostPromotions.FindAsync(promotionId);
            if (promotion == null) return false;

            promotion.Status = "Cancelled";
            // promotion.EndDate = DateTime.Now; // Don't overwrite, so we can restore original duration

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RestorePromotionAsync(int promotionId)
        {
            var promotion = await _context.MarketplacePostPromotions.FindAsync(promotionId);
            if (promotion == null) return false;

            // Restoring only makes sense if it was cancelled
            if (promotion.Status != "Cancelled") return false;

            // If it's already past the end date, set it to Completed instead of Active
            if (promotion.EndDate.HasValue && promotion.EndDate.Value <= DateTime.Now)
            {
                promotion.Status = "Completed";
            }
            else
            {
                promotion.Status = "Active";
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
