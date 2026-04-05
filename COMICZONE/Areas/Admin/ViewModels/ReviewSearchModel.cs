namespace COMICZONE.Areas.Admin.ViewModels
{
    public class ReviewSearchModel : AdminSearchModel
    {
        public int? ReviewId { get; set; }

        public int? ProductId { get; set; }
        public string? ProductName { get; set; }

        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? UserEmail { get; set; }
        public string? CustomerPhoneNumber { get; set; }

        public byte? Rating { get; set; }
        public byte? RatingMin { get; set; }
        public byte? RatingMax { get; set; }

        public string? ReviewContentKeyword { get; set; }

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
        public bool? HasAdminReply { get; set; }
    }
}
