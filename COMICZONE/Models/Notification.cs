using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("NOTIFICATION")]
public partial class Notification
{
    [Key]
    [Column("NOTIFICATION_ID")]
    public int NotificationId { get; set; }

    [Column("USER_ID")]
    public int UserId { get; set; }

    [Column("CREATED_BY")]
    public int? CreatedBy { get; set; }

    [Column("TITLE")]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Column("MESSAGE")]
    public string Message { get; set; } = null!;

    [Column("LINK")]
    [StringLength(500)]
    public string? Link { get; set; }

    [Column("IS_READ")]
    public bool? IsRead { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("NotificationCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("NotificationUsers")]
    public virtual User User { get; set; } = null!;
}
