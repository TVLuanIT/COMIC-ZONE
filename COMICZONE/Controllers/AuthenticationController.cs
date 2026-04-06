using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;

namespace COMICZONE.Controllers
{
    public class AuthenticationController : BaseController
    {
        private readonly ComiczoneContext _context;

        public AuthenticationController(ComiczoneContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password, string confirmPassword, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) ||
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Error = "Các trường không được để trống";
                return View();
            }

            if (password != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu và xác nhận mật khẩu không khớp";
                return View();
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.Error = "Email đã được sử dụng";
                return View();
            }

            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.Error = "Tên người dùng đã tồn tại";
                return View();
            }

            // Encode để tránh lỗi tên có dấu
            var encodedName = Uri.EscapeDataString(username);

            var user = new User
            {
                Username = username,
                Email = email,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "Customer",
                Isactive = true,
                Createdat = DateTime.Now,

                Avatar = $"https://ui-avatars.com/api/?name={encodedName}&background=random&color=fff"
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            var customer = new Customer
            {
                Userid = user.Id,
                Fullname = user.Username,
                Createdat = DateTime.Now
            };

            _context.Customers.Add(customer);
            _context.SaveChanges();

            // Tạo session luôn
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("Avatar", user.Avatar);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
        
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email và mật khẩu không được để trống";
                return View();
            }

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng";
                return View();
            }

            if (user.Isdeleted)
            {
                ViewBag.Error = "Tài khoản của bạn tạm thời đã bị khóa. Vui lòng liên hệ Quản trị viên.";
                return View();
            }

            if (!user.Isactive)
            {
                ViewBag.Error = "Tài khoản của bạn hiện đang ngưng hoạt động.";
                return View();
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Passwordhash);
            if (!passwordValid)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // FALLBACK AVATAR CHO USER CŨ
            if (string.IsNullOrEmpty(user.Avatar))
            {
                var encodedName = Uri.EscapeDataString(user.Username);

                user.Avatar = $"https://ui-avatars.com/api/?name={encodedName}&background=random&color=fff";

                _context.SaveChanges(); // Lưu lại DB
            }

            // Tạo session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);
            HttpContext.Session.SetString("Avatar", user.Avatar); // thêm dòng này

            // Lấy CustomerId
            var customer = _context.Customers
                .FirstOrDefault(c => c.Userid == user.Id);

            if (customer == null)
            {
                customer = new Customer
                {
                    Userid = user.Id,
                    Fullname = user.Username,
                    Createdat = DateTime.Now
                };

                _context.Customers.Add(customer);
                _context.SaveChanges();
            }

            if (rememberMe)
            {
                // TODO: implement cookie login
            }

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
    }
}
