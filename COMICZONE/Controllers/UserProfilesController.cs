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

        public IActionResult MyProfile()
        {
            var customer = GetCustomer();

            return View(customer);
        }

        public IActionResult MyOrders()
        {
            var customer = GetCustomer();

            return View(customer);
        }

        public IActionResult MyReviews()
        {
            var customer = GetCustomer();

            return View(customer);
        }

    }
}
