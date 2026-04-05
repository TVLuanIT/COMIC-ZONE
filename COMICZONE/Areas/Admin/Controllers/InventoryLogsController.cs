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
        public async Task<IActionResult> Index(string? keyword, string? typeFilter, string? sortColumn = "CreatedAt", bool isAscending = false, int page = 1)
        {
            var query = _context.InventoryLogs
                .Include(i => i.Product)
                .AsQueryable();

            // 1. Filter by Type
            if (!string.IsNullOrEmpty(typeFilter))
            {
                query = query.Where(i => i.Type == typeFilter);
            }

            // 3. Search (Product Name, Type, Id, ProductId)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(i => i.Product.Name.Contains(keyword) || 
                                         i.Type.Contains(keyword) ||
                                         i.Id.ToString().Contains(keyword) ||
                                         i.ProductId.ToString().Contains(keyword) ||
                                         i.ChangeAmount.ToString().Contains(keyword));
            }

            var totalItems = await query.CountAsync();

            // 4. Sort
            query = query.ApplySort(sortColumn, isAscending);

            // 5. Paging
            int pageSize = 15;
            query = query.ApplyPagination(page, pageSize);

            var searchModel = new AdminSearchModel
            {
                Keyword = keyword,
                TypeFilter = typeFilter,
                SortColumn = sortColumn,
                IsAscending = isAscending,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
            ViewBag.SearchModel = searchModel;

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
