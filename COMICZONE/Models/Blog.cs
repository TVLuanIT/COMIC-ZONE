using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Models.Enums;

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
    public BlogStatus Status { get; set; }

    [Column("AUTHORID")]
    public int Authorid { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime Createdat { get; set; }

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [Column("ISDELETED")]
    public bool Isdeleted { get; set; }

    [ForeignKey("Authorid")]
    [InverseProperty("Blogs")]
    public virtual User Author { get; set; } = null!;

    [InverseProperty("Blog")]
    public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();

    [ForeignKey("Blogid")]
    [InverseProperty("Blogs")]
    public virtual ICollection<BlogCategory> Categories { get; set; } = new List<BlogCategory>();
}
