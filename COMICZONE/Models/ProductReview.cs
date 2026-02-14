using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PRODUCT_REVIEW")]
[Index("Productid", "Userid", Name = "UQ_REVIEW", IsUnique = true)]
public partial class ProductReview
{
    [Key]
    [Column("REVIEWID")]
    public int Reviewid { get; set; }

    [Column("PRODUCTID")]
    public int Productid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("RATING")]
    public byte Rating { get; set; }

    [Column("REVIEWCONTENT")]
    [StringLength(1000)]
    public string? Reviewcontent { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [Column("ISAPPROVED")]
    public bool? Isapproved { get; set; }

    [ForeignKey("Productid")]
    [InverseProperty("ProductReviews")]
    public virtual Product Product { get; set; } = null!;

    [InverseProperty("Review")]
    public virtual ICollection<ProductReviewLike> ProductReviewLikes { get; set; } = new List<ProductReviewLike>();

    [InverseProperty("Review")]
    public virtual ICollection<ProductReviewReport> ProductReviewReports { get; set; } = new List<ProductReviewReport>();

    [ForeignKey("Userid")]
    [InverseProperty("ProductReviews")]
    public virtual User User { get; set; } = null!;
}
