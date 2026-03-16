using Microsoft.AspNetCore.Mvc;
using COMICZONE.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DashboardController : Controller
    {
        private readonly ComiczoneContext _context;

        public DashboardController(ComiczoneContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            // _Stats.cshtml
            ViewBag.TotalUsers = _context.Users.Count();
            ViewBag.TotalProducts = _context.Products.Count();
            ViewBag.TotalOrders = _context.Orders.Count();

            ViewBag.TotalRevenue = _context.Orders
                .Where(o => o.Status == "Completed")
                .Sum(o => (decimal?)o.TotalAmount) ?? 0;

            // _TopSellingProducts.cshtml
            var topProducts = _context.OrderItems
                .Include(oi => oi.Product)
                    .ThenInclude(p => p.Pictures)
                .GroupBy(oi => oi.ProductId)
                .Select(g => new
                {
                    Product = g.First().Product,
                    TotalSold = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(5)
                .ToList();

            // _RecentOrders.cshtml
            var recentOrders = _context.OrderItems
                .Include(o => o.Order)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Pictures)
                .OrderByDescending(o => o.OrderId)
                .Take(5)
                .Select(o => new {
                    Product = o.Product,
                    Status = o.Order.Status
                })
                .ToList();

            ViewBag.TopSellingProducts = topProducts;
            ViewBag.RecentOrders = recentOrders;

            return View();
        }

    }

}