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

            var reply = await _geminiService.SendAsync(prompt);

            // Chuyển error code thành thông báo thân thiện
            return reply switch
            {
                "__GEMINI_OVERLOADED__" => "⏳ Trợ lý đang bận xử lý nhiều yêu cầu, vui lòng thử lại sau vài giây nhé! Mình luôn sẵn sàng hỗ trợ bạn. 😊",
                "__GEMINI_AUTH_ERROR__" => "⚠️ Trợ lý tạm thời không khả dụng. Vui lòng liên hệ COMICZONE để được hỗ trợ trực tiếp.",
                "__GEMINI_ERROR__"      => "⚠️ Đã xảy ra sự cố kỹ thuật. Vui lòng thử lại sau hoặc liên hệ COMICZONE để được hỗ trợ.",
                _                      => reply
            };
        }
    }
}