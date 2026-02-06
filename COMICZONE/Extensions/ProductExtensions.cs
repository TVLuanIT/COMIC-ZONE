using COMICZONE.Models;

namespace COMICZONE.Extensions
{
    public static class ProductExtensions
    {
        public static string GetImagePath(this Product product)
        {
            var picture = product.Pictures.FirstOrDefault();

            if (picture == null || string.IsNullOrEmpty(picture.FileName))
                return "/images/default.png";

            return "/images/" + picture.FileName;
        }
    }
}
