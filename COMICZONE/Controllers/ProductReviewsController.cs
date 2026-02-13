using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Controllers
{
    public class ProductReviewsController : Controller
    {
        private readonly ComiczoneContext _context;

        public ProductReviewsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: ProductReviews
        public async Task<IActionResult> Index()
        {
            var comiczoneContext = _context.ProductReviews.Include(p => p.Product).Include(p => p.User);
            return View(await comiczoneContext.ToListAsync());
        }

        // POST: ProductReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductReview review)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction("Detail", "Products", new { id = review.Productid });
            }

            // Gán thêm dữ liệu hệ thống
            review.Createdat = DateTime.Now;
            review.Isapproved = true; // hoặc false nếu cần duyệt

            // Nếu có login thì lấy UserId
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userId))
            {
                review.Userid = int.Parse(userId);
            }

            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            return RedirectToAction("Detail", "Products", new { id = review.Productid });
        }
    }
}
