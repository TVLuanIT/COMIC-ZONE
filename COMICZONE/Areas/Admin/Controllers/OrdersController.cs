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
using COMICZONE.Models.Enums;
using COMICZONE.Services;
using COMICZONE.Helpers;
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrdersController : AdminBaseController
    {
        private readonly ComiczoneContext _context;
        private readonly IInvoiceService _invoiceService;

        public OrdersController(ComiczoneContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        // GET: Admin/Orders
        public async Task<IActionResult> Index(string? keyword, string? statusFilter, string? sortColumn = "CreatedAt", bool isAscending = false, int page = 1)
        {
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.Invoices)
                .AsQueryable();

            // 1. Filter by Status
            if (!string.IsNullOrEmpty(statusFilter))
            {
                query = query.Where(o => o.Status == statusFilter);
            }

            // 2. Search (OrderId, PhoneNumber, Username, ShippingAddress)
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(o => o.OrderId.ToString().Contains(keyword) || 
                                         o.PhoneNumber.Contains(keyword) || 
                                         o.User.Username.Contains(keyword) || 
                                         o.ShippingAddress.Contains(keyword));
            }
            
            var totalItems = await query.CountAsync();

            // 3. Sort
            query = query.ApplySort(sortColumn, isAscending);

            // 4. Paging
            int pageSize = 10;
            query = query.ApplyPagination(page, pageSize);

            var searchModel = new AdminSearchModel 
            { 
                Keyword = keyword, 
                StatusFilter = statusFilter,
                SortColumn = sortColumn, 
                IsAscending = isAscending, 
                PageNumber = page, 
                PageSize = pageSize, 
                TotalItems = totalItems 
            };
            ViewBag.SearchModel = searchModel;

            var orders = await query.ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.Pictures)
                .Include(o => o.Invoices)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // GET: Admin/Orders/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", order.UserId);
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Order order)
        {
            if (id != order.OrderId)
                return NotFound();

            ModelState.Remove("User");

            var existingOrder = await _context.Orders
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (existingOrder == null)
                return NotFound();

            if (order.OrderStatusEnum == OrderStatus.Completed && 
                !existingOrder.Payments.Any(p => p.Paymentstatus == "SUCCESS"))
            {
                ModelState.AddModelError("OrderStatusEnum", "Không thể hoàn thành đơn hàng chưa được thanh toán thành công.");
            }

            if (ModelState.IsValid)
            {
                bool isStatusChanged = existingOrder.OrderStatusEnum != order.OrderStatusEnum;
                var oldStatus = existingOrder.OrderStatusEnum;

                existingOrder.OrderStatusEnum = order.OrderStatusEnum;
                existingOrder.PhoneNumber = order.PhoneNumber;
                existingOrder.ShippingAddress = order.ShippingAddress;
                existingOrder.Note = order.Note;

                // Thêm thông báo (chỉ thông báo nếu bản ghi chưa bị xóa mềm)
                if (!existingOrder.Isdeleted)
                {
                    var adminIdStr = HttpContext.Session.GetString("UserId");
                    int? adminId = null;
                    if (int.TryParse(adminIdStr, out int parsedId))
                    {
                        adminId = parsedId;
                    }

                    string notifMsg = $"Đơn hàng #{existingOrder.OrderId} của bạn đã được Admin cập nhật.";
                    if (isStatusChanged)
                    {
                        notifMsg = $"Trạng thái đơn hàng #{existingOrder.OrderId} đã thay đổi: {oldStatus.GetDisplayName()} ➔ {order.OrderStatusEnum.GetDisplayName()}.";
                        
                        // Tự động tạo hóa đơn nếu chuyển trạng thái sang Processing hoặc Completed cho các đơn chưa có hóa đơn
                        if (order.OrderStatusEnum == OrderStatus.Completed || order.OrderStatusEnum == OrderStatus.Processing)
                        {
                            await _invoiceService.CreateInvoiceAsync(id);
                        }
                    }

                    _context.Notifications.Add(new Notification
                    {
                        UserId = existingOrder.UserId,
                        Title = "Cập nhật đơn hàng",
                        Message = notifMsg,
                        CreatedBy = adminId,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        Link = $"/UserProfiles/MyOrders"
                    });
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật đơn hàng thành công!";

                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        // GET: Admin/Orders/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Pictures)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentTransactions)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.Refunds)
                .Include(o => o.Invoices)
                .Include(o => o.OrderStatusHistories)
                .FirstOrDefaultAsync(m => m.OrderId == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .Include(o => o.OrderStatusHistories)
                .Include(o => o.Invoices)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.PaymentTransactions)
                .Include(o => o.Payments)
                    .ThenInclude(p => p.Refunds)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order != null)
            {
                // Thêm thông báo (chỉ thông báo nếu bản ghi chưa bị xóa mềm)
                if (!order.Isdeleted)
                {
                    var adminIdStr = HttpContext.Session.GetString("UserId");
                    int? adminId = null;
                    if (int.TryParse(adminIdStr, out int parsedId))
                    {
                        adminId = parsedId;
                    }

                    _context.Notifications.Add(new Notification
                    {
                        UserId = order.UserId,
                        Title = "Đơn hàng bị hủy",
                        Message = $"Đơn hàng #{order.OrderId} của bạn đã bị hủy/xóa bởi hệ thống.",
                        CreatedBy = adminId,
                        CreatedAt = DateTime.Now,
                        IsRead = false,
                        Link = $"/UserProfiles/MyOrders"
                    });
                }

                // 1. Delete associated Payments and their transactions/refunds
                if (order.Payments != null && order.Payments.Any())
                {
                    foreach (var payment in order.Payments)
                    {
                        if (payment.PaymentTransactions != null && payment.PaymentTransactions.Any())
                            _context.PaymentTransactions.RemoveRange(payment.PaymentTransactions);

                        if (payment.Refunds != null && payment.Refunds.Any())
                            _context.Refunds.RemoveRange(payment.Refunds);
                    }
                    _context.Payments.RemoveRange(order.Payments);
                }

                // 2. Delete OrderStatusHistories
                if (order.OrderStatusHistories != null && order.OrderStatusHistories.Any())
                    _context.OrderStatusHistories.RemoveRange(order.OrderStatusHistories);

                // 3. Delete Invoices
                if (order.Invoices != null && order.Invoices.Any())
                    _context.Invoices.RemoveRange(order.Invoices);

                // 4. Delete OrderItems
                if (order.OrderItems != null && order.OrderItems.Any())
                    _context.OrderItems.RemoveRange(order.OrderItems);

                // 5. Delete Order
                _context.Orders.Remove(order);

                await _context.SaveChangesAsync();
            }
 
            return RedirectToAction(nameof(Index));
        }
 
        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Isdeleted = !order.Isdeleted;

            // Thêm thông báo
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = order.UserId,
                Title = order.Isdeleted ? "Đơn hàng bị ẩn" : "Đơn hàng được hiển thị",
                Message = $"Đơn hàng #{order.OrderId} của bạn đã bị " +
                          (order.Isdeleted ? "Admin ẩn tạm thời khỏi lịch sử." : "Admin cho phép hiển thị lại thành công."),
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = "/UserProfiles/MyOrders"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = order.Isdeleted });
        }
    }
}
