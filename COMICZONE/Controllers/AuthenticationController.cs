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
    public class AuthenticationController : Controller
    {
        private readonly ComiczoneContext _context;

        public AuthenticationController(ComiczoneContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password, string confirmPassword)
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

            var user = new User
            {
                Username = username,
                Email = email,
                Passwordhash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = "User",
                Isactive = true,
                Createdat = DateTime.Now
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // Tạo session luôn
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            return RedirectToAction("Index", "Home");
        }
        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password, bool rememberMe = false)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "Email và mật khẩu không được để trống";
                return View();
            }

            // Lấy user từ database
            var user = _context.Users.FirstOrDefault(u => u.Email == email && u.Isactive);
            if (user == null)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // Kiểm tra mật khẩu
            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.Passwordhash);
            if (!passwordValid)
            {
                ViewBag.Error = "Email hoặc mật khẩu không đúng";
                return View();
            }

            // Tạo session
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("UserRole", user.Role);

            // Nếu rememberMe = true, có thể set cookie dài hạn
            if (rememberMe)
            {
                // TODO: implement cookie login
            }

            return RedirectToAction("Index", "Home");
        }
    }
}
