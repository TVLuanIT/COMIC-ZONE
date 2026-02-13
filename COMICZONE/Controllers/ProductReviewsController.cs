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

        public async Task<IActionResult> Reviews(int productId, int page = 1, int pageSize = 5)
        {
            var reviews = await _context.ProductReviews
                .Include(r => r.User)
                .Where(r => r.Productid == productId)
                .OrderByDescending(r => r.Createdat)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.ProductReviews
                .Where(r => r.Productid == productId)
                .CountAsync();

            ViewBag.ProductId = productId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            return PartialView("_ProductReviewList", reviews);
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
                return RedirectToAction("Detail", "Products", new { id = review.Productid, tab = "comment" });
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

            // Thêm tab=comment để giữ tab bình luận
            return RedirectToAction("Detail", "Products", new { id = review.Productid, tab = "comment" });
        }
    }
}
