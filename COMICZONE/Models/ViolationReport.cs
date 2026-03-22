using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Models;

[Table("VIOLATION_REPORT")]
public partial class ViolationReport
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("USERID")]
    public int Userid { get; set; }

    [Column("REPORTTYPE")]
    public int Reporttype { get; set; }

    [Column("TARGETID")]
    public int Targetid { get; set; }

    [Column("REASON")]
    [StringLength(500)]
    public string Reason { get; set; } = null!;

    [Column("STATUS")]
    public int Status { get; set; }

    [Column("CREATEDAT")]
    public DateTime Createdat { get; set; }

    [Column("ISDELETED")]
    public bool Isdeleted { get; set; }

    [ForeignKey("Userid")]
    [InverseProperty("ViolationReports")]
    public virtual User User { get; set; } = null!;
}
