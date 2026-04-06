using System;

namespace COMICZONE.Areas.Admin.ViewModels
{
    public class NotificationSearchRequest : AdminSearchModel
    {
        public int? NotificationId { get; set; }

        // Recipient filtering
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }
        public string? CustomerPhoneNumber { get; set; }

        // Sender filtering
        public int? CreatedById { get; set; }
        public string? CreatedByUsername { get; set; }

        // Status filtering
        public bool? IsRead { get; set; }
        public bool UnreadOnly { get; set; }
        public bool ReadOnly { get; set; }

        // Content filtering
        public string? TitleKeyword { get; set; }
        public string? MessageKeyword { get; set; }
        public string? LinkKeyword { get; set; }

        // Date filtering
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Soft delete status
        public bool IsDeleted { get; set; }

        // Type filtering
        public bool SystemOnly { get; set; }
        public bool ManualOnly { get; set; }

        public bool UnreadByUserOnly { get; set; }
    }
}
