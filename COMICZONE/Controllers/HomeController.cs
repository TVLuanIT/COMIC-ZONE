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
    public class HomeController : Controller
    {
        private readonly ComiczoneContext _context;

        public HomeController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Home
        public async Task<IActionResult> Index(string? keyword)
        {
            var products = _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var key = keyword.ToLower();

                products = products.Where(p =>
                    (p.Name ?? "").ToLower().Contains(key) ||
                    (p.Author ?? "").ToLower().Contains(key) ||
                    (p.Series ?? "").ToLower().Contains(key) ||
                    (p.Publisher ?? "").ToLower().Contains(key) ||

                    //tìm theo Artist
                    p.Artists.Any(a => (a.Name ?? "").ToLower().Contains(key)) ||

                    //tìm theo Tag
                    p.Tags.Any(t => (t.Name ?? "").ToLower().Contains(key))
                );
            }

            // Gán ViewBag cho carousel Blog
            ViewBag.Blogs = _context.Blogs
                .OrderByDescending(b => b.Createdat) // mới nhất trước
                .Take(9) // lấy 9 bài mới nhất
                .ToList();

            ViewBag.Keyword = keyword;

            return View(await products.ToListAsync());
        }

        // GET: Home/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
