using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_ADVERTISEMENT")]
public partial class MarketplaceAdvertisement
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TITLE")]
    [StringLength(255)]
    public string? Title { get; set; }

    [Column("IMAGE_URL")]
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [Column("TARGET_URL")]
    [StringLength(500)]
    public string? TargetUrl { get; set; }

    [Column("POSITION")]
    [StringLength(50)]
    public string? Position { get; set; }

    [Column("START_DATE", TypeName = "datetime")]
    public DateTime? StartDate { get; set; }

    [Column("END_DATE", TypeName = "datetime")]
    public DateTime? EndDate { get; set; }

    [Column("STATUS")]
    [StringLength(20)]
    public string? Status { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }
}
