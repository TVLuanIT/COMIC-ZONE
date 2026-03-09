using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Requests
{
    public class ReplyRequest
    {
        [Required]
        public int ReviewId { get; set; }

        [Required]
        [StringLength(1000)]
        public string Content { get; set; } = string.Empty;

        public int? ReplyToUserId { get; set; }
    }
}
