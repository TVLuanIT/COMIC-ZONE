using COMICZONE.Helpers;

namespace COMICZONE.Services
{
    public class ChatbotService : IChatbotService
    {
        private readonly IProductSearchService _searchService;
        private readonly IGeminiService _geminiService;

        public ChatbotService(IProductSearchService searchService, IGeminiService geminiService)
        {
            _searchService = searchService;
            _geminiService = geminiService;
        }

        public async Task<string> GetReplyAsync(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "Bạn cần hỗ trợ gì ạ?";

            var intent = await _geminiService.AnalyzeIntentAsync(message);

            if (intent.Intent == "get_shipping_info")
                return FormatProductHelper.GetShippingInfo();

            var dbData = await _searchService.ExecuteDatabaseQueryAsync(intent);

            var prompt = $@"
                Bạn là nhân viên COMICZONE.
                Dữ liệu hệ thống:
                {dbData}
                Câu hỏi khách:
                {message}
                Hãy trả lời thân thiện, chính xác.
                ";

            return await _geminiService.SendAsync(prompt);
        }
    }
}