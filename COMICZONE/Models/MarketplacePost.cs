using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_POST")]
public partial class MarketplacePost
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("SELLERID")]
    public int Sellerid { get; set; }

    [Column("TITLE")]
    [StringLength(500)]
    public string Title { get; set; } = null!;

    [Column("DESCRIPTION")]
    public string? Description { get; set; }

    [Column("PRICE", TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Column("CONDITION")]
    [StringLength(50)]
    [Unicode(false)]
    public string Condition { get; set; } = null!;

    [Column("CATEGORY")]
    [StringLength(100)]
    [Unicode(false)]
    public string Category { get; set; } = null!;

    [Column("STATUS")]
    [StringLength(50)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [Column("ISDELETED")]
    public bool? Isdeleted { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt1 { get; set; }

    [InverseProperty("Post")]
    public virtual ICollection<MarketplaceFavorite> MarketplaceFavorites { get; set; } = new List<MarketplaceFavorite>();

    [InverseProperty("Post")]
    public virtual ICollection<MarketplaceMessage> MarketplaceMessages { get; set; } = new List<MarketplaceMessage>();

    [InverseProperty("Post")]
    public virtual ICollection<MarketplacePostImage> MarketplacePostImages { get; set; } = new List<MarketplacePostImage>();

    [InverseProperty("Post")]
    public virtual ICollection<MarketplacePostPromotion> MarketplacePostPromotions { get; set; } = new List<MarketplacePostPromotion>();

    [ForeignKey("Sellerid")]
    [InverseProperty("MarketplacePosts")]
    public virtual User Seller { get; set; } = null!;
}
