using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PAYMENT")]
public partial class Payment
{
    [Key]
    [Column("PAYMENT_ID")]
    public int PaymentId { get; set; }

    [Column("ORDER_ID")]
    public int OrderId { get; set; }

    [Column("AMOUNT", TypeName = "decimal(12, 2)")]
    public decimal Amount { get; set; }

    [Column("PAYMENT_METHOD_ID")]
    public int PaymentMethodId { get; set; }

    [Column("PAYMENT_STATUS")]
    [StringLength(20)]
    public string? PaymentStatus { get; set; }

    [Column("TRANSACTION_ID")]
    [StringLength(255)]
    public string? TransactionId { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("PAID_AT", TypeName = "datetime")]
    public DateTime? PaidAt { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Payments")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("PaymentMethodId")]
    [InverseProperty("Payments")]
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;

    [InverseProperty("Payment")]
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    [InverseProperty("Payment")]
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
