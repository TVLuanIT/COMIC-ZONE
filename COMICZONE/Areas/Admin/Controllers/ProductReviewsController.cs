using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductReviewsController : Controller
    {
        private readonly ComiczoneContext _context;

        public ProductReviewsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductReviews
        public async Task<IActionResult> Index()
        {
            var comiczoneContext = _context.ProductReviews
                .Include(r => r.Product)
                    .ThenInclude(p => p.Pictures)
                .Include(r => r.User);

            return View(await comiczoneContext.ToListAsync());
        }

        // GET: Admin/ProductReviews/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReview = await _context.ProductReviews
                .Include(p => p.Product)
                    .ThenInclude(p => p.Pictures)
                .Include(p => p.User)
                .Include(r => r.ProductReviewReplies)
                    .ThenInclude(rp => rp.User)
                .FirstOrDefaultAsync(m => m.Reviewid == id);
            if (productReview == null)
            {
                return NotFound();
            }

            return View(productReview);
        }

        // GET: Admin/ProductReviews/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReview = await _context.ProductReviews
                .Include(x => x.Product)
                    .ThenInclude(p => p.Pictures)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Reviewid == id);

            if (productReview == null)
            {
                return NotFound();
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Id", "Id", productReview.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id", productReview.Userid);
            return View(productReview);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductReview model)
        {
            if (id != model.Reviewid) return NotFound();

            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null) return NotFound();

            review.Rating = model.Rating;
            review.Reviewcontent = model.Reviewcontent;
            review.Updatedat = DateTime.Now;

            string errorMessage = "";

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                errorMessage = ex.Message;
            }

            // Load lại đầy đủ Product + User
            var reviewFull = await _context.ProductReviews
                .Include(x => x.Product)
                    .ThenInclude(p => p.Pictures)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Reviewid == id);

            return View(reviewFull);
        }





        // GET: Admin/ProductReviews/Create
        public IActionResult Create()
        {
            ViewData["Productid"] = new SelectList(_context.Products, "Id", "Id");
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/ProductReviews/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Reviewid,Productid,Userid,Rating,Reviewcontent,Createdat,Updatedat")] ProductReview productReview)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productReview);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Productid"] = new SelectList(_context.Products, "Id", "Id", productReview.Productid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id", productReview.Userid);
            return View(productReview);
        }

        // GET: Admin/ProductReviews/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReview = await _context.ProductReviews
                .Include(p => p.Product)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Reviewid == id);
            if (productReview == null)
            {
                return NotFound();
            }

            return View(productReview);
        }

        // POST: Admin/ProductReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productReview = await _context.ProductReviews.FindAsync(id);
            if (productReview != null)
            {
                _context.ProductReviews.Remove(productReview);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductReviewExists(int id)
        {
            return _context.ProductReviews.Any(e => e.Reviewid == id);
        }
    }
}
