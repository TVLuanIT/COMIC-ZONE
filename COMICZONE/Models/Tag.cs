using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("TAG")]
[Index("Name", Name = "UQ__TAGS__D9C1FA00C06004D3", IsUnique = true)]
public partial class Tag
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("NAME")]
    [StringLength(150)]
    public string? Name { get; set; }

    [ForeignKey("TagId")]
    [InverseProperty("Tags")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
