using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Requests
{
    public class ReportProductReviewRequest
    {
        public int? ReviewId { get; set; }
        public int? ReplyId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do.")]
        public required string Reason { get; set; }
    }
}
