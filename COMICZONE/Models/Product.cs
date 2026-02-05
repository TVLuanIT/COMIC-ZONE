using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("PRODUCTS")]
public partial class Product
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("STT")]
    public int? Stt { get; set; }

    [Column("NAME")]
    [StringLength(255)]
    public string? Name { get; set; }

    [Column("PRICE")]
    public int? Price { get; set; }

    [Column("DISTRIBUTOR")]
    [StringLength(150)]
    public string? Distributor { get; set; }

    [Column("AUTHOR")]
    [StringLength(150)]
    public string? Author { get; set; }

    [Column("TRANSLATOR")]
    [StringLength(150)]
    public string? Translator { get; set; }

    [Column("SERIES")]
    [StringLength(255)]
    public string? Series { get; set; }

    [Column("DESCRIPTION")]
    public string? Description { get; set; }

    [Column("STOCK_QUANTITY")]
    public int? StockQuantity { get; set; }

    [Column("FORMAT")]
    [StringLength(50)]
    public string? Format { get; set; }

    [Column("SIZE")]
    [StringLength(50)]
    public string? Size { get; set; }

    [Column("WEIGHT")]
    [StringLength(50)]
    public string? Weight { get; set; }

    [Column("PAGES")]
    public int? Pages { get; set; }

    [Column("ILLUSTRATION_TYPE")]
    [StringLength(50)]
    public string? IllustrationType { get; set; }

    [Column("RELEASE_DATE")]
    public DateOnly? ReleaseDate { get; set; }

    [Column("PUBLISHER")]
    [StringLength(150)]
    public string? Publisher { get; set; }

    [Column("AGE_GROUP")]
    [StringLength(20)]
    public string? AgeGroup { get; set; }

    [ForeignKey("ProductId")]
    [InverseProperty("Products")]
    public virtual ICollection<Artist> Artists { get; set; } = new List<Artist>();

    [ForeignKey("ProductId")]
    [InverseProperty("Products")]
    public virtual ICollection<Picture> Pictures { get; set; } = new List<Picture>();

    [ForeignKey("ProductId")]
    [InverseProperty("Products")]
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
