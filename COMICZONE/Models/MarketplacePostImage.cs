using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_POST_IMAGE")]
public partial class MarketplacePostImage
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("POSTID")]
    public int Postid { get; set; }

    [Column("FILENAME")]
    [StringLength(255)]
    public string Filename { get; set; } = null!;

    [ForeignKey("Postid")]
    [InverseProperty("MarketplacePostImages")]
    public virtual MarketplacePost Post { get; set; } = null!;
}
