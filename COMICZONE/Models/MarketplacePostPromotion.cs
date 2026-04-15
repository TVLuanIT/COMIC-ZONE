using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_POST_PROMOTION")]
public partial class MarketplacePostPromotion
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("POSTID")]
    public int Postid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("PROMOTION_TYPE")]
    [StringLength(50)]
    public string? PromotionType { get; set; }

    [Column("START_DATE", TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column("END_DATE", TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [Column("PRICE", TypeName = "decimal(10, 2)")]
    public decimal? Price { get; set; }

    [Column("STATUS")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("ISDELETED")]
    public bool Isdeleted { get; set; }

    [ForeignKey("Postid")]
    [InverseProperty("MarketplacePostPromotions")]
    public virtual MarketplacePost Post { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("MarketplacePostPromotions")]
    public virtual User User { get; set; } = null!;
}
