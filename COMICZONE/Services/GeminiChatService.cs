using System.Text;
using System.Text.Json;

public class GeminiChatService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public GeminiChatService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> SendMessageAsync(string message)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:Model"];

        var url =
            $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

        var systemPrompt = @"
            Bạn là nhân viên tư vấn của cửa hàng COMICZONE.

            Nhiệm vụ của bạn:

            - Trả lời chính xác câu hỏi của khách
            - Nếu khách hỏi sản phẩm → giới thiệu sản phẩm phù hợp
            - Nếu không biết thông tin → nói rõ không có dữ liệu
            - Không tự bịa thông tin
            - Trả lời ngắn gọn, dễ đọc
            - Luôn trả lời bằng tiếng Việt thân thiện

            Danh mục sản phẩm COMICZONE gồm:

            - Truyện tranh
            - Manga

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