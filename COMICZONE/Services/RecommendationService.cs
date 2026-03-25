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
        var userIdInt = int.Parse(userId);

        // Lấy sản phẩm user đã mua
        var purchasedProducts = await _context.OrderItems
            .Where(o => o.Order.UserId == userIdInt)
            .Select(o => o.Product)
            .ToListAsync();

        if (!purchasedProducts.Any())
        {
            return await GetFallbackProducts();
        }

        var authors = purchasedProducts
            .Where(p => p.Author != null)
            .Select(p => p.Author)
            .Distinct()
            .ToList();

        var series = purchasedProducts
            .Where(p => p.Series != null)
            .Select(p => p.Series)
            .Distinct()
            .ToList();

        var publishers = purchasedProducts
            .Where(p => p.Publisher != null)
            .Select(p => p.Publisher)
            .Distinct()
            .ToList();

        var ageGroups = purchasedProducts
            .Where(p => p.AgeGroup != null)
            .Select(p => p.AgeGroup)
            .Distinct()
            .ToList();

        var tagIds = await _context.OrderItems
            .Where(o => o.Order.UserId == userIdInt)
            .SelectMany(o => o.Product.Tags)
            .Select(t => t.Id)
            .Distinct()
            .ToListAsync();

        var purchasedIds = purchasedProducts.Select(p => p.Id).ToList();

        var recommended = await _context.Products
            .Where(p =>
                !purchasedIds.Contains(p.Id) &&
                (
                    authors.Contains(p.Author) ||
                    series.Contains(p.Series) ||
                    publishers.Contains(p.Publisher) ||
                    ageGroups.Contains(p.AgeGroup) ||
                    p.Tags.Any(t => tagIds.Contains(t.Id))
                )
            )
            .OrderByDescending(p => p.ReleaseDate)
            .Include(p => p.Pictures)
            .Take(8)
            .ToListAsync();

        if (!recommended.Any())
        {
            return await GetFallbackProducts();
        }

        return recommended;
    }

    private async Task<List<Product>> GetFallbackProducts()
    {
        return await _context.Products
            .OrderByDescending(p => p.OrderItems.Count)
            .ThenByDescending(p => p.ReleaseDate)
            .Take(8)
            .Include(p => p.Pictures)
            .ToListAsync();
    }
}