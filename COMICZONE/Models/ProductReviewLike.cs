using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[PrimaryKey("Reviewid", "Userid")]
[Table("PRODUCT_REVIEW_LIKE")]
public partial class ProductReviewLike
{
    [Key]
    [Column("REVIEWID")]
    public int Reviewid { get; set; }

    [Key]
    [Column("USERID")]
    public int Userid { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Reviewid")]
    [InverseProperty("ProductReviewLikes")]
    public virtual ProductReview Review { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("ProductReviewLikes")]
    public virtual User User { get; set; } = null!;
}
