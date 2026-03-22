using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models
{
    public enum ReportType
    {
        Review,
        Reply
    }

    public partial class ViolationReport
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [Required]
        public ReportType ReportType { get; set; }  // Loại đối tượng bị báo cáo

        public int? ReviewId { get; set; }

        public int? ReplyId { get; set; }
        
        public int? BlogCommentId { get; set; }
        
        public int? ProductId { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; } = null!;

        public bool IsResolved { get; set; } = false;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
