using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Services;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Helpers;

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

    public async Task<string> ExecuteDatabaseQueryAsync(ChatbotIntent intent)
    {
        if (intent == null || string.IsNullOrEmpty(intent.Intent))
        {
            return "Xin lỗi, tôi chưa hiểu rõ câu hỏi. Bạn có thể hỏi lại giúp tôi không?";
        }

        switch (intent.Intent)
        {
            case "count_products":

                var total = await _context.Products.CountAsync();

                return $"Tổng số truyện hiện có: {total}";


            case "search_products":

                if (string.IsNullOrEmpty(intent.Keyword))
                    return "Bạn muốn tìm truyện gì ạ?";

                var products = await _context.Products
                    .Where(p => p.Name.Contains(intent.Keyword))
                    .Take(5)
                    .ToListAsync();

                return FormatProductHelper.FormatProducts(products);


            case "filter_products":

                if (intent.MaxPrice == null)
                    return "Bạn muốn lọc theo mức giá bao nhiêu ạ?";

                var filtered = await _context.Products
                    .Where(p => p.Price <= intent.MaxPrice)
                    .Take(5)
                    .ToListAsync();

                return FormatProductHelper.FormatProducts(filtered);


            case "get_new_products":

                var newest = await _context.Products
                    .Where(p => p.ReleaseDate != null)
                    .OrderByDescending(p => p.ReleaseDate)
                    .Take(5)
                    .ToListAsync();

                return FormatProductHelper.FormatProducts(newest);


            case "get_shipping_info":

                return FormatProductHelper.GetShippingInfo();


            case "general_chat":
                return "Bạn là nhân viên thân thiện, hãy chào hỏi và sẵn sàng giúp khách tìm truyện, kiểm tra tồn kho, hoặc báo giá.";

            default:
                return "Dữ liệu trống, hãy tự trả lời khéo léo.";
        }
    }
}