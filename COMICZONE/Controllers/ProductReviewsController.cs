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
using Microsoft.AspNetCore.Authorization;

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
            // Kiểm tra người dùng đã login chưa
            var userId = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userId))
            {
                // Chuyển tới trang login tự tạo của bạn
                return RedirectToAction("Login", "Authentication");
            }

            // Gán UserId từ session
            review.Userid = int.Parse(userId);

            // Gán thời gian tạo
            review.Createdat = DateTime.Now;

            // Lưu review
            _context.ProductReviews.Add(review);
            await _context.SaveChangesAsync();

            // Quay lại trang chi tiết sản phẩm
            return RedirectToAction("Detail", "Products",
                new { id = review.Productid, tab = "comment" });
        }
    }
}
