using System;

namespace COMICZONE.Areas.Admin.ViewModels.Reports
{
    public class SalesReportViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue => OrderCount > 0 ? TotalRevenue / OrderCount : 0;
    }
}
