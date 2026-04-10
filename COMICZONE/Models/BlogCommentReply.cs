using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("BLOG_COMMENT_REPLY")]
public partial class BlogCommentReply
{
    [Key]
    [Column("REPLYID")]
    public int Replyid { get; set; }

    [Column("COMMENTID")]
    public int Commentid { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("CONTENT")]
    public string Content { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("UPDATEDAT", TypeName = "datetime")]
    public DateTime? Updatedat { get; set; }

    [Column("REPLYTOUSERID")]
    public int? Replytouserid { get; set; }

    [Column("PARENTREPLYID")]
    public int? Parentreplyid { get; set; }

    [Column("ISDELETED")]
    public bool? Isdeleted { get; set; }

    [InverseProperty("Reply")]
    public virtual ICollection<BlogCommentReplyLike> BlogCommentReplyLikes { get; set; } = new List<BlogCommentReplyLike>();

    [ForeignKey("Commentid")]
    [InverseProperty("BlogCommentReplies")]
    public virtual BlogComment Comment { get; set; } = null!;

    [InverseProperty("Parentreply")]
    public virtual ICollection<BlogCommentReply> InverseParentreply { get; set; } = new List<BlogCommentReply>();

    [ForeignKey("Parentreplyid")]
    [InverseProperty("InverseParentreply")]
    public virtual BlogCommentReply? Parentreply { get; set; }

    [ForeignKey("Replytouserid")]
    [InverseProperty("BlogCommentReplyReplytousers")]
    public virtual User? Replytouser { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("BlogCommentReplyUsers")]
    public virtual User User { get; set; } = null!;
}
