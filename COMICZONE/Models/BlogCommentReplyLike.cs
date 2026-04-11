using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[PrimaryKey("Replyid", "Userid")]
[Table("BLOG_COMMENT_REPLY_LIKE")]
public partial class BlogCommentReplyLike
{
    [Key]
    [Column("REPLYID")]
    public int Replyid { get; set; }

    [Key]
    [Column("USERID")]
    public int Userid { get; set; }

    [Column("ISLIKE")]
    public bool? Islike { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [ForeignKey("Replyid")]
    [InverseProperty("BlogCommentReplyLikes")]
    public virtual BlogCommentReply Reply { get; set; } = null!;

    [ForeignKey("Userid")]
    [InverseProperty("BlogCommentReplyLikes")]
    public virtual User User { get; set; } = null!;
}
