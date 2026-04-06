using COMICZONE.Models;

namespace COMICZONE.Areas.Admin.ViewModels
{
    public class ReviewManagementViewModel
    {
        public List<ProductReview>? Reviews { get; set; }
        public List<ProductReviewReply>? Replies { get; set; }

        public ReviewSearchModel ReviewSearch { get; set; } = new();
        public ReviewReplySearchModel ReplySearch { get; set; } = new();

        public string ActiveTab { get; set; } = "Reviews"; // "Reviews" or "Replies"
    }
}
