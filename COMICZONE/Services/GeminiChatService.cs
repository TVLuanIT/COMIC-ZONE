using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;

public class GeminiChatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ComiczoneContext _context;

    public GeminiChatService(HttpClient httpClient, IConfiguration config, ComiczoneContext context)
    {
        _httpClient = httpClient;
        _config = config;
        _context = context;
    }

    public async Task<string> SendMessageAsync(string message)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:Model"];

        var url =
            $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

        // 1. Thống kê chung
        var totalProducts = await _context.Products.CountAsync();

        // 2. Kỹ thuật RAG: Tìm kiếm sản phẩm liên quan đến tin nhắn của khách
        // Loại bỏ các từ vô nghĩa (stop words)
        var stopWords = new[] { "có", "không", "bán", "giá", "bao", "nhiêu", "cho", "mình", "hỏi", "tôi", "muốn", "mua", "tìm", "cuốn", "tập", "bộ", "quyển", "chào", "bạn", "ạ", "nhé", "truyện", "manga", "shop", "quán" };
        
        var keywords = message.ToLower()
            .Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !stopWords.Contains(w))
            .ToList();

        string ragsData = "Không phát hiện sản phẩm cụ thể nào khớp với câu hỏi.";
        
        if (keywords.Any())
        {
            // Tải danh sách tên truyện (để xử lý tìm kiếm nhanh in-memory cho đồ án)
            var allProducts = await _context.Products
                .Select(p => new { p.Id, p.Name, p.Price, p.StockQuantity })
                .ToListAsync();

            var matchingProducts = allProducts
                .Where(p => p.Name != null && keywords.Any(k => p.Name.ToLower().Contains(k)))
                .Take(5)
                .Select(p => $"- Tên: {p.Name} | Giá: {p.Price?.ToString("N0")} VNĐ | Tồn kho: {(p.StockQuantity > 0 ? p.StockQuantity.ToString() : "Hết hàng")}")
                .ToList();

            if (matchingProducts.Any())
            {
                ragsData = string.Join("\n            ", matchingProducts);
            }
        }

        var systemPrompt = $@"
            Bạn là nhân viên tư vấn của cửa hàng COMICZONE.

            Nhiệm vụ của bạn:

            - Trả lời chính xác câu hỏi của khách dựa trên dữ liệu thật của cửa hàng.
            - Nếu không biết thông tin → nói rõ ""hiện tại cửa hàng chưa có thông tin về sản phẩm này"".
            - Không tự bịa thông tin, không bịa giá tiền hay tên truyện.
            - Trả lời ngắn gọn, thân thiện, dễ hiểu.

            DỮ LIỆU CỬA HÀNG HIỆN TẠI:
            - Tổng số đầu truyện đang bán: {totalProducts}
            - Dữ liệu tra cứu được liên quan đến câu hỏi (RAG):
            {ragsData}

            Câu hỏi của khách:
            ";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = systemPrompt }
                    }
                },
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = message }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        var response = await _httpClient.PostAsync(
            url,
            new StringContent(json, Encoding.UTF8, "application/json")
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return "Gemini API lỗi: " + error;
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(responseContent);

        return doc
            .RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();
    }
}