using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum MarketplacePostCondition
    {
        [Display(Name = "Như mới (99%)")]
        NewLike,

        [Display(Name = "Rất tốt (90-95%)")]
        VeryGood,

        [Display(Name = "Tốt (80-85%)")]
        Good,

        [Display(Name = "Sử dụng được")]
        Acceptable
    }
}
