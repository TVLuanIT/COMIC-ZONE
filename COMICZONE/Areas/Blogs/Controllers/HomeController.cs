using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;

namespace COMICZONE.Areas.Blogs.Controllers
{
    [Area("Blogs")]
    public class HomeController : COMICZONE.Controllers.BaseController
    {
        private readonly ComiczoneContext _context;

        public HomeController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Blogs
        public async Task<IActionResult> Index(int page = 1)
        {
            int pageSize = 9;
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted);

            int totalItems = await query.CountAsync();
            var blogs = await query
                .OrderByDescending(b => b.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pagination = new PaginationModel
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Action = "Index",
                Controller = "Home",
                PageParam = "page",
                ExtraParams = new Dictionary<string, string>()
            };

            ViewBag.PopularCategories = await _context.BlogCategories
                .Where(c => !c.Isdeleted)
                .Take(10)
                .ToListAsync();

            ViewBag.TopUsers = await _context.Users
                .Where(u => !u.Isdeleted && u.Isactive)
                .OrderBy(u => u.Createdat)
                .Take(3)
                .ToListAsync();

            return View(blogs);
        }

        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Include(b => b.BlogComments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(m => m.Id == id && !m.Isdeleted);

            if (blog == null)
            {
                return NotFound();
            }

            // Bài viết liên quan
            ViewBag.RelatedBlogs = await _context.Blogs
                .Where(b => b.Id != id && b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted)
                .Include(b => b.Author)
                .OrderByDescending(b => b.Createdat)
                .Take(3)
                .ToListAsync();

            return View(blog);
        }
    }
}
