using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using Microsoft.EntityFrameworkCore;
using COMICZONE.ViewModels.Admin.Reports;
using COMICZONE.Areas.Admin.ViewModels.Dashboard;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public DashboardController(ComiczoneContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var today = now.Date;

            // 1. Thống kê chung (Dùng lại DashboardSummaryViewModel)
            var summary = new DashboardSummaryViewModel
            {
                TotalUsers = await _context.Orders
                    .Where(o => o.Status != "Cancelled")
                    .Select(o => o.UserId)
                    .Distinct()
                    .CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync(),
                TotalRevenue = await _context.Orders
                    .Where(o => o.Status == "Completed")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Pending")
            };
            summary.RevenueGrowth = 12.5m; // Ví dụ

            // 2. Thống kê hôm nay
            var todayStats = new TodayStatsViewModel
            {
                NewOrders = await _context.Orders.CountAsync(o => o.CreatedAt >= today),
                Revenue = await _context.Orders
                    .Where(o => o.CreatedAt >= today && o.Status == "Completed")
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0,
                PendingReports = await _context.ViolationReports.CountAsync(v => v.Status == 1),
                LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity < 10)
            };

            // 3. Top sản phẩm bán chạy (mọi thời đại hoặc gần đây)
            var topProducts = await _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Pictures)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new TopProductViewModel
                {
                    Product = g.First().Product,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToListAsync();

            // 4. Đơn hàng gần đây
            var recentOrders = await _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Pictures)
                .OrderByDescending(o => o.Order.OrderDate)
                .Take(5)
                .Select(o => new RecentOrderViewModel
                {
                    OrderId = o.OrderId,
                    Product = o.Product,
                    Status = o.Order.Status,
                    OrderDate = o.Order.OrderDate ?? DateTime.Now
                })
                .ToListAsync();

            // 5. Sản phẩm sắp hết hàng
            var lowStock = await _context.Products
                .Include(p => p.Pictures)
                .Where(p => p.StockQuantity < 10 && !p.Isdeleted)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();

            // 6. Đánh giá mới nhất
            var latestReviews = await _context.ProductReviews
                .Include(r => r.Product)
                    .ThenInclude(p => p.Pictures)
                .Include(r => r.User)
                    .ThenInclude(u => u.Customer)
                .OrderByDescending(r => r.Createdat)
                .Take(5)
                .ToListAsync();

            var hour = DateTime.Now.Hour;
            string greeting = hour switch
            {
                >= 5 and < 12 => "Chào buổi sáng",
                >= 12 and < 18 => "Chào buổi chiều",
                _ => "Chào buổi tối"
            };

            var viewModel = new AdminDashboardViewModel
            {
                UserName = HttpContext.Session.GetString("Username") ?? "Quản trị viên",
                Greeting = greeting,
                Summary = summary,
                TodayStats = todayStats,
                TopSellingProducts = topProducts,
                RecentOrders = recentOrders,
                LowStockProducts = lowStock,
                LatestReviews = latestReviews
            };

            return View(viewModel);
        }
    }
}