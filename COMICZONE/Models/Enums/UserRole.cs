using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum UserRole
    {
        [Display(Name = "Quản trị viên")]
        Admin = 1,

        [Display(Name = "Người dùng")]
        User = 2
    }
}
