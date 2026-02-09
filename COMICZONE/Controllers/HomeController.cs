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
            // Nổi bật trong tuần (Featured) → ví dụ dựa vào view count hoặc tiêu chí khác
            var ModelFeatured = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .OrderByDescending(p => p.Id) // giả sử ID càng cao → sản phẩm mới/được quan tâm
                .Take(8)
                .ToListAsync();

            // Mới nhất trong tuần (Latest) – ví dụ lấy 8 sản phẩm mới cập nhật
            var ModelLatest = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .OrderByDescending(p => p.ReleaseDate) // mới nhất
                .Take(8)
                .ToListAsync();

            // Gán ViewBag cho carousel Blog
            ViewBag.Blogs = await _context.Blogs
                .OrderByDescending(b => b.Createdat) // mới nhất trước
                .Take(9) // lấy 9 bài mới nhất
                .ToListAsync();

            // Gửi dữ liệu vào ViewBag để Index.cshtml sử dụng
            ViewBag.ModelFeatured = ModelFeatured;
            ViewBag.ModelLatest = ModelLatest;

            return View();
        }

        // GET: /Home/Search?keyword=...
        // Trang tìm kiếm sản phẩm
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction("Index"); // không có keyword → về trang chủ

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
