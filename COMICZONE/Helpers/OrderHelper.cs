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
    }
}
