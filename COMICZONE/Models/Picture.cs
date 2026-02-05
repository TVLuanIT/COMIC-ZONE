using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PICTURES")]
public partial class Picture
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("FILE_NAME")]
    [StringLength(200)]
    public string? FileName { get; set; }

    [ForeignKey("PictureId")]
    [InverseProperty("Pictures")]
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
