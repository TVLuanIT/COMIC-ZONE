using COMICZONE.Models;

namespace COMICZONE.Extensions
{
    public static class ProductExtensions
    {
        public static string GetImagePath(this Product product)
        {
            var picture = product?.Pictures?.FirstOrDefault();

            if (picture == null || string.IsNullOrEmpty(picture.FileName))
                return "/images/products/default.png";

            // Nếu DB đã có extension
            if (Path.HasExtension(picture.FileName))
            {
                return "/images/products/" + picture.FileName;
            }

            // Nếu DB không có extension thì tìm file thật
            string folder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot/images/products"
            );

            string[] extensions = { ".jpg", ".jpeg", ".png", ".webp" };

            foreach (var ext in extensions)
            {
                var path = Path.Combine(folder, picture.FileName + ext);

                if (File.Exists(path))
                {
                    return "/images/products/" + picture.FileName + ext;
                }
            }

            return "/images/products/default.png";
        }
    }
}