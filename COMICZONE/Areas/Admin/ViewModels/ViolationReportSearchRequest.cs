using System;

namespace COMICZONE.Areas.Admin.ViewModels
{
    public class ViolationReportSearchRequest : AdminSearchModel
    {
        public int? Id { get; set; }

        // Reporter filtering
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }
        public string? CustomerPhoneNumber { get; set; }

        // Type filtering
        public int? ReportType { get; set; }
        public int[]? ReportTypes { get; set; }

        public int? TargetId { get; set; }

        // Content filtering
        public string? ReasonKeyword { get; set; }

        // Status filtering
        public int? Status { get; set; }
        public int[]? Statuses { get; set; }

        // Date filtering
        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        // Soft delete status
        public bool IsDeleted { get; set; }

        // Specialized flags
        public bool HighPriorityOnly { get; set; }
        public bool DuplicateTargetOnly { get; set; }
        public bool UnresolvedOnly { get; set; }
    }
}
