using System.ComponentModel.DataAnnotations;

namespace COMICZONE.Models.Enums
{
    public enum MarketplacePostCategory
    {
        [Display(Name = "Manga")]
        Manga,

        [Display(Name = "Manhua")]
        Manhua,

        [Display(Name = "Manhwa")]
        Manhwa,

        [Display(Name = "Comics (Âu Mỹ)")]
        Comics,

        [Display(Name = "Light Novel / Tiểu thuyết")]
        Novel,

        [Display(Name = "Anime Goods (Phụ kiện)")]
        AnimeGoods,

        [Display(Name = "Figures / Models (Mô hình)")]
        Figures,

        [Display(Name = "Khác")]
        Others
    }
}
