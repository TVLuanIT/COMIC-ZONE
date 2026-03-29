using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductReviewsController : AdminBaseController
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
            review.Isdeleted = model.Isdeleted;
            review.Updatedat = DateTime.Now;

            string errorMessage = "";

            try
            {
                await _context.SaveChangesAsync();
                await UpdateProductReviewSummary(review.Productid);
            }
            catch (DbUpdateException ex)
            {
                errorMessage = ex.Message;
            }

            if (errorMessage == "")
            {
                return RedirectToAction(nameof(Index));
            }

            // Load lại đầy đủ Product + User nếu có lỗi
            var reviewFull = await _context.ProductReviews
                .Include(x => x.Product)
                    .ThenInclude(p => p.Pictures)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.Reviewid == id);

            return View(reviewFull);
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
                    .ThenInclude(p => p.Pictures)
                .Include(p => p.User)
                .Include(p => p.ProductReviewReplies)
                    .ThenInclude(r => r.User)
                .Include(p => p.ProductReviewLikes)
                .FirstOrDefaultAsync(m => m.Reviewid == id);
            if (productReview == null)
            {
                return NotFound();
            }

            return View(productReview);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var review = await _context.ProductReviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }

            review.Isdeleted = !review.Isdeleted;
            await _context.SaveChangesAsync();
            await UpdateProductReviewSummary(review.Productid);

            return Json(new { success = true, isDeleted = review.Isdeleted });
        }

        // POST: Admin/ProductReviews/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productReview = await _context.ProductReviews
                .Include(r => r.ProductReviewLikes)
                .Include(r => r.ProductReviewReplies)
                .FirstOrDefaultAsync(m => m.Reviewid == id);

            if (productReview != null)
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Xóa Lượt thích của đánh giá
                    if (productReview.ProductReviewLikes.Any())
                    {
                        _context.ProductReviewLikes.RemoveRange(productReview.ProductReviewLikes);
                    }

                    // 2. Xóa các Phản hồi và dữ liệu liên quan của phản hồi
                    var replyIds = productReview.ProductReviewReplies.Select(r => r.Replyid).ToList();
                    
                    if (replyIds.Any())
                    {
                        // Xóa Lượt thích của các phản hồi
                        var replyLikes = await _context.ProductReviewReplyLikes
                            .Where(rl => replyIds.Contains(rl.Replyid))
                            .ToListAsync();
                        if (replyLikes.Any()) _context.ProductReviewReplyLikes.RemoveRange(replyLikes);

                        // Xóa Báo cáo vi phạm của các phản hồi
                        var replyReports = await _context.ViolationReports
                            .Where(vr => vr.Reporttype == (int)ReportType.Reply && replyIds.Contains(vr.Targetid))
                            .ToListAsync();
                        if (replyReports.Any()) _context.ViolationReports.RemoveRange(replyReports);

                        // Xóa các phản hồi
                        _context.ProductReviewReplies.RemoveRange(productReview.ProductReviewReplies);
                    }

                    // 3. Xóa Báo cáo vi phạm trực tiếp của đánh giá
                    var reviewReports = await _context.ViolationReports
                        .Where(vr => vr.Reporttype == (int)ReportType.Review && vr.Targetid == id)
                        .ToListAsync();
                    if (reviewReports.Any()) _context.ViolationReports.RemoveRange(reviewReports);

                    // 4. Cuối cùng xóa bản ghi Đánh giá chính
                    _context.ProductReviews.Remove(productReview);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task UpdateProductReviewSummary(int productId)
        {
            var reviewsQuery = _context.ProductReviews
                .Where(r => r.Productid == productId && !r.Isdeleted);

            var total = await reviewsQuery.CountAsync();
            decimal average = 0;

            if (total > 0)
            {
                average = await reviewsQuery.AverageAsync(r => (decimal)r.Rating);
            }

            var summary = await _context.ProductReviewSummaries
                .FirstOrDefaultAsync(s => s.Productid == productId);

            if (summary == null)
            {
                summary = new ProductReviewSummary
                {
                    Productid = productId,
                    Totalreview = total,
                    Averagerating = average,
                    Lastupdated = DateTime.Now
                };
                _context.ProductReviewSummaries.Add(summary);
            }
            else
            {
                summary.Totalreview = total;
                summary.Averagerating = average;
                summary.Lastupdated = DateTime.Now;
            }

            await _context.SaveChangesAsync();
        }
    }
}
