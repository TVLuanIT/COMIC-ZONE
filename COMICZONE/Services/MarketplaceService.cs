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

        public async Task<List<MarketplacePost>> GetAllPostsAsync(string status = "Approved")
        {
            return await _context.MarketplacePosts
                .Include(p => p.Seller)
                .Include(p => p.MarketplacePostImages)
                .Where(p => p.Status == status && p.Isdeleted == false)
                .OrderByDescending(p => p.Createdat)
                .ToListAsync();
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
    }
}
