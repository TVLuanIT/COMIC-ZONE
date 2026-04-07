using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("BLOG_COMMENT")]
public partial class BlogComment
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("BLOGID")]
    public int Blogid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("CONTENT")]
    [StringLength(500)]
    public string Content { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime Createdat { get; set; }

    [ForeignKey("Blogid")]
    [InverseProperty("BlogComments")]
    public virtual Blog Blog { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("BlogComments")]
    public virtual User User { get; set; } = null!;
}
