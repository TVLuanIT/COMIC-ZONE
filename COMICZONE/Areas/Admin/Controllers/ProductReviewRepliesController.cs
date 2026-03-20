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
            ViewData["Replytouserid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Replytouserid);
            ViewData["Reviewid"] = new SelectList(_context.ProductReviews, "Reviewid", "Reviewid", productReviewReply.Reviewid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Userid);
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



        // GET: Admin/ProductReviewReplies/Create
        public IActionResult Create()
        {
            ViewData["Replytouserid"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["Reviewid"] = new SelectList(_context.ProductReviews, "Reviewid", "Reviewid");
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: Admin/ProductReviewReplies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Replyid,Reviewid,Userid,Replycontent,Createdat,Replytouserid,Updatedat")] ProductReviewReply productReviewReply)
        {
            if (ModelState.IsValid)
            {
                _context.Add(productReviewReply);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Replytouserid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Replytouserid);
            ViewData["Reviewid"] = new SelectList(_context.ProductReviews, "Reviewid", "Reviewid", productReviewReply.Reviewid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Userid);
            return View(productReviewReply);
        }
    }
}
