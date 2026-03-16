using COMICZONE.Models;
using COMICZONE.Extensions;

namespace COMICZONE.Helpers
{
    public static class OrderHelper
    {
        public static string GetStatusBadge(string status)
        {
            if (status == "Completed")
                return "<span class='badge bg-success'>Hoàn thành</span>";

            if (status == "Pending")
                return "<span class='badge bg-warning'>Chờ xử lý</span>";

            return "<span class='badge bg-primary'>Đang xử lý</span>";
        }

        public static string GetProductCell(this Product product)
        {
            return $"""
                <div class='d-flex align-items-center gap-2'>
                    <img src='{product.GetImagePath()}'
                         style='width:50px;height:70px;object-fit:cover;border-radius:6px;'>
                    <span>{product.Name}</span>
                </div>
                """;
        }
    }
}
