using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("BLOG_CATEGORY")]
[Index("Slug", Name = "UQ_BLOG_CATEGORY_SLUG", IsUnique = true)]
public partial class BlogCategory
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

    [Column("ISDELETED")]
    public bool Isdeleted { get; set; }

    [ForeignKey("Categoryid")]
    [InverseProperty("Categories")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
}
