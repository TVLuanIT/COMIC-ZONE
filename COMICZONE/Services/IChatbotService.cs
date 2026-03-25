namespace COMICZONE.Services
{
    public interface IChatbotService
    {
        Task<string> GetReplyAsync(string message);
    }
}
