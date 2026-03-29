using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Helpers;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class User
    {
        [NotMapped]
        public UserRole UserRoleEnum
        {
            get => EnumHelper.ParseOrThrow<UserRole>(Role);

            set => Role = value.ToString();
        }
    }
}
