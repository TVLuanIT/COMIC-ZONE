using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductReviewRepliesController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public ProductReviewRepliesController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/ProductReviewReplies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReviewReply = await _context.ProductReviewReplies
                .Include(p => p.Parentreply!)
                    .ThenInclude(pr => pr.User)
                .Include(p => p.Review)
                    .ThenInclude(r => r.Product)
                .Include(p => p.Review)
                    .ThenInclude(r => r.User)
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.Replyid == id);

            if (productReviewReply == null)
            {
                return NotFound();
            }

            return View(productReviewReply);
        }

        // GET: Admin/ProductReviewReplies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReviewReply = await _context.ProductReviewReplies
                .Include(x => x.Review)
                    .ThenInclude(r => r.User)
                .Include(x => x.Replytouser)
                .Include(x => x.Parentreply)
                    .ThenInclude(p => p!.User)
                .FirstOrDefaultAsync(x => x.Replyid == id);

            if (productReviewReply == null)
            {
                return NotFound();
            }

            return View(productReviewReply);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductReviewReply productReviewReply)
        {
            var existing = await _context.ProductReviewReplies
                .Include(r => r.Review)
                    .ThenInclude(rev => rev.Product)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (existing == null) return NotFound();

            var oldContent = existing.Replycontent;
            var changes = new List<string>();
            if (oldContent != productReviewReply.Replycontent)
            {
                changes.Add($"Nội dung: \"{oldContent}\" ➔ \"{productReviewReply.Replycontent}\"");
            }

            existing.Replycontent = productReviewReply.Replycontent;
            existing.Updatedat = DateTime.Now;

            // Gửi thông báo nếu có thay đổi
            if (changes.Any())
            {
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                _context.Notifications.Add(new Notification
                {
                    UserId = existing.Userid,
                    Title = "Cập nhật phản hồi đánh giá",
                    Message = $"Phản hồi của bạn trong bài đánh giá sản phẩm \"{(existing.Review?.Product?.Name ?? "Sản phẩm")}\" đã được Admin cập nhật:\n- " + string.Join("\n- ", changes),
                    CreatedBy = adminId,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Link = $"/Products/Detail/{existing.Review?.Productid}"
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ProductReviews", new { id = existing.Reviewid });
        }

        // GET: Admin/ProductReviewReplies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReviewReply = await _context.ProductReviewReplies
                .Include(p => p.User)
                .Include(p => p.Parentreply)
                    .ThenInclude(pr => pr!.User)
                .Include(p => p.Replytouser)
                .Include(p => p.Review)
                    .ThenInclude(r => r.Product)
                        .ThenInclude(pr => pr.Pictures)
                .Include(p => p.Review)
                    .ThenInclude(r => r.User)
                .Include(p => p.ProductReviewReplyLikes)
                .FirstOrDefaultAsync(m => m.Replyid == id);

            if (productReviewReply == null)
            {
                return NotFound();
            }

            return View(productReviewReply);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reply = await _context.ProductReviewReplies
                .Where(r => r.Parentreplyid == id)
                .ToListAsync();

            // Gỡ liên kết con
            foreach (var child in reply)
            {
                child.Parentreplyid = null;
            }

            var parent = await _context.ProductReviewReplies
                .Include(r => r.Review)
                    .ThenInclude(rev => rev.Product)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (parent == null) return NotFound();

            // Thêm thông báo trước khi xóa
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = parent.Userid,
                Title = "Xóa phản hồi đánh giá",
                Message = $"Phản hồi của bạn trong bài đánh giá sản phẩm \"{(parent.Review?.Product?.Name ?? "Sản phẩm")}\" đã bị Admin xóa khỏi hệ thống.",
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false
            });

            int reviewId = parent.Reviewid;

            _context.ProductReviewReplies.Remove(parent);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ProductReviews", new { id = reviewId });
        }

        public IActionResult Create(int reviewId, int? parentReplyId)
        {
            if (reviewId <= 0)
                return NotFound();

            ViewBag.ReviewId = reviewId;

            if (parentReplyId != null)
            {
                var parent = _context.ProductReviewReplies.Find(parentReplyId);
                if (parent == null) return NotFound();

                ViewBag.ParentReplyId = parentReplyId;
                ViewBag.ParentReplyUserId = parent.Userid;
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Replycontent,Reviewid,Replytouserid,Parentreplyid")]
            ProductReviewReply reply)
        {
            ModelState.Remove("User");
            ModelState.Remove("Review");

            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Authentication", new { area = "" });
            }

            reply.Userid = int.Parse(userId);

            if (!ModelState.IsValid)
            {
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        Console.WriteLine($"{state.Key}: {error.ErrorMessage}");
                    }
                }

                ViewBag.ReviewId = reply.Reviewid;
                ViewBag.ParentReplyId = reply.Parentreplyid;
                ViewBag.ParentReplyUserId = reply.Replytouserid;

                return View(reply);
            }

            reply.Createdat = DateTime.Now;

            _context.Add(reply);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "ProductReviews",
                new { id = reply.Reviewid, area = "Admin" });
        }
    }
}
