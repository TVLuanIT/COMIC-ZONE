using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PRODUCT_REVIEW_SUMMARY")]
public partial class ProductReviewSummary
{
    [Key]
    [Column("PRODUCTID")]
    public int Productid { get; set; }

    [Column("TOTALREVIEW")]
    public int Totalreview { get; set; }

    [Column("AVERAGERATING", TypeName = "decimal(3, 2)")]
    public decimal Averagerating { get; set; }

    [Column("LASTUPDATED", TypeName = "datetime")]
    public DateTime? Lastupdated { get; set; }

    [ForeignKey("Productid")]
    [InverseProperty("ProductReviewSummary")]
    public virtual Product Product { get; set; } = null!;
}
