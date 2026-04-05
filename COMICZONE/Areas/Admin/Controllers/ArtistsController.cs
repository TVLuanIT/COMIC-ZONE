using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Extensions;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ArtistsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public ArtistsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Artists
        public async Task<IActionResult> Index(string? keyword, string? sortColumn = "Name", bool isAscending = true, int page = 1)
        {
            var query = _context.Artists
                .Include(a => a.Products)
                .AsQueryable();

            // 1. Search (Name)
            query = query.ApplySearch(keyword, "Name");
            var totalItems = await query.CountAsync();

            // 2. Sort
            query = query.ApplySort(sortColumn, isAscending);

            // 3. Paging
            int pageSize = 10;
            query = query.ApplyPagination(page, pageSize);

            var searchModel = new AdminSearchModel 
            { 
                Keyword = keyword, 
                SortColumn = sortColumn, 
                IsAscending = isAscending, 
                PageNumber = page, 
                PageSize = pageSize, 
                TotalItems = totalItems 
            };
            ViewBag.SearchModel = searchModel;

            var artists = await query.ToListAsync();

            return View(artists);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .Include(a => a.Products)
                    .ThenInclude(p => p.Pictures)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        // GET: Admin/Artists/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Artists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Artist artist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        // GET: Admin/Artists/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }
            return View(artist);
        }

        // POST: Admin/Artists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] Artist artist)
        {
            if (id != artist.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistExists(artist.Id))
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
            return View(artist);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null)
            {
                return NotFound();
            }

            artist.Isdeleted = !artist.Isdeleted;
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = artist.Isdeleted });
        }

        // GET: Admin/Artists/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var artist = await _context.Artists
                .FirstOrDefaultAsync(m => m.Id == id);
            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        // POST: Admin/Artists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artists.FindAsync(id);

            if (artist == null)
            {
                return NotFound();
            }

            try
            {
                _context.Artists.Remove(artist);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                ModelState.AddModelError("", "Không thể xóa họa sĩ vì đang có dữ liệu liên quan.");

                return View(artist);
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Đã xảy ra lỗi khi xóa họa sĩ.");
                return View(artist);
            }
        }

        private bool ArtistExists(int id)
        {
            return _context.Artists.Any(e => e.Id == id);
        }
    }
}
