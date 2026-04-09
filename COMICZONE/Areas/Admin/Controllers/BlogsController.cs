using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using COMICZONE.Extensions;
using COMICZONE.Models.Enums;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public BlogsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Blogs
        public async Task<IActionResult> Index(BlogSearchModel search)
        {
            var query = _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .AsQueryable();

            // 1. Filter
            query = query.ApplyBlogFilters(search);

            var totalItems = await query.CountAsync();

            // 2. Sort
            query = query.ApplySort(search.SortColumn ?? "Id", search.IsAscending);

            // 3. Pagination
            int pageSize = search.PageSize > 0 ? search.PageSize : 10;
            int pageNumber = search.Page > 0 ? search.Page : 1;
            query = query.ApplyPagination(pageNumber, pageSize);

            // Update search model
            search.TotalCount = totalItems;
            search.Page = pageNumber;
            search.PageSize = pageSize;

            ViewBag.SearchModel = search;
            ViewBag.Categories = await _context.BlogCategories.Where(c => !c.Isdeleted).ToListAsync();

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        // GET: Admin/Blogs/Create
        public IActionResult Create()
        {
            ViewData["Authorid"] = new SelectList(_context.Users, "Id", "Username");
            ViewBag.Categories = _context.BlogCategories.Where(c => !c.Isdeleted).ToList();
            return View(new Blog { BlogStatusEnum = BlogStatus.Draft });
        }

        // POST: Admin/Blogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Slug,Shortdescription,Content,Status,Authorid,BlogStatusEnum")] Blog blog, int[] selectedCategories, IFormFile? thumbnailFile)
        {
            ModelState.Remove("Author");
            ModelState.Remove("Categories");
            ModelState.Remove("BlogComments");

            if (selectedCategories == null || selectedCategories.Length == 0)
            {
                ModelState.AddModelError("selectedCategories", "Vui lòng chọn ít nhất một danh mục cho bài viết.");
            }

            if (ModelState.IsValid)
            {
                if (thumbnailFile != null && thumbnailFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs", fileName);
                    
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await thumbnailFile.CopyToAsync(stream);
                    }
                    blog.Thumbnail = fileName;
                }

                blog.Createdat = DateTime.Now;
                blog.Updatedat = DateTime.Now;

                // Set default status if not provided
                if (string.IsNullOrEmpty(blog.Status))
                {
                    blog.BlogStatusEnum = BlogStatus.Pending;
                }

                if (selectedCategories != null)
                {
                    foreach (var categoryId in selectedCategories)
                    {
                        var category = await _context.BlogCategories.FindAsync(categoryId);
                        if (category != null) blog.Categories.Add(category);
                    }
                }

                _context.Add(blog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Authorid"] = new SelectList(_context.Users, "Id", "Username", blog.Authorid);
            ViewBag.Categories = _context.BlogCategories.Where(c => !c.Isdeleted).ToList();
            ViewBag.SelectedCategories = selectedCategories?.ToList() ?? new List<int>();
            return View(blog);
        }

        // GET: Admin/Blogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }
            ViewData["Authorid"] = new SelectList(_context.Users, "Id", "Username", blog.Authorid);
            ViewBag.Categories = _context.BlogCategories.Where(c => !c.Isdeleted).ToList();
            ViewBag.SelectedCategories = blog.Categories.Select(c => c.Id).ToList();
            return View(blog);
        }

        // POST: Admin/Blogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Slug,Shortdescription,Content,Status,Authorid,Createdat,Thumbnail,Isdeleted,BlogStatusEnum")] Blog blog, int[] selectedCategories, IFormFile? thumbnailFile)
        {
            if (id != blog.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Author");
            ModelState.Remove("Categories");
            ModelState.Remove("BlogComments");

            if (selectedCategories == null || selectedCategories.Length == 0)
            {
                ModelState.AddModelError("selectedCategories", "Vui lòng chọn ít nhất một danh mục cho bài viết.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (thumbnailFile != null && thumbnailFile.Length > 0)
                    {
                        // Xóa ảnh cũ nếu có
                        if (!string.IsNullOrEmpty(blog.Thumbnail))
                        {
                            var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs", blog.Thumbnail);
                            if (System.IO.File.Exists(oldPath)) System.IO.File.Delete(oldPath);
                        }

                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs", fileName);
                        
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await thumbnailFile.CopyToAsync(stream);
                        }
                        blog.Thumbnail = fileName;
                    }

                    blog.Updatedat = DateTime.Now;

                    // Update Categories
                    var existingBlog = await _context.Blogs
                        .Include(b => b.Categories)
                        .FirstOrDefaultAsync(b => b.Id == id);

                    if (existingBlog == null) return NotFound();

                    // Update scalar properties
                    _context.Entry(existingBlog).CurrentValues.SetValues(blog);
                    existingBlog.Updatedat = DateTime.Now;
                    if (!string.IsNullOrEmpty(blog.Thumbnail)) existingBlog.Thumbnail = blog.Thumbnail;

                    // Update Many-to-Many
                    existingBlog.Categories.Clear();
                    if (selectedCategories != null)
                    {
                        foreach (var categoryId in selectedCategories)
                        {
                            var category = await _context.BlogCategories.FindAsync(categoryId);
                            if (category != null) existingBlog.Categories.Add(category);
                        }
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogExists(blog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Authorid"] = new SelectList(_context.Users, "Id", "Username", blog.Authorid);
            ViewBag.Categories = _context.BlogCategories.Where(c => !c.Isdeleted).ToList();
            ViewBag.SelectedCategories = selectedCategories?.ToList() ?? new List<int>();
            return View(blog);
        }

        // GET: Admin/Blogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blog == null)
            {
                return NotFound();
            }

            return View(blog);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog == null)
            {
                return Json(new { success = false, message = "Không tìm thấy bài viết" });
            }

            blog.Isdeleted = !blog.Isdeleted;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = blog.Isdeleted });
        }

        // POST: Admin/Blogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);
            if (blog != null)
            {
                // Xóa tệp hình ảnh thumbnail nếu có
                if (!string.IsNullOrEmpty(blog.Thumbnail))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs", blog.Thumbnail);
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Blogs.Remove(blog);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BlogExists(int id)
        {
            return _context.Blogs.Any(e => e.Id == id);
        }
    }
}
