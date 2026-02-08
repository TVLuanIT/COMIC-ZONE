using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("BLOG")]
[Index("Slug", Name = "UQ__BLOG__A43AD45CCDF52DC9", IsUnique = true)]
public partial class Blog
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("TITLE")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("SLUG")]
    [StringLength(255)]
    public string Slug { get; set; } = null!;

    [Column("SHORTDESCRIPTION")]
    [StringLength(500)]
    public string Shortdescription { get; set; } = null!;

    [Column("CONTENT")]
    public string Content { get; set; } = null!;

    [Column("THUMBNAIL")]
    [StringLength(255)]
    public string? Thumbnail { get; set; }

    [Column("STATUS")]
    [StringLength(20)]
    public string Status { get; set; } = null!;

    [Column("AUTHORID")]
    public int Authorid { get; set; }

    [Column("CATEGORYID")]
    public int Categoryid { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime Createdat { get; set; }

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [ForeignKey("Authorid")]
    [InverseProperty("Blogs")]
    public virtual User Author { get; set; } = null!;

    [InverseProperty("Blog")]
    public virtual ICollection<Blogcomment> Blogcomments { get; set; } = new List<Blogcomment>();

    [ForeignKey("Categoryid")]
    [InverseProperty("Blogs")]
    public virtual Blogcategory Category { get; set; } = null!;

    [ForeignKey("Blogid")]
    [InverseProperty("BlogsNavigation")]
    public virtual ICollection<Blogcategory> Categories { get; set; } = new List<Blogcategory>();
}
