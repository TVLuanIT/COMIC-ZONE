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

        // GET: Blogs/Create
        public async Task<IActionResult> Create()
        {
            if (!IsLoggedIn())
            {
                TempData["LoginRequired"] = "Bạn cần đăng nhập để viết bài.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categories = await _context.BlogCategories.Where(c => !c.Isdeleted).ToListAsync();
            return View();
        }

        // POST: Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Shortdescription,Content")] Blog blog, int[] selectedCategories, IFormFile? thumbnailFile)
        {
            if (!IsLoggedIn())
            {
                TempData["LoginRequired"] = "Bạn cần đăng nhập để viết bài.";
                return RedirectToAction(nameof(Index));
            }

            ModelState.Remove("Author");
            ModelState.Remove("Categories");
            ModelState.Remove("Slug");
            ModelState.Remove("Status");

            if (selectedCategories == null || selectedCategories.Length == 0)
            {
                ModelState.AddModelError("selectedCategories", "Vui lòng chọn ít nhất một danh mục cho bài viết.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle Thumbnail
                    if (thumbnailFile != null && thumbnailFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs");

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        var filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await thumbnailFile.CopyToAsync(stream);
                        }
                        blog.Thumbnail = fileName;
                    }

                    // Metadata
                    blog.Authorid = int.Parse(CurrentUserId()!);
                    blog.Createdat = DateTime.Now;
                    blog.Updatedat = DateTime.Now;
                    blog.Status = BlogStatus.Pending.ToString();
                    blog.Isdeleted = false;

                    // Generate Slug
                    blog.Slug = GenerateSlug(blog.Title);

                    // Categories
                    if (selectedCategories != null && selectedCategories.Length > 0)
                    {
                        foreach (var categoryId in selectedCategories)
                        {
                            var category = await _context.BlogCategories.FindAsync(categoryId);
                            if (category != null)
                            {
                                blog.Categories.Add(category);
                            }
                        }
                    }

                    _context.Add(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Bài viết của bạn đã được gửi và đang chờ phê duyệt. Cảm ơn bạn đã đóng góp!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu bài viết: " + ex.Message);
                }
            }

            ViewBag.Categories = await _context.BlogCategories.Where(c => !c.Isdeleted).ToListAsync();
            ViewBag.SelectedCategories = selectedCategories?.ToList() ?? new List<int>();
            return View(blog);
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";

            string slug = title.ToLowerInvariant();

            // Thay thế các ký tự có dấu
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    slug = slug.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            // Xóa các ký tự đặc biệt
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            // Xóa khoảng trắng thừa
            slug = Regex.Replace(slug, @"\s+", " ").Trim();
            // Thay khoảng trắng bằng gạch ngang
            slug = slug.Replace(" ", "-");

            // Đảm bảo unique (trong thực tế nên check DB, ở đây làm đơn giản thêm ID ngẫu nhiên)
            slug += "-" + Guid.NewGuid().ToString().Substring(0, 4);

            return slug;
        }

        // POST: Blogs/Home/AddComment
        [HttpPost]
        public async Task<IActionResult> AddComment(int blogId, string content)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để bình luận." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung bình luận không được để trống." });
            }

            try
            {
                var comment = new BlogComment
                {
                    Blogid = blogId,
                    Userid = int.Parse(CurrentUserId()!),
                    Content = content,
                    Createdat = DateTime.Now,
                    Isdeleted = false
                };

                _context.BlogComments.Add(comment);
                await _context.SaveChangesAsync();

                // Nạp thêm thông tin User để trả về cho Client
                await _context.Entry(comment).Reference(c => c.User).LoadAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm bình luận thành công.",
                    comment = new
                    {
                        username = comment.User?.Username ?? "User",
                        avatar = COMICZONE.Extensions.StringExtensions.AvatarOrDefault(comment.User?.Avatar),
                        content = comment.Content,
                        createdAt = comment.Createdat.ToString("dd/MM/yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
    }
}
