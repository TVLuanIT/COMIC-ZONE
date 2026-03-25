namespace COMICZONE.Services
{
    public interface IGeminiService
    {
        Task<string> SendAsync(string prompt);
    }
}
