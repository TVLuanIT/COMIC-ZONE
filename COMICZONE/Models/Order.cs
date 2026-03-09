using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("ORDER")]
public partial class Order
{
    [Key]
    [Column("ORDER_ID")]
    public int OrderId { get; set; }

    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("ORDER_DATE", TypeName = "datetime")]
    public DateTime? OrderDate { get; set; }

    [Column("TOTAL_AMOUNT", TypeName = "decimal(12, 2)")]
    public decimal? TotalAmount { get; set; }

    [Column("STATUS")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("SHIPPING_ADDRESS")]
    [StringLength(255)]
    public string? ShippingAddress { get; set; }

    [Column("PHONE_NUMBER")]
    [StringLength(20)]
    [Unicode(false)]
    public string? PhoneNumber { get; set; }

    [Column("NOTE")]
    [StringLength(500)]
    public string? Note { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [InverseProperty("Order")]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [InverseProperty("Order")]
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    [ForeignKey("UserId")]
    [InverseProperty("Orders")]
    public virtual User User { get; set; } = null!;
}
