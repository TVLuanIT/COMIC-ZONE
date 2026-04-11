using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_REVIEW")]
public partial class MarketplaceReview
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ORDERID")]
    public int Orderid { get; set; }

    [Column("REVIEWERID")]
    public int Reviewerid { get; set; }

    [Column("RATING")]
    public int Rating { get; set; }

    [Column("COMMENT")]
    public string? Comment { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Orderid")]
    [InverseProperty("MarketplaceReviews")]
    public virtual MarketplaceOrder Order { get; set; } = null!;

    [ForeignKey("Reviewerid")]
    [InverseProperty("MarketplaceReviews")]
    public virtual User Reviewer { get; set; } = null!;
}
