using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PRODUCT_REVIEW_REPORT")]
public partial class ProductReviewReport
{
    [Key]
    [Column("REPORTID")]
    public int Reportid { get; set; }

    [Column("REVIEWID")]
    public int Reviewid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("REASON")]
    [StringLength(255)]
    public string Reason { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Reviewid")]
    [InverseProperty("ProductReviewReports")]
    public virtual ProductReview Review { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("ProductReviewReports")]
    public virtual User User { get; set; } = null!;
}
