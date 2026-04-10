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
    public class BlogsController : COMICZONE.Controllers.BaseController
    {
        private readonly ComiczoneContext _context;

        public BlogsController(ComiczoneContext context)
        {
            _context = context;
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
                return RedirectToAction("Index", "Home");
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
                return RedirectToAction("Index", "Home");
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
                    return RedirectToAction("Index", "Home");
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

        // POST: Blogs/Blogs/AddComment
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
