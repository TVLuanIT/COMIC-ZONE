namespace COMICZONE.Areas.Admin.ViewModels
{
    public class BlogCommentReplySearchModel : AdminSearchModel
    {
        public int? ReplyId { get; set; }

        public int? CommentId { get; set; }
        public int? BlogId { get; set; }
        public string? BlogTitle { get; set; }

        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }

        public int? ReplyToUserId { get; set; }
        public string? ReplyToUsername { get; set; }

        public int? ParentReplyId { get; set; }

        public bool? TopLevelOnly { get; set; }
        public bool? ChildReplyOnly { get; set; }

        public DateTime? CreatedFrom { get; set; }
        public DateTime? CreatedTo { get; set; }

        public DateTime? UpdatedFrom { get; set; }
        public DateTime? UpdatedTo { get; set; }

        public bool? IsDeleted { get; set; }

        public int? LikeCountMin { get; set; }
        public int? LikeCountMax { get; set; }

        public bool? NoChildReplyOnly { get; set; }
    }
}
