using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("USER")]
[Index("Email", Name = "UQ__USERS__161CF72446248F65", IsUnique = true)]
[Index("Username", Name = "UQ__USERS__B15BE12E39282666", IsUnique = true)]
public partial class User
{
    [Key]
    public int Id { get; set; }

    [Column("USERNAME")]
    [StringLength(50)]
    public string Username { get; set; } = null!;

    [Column("PASSWORDHASH")]
    [StringLength(255)]
    public string Passwordhash { get; set; } = null!;

    [Column("EMAIL")]
    [StringLength(100)]
    public string? Email { get; set; }

    [Column("ROLE")]
    [StringLength(20)]
    public string Role { get; set; } = null!;

    [Column("ISACTIVE")]
    public bool Isactive { get; set; }

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime Createdat { get; set; }

    [Column("AVATAR")]
    [StringLength(500)]
    public string? Avatar { get; set; }

    [Column("RESET_TOKEN")]
    [StringLength(200)]
    public string? ResetToken { get; set; }

    [Column("RESET_TOKEN_EXPIRE", TypeName = "datetime")]
    public DateTime? ResetTokenExpire { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Blogcomment> Blogcomments { get; set; } = new List<Blogcomment>();

    [InverseProperty("Author")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    [InverseProperty("User")]
    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [InverseProperty("User")]
    public virtual Customer? Customer { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Notification> NotificationCreatedByNavigations { get; set; } = new List<Notification>();

    [InverseProperty("User")]
    public virtual ICollection<Notification> NotificationUsers { get; set; } = new List<Notification>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<OrderStatusHistory> OrderStatusHistories { get; set; } = new List<OrderStatusHistory>();

    [InverseProperty("User")]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    [InverseProperty("User")]
    public virtual ICollection<ProductReviewLike> ProductReviewLikes { get; set; } = new List<ProductReviewLike>();

    [InverseProperty("User")]
    public virtual ICollection<ProductReviewReplyLike> ProductReviewReplyLikes { get; set; } = new List<ProductReviewReplyLike>();

    [InverseProperty("Replytouser")]
    public virtual ICollection<ProductReviewReply> ProductReviewReplyReplytousers { get; set; } = new List<ProductReviewReply>();

    [InverseProperty("User")]
    public virtual ICollection<ProductReviewReply> ProductReviewReplyUsers { get; set; } = new List<ProductReviewReply>();

    [InverseProperty("User")]
    public virtual ICollection<ProductReview> ProductReviews { get; set; } = new List<ProductReview>();

    [InverseProperty("User")]
    public virtual ICollection<ViolationReport> ViolationReports { get; set; } = new List<ViolationReport>();
}
