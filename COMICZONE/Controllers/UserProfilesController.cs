using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using System.Net;
using System.Net.Mail;

namespace COMICZONE.Controllers
{
    public class UserProfilesController : BaseController
    {
        private readonly ComiczoneContext _context;

        public UserProfilesController(ComiczoneContext context)
        {
            _context = context;
        }

        public IActionResult Settings()
        {
            var customer = GetCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            ViewBag.Page = "Settings";

            return View(customer);
        }

        public IActionResult ResetPassword(string token)
        {
            var user = _context.Users
                .FirstOrDefault(x => x.ResetToken == token);

            if (user == null || user.ResetTokenExpire == null || user.ResetTokenExpire < DateTime.Now)
            {
                TempData["Error"] = "Link không hợp lệ hoặc đã hết hạn";
                return RedirectToAction("MyProfile");
            }

            ViewBag.Token = token;

            return View();
        }

        [HttpPost]
        public IActionResult ResetPassword(string token, string newPassword, string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword))
            {
                TempData["Error"] = "Mật khẩu không được để trống";
                return RedirectToAction("ResetPassword", new { token });
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Mật khẩu và xác nhận mật khẩu không khớp";
                return RedirectToAction("ResetPassword", new { token });
            }

            var user = _context.Users
                .FirstOrDefault(x => x.ResetToken == token && x.ResetTokenExpire > DateTime.Now);

            if (user == null)
            {
                TempData["Error"] = "Token không hợp lệ hoặc đã hết hạn";
                return RedirectToAction("MyProfile");
            }

            user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpire = null;

            _context.SaveChanges();

