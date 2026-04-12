using COMICZONE.Models;

namespace COMICZONE.Areas.Admin.ViewModels
{
    public class BlogCommentManagementViewModel
    {
        public List<BlogComment>? Comments { get; set; }
        public List<BlogCommentReply>? Replies { get; set; }

        public BlogCommentSearchModel CommentSearch { get; set; } = new();
        public BlogCommentReplySearchModel ReplySearch { get; set; } = new();

        public string ActiveTab { get; set; } = "Comments"; // "Comments" or "Replies"
    }
}
