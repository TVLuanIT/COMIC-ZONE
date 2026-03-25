using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Services;
using Microsoft.EntityFrameworkCore;

public class RecommendationService : IRecommendationService
{
    private readonly ComiczoneContext _context;

    public RecommendationService(ComiczoneContext context)
    {
        _context = context;
    }

    public async Task<List<Product>> GetRecommendedProductsAsync(string userId)
    {
        if (!int.TryParse(userId, out int userIdInt))
            return await GetFallbackProducts();

        // lịch sử xem
        var viewedProductIds = await _context.UserProductViews
            .Where(v => v.UserId == userIdInt)
            .Select(v => v.ProductId)
            .ToListAsync();

        // lịch sử mua
        var purchasedProductIds = await _context.OrderItems
            .Where(o => o.Order.UserId == userIdInt)
            .Select(o => o.ProductId)
            .ToListAsync();

        var interactedProductIds = viewedProductIds
            .Union(purchasedProductIds)
            .Distinct()
            .ToList();

        if (!interactedProductIds.Any())
            return await GetFallbackProducts();


        // AUTHOR
        var authors = await _context.Products
            .Where(p => interactedProductIds.Contains(p.Id))
            .Select(p => p.Author)
            .Where(a => a != null)
            .Distinct()
            .ToListAsync();


        // SERIES
        var series = await _context.Products
            .Where(p => interactedProductIds.Any(id => id == p.Id))
            .Select(p => p.Series)
            .Where(s => s != null)
            .Distinct()
            .ToListAsync();


        // TAG (dùng navigation property Tags trực tiếp)
        var tagIds = await _context.Products
            .Where(p => interactedProductIds.Any(id => id == p.Id))
            .SelectMany(p => p.Tags)
            .Select(t => t.Id)
            .Distinct()
            .ToListAsync();

        var recommended = await _context.Products
            .Where(p =>
                !interactedProductIds.Contains(p.Id)
                &&
                (
                    authors.Contains(p.Author)
                    || series.Contains(p.Series)
                    || p.Tags.Any(t => tagIds.Contains(t.Id))
                )
            )
            .Include(p => p.Pictures)
            .Include(p => p.Tags)
            .OrderByDescending(p => p.ReleaseDate)
            .Take(8)
            .ToListAsync();

        if (!recommended.Any())
            return await GetFallbackProducts();

        return recommended;
    }

    private async Task<List<Product>> GetFallbackProducts()
    {
        return await _context.Products
            .Include(p => p.Pictures)
            .Include(p => p.Tags)
            .OrderByDescending(p => p.OrderItems.Count)
            .ThenByDescending(p => p.ReleaseDate)
            .Take(8)
            .ToListAsync();
    }
}