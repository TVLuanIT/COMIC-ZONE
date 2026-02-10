using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("BLOGCATEGORY")]
[Index("Slug", Name = "UQ__BLOGCATE__A43AD45C2F2F6D68", IsUnique = true)]
public partial class Blogcategory
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("NAME")]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Column("SLUG")]
    [StringLength(100)]
    public string Slug { get; set; } = null!;

    [InverseProperty("Category")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    [ForeignKey("Categoryid")]
    [InverseProperty("Categories")]
    public virtual ICollection<Blog> BlogsNavigation { get; set; } = new List<Blog>();
}
