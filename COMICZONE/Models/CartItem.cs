using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("CART_ITEM")]
[Index("CartId", "ProductId", Name = "UQ_CART_PRODUCT", IsUnique = true)]
public partial class CartItem
{
    [Key]
    [Column("CART_ITEM_ID")]
    public int CartItemId { get; set; }

    [Column("CART_ID")]
    public int CartId { get; set; }

    [Column("PRODUCT_ID")]
    public int ProductId { get; set; }

    [Column("QUANTITY")]
    public int Quantity { get; set; }

    [ForeignKey("CartId")]
    [InverseProperty("CartItems")]
    public virtual Cart Cart { get; set; } = null!;

    [ForeignKey("ProductId")]
    [InverseProperty("CartItems")]
    public virtual Product Product { get; set; } = null!;
}
