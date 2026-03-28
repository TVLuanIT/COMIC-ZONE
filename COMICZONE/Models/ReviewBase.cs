using System.ComponentModel.DataAnnotations.Schema;

namespace COMICZONE.Models
{
    public abstract class ReviewBase
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
