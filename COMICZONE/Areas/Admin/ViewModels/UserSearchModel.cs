namespace COMICZONE.Areas.Admin.ViewModels
{
    public class UserSearchModel : AdminSearchModel
    {
        public int? Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }

        public List<string>? Roles { get; set; }

        public bool? IsActive { get; set; }
        public bool? IsDeleted { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Customer details
        public string? CustomerFullName { get; set; }
        public string? CustomerPhoneNumber { get; set; }

        // Orders activity
        public bool? HasOrders { get; set; }
        public int? OrderCountMin { get; set; }
        public int? OrderCountMax { get; set; }

        // Reviews activity
        public bool? HasReviews { get; set; }
        public int? ReviewCountMin { get; set; }
        public int? ReviewCountMax { get; set; }

        // Blogs activity
        public bool? HasBlogs { get; set; }
        public int? BlogCountMin { get; set; }
        public int? BlogCountMax { get; set; }

        // Social/Notifications
        public bool? HasNotifications { get; set; }

        // Violations
        public bool? HasViolations { get; set; }
        public int? ViolationCountMin { get; set; }
        public int? ViolationCountMax { get; set; }

        // Security/Tokens
        public bool? HasResetToken { get; set; }
        public DateTime? ResetTokenExpireFrom { get; set; }
        public DateTime? ResetTokenExpireTo { get; set; }

        // Visuals
        public bool? HasAvatar { get; set; }
    }
}
