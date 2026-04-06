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
    public class InventoryLogsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public InventoryLogsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/InventoryLogs
        public async Task<IActionResult> Index(InventoryLogSearchModel search)
        {
            var query = _context.InventoryLogs
                .Include(i => i.Product)
                .AsQueryable();

            // 1. Search & Filter
            query = query.ApplyInventoryLogFilters(search);

            var totalItems = await query.CountAsync();

            // 2. Sort
            query = query.ApplySort(search.SortColumn ?? "CreatedAt", search.IsAscending);

            // 3. Paging
            int pageSize = search.PageSize > 0 ? search.PageSize : 15;
            int pageNumber = search.Page > 0 ? search.Page : 1;
            query = query.ApplyPagination(pageNumber, pageSize);

            // Update search model for the view
            search.TotalCount = totalItems;
            search.Page = pageNumber;
            search.PageSize = pageSize;

            ViewBag.SearchModel = search;

            var logs = await query.ToListAsync();

            return View(logs);
        }

        // GET: Admin/InventoryLogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryLog = await _context.InventoryLogs
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryLog == null)
            {
                return NotFound();
            }

            return View(inventoryLog);
        }

        // GET: Admin/InventoryLogs/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name");
            return View();
        }

        // POST: Admin/InventoryLogs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProductId,ChangeAmount,Type,CreatedAt")] InventoryLog inventoryLog)
        {
            if (inventoryLog.Type == "Import" && inventoryLog.ChangeAmount <= 0)
            {
                ModelState.AddModelError("ChangeAmount", "Nhập kho phải có số lượng lớn hơn 0 (+)");
            }
            if (inventoryLog.Type == "Export" && inventoryLog.ChangeAmount >= 0)
            {
                ModelState.AddModelError("ChangeAmount", "Xuất kho phải có số lượng nhỏ hơn 0 (-)");
            }

            ModelState.Remove("Product");
            if (ModelState.IsValid)
            {
                _context.Add(inventoryLog);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm log kho thành công!";
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", inventoryLog.ProductId);
            return View(inventoryLog);
        }

        // GET: Admin/InventoryLogs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryLog = await _context.InventoryLogs.FindAsync(id);
            if (inventoryLog == null)
            {
                return NotFound();
            }
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", inventoryLog.ProductId);
            return View(inventoryLog);
        }

        // POST: Admin/InventoryLogs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProductId,ChangeAmount,Type,CreatedAt")] InventoryLog inventoryLog)
        {
            if (id != inventoryLog.Id)
            {
                return NotFound();
            }

            if (inventoryLog.Type == "Import" && inventoryLog.ChangeAmount <= 0)
            {
                ModelState.AddModelError("ChangeAmount", "Nhập kho phải có số lượng lớn hơn 0 (+)");
            }
            if (inventoryLog.Type == "Export" && inventoryLog.ChangeAmount >= 0)
            {
                ModelState.AddModelError("ChangeAmount", "Xuất kho phải có số lượng nhỏ hơn 0 (-)");
            }

            ModelState.Remove("Product");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventoryLog);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật log kho thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryLogExists(inventoryLog.Id))
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
            ViewData["ProductId"] = new SelectList(_context.Products, "Id", "Name", inventoryLog.ProductId);
            return View(inventoryLog);
        }

        // GET: Admin/InventoryLogs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventoryLog = await _context.InventoryLogs
                .Include(i => i.Product)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inventoryLog == null)
            {
                return NotFound();
            }

            return View(inventoryLog);
        }

        // POST: Admin/InventoryLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventoryLog = await _context.InventoryLogs.FindAsync(id);
            if (inventoryLog != null)
            {
                _context.InventoryLogs.Remove(inventoryLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InventoryLogExists(int id)
        {
            return _context.InventoryLogs.Any(e => e.Id == id);
        }
    }
}
