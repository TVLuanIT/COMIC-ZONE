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
using COMICZONE.Areas.Admin.ViewModels;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UsersController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public UsersController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(UserSearchModel search)
        {
            ViewBag.CurrentUserId = HttpContext.Session.GetString("UserId");

            var query = _context.Users
                .Include(u => u.Customer)
                .AsQueryable();

            // 1. Search & Filter
            query = query.ApplyUserFilters(search);

            var totalCount = await query.CountAsync();

            // 2. Sort
            query = query.ApplySort(search.SortColumn ?? "Id", search.IsAscending);

            // 3. Paging
            int pageSize = search.PageSize > 0 ? search.PageSize : 10;
            int pageNumber = search.Page > 0 ? search.Page : 1;
            query = query.ApplyPagination(pageNumber, pageSize);

            // Update search model for the view
            search.TotalCount = totalCount;
            search.Page = pageNumber;
            search.PageSize = pageSize;

            ViewBag.SearchModel = search;

            var users = await query.ToListAsync();

            return View(users);
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Username,Email,Role,Isactive,Avatar")]
            User user,
            string Password)
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                ModelState.AddModelError("Password", "Password không được để trống");
            }
            else if (Password.Length < 6)
            {
                ModelState.AddModelError("Password", "Password tối thiểu 6 ký tự");
            }

            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ModelState.AddModelError("Username", "Username đã tồn tại");
            }

            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ModelState.AddModelError("Email", "Email đã tồn tại");
            }

            ModelState.Remove(nameof(Models.User.Passwordhash));

            if (ModelState.IsValid)
            {
                user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(Password);

                user.Createdat = DateTime.UtcNow;

                user.ResetToken = null;
                user.ResetTokenExpire = null;

                _context.Users.Add(user);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            string Username,
            string? Email,
            string Role,
            bool Isactive,
            string? Avatar,
            string? NewPassword)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
                return NotFound();

            // Lưu lại giá trị cũ để kiểm tra thay đổi
            var oldUsername = existingUser.Username;
            var oldEmail = existingUser.Email;
            var oldRole = existingUser.Role;
            var oldIsactive = existingUser.Isactive;

            var changes = new List<string>();
            if (oldUsername != Username) changes.Add($"Tên người dùng: {oldUsername} ➔ {Username}");
            if (oldEmail != Email) changes.Add($"Email: {oldEmail} ➔ {Email}");
            if (oldRole != Role) changes.Add($"Vai trò: {oldRole} ➔ {Role}");
            if (oldIsactive != Isactive) changes.Add($"Trạng thái: {(oldIsactive ? "Hoạt động" : "Ngưng hoạt động")} ➔ {(Isactive ? "Hoạt động" : "Ngưng hoạt động")}");
            if (!string.IsNullOrEmpty(NewPassword)) changes.Add("Mật khẩu đã được cập nhật bởi Admin.");

            existingUser.Username = Username;
            existingUser.Email = Email;
            existingUser.Role = Role;
            existingUser.Avatar = Avatar;

            // Sync Isactive and Isdeleted
            if (existingUser.Isactive != Isactive)
            {
                existingUser.Isactive = Isactive;
                existingUser.Isdeleted = !Isactive; // If active = true, deleted = false. If active = false, deleted = true.
            }

            if (!string.IsNullOrEmpty(NewPassword))
            {
                existingUser.Passwordhash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }

            // Gửi thông báo nếu có thay đổi và không phải trường hợp đang bị vô hiệu hóa (xóa mềm)
            bool statusChanged = oldIsactive != Isactive;
            bool remainsHidden = !oldIsactive && !Isactive;

            if (changes.Any() && !remainsHidden)
            {
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                _context.Notifications.Add(new Notification
                {
                    UserId = existingUser.Id,
                    Title = statusChanged ? (Isactive ? "Tài khoản đã được khôi phục" : "Tài khoản bị vô hiệu hóa") : "Cập nhật tài khoản",
                    Message = statusChanged
                        ? (Isactive ? "Tài khoản của bạn đã được Quản trị viên khôi phục thành công." : "Tài khoản của bạn đã bị vô hiệu hóa bởi Quản trị viên.")
                        : "Thông tin tài khoản của bạn đã được Admin cập nhật:\n- " + string.Join("\n- ", changes),
                    CreatedBy = adminId,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Link = "/Account/UserProfiles/Notifications"
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var user = await _context.Users
                .Include(u => u.NotificationUsers)
                .Include(u => u.NotificationCreatedByNavigations)
                .Include(u => u.Orders)
                .Include(u => u.Carts)
                .Include(u => u.Blogs)
                .Include(u => u.BlogComments)
                .Include(u => u.ProductReviews)
                .Include(u => u.ViolationReports)
                .Include(u => u.ProductReviewReplyUsers)
                .Include(u => u.ProductReviewReplyReplytousers)
                .Include(u => u.ProductReviewLikes)
                .Include(u => u.ProductReviewReplyLikes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // Tạo danh sách các bảng còn liên quan với chi tiết
            var relatedData = new Dictionary<string, IEnumerable<string>>();

            if (user.NotificationUsers.Any())
                relatedData.Add("Notifications nhận", user.NotificationUsers.Select(n =>
                    $"Notification ID: {n.NotificationId}, Title: {n.Title}, Ngày: {n.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-"}"));

            if (user.NotificationCreatedByNavigations.Any())
                relatedData.Add("Notifications tạo bởi", user.NotificationCreatedByNavigations.Select(n =>
                    $"Notification ID: {n.NotificationId}, Title: {n.Title}, Ngày: {n.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-"}"));

            if (user.Orders.Any())
            {
                relatedData.Add("Orders", user.Orders.Select(o =>
                    $"Order ID: {o.OrderId}, Trạng thái: {o.Status ?? "Chưa cập nhật"}, " +
                    $"Tổng tiền: {o.TotalAmount?.ToString("C") ?? "0"}, Ngày tạo: {o.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-"}"));
            }

            if (user.Carts.Any())
            {
                relatedData.Add("Carts", user.Carts
                    .Select(c => $"Cart ID: {c.CartId}, Ngày tạo: {c.CreatedAt?.ToString("dd/MM/yyyy HH:mm")}, Số item: {c.CartItems.Count}"));
            }

            if (user.Blogs.Any())
                relatedData.Add("Blogs", user.Blogs.Select(b => $"Blog ID: {b.Id}, Title: {b.Title}, Ngày tạo: {b.Createdat.ToString("dd/MM/yyyy HH:mm")}"));

            if (user.BlogComments.Any())
                relatedData.Add("Blog Comments", user.BlogComments.Select(c => $"Comment ID: {c.Id}, Nội dung: {(c.Content.Length > 50 ? c.Content.Substring(0, 50) + "..." : c.Content)}"));

            if (user.ProductReviews.Any())
                relatedData.Add("Product Reviews", user.ProductReviews.Select(r => $"Review ID: {r.Reviewid}, Ngôi sao: {r.Rating}, Nội dung: {(r.Reviewcontent.Length > 50 ? r.Reviewcontent.Substring(0, 50) + "..." : r.Reviewcontent)}"));

            if (user.ViolationReports.Any())
                relatedData.Add("Violation Reports", user.ViolationReports.Select(v => $"Report ID: {v.Id}, Lý do: {v.Reason}, Loại: {v.Reporttype}"));

            if (user.ProductReviewReplyUsers.Any())
                relatedData.Add("Replies đã gửi", user.ProductReviewReplyUsers.Select(r => $"Reply ID: {r.Replyid}, Nội dung: {(r.Replycontent.Length > 50 ? r.Replycontent.Substring(0, 50) + "..." : r.Replycontent)}"));

            if (user.ProductReviewReplyReplytousers.Any())
                relatedData.Add("Replies đã nhận", user.ProductReviewReplyReplytousers.Select(r => $"Reply ID: {r.Replyid}, Từ: {r.User?.Username ?? "Ẩn danh"}"));

            if (user.ProductReviewLikes.Any())
                relatedData.Add("Review Likes", user.ProductReviewLikes.Select(l => $"Review ID: {l.Reviewid}"));

            if (user.ProductReviewReplyLikes.Any())
                relatedData.Add("Reply Likes", user.ProductReviewReplyLikes.Select(l => $"Reply ID: {l.Replyid}"));

            ViewBag.RelatedData = relatedData;

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var currentUserIdStr = HttpContext.Session.GetString("UserId");
            if (currentUserIdStr == id.ToString())
            {
                TempData["Error"] = "Bạn không thể tự xóa tài khoản của chính mình!";
                return RedirectToAction("Delete", new { id = id });
            }

            var user = await _context.Users
                .Include(u => u.NotificationUsers)
                .Include(u => u.NotificationCreatedByNavigations)
                .Include(u => u.Orders)
                .Include(u => u.Carts)
                .Include(u => u.Blogs)
                .Include(u => u.BlogComments)
                .Include(u => u.ProductReviews)
                .Include(u => u.ViolationReports)
                .Include(u => u.ProductReviewReplyUsers)
                .Include(u => u.ProductReviewReplyReplytousers)
                .Include(u => u.ProductReviewLikes)
                .Include(u => u.ProductReviewReplyLikes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Thêm thông báo trước khi xóa (chỉ thông báo nếu bản ghi chưa bị xóa mềm)
            if (!user.Isdeleted)
            {
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                _context.Notifications.Add(new Notification
                {
                    UserId = user.Id,
                    Title = "Xóa tài khoản vĩnh viễn",
                    Message = "Tài khoản của bạn đã bị Admin xóa vĩnh viễn khỏi hệ thống bởi các hành vi vi phạm nghiêm trọng.",
                    CreatedBy = adminId,
                    CreatedAt = DateTime.Now,
                    IsRead = false
                });
            }

            // Kiểm tra nếu còn dữ liệu liên quan thì không xóa
            if (user.NotificationUsers.Any() || user.NotificationCreatedByNavigations.Any()
                || user.Orders.Any() || user.Carts.Any() || user.Blogs.Any() || user.BlogComments.Any()
                || user.ProductReviews.Any() || user.ViolationReports.Any() || user.ProductReviewReplyUsers.Any()
                || user.ProductReviewReplyReplytousers.Any() || user.ProductReviewLikes.Any() || user.ProductReviewReplyLikes.Any())
            {
                TempData["Error"] = "Người dùng này vẫn còn dữ liệu liên quan, không thể xóa!";
                return RedirectToAction("Delete", new { id = id });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var currentUserIdStr = HttpContext.Session.GetString("UserId");
            if (currentUserIdStr == id.ToString())
            {
                return Json(new { success = false, message = "Bạn không thể tự xóa mềm tài khoản của chính mình!" });
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Isdeleted = !user.Isdeleted;
            
            // Sync Isactive
            if (user.Isdeleted)
            {
                user.Isactive = false;
            }
            // else: If restored, we keep Isactive = false (do nothing).

            // Thêm thông báo
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = user.Id,
                Title = user.Isdeleted ? "Tài khoản bị vô hiệu hóa" : "Tài khoản đã được khôi phục",
                Message = user.Isdeleted 
                    ? "Tài khoản của bạn đã bị vô hiệu hóa (Xóa mềm) bởi Quản trị viên." 
                    : "Tài khoản của bạn đã được Quản trị viên khôi phục thành công.",
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = "/Account/UserProfiles/Notifications"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = user.Isdeleted });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForceDelete(int id, bool confirmRisk)
        {
            var currentUserIdStr = HttpContext.Session.GetString("UserId");
            if (currentUserIdStr == id.ToString())
            {
                TempData["Error"] = "Bạn không thể tự xóa cưỡng bức tài khoản của chính mình!";
                return RedirectToAction("Delete", new { id = id });
            }

            if (!confirmRisk)
            {
                TempData["Error"] = "Bạn phải xác nhận rủi ro trước khi thực hiện xóa cưỡng bức!";
                return RedirectToAction("Delete", new { id = id });
            }

            var user = await _context.Users
                .Include(u => u.Customer)
                .Include(u => u.BlogComments)
                .Include(u => u.Blogs).ThenInclude(b => b.BlogComments)
                .Include(u => u.Carts).ThenInclude(c => c.CartItems)
                .Include(u => u.NotificationCreatedByNavigations)
                .Include(u => u.NotificationUsers)
                .Include(u => u.OrderStatusHistories)
                .Include(u => u.Orders).ThenInclude(o => o.OrderItems)
                .Include(u => u.Orders).ThenInclude(o => o.Payments).ThenInclude(p => p.Refunds)
                .Include(u => u.Orders).ThenInclude(o => o.Invoices)
                .Include(u => u.Orders).ThenInclude(o => o.OrderStatusHistories)
                .Include(u => u.ProductReviewLikes)
                .Include(u => u.ProductReviewReplyLikes)
                .Include(u => u.ProductReviewReplyReplytousers)
                .Include(u => u.ProductReviewReplyUsers)
                .Include(u => u.ProductReviews)
                .Include(u => u.UserProductViews)
                .Include(u => u.ViolationReports)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Xóa Account/Profile
                if (user.Customer != null) _context.Customers.Remove(user.Customer);

                // 2. Xóa Interaction (Review, Like, Comment)
                _context.ProductReviewLikes.RemoveRange(user.ProductReviewLikes);
                _context.ProductReviewReplyLikes.RemoveRange(user.ProductReviewReplyLikes);
                _context.ProductReviewReplies.RemoveRange(user.ProductReviewReplyUsers);
                _context.ProductReviewReplies.RemoveRange(user.ProductReviewReplyReplytousers);
                _context.ProductReviews.RemoveRange(user.ProductReviews);
                _context.BlogComments.RemoveRange(user.BlogComments);

                // 3. Xóa Commercial (Carts, Orders)
                foreach(var cart in user.Carts) _context.CartItems.RemoveRange(cart.CartItems);
                _context.Carts.RemoveRange(user.Carts);

                foreach (var order in user.Orders)
                {
                    _context.OrderItems.RemoveRange(order.OrderItems);
                    _context.Invoices.RemoveRange(order.Invoices);
                    _context.OrderStatusHistories.RemoveRange(order.OrderStatusHistories);
                    foreach (var payment in order.Payments)
                    {
                        _context.Refunds.RemoveRange(payment.Refunds);
                        _context.Payments.Remove(payment);
                    }
                    _context.Orders.Remove(order);
                }

                // 4. Xóa Content (Blogs authored by user)
                foreach(var blog in user.Blogs) _context.BlogComments.RemoveRange(blog.BlogComments);
                _context.Blogs.RemoveRange(user.Blogs);

                // 5. Xóa System (Notifications, Reports...)
                _context.Notifications.RemoveRange(user.NotificationCreatedByNavigations);
                _context.Notifications.RemoveRange(user.NotificationUsers);
                _context.ViolationReports.RemoveRange(user.ViolationReports);
                _context.UserProductViews.RemoveRange(user.UserProductViews);
                _context.OrderStatusHistories.RemoveRange(user.OrderStatusHistories);

                // 6. Xóa User Chính
                _context.Users.Remove(user);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = $"Đã xóa vĩnh viễn người dùng {user.Username} và toàn bộ dữ liệu liên quan thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = $"Lỗi khi xóa cưỡng bức: {ex.Message}";
                return RedirectToAction("Delete", new { id = id });
            }
        }
    }
}
