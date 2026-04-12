namespace COMICZONE.Areas.Admin.ViewModels
{
    public class BlogCommentSearchModel : AdminSearchModel
    {
        public int? CommentId { get; set; }

        public int? BlogId { get; set; }
        public string? BlogTitle { get; set; }

        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }

        public string? ContentKeyword { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }

        public bool? IsDeleted { get; set; }

        public int? ReplyCountMin { get; set; }
        public int? ReplyCountMax { get; set; }
        public bool? HasReplies { get; set; }

        public int? LikeCountMin { get; set; }
        public int? LikeCountMax { get; set; }

        public bool? NoReplyOnly { get; set; }
    }
}
