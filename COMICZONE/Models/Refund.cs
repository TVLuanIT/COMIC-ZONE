using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("REFUND")]
public partial class Refund
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("PAYMENT_ID")]
    public int PaymentId { get; set; }

    [Column("AMOUNT", TypeName = "decimal(12, 2)")]
    public decimal Amount { get; set; }

    [Column("STATUS")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("REASON")]
    [StringLength(500)]
    public string? Reason { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("PaymentId")]
    [InverseProperty("Refunds")]
    public virtual Payment Payment { get; set; } = null!;
}
