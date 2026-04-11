using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_ORDER")]
public partial class MarketplaceOrder
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("POSTID")]
    public int Postid { get; set; }

    [Column("BUYERID")]
    public int Buyerid { get; set; }

    [Column("SELLERID")]
    public int Sellerid { get; set; }

    [Column("PRICE", TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [Column("STATUS")]
    [StringLength(50)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Buyerid")]
    [InverseProperty("MarketplaceOrderBuyers")]
    public virtual User Buyer { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<MarketplaceReview> MarketplaceReviews { get; set; } = new List<MarketplaceReview>();

    [ForeignKey("Postid")]
    [InverseProperty("MarketplaceOrders")]
    public virtual MarketplacePost Post { get; set; } = null!;

    [ForeignKey("Sellerid")]
    [InverseProperty("MarketplaceOrderSellers")]
    public virtual User Seller { get; set; } = null!;
}
