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
                
            var contextData = await _searchService.GetStoreContextAsync(message);

            var prompt = $@"
                Bạn là trợ lý ảo thân thiện của nhà sách COMICZONE.
                
                DỮ LIỆU HỆ THỐNG CUNG CẤP CHO BẠN:
                {contextData}
                
                NGUYÊN TẮC: 
                - Hãy dùng [THỐNG KÊ CỬA HÀNG] để trả lời nếu khách hỏi thông tin quy mô, số lượng.
                - Hãy dùng [DỮ LIỆU TÌM KIẾM RIÊNG] để trả lời các tên truyện/giá truyện khách hỏi.
                - Không bịa thông tin không có trong dữ liệu.

                Câu hỏi khách hàng: {message}
                ";

            return await _geminiService.SendAsync(prompt);
        }
    }
}