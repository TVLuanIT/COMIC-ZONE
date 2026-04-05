using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using COMICZONE.Extensions;
using COMICZONE.Areas.Admin.ViewModels;
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
        public async Task<IActionResult> Index(ReviewSearchModel reviewSearch, ReviewReplySearchModel replySearch, string activeTab = "Reviews")
        {
            var viewModel = new ReviewManagementViewModel
            {
                ReviewSearch = reviewSearch,
                ReplySearch = replySearch,
                ActiveTab = activeTab
            };

            if (activeTab == "Reviews")
            {
                var query = _context.ProductReviews
                    .Include(r => r.Product)
                        .ThenInclude(p => p.Pictures)
                    .Include(r => r.User)
                    .Include(r => r.ProductReviewReplies)
                    .AsQueryable();

                query = query.ApplyReviewFilters(reviewSearch);
                reviewSearch.TotalCount = await query.CountAsync();

                query = query.ApplySort(reviewSearch.SortColumn ?? "Createdat", reviewSearch.IsAscending);
                
                int pageSize = reviewSearch.PageSize > 0 ? reviewSearch.PageSize : 10;
                int pageNumber = reviewSearch.Page > 0 ? reviewSearch.Page : 1;
                
                viewModel.Reviews = await query.ApplyPagination(pageNumber, pageSize).ToListAsync();
                
                reviewSearch.Page = pageNumber;
                reviewSearch.PageSize = pageSize;
            }
            else // Replies
            {
                var query = _context.ProductReviewReplies
                    .Include(r => r.User)
                    .Include(r => r.Review)
                        .ThenInclude(rev => rev.Product)
                    .Include(r => r.Replytouser)
                    .AsQueryable();

                query = query.ApplyReplyFilters(replySearch);
                replySearch.TotalCount = await query.CountAsync();

                query = query.ApplySort(replySearch.SortColumn ?? "Createdat", replySearch.IsAscending);

                int pageSize = replySearch.PageSize > 0 ? replySearch.PageSize : 10;
                int pageNumber = replySearch.Page > 0 ? replySearch.Page : 1;

                viewModel.Replies = await query.ApplyPagination(pageNumber, pageSize).ToListAsync();

                replySearch.Page = pageNumber;
                replySearch.PageSize = pageSize;
            }

            return View(viewModel);
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

            var review = await _context.ProductReviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Reviewid == id);

            if (review == null) return NotFound();

            // Lưu lại giá trị cũ để kiểm tra thay đổi
            var oldRating = review.Rating;
            var oldContent = review.Reviewcontent;
            var oldIsDeleted = review.Isdeleted;

            var changes = new List<string>();
            if (oldRating != model.Rating) changes.Add($"Số sao: {oldRating} ➔ {model.Rating}");
            if (oldContent != model.Reviewcontent) changes.Add($"Nội dung: \"{oldContent}\" ➔ \"{model.Reviewcontent}\"");
            if (oldIsDeleted != model.Isdeleted) changes.Add($"Trạng thái hiển thị: {(oldIsDeleted ? "Đã ẩn" : "Đang hiển thị")} ➔ {(model.Isdeleted ? "Đã ẩn" : "Đang hiển thị")}");

            review.Rating = model.Rating;
            review.Reviewcontent = model.Reviewcontent;
            review.Isdeleted = model.Isdeleted;
            review.Updatedat = DateTime.Now;

            // Gửi thông báo nếu có thay đổi và không phải trường hợp đang bị ẩn (xóa mềm)
            bool statusChanged = oldIsDeleted != model.Isdeleted;
            bool remainsHidden = oldIsDeleted && model.Isdeleted;

            if (changes.Any() && !remainsHidden)
            {
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                _context.Notifications.Add(new Notification
                {
                    UserId = review.Userid,
                    Title = statusChanged ? (review.Isdeleted ? "Đánh giá bị ẩn" : "Đánh giá đã được khôi phục") : "Cập nhật đánh giá sản phẩm",
                    Message = statusChanged
                        ? $"Đánh giá của bạn cho sản phẩm \"{review.Product.Name}\" đã bị " + (review.Isdeleted ? "ẩn bởi Admin." : "Admin khôi phục thành công.")
                        : $"Đánh giá của bạn cho sản phẩm \"{review.Product.Name}\" đã được Admin cập nhật:\n- " + string.Join("\n- ", changes),
                    CreatedBy = adminId,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Link = $"/Products/Detail/{review.Productid}"
                });
            }

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
            var review = await _context.ProductReviews
                .Include(r => r.Product)
                .FirstOrDefaultAsync(r => r.Reviewid == id);

            if (review == null)
            {
                return NotFound();
            }

            review.Isdeleted = !review.Isdeleted;

            // Thêm thông báo
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = review.Userid,
                Title = review.Isdeleted ? "Đánh giá bị ẩn" : "Đánh giá đã được khôi phục",
                Message = $"Đánh giá của bạn cho sản phẩm \"{review.Product.Name}\" đã bị " +
                          (review.Isdeleted ? "ẩn bởi Admin do vi phạm chính sách hoặc nội dung không phù hợp." : "Admin khôi phục thành công."),
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = $"/Products/Detail/{review.Productid}"
            });

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
                .Include(r => r.Product)
                .Include(r => r.ProductReviewLikes)
                .Include(r => r.ProductReviewReplies)
                .FirstOrDefaultAsync(m => m.Reviewid == id);

            if (productReview != null)
            {
                // Thêm thông báo trước khi xóa vĩnh viễn (chỉ thông báo nếu bản ghi chưa bị xóa mềm)
                if (!productReview.Isdeleted)
                {
                    var adminIdStr = HttpContext.Session.GetString("UserId");
                    int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                    _context.Notifications.Add(new Notification
                    {
                        UserId = productReview.Userid,
                        Title = "Xóa đánh giá sản phẩm vĩnh viễn",
                        Message = $"Đánh giá của bạn cho sản phẩm \"{productReview.Product.Name}\" đã bị Admin xóa vĩnh viễn khỏi hệ thống.",
                        CreatedBy = adminId,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }

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
