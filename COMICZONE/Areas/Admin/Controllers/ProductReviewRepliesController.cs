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
    public class ProductReviewRepliesController : Controller
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
            var existing = await _context.ProductReviewReplies.FindAsync(id);

            if (existing == null) return NotFound();

            existing.Replycontent = productReviewReply.Replycontent;
            existing.Updatedat = DateTime.Now;

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
                .Include(p => p.Review)
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

            var parent = await _context.ProductReviewReplies.FindAsync(id);
            if (parent == null) return NotFound();

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
            [Bind("Replycontent,Reviewid,Userid,Replytouserid,Parentreplyid")]
            ProductReviewReply reply)
        {
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
