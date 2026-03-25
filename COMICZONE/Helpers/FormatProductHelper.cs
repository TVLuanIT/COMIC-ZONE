using COMICZONE.Models;

namespace COMICZONE.Helpers
{
    public static class FormatProductHelper
    {
        public static string FormatProducts(List<Product> products)
        {
            if (products == null || !products.Any())
                return "Không tìm thấy sản phẩm phù hợp.";

            return string.Join("\n",
                products.Select(p =>
                    $"{p.Name} - {p.Price:N0} VNĐ"));
        }

        public static string GetShippingInfo()
        {
            return @"
                COMICZONE hỗ trợ:

                • Giao hàng toàn quốc
                • Thanh toán VNPay
                • Thanh toán khi nhận hàng (COD)
                • Thời gian giao: 2–5 ngày làm việc
                ";
        }
    }
}