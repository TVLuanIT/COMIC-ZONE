using COMICZONE.Models;

namespace COMICZONE.Extensions
{
    public static class ProductExtensions
    {
        public static string GetImagePath(this Product product)
        {
            var picture = product.Pictures.FirstOrDefault();
            Console.WriteLine($"Product ID: {product.Id}, Picture File: {picture?.FileName}");
            if (picture == null || string.IsNullOrEmpty(picture.FileName))
                return "/images/default.png";

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");

            // Tìm file thực tế có tên như trong DB, bất kể extension
            var file = Directory.GetFiles(folder, picture.FileName + ".*")
                                .FirstOrDefault();

            if (!string.IsNullOrEmpty(file))
            {
                // Lấy tên file có extension
                return "/images/" + Path.GetFileName(file);
            }

            // Nếu không tìm thấy, dùng ảnh mặc định
            return "/images/default.png";
        }
    }
}
