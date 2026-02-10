using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ComiczoneContext _context;

        public ProductsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: /Products
        public async Task<IActionResult> Index(string sortOrder, int page = 1)
        {
            int pageSize = 9; // số sản phẩm mỗi trang

            var products = _context.Products
                                    .Include(p=>p.Artists)
                                    .Include(p=>p.Tags)
                                    .Include(p=>p.Pictures)
                                    .AsQueryable();
            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "newest":
                    products = products.OrderByDescending(p => p.Id); // giả sử Id tăng theo thời gian
                    break;
                case "oldest":
                    products = products.OrderBy(p => p.Id); // giả sử Id giảm theo thời gian
                    break;
                default:
                    products = products.OrderBy(p => p.Name); // mặc định theo tên tăng dần
                    break;
            }

            int totalItems = await products.CountAsync();
            int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var pagedProducts = await products
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.SectionTitle = "Danh sách sản phẩm";
            // Xử lý sắp xếp
            ViewBag.CurrentSort = sortOrder; // giữ giá trị hiện tại cho dropdown
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedProducts);
        }

        // GET: /Products/Search?keyword=...
        // Trang tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                ViewBag.Message = "Vui lòng nhập từ khóa tìm kiếm";
                return View(new List<Product>());
            }

            var key = keyword.ToLower();

            var products = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .Where(p =>
                    (p.Name ?? "").ToLower().Contains(key) ||
                    (p.Author ?? "").ToLower().Contains(key) ||
                    (p.Series ?? "").ToLower().Contains(key) ||
                    (p.Publisher ?? "").ToLower().Contains(key) ||
                    p.Artists.Any(a => (a.Name ?? "").ToLower().Contains(key)) ||
                    p.Tags.Any(t => (t.Name ?? "").ToLower().Contains(key))
                )
                .ToListAsync();

            ViewBag.Keyword = keyword;

            return View(products); // Trả về Search.cshtml
        }
    }
}
