using System.ComponentModel.DataAnnotations.Schema;
using COMICZONE.Helpers;
using COMICZONE.Models.Enums;

namespace COMICZONE.Models
{
    public partial class Blog
    {
        [NotMapped]
        public BlogStatus BlogStatusEnum
        {
            get => EnumHelper.ParseOrThrow<BlogStatus>(Status);
            set => Status = value.ToString();
        }
    }
}
