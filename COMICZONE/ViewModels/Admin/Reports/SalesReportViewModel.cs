using System;

namespace COMICZONE.ViewModels.Admin.Reports
{
    public class SalesReportViewModel
    {
        public DateTime Date { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageOrderValue => OrderCount > 0 ? TotalRevenue / OrderCount : 0;
    }
}
