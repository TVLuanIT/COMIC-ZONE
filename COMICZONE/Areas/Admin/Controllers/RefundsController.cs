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
    public class RefundsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public RefundsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Refunds
        public async Task<IActionResult> Index(RefundSearchModel search)
        {
            var query = _context.Refunds
                .Include(r => r.Payment)
                    .ThenInclude(p => p.Order)
                        .ThenInclude(o => o.User)
                .AsQueryable();

            // 1. Search & Filter
            query = query.ApplyRefundFilters(search);

            var totalCount = await query.CountAsync();

            // 2. Sort
            query = query.ApplySort(search.SortColumn ?? "CreatedAt", search.IsAscending);

            // 3. Paging
            int pageSize = search.PageSize > 0 ? search.PageSize : 12;
            int pageNumber = search.Page > 0 ? search.Page : 1;
            query = query.ApplyPagination(pageNumber, pageSize);

            // Update search model for the view
            search.TotalCount = totalCount;
            search.Page = pageNumber;
            search.PageSize = pageSize;

            ViewBag.SearchModel = search;

            var refunds = await query.ToListAsync();

            return View(refunds);
        }

        // GET: Admin/Refunds/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var refund = await _context.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (refund == null)
            {
                return NotFound();
            }

            return View(refund);
        }

        // GET: Admin/Refunds/Create
        public IActionResult Create()
        {
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Paymentid", "Paymentid");
            return View();
        }

        // POST: Admin/Refunds/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PaymentId,Amount,Status,Reason,CreatedAt")] Refund refund)
        {
            if (ModelState.IsValid)
            {
                _context.Add(refund);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Paymentid", "Paymentid", refund.PaymentId);
            return View(refund);
        }

        // GET: Admin/Refunds/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var refund = await _context.Refunds.FindAsync(id);
            if (refund == null)
            {
                return NotFound();
            }
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Paymentid", "Paymentid", refund.PaymentId);
            return View(refund);
        }

        // POST: Admin/Refunds/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PaymentId,Amount,Status,Reason,CreatedAt")] Refund refund)
        {
            if (id != refund.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(refund);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RefundExists(refund.Id))
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
            ViewData["PaymentId"] = new SelectList(_context.Payments, "Paymentid", "Paymentid", refund.PaymentId);
            return View(refund);
        }

        // GET: Admin/Refunds/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var refund = await _context.Refunds
                .Include(r => r.Payment)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (refund == null)
            {
                return NotFound();
            }

            return View(refund);
        }

        // POST: Admin/Refunds/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var refund = await _context.Refunds.FindAsync(id);
            if (refund != null)
            {
                _context.Refunds.Remove(refund);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RefundExists(int id)
        {
            return _context.Refunds.Any(e => e.Id == id);
        }
    }
}
