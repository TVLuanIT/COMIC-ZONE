using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_FAVORITE")]
public partial class MarketplaceFavorite
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("POSTID")]
    public int Postid { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Postid")]
    [InverseProperty("MarketplaceFavorites")]
    public virtual MarketplacePost Post { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("MarketplaceFavorites")]
    public virtual User User { get; set; } = null!;
}
