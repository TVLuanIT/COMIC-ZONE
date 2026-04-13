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

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogCategoriesController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public BlogCategoriesController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/BlogCategories
        public async Task<IActionResult> Index(BlogCategorySearchModel search)
        {
            var query = _context.BlogCategories
                .Include(c => c.Blogs)
                .AsQueryable();

            // 1. Filter
            query = query.ApplyBlogCategoryFilters(search);

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

            return View(await query.ToListAsync());
        }

        // GET: Admin/BlogCategories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogCategory = await _context.BlogCategories
                .Include(c => c.Blogs)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogCategory == null)
            {
                return NotFound();
            }

            return View(blogCategory);
        }

        // GET: Admin/BlogCategories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/BlogCategories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Slug")] BlogCategory blogCategory)
        {
            ModelState.Remove("Blogs");

            if (ModelState.IsValid)
            {
                _context.Add(blogCategory);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(blogCategory);
        }

        // GET: Admin/BlogCategories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogCategory = await _context.BlogCategories
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogCategory == null)
            {
                return NotFound();
            }
            return View(blogCategory);
        }

        // POST: Admin/BlogCategories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Slug,Isdeleted")] BlogCategory blogCategory)
        {
            if (id != blogCategory.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Blogs");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(blogCategory);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BlogCategoryExists(blogCategory.Id))
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
            return View(blogCategory);
        }

        // GET: Admin/BlogCategories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blogCategory = await _context.BlogCategories
                .Include(c => c.Blogs)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (blogCategory == null)
            {
                return NotFound();
            }

            return View(blogCategory);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var blogCategory = await _context.BlogCategories.FindAsync(id);
            if (blogCategory == null)
            {
                return NotFound();
            }

            blogCategory.Isdeleted = !blogCategory.Isdeleted;
            
            // Cascading to blogs (Option B)
            var blogs = await _context.Blogs
                .Include(b => b.Categories)
                .Where(b => b.Categories.Any(c => c.Id == id))
                .ToListAsync();

            foreach (var blog in blogs)
            {
                if (blogCategory.Isdeleted)
                {
                    // If category is hidden, hide blog only if ALL its categories are hidden
                    if (blog.Categories.All(c => c.Isdeleted || c.Id == id))
                    {
                        blog.Isdeleted = true;
                    }
                }
                else
                {
                    // If category is restored, always restore the blog
                    blog.Isdeleted = false;
                }
                _context.Update(blog);
            }

            _context.Update(blogCategory);
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = blogCategory.Isdeleted });
        }

        // POST: Admin/BlogCategories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var blogCategory = await _context.BlogCategories
                .Include(c => c.Blogs)
                    .ThenInclude(b => b.Categories)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (blogCategory != null)
            {
                // Soft delete associated blogs if they only belong to this category or other hidden categories
                foreach (var blog in blogCategory.Blogs)
                {
                    if (blog.Categories.All(c => c.Isdeleted || c.Id == id))
                    {
                        blog.Isdeleted = true;
                        // EF will track this change automatically, no need for _context.Update(blog)
                    }
                }

                _context.BlogCategories.Remove(blogCategory);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool BlogCategoryExists(int id)
        {
            return _context.BlogCategories.Any(e => e.Id == id);
        }
    }
}
