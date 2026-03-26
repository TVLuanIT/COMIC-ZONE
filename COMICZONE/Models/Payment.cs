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
    [Column("PAYMENTID")]
    public int Paymentid { get; set; }

    [Column("ORDERID")]
    public int Orderid { get; set; }

    [Column("AMOUNT", TypeName = "decimal(12, 2)")]
    public decimal Amount { get; set; }

    [Column("PAYMENTSTATUS")]
    [StringLength(20)]
    public string? Paymentstatus { get; set; }

    [Column("TRANSACTIONID")]
    [StringLength(255)]
    public string? Transactionid { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("PAIDAT", TypeName = "datetime")]
    public DateTime? Paidat { get; set; }

    [Column("PAYMENTMETHOD")]
    [StringLength(50)]
    [Unicode(false)]
    public string? Paymentmethod { get; set; }

    [ForeignKey("Orderid")]
    [InverseProperty("Payments")]
    public virtual Order Order { get; set; } = null!;

    [InverseProperty("Payment")]
    public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();

    [InverseProperty("Payment")]
    public virtual ICollection<Refund> Refunds { get; set; } = new List<Refund>();
}
