using System;
using System.Collections.Generic;
using COMICZONE.ViewModels.Admin.Reports;
using COMICZONE.Models;

namespace COMICZONE.Areas.Admin.ViewModels.Dashboard
{
    public class AdminDashboardViewModel
    {
        public string? UserName { get; set; }
        public string Greeting { get; set; } = "Chào buổi sáng";
        public DashboardSummaryViewModel Summary { get; set; } = null!;
        public IEnumerable<TopProductViewModel> TopSellingProducts { get; set; }
        public IEnumerable<RecentOrderViewModel> RecentOrders { get; set; }
        
        // Dữ liệu vận hành mới
        public IEnumerable<Product>? LowStockProducts { get; set; }
        public IEnumerable<ProductReview>? LatestReviews { get; set; }
        public TodayStatsViewModel? TodayStats { get; set; }
    }

    public class TopProductViewModel
    {
        public Product Product { get; set; } = null!;
        public int TotalSold { get; set; }
    }

    public class RecentOrderViewModel
    {
        public Product Product { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime OrderDate { get; set; }
        public int OrderId { get; set; }
    }

    public class TodayStatsViewModel
    {
        public int NewOrders { get; set; }
        public decimal Revenue { get; set; }
        public int PendingReports { get; set; }
        public int LowStockCount { get; set; }
    }
}
