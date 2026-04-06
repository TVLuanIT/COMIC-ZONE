using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum UserRole
    {
        [Display(Name = "Quản trị viên")]
        Admin = 1,

        [Display(Name = "Khách hàng")]
        Customer = 2
    }
}
