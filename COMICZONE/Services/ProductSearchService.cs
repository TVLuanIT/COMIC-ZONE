using COMICZONE.Data;
using COMICZONE.Services;
using Microsoft.EntityFrameworkCore;

public class ProductSearchService : IProductSearchService
{
    private readonly ComiczoneContext _context;

    public ProductSearchService(ComiczoneContext context)
    {
        _context = context;
    }

    public async Task<string> GetStoreContextAsync(string message)
    {
        // 1. Luôn luôn cung cấp các thông số chung của cửa hàng (Giúp trả lời các câu hỏi chung chung)
        var totalComics = await _context.Products.CountAsync();

        // 2. Tìm kiếm sản phẩm chi tiết (Giúp trả lời các câu hỏi về 1 cuốn cụ thể)
        var stopWords = new[] { "có", "không", "bán", "giá", "bao", "nhiêu", "cho", "mình", "hỏi", "tôi", "muốn", "mua", "tìm", "cuốn", "tập", "bộ", "quyển", "chào", "bạn", "ạ", "nhé", "truyện", "manga", "shop", "quán", "vậy", "trang", "web" };
        
        var keywords = message.ToLower()
            .Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .ToList();

        string searchData = "Không phát hiện sản phẩm cụ thể nào.";

        if (keywords.Any())
        {
            var allProducts = await _context.Products
                .Select(p => new { p.Name, p.Price, p.StockQuantity })
                .ToListAsync();

            var matchingProducts = allProducts
                .Where(p => p.Name != null && keywords.Any(k => p.Name.ToLower().Contains(k)))
                .Take(5)
                .ToList();

            if (matchingProducts.Any())
            {
                searchData = string.Join("\n", matchingProducts.Select(p =>
                    $"- Tên: {p.Name} | Giá: {p.Price:N0} VNĐ | Tồn kho: {(p.StockQuantity > 0 ? p.StockQuantity.ToString() : "Hết hàng")}"
                ));
            }
        }

        return $@"
[THỐNG KÊ CỬA HÀNG]
- Tổng số lượng bộ truyện trên website COMICZONE là: {totalComics} bộ.

[DỮ LIỆU TÌM KIẾM RIÊNG CHO CÂU HỎI LẦN NÀY]
{searchData}
";
    }
}