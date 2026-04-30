using System.Text;
using System.Text.Json;
using COMICZONE.Models;
using COMICZONE.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;

    public GeminiService(HttpClient httpClient, IConfiguration config)
    {
        _httpClient = httpClient;
        _config = config;
    }

    public async Task<string> SendAsync(string prompt)
    {
        var apiKey = _config["Gemini:ApiKey"];
        var model = _config["Gemini:Model"];
        var url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new { role = "user", parts = new[] { new { text = prompt } } }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);

        // Retry tối đa 3 lần khi gặp lỗi 503 (overloaded) hoặc 429 (rate limit)
        int maxRetries = 3;
        int[] retryDelaysMs = { 2000, 4000, 6000 };

        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            var response = await _httpClient.PostAsync(
                url,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            if (response.IsSuccessStatusCode)
            {
                using var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                return doc.RootElement.GetProperty("candidates")[0]
                                      .GetProperty("content")
                                      .GetProperty("parts")[0]
                                      .GetProperty("text")
                                      .GetString();
            }

            var statusCode = (int)response.StatusCode;

            // Nếu là lỗi tạm thời (503 overloaded, 429 rate limit) → retry
            if ((statusCode == 503 || statusCode == 429) && attempt < maxRetries - 1)
            {
                await Task.Delay(retryDelaysMs[attempt]);
                continue;
            }

            // Lỗi cuối cùng hoặc lỗi khác → trả thông báo thân thiện
            if (statusCode == 503 || statusCode == 429)
                return "__GEMINI_OVERLOADED__";

            if (statusCode == 401 || statusCode == 403)
                return "__GEMINI_AUTH_ERROR__";

            return "__GEMINI_ERROR__";
        }

        return "__GEMINI_OVERLOADED__";
    }

    public async Task<ChatbotIntent> AnalyzeIntentAsync(string message)
    {
        var prompt = $@"
            Bạn là AI phân tích intent chatbot COMICZONE.

            Chỉ trả về JSON hợp lệ.

            Intent có thể gồm:
            count_products
            search_products
            filter_products
            get_new_products
            get_shipping_info
            check_order
            general_chat

            Tin nhắn khách:
            ""{message}""

            Output JSON format:
            {{
                ""intent"": """",
                ""keyword"": null,
                ""maxPrice"": null,
                ""minPrice"": null,
                ""category"": null,
                ""orderId"": null
            }}
            ";

        var response = await SendAsync(prompt);
        var cleanJson = response.Replace("```json", "").Replace("```", "").Trim();

        try
        {
            var intent = JsonSerializer.Deserialize<ChatbotIntent>(cleanJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return intent ?? new ChatbotIntent { Intent = "general_chat" };
        }
        catch (Exception)
        {
            return new ChatbotIntent { Intent = "general_chat" };
        }
    }
}