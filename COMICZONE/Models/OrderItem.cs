using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("ORDER_ITEM")]
public partial class OrderItem
{
    [Key]
    [Column("ORDER_ITEM_ID")]
    public int OrderItemId { get; set; }

    [Column("ORDER_ID")]
    public int OrderId { get; set; }

    [Column("PRODUCT_ID")]
    public int ProductId { get; set; }

    [Column("QUANTITY")]
    public int Quantity { get; set; }

    [Column("PRICE", TypeName = "decimal(12, 2)")]
    public decimal? Price { get; set; }

    [Column("SUBTOTAL", TypeName = "decimal(12, 2)")]
    public decimal? Subtotal { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderItems")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("OrderItems")]
    public virtual Product Product { get; set; } = null!;
}
