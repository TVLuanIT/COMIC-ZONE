using COMICZONE.Models;

namespace COMICZONE.Services
{
    public interface IGeminiService
    {
        Task<string> SendAsync(string prompt);
        Task<ChatbotIntent> AnalyzeIntentAsync(string message);
    }
}
