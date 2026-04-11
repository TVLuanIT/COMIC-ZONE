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
    public string Content { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime Createdat { get; set; }

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [Column("ISDELETED")]
    public bool? Isdeleted { get; set; }

    [ForeignKey("Blogid")]
    [InverseProperty("BlogComments")]
    public virtual Blog Blog { get; set; } = null!;

    [InverseProperty("Comment")]
    public virtual ICollection<BlogCommentLike> BlogCommentLikes { get; set; } = new List<BlogCommentLike>();

    [InverseProperty("Comment")]
    public virtual ICollection<BlogCommentReply> BlogCommentReplies { get; set; } = new List<BlogCommentReply>();

    [ForeignKey("Userid")]
    [InverseProperty("BlogComments")]
    public virtual User User { get; set; } = null!;
}
