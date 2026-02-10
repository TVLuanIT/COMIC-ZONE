using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;

namespace COMICZONE.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ComiczoneContext _context;

        public ProductsController(ComiczoneContext context)
        {
            _context = context;
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
