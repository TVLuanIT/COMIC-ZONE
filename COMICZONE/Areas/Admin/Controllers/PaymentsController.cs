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
    public class PaymentsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public PaymentsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Payments
        public async Task<IActionResult> Index(string? keyword, string? statusFilter, string? sortColumn, bool isAscending = false, int page = 1)
        {
            var query = _context.Payments
                .Include(p => p.Order)
                .AsQueryable();

            // 1. Filter by Status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(p => p.Paymentstatus == statusFilter);
            }

            // 2. Search (Paymentid, Orderid, Transactionid)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Paymentid.ToString().Contains(keyword) || 
                                         p.Orderid.ToString().Contains(keyword) || 
                                         p.Transactionid.Contains(keyword));
            }

            // 3. Total count
            var totalCount = await query.CountAsync();

            // 4. Sort
            if (string.IsNullOrEmpty(sortColumn)) sortColumn = "Createdat";
            query = query.ApplySort(sortColumn, isAscending);

            // 5. Paging
            const int pageSize = 12;
            query = query.ApplyPagination(page, pageSize);

            var searchModel = new AdminSearchModel 
            { 
                Keyword = keyword, 
                StatusFilter = statusFilter,
                SortColumn = sortColumn, 
                IsAscending = isAscending, 
                PageNumber = page, 
                PageSize = pageSize, 
                TotalItems = totalCount 
            };
            ViewBag.SearchModel = searchModel;

            return View(await query.ToListAsync());
        }

        // GET: Admin/Payments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Paymentid == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }


        // GET: Admin/Payments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                return NotFound();
            }
            ViewData["Orderid"] = new SelectList(_context.Orders, "OrderId", "OrderId", payment.Orderid);
            return View(payment);
        }

        // POST: Admin/Payments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Paymentid,Orderid,Amount,Paymentstatus,Transactionid,Createdat,Paidat,Paymentmethod")] Payment payment)
        {
            if (id != payment.Paymentid)
            {
                return NotFound();
            }

            ModelState.Remove("Order");
            if (ModelState.IsValid)
            {
                try
                {
                    // Lấy dữ liệu cũ để so sánh và lấy thông tin User
                    var existingPayment = await _context.Payments
                        .Include(p => p.Order)
                        .FirstOrDefaultAsync(p => p.Paymentid == payment.Paymentid);

                    if (existingPayment == null) return NotFound();

                    var oldStatus = existingPayment.Paymentstatus;
                    var newStatus = payment.Paymentstatus;

                    // Cập nhật các trường
                    existingPayment.Orderid = payment.Orderid;
                    existingPayment.Amount = payment.Amount;
                    existingPayment.Paymentstatus = payment.Paymentstatus;
                    existingPayment.Transactionid = payment.Transactionid;
                    existingPayment.Createdat = payment.Createdat;
                    existingPayment.Paidat = payment.Paidat;
                    existingPayment.Paymentmethod = payment.Paymentmethod;

                    _context.Update(existingPayment);
                    
                    // Nếu trạng thái thay đổi, gửi thông báo cho khách hàng
                    if (oldStatus != newStatus)
                    {
                        var adminIdStr = HttpContext.Session.GetString("UserId");
                        int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                        _context.Notifications.Add(new Notification
                        {
                            UserId = existingPayment.Order.UserId,
                            Title = "Cập nhật trạng thái thanh toán",
                            Message = $"Thanh toán cho đơn hàng #{existingPayment.Orderid} đã thay đổi từ '{oldStatus}' sang '{newStatus}'.",
                            Link = $"/UserProfiles/OrderDetails/{existingPayment.Orderid}",
                            CreatedBy = adminId,
                            CreatedAt = DateTime.Now,
                            IsRead = false
                        });
                    }

                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin thanh toán thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PaymentExists(payment.Paymentid))
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
            ViewData["Orderid"] = new SelectList(_context.Orders, "OrderId", "OrderId", payment.Orderid);
            return View(payment);
        }

        // GET: Admin/Payments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var payment = await _context.Payments
                .Include(p => p.Order)
                .FirstOrDefaultAsync(m => m.Paymentid == id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // POST: Admin/Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentExists(int id)
        {
            return _context.Payments.Any(e => e.Paymentid == id);
        }
    }
}
