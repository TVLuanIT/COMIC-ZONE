using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("ORDER_STATUS_HISTORY")]
public partial class OrderStatusHistory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("ORDER_ID")]
    public int OrderId { get; set; }

    [Column("STATUS")]
    [StringLength(50)]
    public string? Status { get; set; }

    [Column("UPDATED_AT", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [Column("UPDATED_BY")]
    public int? UpdatedBy { get; set; }

    [ForeignKey("OrderId")]
    [InverseProperty("OrderStatusHistories")]
    public virtual Order Order { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("OrderStatusHistories")]
    public virtual User? UpdatedByNavigation { get; set; }
}
