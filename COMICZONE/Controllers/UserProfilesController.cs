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

        public IActionResult MyReviews(int page = 1)
        {
            int pageSize = 5;

            var userIdStr = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userIdStr))
                return View(new List<ProductReview>());

            int userId = int.Parse(userIdStr);

            var query = _context.ProductReviews.Where(r => r.Userid == userId);
            int totalReviews = query.Count();

            var reviews = GetReviews(page, pageSize, userId);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalReviews / pageSize);

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

            return View(customer);
        }
    }
}
