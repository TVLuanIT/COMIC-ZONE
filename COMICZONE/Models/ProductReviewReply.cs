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

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [Column("PARENTREPLYID")]
    public int? Parentreplyid { get; set; }

    [InverseProperty("Parentreply")]
    public virtual ICollection<ProductReviewReply> InverseParentreply { get; set; } = new List<ProductReviewReply>();

    [ForeignKey("Parentreplyid")]
    [InverseProperty("InverseParentreply")]
    public virtual ProductReviewReply? Parentreply { get; set; }

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