            return RedirectToAction("Login", "Authentication");
        }

        public IActionResult ForgotPassword()
        {
            var customer = GetCustomer();

            ViewBag.Page = "ForgotPassword";

            return View("MyProfile", customer);
        }

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            var customer = GetCustomer();

            if (user == null)
            {
                TempData["Error"] = "Email không tồn tại!";
                ViewBag.Page = "ForgotPassword";
                return View("MyProfile", customer);
            }

            // tạo token reset
            var token = Guid.NewGuid().ToString();

            user.ResetToken = token;
            user.ResetTokenExpire = DateTime.Now.AddMinutes(30);

            _context.SaveChanges();

            // tạo link reset password
            var resetLink = Url.Action(
                "ResetPassword",
                "UserProfiles",
                new { token = token },
                Request.Scheme
            );

            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(
                        "luan31032004@gmail.com",
                        "rufy atyg tjli tmhq"
                    );

                    smtp.EnableSsl = true;

                    using (var mail = new MailMessage())
                    {
                        mail.From = new MailAddress("luan31032004@gmail.com");
                        mail.To.Add(email);
                        mail.Subject = "Đặt lại mật khẩu - ComicZone";

                        mail.Body =
                            $"Nhấn vào link sau để đặt lại mật khẩu:\n\n{resetLink}\n\nLink có hiệu lực trong 30 phút.";

                        smtp.Send(mail);
                    }
                }

                TempData["Success"] = "Link đặt lại mật khẩu đã được gửi tới email.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Không gửi được email: " + ex.Message;
            }

            ViewBag.Page = "ForgotPassword";

            return View("MyProfile", customer);
        }

        [HttpPost]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            if (avatar == null || avatar.Length == 0)
                return Json(new { success = false, message = "File không hợp lệ" });

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(avatar.FileName).ToLower();

            if (!allowed.Contains(ext))
                return Json(new { success = false, message = "Chỉ cho phép JPG, PNG, WEBP" });

            string folder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/avatar");

            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            string fileName = Guid.NewGuid() + ext;
            string filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await avatar.CopyToAsync(stream);
            }

            string avatarUrl = "/uploads/avatar/" + fileName;

            var user = _context.Users.Find(userId);

            if (user != null)
            {
                // xóa avatar cũ
                if (!string.IsNullOrEmpty(user.Avatar))
                {
                    var oldFile = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        user.Avatar.TrimStart('/')
                    );

                    if (System.IO.File.Exists(oldFile))
                        System.IO.File.Delete(oldFile);
                }

                user.Avatar = avatarUrl;
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.SetString("Avatar", avatarUrl);

            return Json(new { success = true });
        }

        public IActionResult ReadNotification(int id)
        {
            var notification = _context.Notifications.FirstOrDefault(n => n.NotificationId == id);

            if (notification != null)
            {
                notification.IsRead = true;
                _context.SaveChanges();

                if (!string.IsNullOrEmpty(notification.Link))
                {
                    return Redirect(notification.Link);
                }
            }

            return RedirectToAction("Notifications");
        }

        public IActionResult Notifications(int page = 1)
        {
            int pageSize = 6;
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(userIdStr);

            var query = _context.Notifications
                .Where(n => n.UserId == userId && !n.Isdeleted);

            int totalItems = query.Count();

            var notifications = query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.Pagination = new PaginationModel
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Action = "Notifications",
                Controller = "UserProfiles",
                PageParam = "page",
                ExtraParams = new Dictionary<string, string>()
            };

            return View(notifications);
        }

        public IActionResult ChangePassword()
        {
            var customer = GetCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            ViewBag.Page = "ChangePassword";

            return View("MyProfile", customer);
        }

        [HttpPost]
        public IActionResult ChangePassword(string currentPassword, string newPassword, string confirmPassword)
        {
            var customer = GetCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == customer.Userid);

            if (user == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.Passwordhash))
            {
                TempData["Error"] = "Mật khẩu hiện tại không đúng";
                ViewBag.Page = "ChangePassword";
                return View("MyProfile", customer);
            }

            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Xác nhận mật khẩu không khớp";
                ViewBag.Page = "ChangePassword";
                return View("MyProfile", customer);
            }

            user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(newPassword);

            _context.SaveChanges();

            TempData["Success"] = "Đổi mật khẩu thành công";

            return RedirectToAction("MyProfile");
        }

        public IActionResult EditProfile()
        {
            var customer = GetCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            ViewBag.Page = "Edit";

            return View("MyProfile", customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(Customer model)
        {
            var customer = GetCustomer();

            if (customer == null)
                return RedirectToAction("Login", "Authentication");

            bool isChanged = false;

            if (customer.Fullname != model.Fullname)
            {
                customer.Fullname = model.Fullname;
                isChanged = true;
            }

            if (customer.Phone != model.Phone)
            {
                customer.Phone = model.Phone;
                isChanged = true;
            }

            if (customer.Address != model.Address)
            {
                customer.Address = model.Address;
                isChanged = true;
            }

            if (isChanged)
            {
                _context.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin thành công";
            }
            else
            {
                TempData["Info"] = "Không có thay đổi nào";
            }

            return RedirectToAction("MyProfile");
        }

        private List<ProductReviewReply> GetReplies(int page, int pageSize, int userId)
        {
            return _context.ProductReviewReplies
                .Include(r => r.Review)
                .ThenInclude(r => r.Product)
                .Where(r => r.Userid == userId && !r.Isdeleted)
                .OrderByDescending(r => r.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        private List<ProductReview> GetReviews(int page, int pageSize, int userId)
        {
            return _context.ProductReviews
                .Include(r => r.Product)
                .Where(r => r.Userid == userId && !r.Isdeleted)
                .OrderByDescending(r => r.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        private List<Order> GetOrders(int page, int pageSize, out int totalItems)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");
            totalItems = 0;

            if (string.IsNullOrEmpty(userIdStr))
            {
                return new List<Order>();
            }

            int userId = int.Parse(userIdStr);

            var query = _context.Orders
                .Where(o => o.UserId == userId && !o.Isdeleted);

            totalItems = query.Count();

            var orders = query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return orders;
        }

        private Customer? GetCustomer()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return null;
            }

            int userId = int.Parse(userIdStr);

            var customer = _context.Customers
                .Include(c => c.User)
                .FirstOrDefault(c => c.Userid == userId);

            return customer;
        }

        public IActionResult MyReviews(int reviewPage = 1, int replyPage = 1)
        {
            int pageSize = 5;

            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return View(new List<ProductReview>());

            int userId = int.Parse(userIdStr);

            // REVIEWS
            var reviewQuery = _context.ProductReviews.Where(r => r.Userid == userId && !r.Isdeleted);
            int totalReviews = reviewQuery.Count();

            var reviews = GetReviews(reviewPage, pageSize, userId);

            // REPLIES
            var replyQuery = _context.ProductReviewReplies.Where(r => r.Userid == userId && !r.Isdeleted);
            int totalReplies = replyQuery.Count();

            var replies = GetReplies(replyPage, pageSize, userId);

            ViewData["MyReplies"] = replies;

            ViewBag.ReviewPage = reviewPage;
            ViewBag.ReviewTotalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            ViewBag.ReplyPage = replyPage;
            ViewBag.ReplyTotalPages = (int)Math.Ceiling((double)totalReplies / pageSize);

            return View(reviews);
        }

        public IActionResult MyOrders(int page = 1)
        {
            int pageSize = 6;
            int totalItems;
            var orders = GetOrders(page, pageSize, out totalItems);

            ViewBag.Pagination = new PaginationModel
            {
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling((double)totalItems / pageSize),
                Action = "MyOrders",
                Controller = "UserProfiles",
                PageParam = "page",
                ExtraParams = new Dictionary<string, string>()
            };

            return View(orders);
        }

        public IActionResult OrderDetails(int id)
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return RedirectToAction("Login", "Authentication");
            }

            int userId = int.Parse(userIdStr);

            var order = _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(pr => pr.Pictures)
                .FirstOrDefault(o => o.OrderId == id && o.UserId == userId && !o.Isdeleted);

            if (order == null)
            {
                return NotFound();
            }

            ViewBag.Page = "OrderDetails";
            ViewBag.Order = order;

            return View("MyOrders", new List<Order> { order });
        }

        public IActionResult MyProfile()
        {
            var customer = GetCustomer();

            if (customer == null)
            {
                return RedirectToAction("Login", "Authentication");
            }

            ViewBag.Page = "Profile";

            return View(customer);
        }

    }
}
