using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

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
            // 1. Lấy bài viết nổi bật (luôn là bài mới nhất)
            var featuredBlog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted)
                .OrderByDescending(b => b.Createdat)
                .FirstOrDefaultAsync();

            ViewBag.FeaturedBlog = featuredBlog;

            // 2. Lấy danh sách các bài viết còn lại (phân trang)
            int pageSize = 6; // Hiển thị 6 bài mỗi trang để tổng cộng khoảng 7 bài (1 nổi bật + 6 thường)
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted);

            if (featuredBlog != null)
            {
                query = query.Where(b => b.Id != featuredBlog.Id);
            }

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
                Area = "Blogs",
                Action = "Index",
                Controller = "Home",
                PageParam = "page",
                ExtraParams = new Dictionary<string, string>()
            };

            ViewBag.PopularCategories = await _context.BlogCategories
                .Where(c => !c.Isdeleted)
                .Include(c => c.Blogs.Where(b => b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted))
                .OrderByDescending(c => c.Blogs.Count(b => b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted))
                .Take(10)
                .ToListAsync();

            ViewBag.TopUsers = await _context.Users
                .Where(u => !u.Isdeleted && u.Isactive)
                .OrderBy(u => u.Createdat)
                .Take(3)
                .ToListAsync();

            return View(blogs);
        }

        // GET: Blogs/Search
        public async Task<IActionResult> Search(string? keyword, string? sortBy, int? categoryId, int? authorId, string? dateRange, int page = 1)
        {

            int pageSize = 9;
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Where(b => b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted);

            // Filter by keyword
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(b => b.Title.Contains(keyword) || 
                                       b.Shortdescription.Contains(keyword) || 
                                       b.Content.Contains(keyword));
            }

            // Filter by category
            if (categoryId.HasValue)
            {
                query = query.Where(b => b.Categories.Any(c => c.Id == categoryId.Value));
            }

            // Filter by author
            if (authorId.HasValue)
            {
                query = query.Where(b => b.Authorid == authorId.Value);
            }

            // Filter by date range
            if (!string.IsNullOrEmpty(dateRange))
            {
                var now = DateTime.Now;
                switch (dateRange)
                {
                    case "today":
                        query = query.Where(b => b.Createdat >= now.Date);
                        break;
                    case "week":
                        query = query.Where(b => b.Createdat >= now.AddDays(-7));
                        break;
                    case "month":
                        query = query.Where(b => b.Createdat >= now.AddDays(-30));
                        break;
                    case "year":
                        query = query.Where(b => b.Createdat >= now.AddDays(-365));
                        break;
                }
            }

            // Sorting logic
            switch (sortBy)
            {
                case "oldest":
                    query = query.OrderBy(b => b.Createdat);
                    break;
                case "title_asc":
                    query = query.OrderBy(b => b.Title);
                    break;
                case "title_desc":
                    query = query.OrderByDescending(b => b.Title);
                    break;
                case "newest":
                default:
                    query = query.OrderByDescending(b => b.Createdat);
                    sortBy = "newest";
                    break;
            }

            ViewBag.Keyword = keyword;
            ViewBag.SortBy = sortBy;
            ViewBag.SelectedCategoryId = categoryId;
            ViewBag.SelectedAuthorId = authorId;
            ViewBag.SelectedDateRange = dateRange;

            int totalItems = await query.CountAsync();
            var blogs = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Pagination = new PaginationModel
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Area = "Blogs",
                Action = "Search",
                Controller = "Home",
                PageParam = "page",
                ExtraParams = new Dictionary<string, string> 
                { 
                    { "keyword", keyword },
                    { "sortBy", sortBy }
                }
            };

            if (categoryId.HasValue) ViewBag.Pagination.ExtraParams.Add("categoryId", categoryId.Value.ToString());
            if (authorId.HasValue) ViewBag.Pagination.ExtraParams.Add("authorId", authorId.Value.ToString());
            if (!string.IsNullOrEmpty(dateRange)) ViewBag.Pagination.ExtraParams.Add("dateRange", dateRange);

            ViewBag.AllCategories = await _context.BlogCategories.Where(c => !c.Isdeleted).ToListAsync();
            
            // Lấy danh sách các tác giả có ít nhất 1 bài viết được duyệt
            ViewBag.Authors = await _context.Users
                .Where(u => _context.Blogs.Any(b => b.Authorid == u.Id && b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted))
                .Select(u => new { u.Id, u.Username })
                .ToListAsync();

            ViewBag.TopUsers = await _context.Users
                .Where(u => !u.Isdeleted && u.Isactive)
                .OrderBy(u => u.Createdat)
                .Take(3)
                .ToListAsync();

            return View(blogs);
        }
    }
}
