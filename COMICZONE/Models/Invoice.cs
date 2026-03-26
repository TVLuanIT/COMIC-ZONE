using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("INVOICE")]
public partial class Invoice
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ORDER_ID")]
    public int OrderId { get; set; }

    [Column("TOTAL_AMOUNT", TypeName = "decimal(12, 2)")]
    public decimal? TotalAmount { get; set; }

    [Column("ISSUE_DATE", TypeName = "datetime")]
    public DateTime? IssueDate { get; set; }

    [Column("CUSTOMER_NAME")]
    [StringLength(255)]
    public string? CustomerName { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("Invoices")]
    public virtual Order Order { get; set; } = null!;
}
