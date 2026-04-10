using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[PrimaryKey("Commentid", "Userid")]
[Table("BLOG_COMMENT_LIKE")]
public partial class BlogCommentLike
{
    [Key]
    [Column("COMMENTID")]
    public int Commentid { get; set; }

    [Key]
    [Column("USERID")]
    public int Userid { get; set; }

    [Column("ISLIKE")]
    public bool? Islike { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Commentid")]
    [InverseProperty("BlogCommentLikes")]
    public virtual BlogComment Comment { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("BlogCommentLikes")]
    public virtual User User { get; set; } = null!;
}
