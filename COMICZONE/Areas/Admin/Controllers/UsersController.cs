using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;

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
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
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

            existingUser.Username = Username;
            existingUser.Email = Email;
            existingUser.Role = Role;
            existingUser.Isactive = Isactive;
            existingUser.Avatar = Avatar;

            if (!string.IsNullOrEmpty(NewPassword))
            {
                existingUser.Passwordhash = BCrypt.Net.BCrypt.HashPassword(NewPassword);
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
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            // Tạo danh sách các bảng còn liên quan với chi tiết
            var relatedData = new Dictionary<string, IEnumerable<string>>();

            if (user.NotificationUsers.Any())
                relatedData.Add("Notifications", user.NotificationUsers.Select(n =>
                    $"Notification ID: {n.NotificationId}, Title: {n.Title}, Ngày: {n.CreatedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-"}"));

            if (user.NotificationCreatedByNavigations.Any())
                relatedData.Add("Notifications được tạo bởi", user.NotificationCreatedByNavigations.Select(n =>
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

            ViewBag.RelatedData = relatedData;

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users
                .Include(u => u.NotificationUsers)
                .Include(u => u.NotificationCreatedByNavigations)
                .Include(u => u.Orders)
                .Include(u => u.Carts)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound();

            // Kiểm tra nếu còn dữ liệu liên quan thì không xóa
            if (user.NotificationUsers.Any() || user.NotificationCreatedByNavigations.Any()
                || user.Orders.Any() || user.Carts.Any())
            {
                TempData["Error"] = "Người dùng này vẫn còn dữ liệu liên quan, không thể xóa!";
                return RedirectToAction("Delete", new { id = id });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
