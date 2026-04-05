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
    public class InvoicesController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public InvoicesController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Invoices
        public async Task<IActionResult> Index(string? keyword, string? sortColumn, bool isAscending = false, int page = 1)
        {
            const int pageSize = 12;

            var query = _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.User)
                .AsQueryable();

            // 1. Search (OrderId, CustomerName, Id)
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(i => i.OrderId.ToString().Contains(keyword) || 
                                         i.CustomerName.Contains(keyword) ||
                                         i.Id.ToString().Contains(keyword));
            }

            // 2. Sort
            if (string.IsNullOrEmpty(sortColumn))
            {
                sortColumn = "IssueDate";
                isAscending = false;
            }
            query = query.ApplySort(sortColumn, isAscending);

            // 3. Total count
            var totalCount = await query.CountAsync();

            // 4. Pagination
            var pagedResults = await query.ApplyPagination(page, pageSize).ToListAsync();

            var searchModel = new AdminSearchModel
            {
                Keyword = keyword,
                SortColumn = sortColumn,
                IsAscending = isAscending,
                PageNumber = page, // Fixed: Using PageNumber property
                PageSize = pageSize,
                TotalItems = totalCount // Fixed: Using TotalItems property
            };

            ViewBag.SearchModel = searchModel;

            return View(pagedResults);
        }

        // GET: Admin/Invoices/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Order)
                    .ThenInclude(o => o.OrderItems)
                        .ThenInclude(oi => oi.Product)
                            .ThenInclude(pr => pr.Pictures)
                .Include(i => i.Order)
                    .ThenInclude(o => o.Payments)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }


        // GET: Admin/Invoices/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", invoice.OrderId);
            return View(invoice);
        }

        // POST: Admin/Invoices/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,OrderId,TotalAmount,IssueDate,CustomerName")] Invoice invoice)
        {
            if (id != invoice.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Order");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật hóa đơn thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InvoiceExists(invoice.Id))
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
            TempData["Error"] = "Cập nhật hóa đơn thất bại. Vui lòng kiểm tra lại!";
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", invoice.OrderId);
            return View(invoice);
        }

        // GET: Admin/Invoices/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var invoice = await _context.Invoices
                .Include(i => i.Order)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (invoice == null)
            {
                return NotFound();
            }

            return View(invoice);
        }

        // POST: Admin/Invoices/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice != null)
            {
                _context.Invoices.Remove(invoice);
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa vĩnh viễn hóa đơn thành công!";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Invoices/ToggleDelete/5
        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return Json(new { success = false, message = "Không tìm thấy hóa đơn." });
            }

            invoice.Isdeleted = !invoice.Isdeleted;
            _context.Update(invoice);
            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = invoice.Isdeleted });
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }
    }
}
