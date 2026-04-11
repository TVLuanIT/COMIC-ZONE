using System.Threading.Tasks;

namespace COMICZONE.Services
{
    public interface INotificationService
    {
        Task SendNotificationAsync(int userId, int? createdBy, string title, string message, string? link = null);
    }
}
