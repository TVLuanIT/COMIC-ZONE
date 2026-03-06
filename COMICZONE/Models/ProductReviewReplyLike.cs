using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[PrimaryKey("Replyid", "Userid")]
[Table("PRODUCT_REVIEW_REPLY_LIKE")]
public partial class ProductReviewReplyLike
{
    [Key]
    [Column("REPLYID")]
    public int Replyid { get; set; }

    [Key]
    [Column("USERID")]
    public int Userid { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("ISLIKE")]
    public bool? Islike { get; set; }

    [ForeignKey("Replyid")]
    [InverseProperty("ProductReviewReplyLikes")]
    public virtual ProductReviewReply Reply { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("ProductReviewReplyLikes")]
    public virtual User User { get; set; } = null!;
}
