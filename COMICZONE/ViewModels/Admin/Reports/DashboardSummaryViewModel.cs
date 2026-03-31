namespace COMICZONE.ViewModels.Admin.Reports
{
    public class DashboardSummaryViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int PendingOrders { get; set; }
        public decimal RevenueGrowth { get; set; } // Tỷ lệ tăng trưởng %
    }
}
