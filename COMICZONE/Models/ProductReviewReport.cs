using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PRODUCT_REVIEW_REPORT")]
[Index("Reviewid", "Replyid", "Userid", Name = "UX_REPORT_UNIQUE", IsUnique = true)]
public partial class ProductReviewReport
{
    [Key]
    [Column("REPORTID")]
    public int Reportid { get; set; }

    [Column("REVIEWID")]
    public int? Reviewid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("REASON")]
    [StringLength(255)]
    public string Reason { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("STATUS")]
    [StringLength(50)]
    public string Status { get; set; } = null!;

    [Column("REPLYID")]
    public int? Replyid { get; set; }

    [ForeignKey("Replyid")]
    [InverseProperty("ProductReviewReports")]
    public virtual ProductReviewReply? Reply { get; set; }

    [ForeignKey("Reviewid")]
    [InverseProperty("ProductReviewReports")]
    public virtual ProductReview? Review { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("ProductReviewReports")]
    public virtual User User { get; set; } = null!;
}
