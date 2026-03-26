using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PAYMENT_TRANSACTION")]
public partial class PaymentTransaction
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("PAYMENT_ID")]
    public int PaymentId { get; set; }

    [Column("GATEWAY")]
    [StringLength(50)]
    public string? Gateway { get; set; }

    [Column("RAW_RESPONSE")]
    public string? RawResponse { get; set; }

    [Column("STATUS")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("PaymentId")]
    [InverseProperty("PaymentTransactions")]
    public virtual Payment Payment { get; set; } = null!;
}
