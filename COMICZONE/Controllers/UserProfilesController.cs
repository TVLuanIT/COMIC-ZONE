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
    public class UserProfilesController : BaseController
    {
        private readonly ComiczoneContext _context;

        public UserProfilesController(ComiczoneContext context)
        {
            _context = context;
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
                .Where(r => r.Userid == userId)
                .OrderByDescending(r => r.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        private List<ProductReview> GetReviews(int page, int pageSize, int userId)
        {
            return _context.ProductReviews
                .Include(r => r.Product)
                .Where(r => r.Userid == userId)
                .OrderByDescending(r => r.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();
        }

        private List<Order> GetOrders()
        {
            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
            {
                return new List<Order>();
            }

            int userId = int.Parse(userIdStr);

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
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
            var reviewQuery = _context.ProductReviews.Where(r => r.Userid == userId);
            int totalReviews = reviewQuery.Count();

            var reviews = GetReviews(reviewPage, pageSize, userId);

            // REPLIES
            var replyQuery = _context.ProductReviewReplies.Where(r => r.Userid == userId);
            int totalReplies = replyQuery.Count();

            var replies = GetReplies(replyPage, pageSize, userId);

            ViewData["MyReplies"] = replies;

            ViewBag.ReviewPage = reviewPage;
            ViewBag.ReviewTotalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

            ViewBag.ReplyPage = replyPage;
            ViewBag.ReplyTotalPages = (int)Math.Ceiling((double)totalReplies / pageSize);

            return View(reviews);
        }

        public IActionResult MyOrders()
        {
            var orders = GetOrders();

            return View(orders);
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
