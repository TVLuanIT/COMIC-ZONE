using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum ReportType
    {
        [Display(Name = "Đánh giá sản phẩm")]
        Review = 1,

        [Display(Name = "Phản hồi đánh giá")]
        Reply = 2,

        [Display(Name = "Bình luận Blog")]
        BlogComment = 3,

        [Display(Name = "Phản hồi Blog")]
        BlogCommentReply = 4
    }
}
