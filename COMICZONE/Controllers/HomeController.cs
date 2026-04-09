using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ComiczoneContext _context;

        private readonly IRecommendationService _recommendService;

        public HomeController(ComiczoneContext context, IRecommendationService recommendService)
        {
            _context = context;

            _recommendService = recommendService;
        }

        public IActionResult About()
        {
            return View();
        }

        // GET: Home
        public async Task<IActionResult> Index(string? keyword)
        {
            var userId = CurrentUserId();

            if (IsLoggedIn() && userId != null)
            {
                ViewBag.ModelRecommended = await _recommendService.GetRecommendedProductsAsync(userId);
            }
            else
            {
                ViewBag.ModelRecommended = await _context.Products
                    .Include(p => p.Pictures)
                    .Where(p => !p.Isdeleted)
                    .OrderByDescending(p => p.ReleaseDate)
                    .ThenByDescending(p => p.OrderItems.Count)
                    .Take(8)
                    .ToListAsync();
            }


            // Mới nhất trong tuần (Latest) – ví dụ lấy 8 sản phẩm mới cập nhật
            var ModelLatest = await _context.Products
                .Include(p => p.Pictures)
                .Include(p => p.Artists)
                .Include(p => p.Tags)
                .Where(p => !p.Isdeleted)
                .OrderByDescending(p => p.ReleaseDate) // mới nhất
                .Take(8)
                .ToListAsync();

            // Lấy danh sách thể loại (biểu tượng/tên)
            ViewBag.PopularTags = await _context.Tags
                .Where(t => !t.Isdeleted && t.Products.Any())
                .OrderByDescending(t => t.Products.Count)
                .Take(8)
                .ToListAsync();

            // Thống kê cơ bản
            ViewBag.TotalProducts = await _context.Products.CountAsync(p => !p.Isdeleted);
            ViewBag.TotalArtists = await _context.Artists.CountAsync();
            ViewBag.TotalUsers = await _context.Users.CountAsync(u => (bool)u.Isactive);

            // Gán ViewBag cho carousel Blog
            ViewBag.Blogs = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .OrderByDescending(b => b.Createdat) // mới nhất trước
                .Take(9) // lấy 9 bài mới nhất
                .ToListAsync();

            // Gửi dữ liệu vào ViewBag để Index.cshtml sử dụng
            ViewBag.ModelLatest = ModelLatest;

            return View();
        }
    }
}