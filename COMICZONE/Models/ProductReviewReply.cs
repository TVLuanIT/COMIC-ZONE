using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PRODUCT_REVIEW_REPLY")]
public partial class ProductReviewReply
{
    [Key]
    [Column("REPLYID")]
    public int Replyid { get; set; }

    [Column("REVIEWID")]
    public int Reviewid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("REPLYCONTENT")]
    public string Replycontent { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("REPLYTOUSERID")]
    public int? Replytouserid { get; set; }
    [NotMapped]
    public int LikeCount { get; set; }

    [NotMapped]
    public bool IsLikedByUser { get; set; }

    [NotMapped]
    public bool IsDislikedByUser { get; set; }
    [InverseProperty("Reply")]
    public virtual ICollection<ProductReviewReplyLike> ProductReviewReplyLikes { get; set; } = new List<ProductReviewReplyLike>();

    [InverseProperty("Reply")]
    public virtual ICollection<ProductReviewReport> ProductReviewReports { get; set; } = new List<ProductReviewReport>();

    [ForeignKey("Replytouserid")]
    [InverseProperty("ProductReviewReplyReplytousers")]
    public virtual User? Replytouser { get; set; }

    [ForeignKey("Reviewid")]
    [InverseProperty("ProductReviewReplies")]
    public virtual ProductReview Review { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("ProductReviewReplyUsers")]
    public virtual User User { get; set; } = null!;
}
