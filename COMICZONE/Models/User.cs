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

    [InverseProperty("User")]
    public virtual ICollection<Blogcomment> Blogcomments { get; set; } = new List<Blogcomment>();

    [InverseProperty("Author")]
    public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();

    [InverseProperty("User")]
    public virtual Customer? Customer { get; set; }
}
