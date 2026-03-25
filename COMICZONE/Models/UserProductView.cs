using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("USER_PRODUCT_VIEW")]
public partial class UserProductView
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("PRODUCT_ID")]
    public int ProductId { get; set; }

    [Column("VIEWED_AT", TypeName = "datetime")]
    public DateTime? ViewedAt { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("UserProductViews")]
    public virtual Product Product { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserProductViews")]
    public virtual User User { get; set; } = null!;
}
