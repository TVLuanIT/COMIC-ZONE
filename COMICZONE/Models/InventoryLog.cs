using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("INVENTORY_LOG")]
public partial class InventoryLog
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("PRODUCT_ID")]
    public int ProductId { get; set; }

    [Column("CHANGE_AMOUNT")]
    public int ChangeAmount { get; set; }

    [Column("TYPE")]
    [StringLength(50)]
    public string? Type { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("InventoryLogs")]
    public virtual Product Product { get; set; } = null!;
}
