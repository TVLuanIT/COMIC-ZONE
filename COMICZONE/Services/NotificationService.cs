using System;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;

namespace COMICZONE.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ComiczoneContext _context;

        public NotificationService(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task SendNotificationAsync(int userId, int? createdBy, string title, string message, string? link = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                CreatedBy = createdBy,
                Title = title,
                Message = message,
                Link = link,
                IsRead = false,
                CreatedAt = DateTime.Now,
                Isdeleted = false
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }
    }
}
