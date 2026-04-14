using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("MARKETPLACE_MESSAGE")]
public partial class MarketplaceMessage
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("SENDERID")]
    public int Senderid { get; set; }

    [Column("RECEIVERID")]
    public int Receiverid { get; set; }

    [Column("POSTID")]
    public int? Postid { get; set; }

    [Column("MESSAGE")]
    public string Message { get; set; } = null!;

    [Column("CREATEDAT", TypeName = "datetime")]
    public DateTime? Createdat { get; set; }

    [Column("ISREAD")]
    public bool? Isread { get; set; }

    [Column("CREATED_AT", TypeName = "datetime")]
    public DateTime? CreatedAt1 { get; set; }

    [ForeignKey("Postid")]
    [InverseProperty("MarketplaceMessages")]
    public virtual MarketplacePost? Post { get; set; }

    [ForeignKey("Receiverid")]
    [InverseProperty("MarketplaceMessageReceivers")]
    public virtual User Receiver { get; set; } = null!;

    [ForeignKey("Senderid")]
    [InverseProperty("MarketplaceMessageSenders")]
    public virtual User Sender { get; set; } = null!;
}
