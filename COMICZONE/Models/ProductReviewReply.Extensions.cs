using System.ComponentModel.DataAnnotations.Schema;

namespace COMICZONE.Models
{
    public partial class ProductReviewReply
    {
        [NotMapped]
        public int LikeCount { get; set; }

        [NotMapped]
        public bool IsLikedByUser { get; set; }

        [NotMapped]
        public bool IsDislikedByUser { get; set; }

        [NotMapped]
        public bool IsReportedByUser { get; set; } = false;

        [NotMapped]
        public string? ReportStatus { get; set; }
    }
}
