using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("ARTIST")]
[Index("Name", Name = "UQ__ARTISTS__D9C1FA00F949CE6A", IsUnique = true)]
public partial class Artist
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("NAME")]
    [StringLength(150)]
    public string? Name { get; set; }

    [ForeignKey("ArtistId")]
    [InverseProperty("Artists")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
