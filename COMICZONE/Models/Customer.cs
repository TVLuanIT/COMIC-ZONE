using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("CUSTOMERS")]
[Index("Userid", Name = "UQ__CUSTOMER__7B9E7F34BE78050D", IsUnique = true)]
public partial class Customer
{
    [Key]
    [Column("CUSTOMERID")]
    public int Customerid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("FULLNAME")]
    [StringLength(100)]
    public string Fullname { get; set; } = null!;

    [Column("PHONE")]
    [StringLength(20)]
    public string? Phone { get; set; }

    [Column("ADDRESS")]
    [StringLength(255)]
    public string? Address { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime Createdat { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("Customer")]
    public virtual User User { get; set; } = null!;
}
