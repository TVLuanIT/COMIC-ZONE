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

        // GET: Admin/ProductReviewReplies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productReviewReply = await _context.ProductReviewReplies.FindAsync(id);
            if (productReviewReply == null)
            {
                return NotFound();
            }
            ViewData["Replytouserid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Replytouserid);
            ViewData["Reviewid"] = new SelectList(_context.ProductReviews, "Reviewid", "Reviewid", productReviewReply.Reviewid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Userid);
            return View(productReviewReply);
        }

        // POST: Admin/ProductReviewReplies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Replyid,Reviewid,Userid,Replycontent,Createdat,Replytouserid,Updatedat")] ProductReviewReply productReviewReply)
        {
            if (id != productReviewReply.Replyid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(productReviewReply);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductReviewReplyExists(productReviewReply.Replyid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["Replytouserid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Replytouserid);
            ViewData["Reviewid"] = new SelectList(_context.ProductReviews, "Reviewid", "Reviewid", productReviewReply.Reviewid);
            ViewData["Userid"] = new SelectList(_context.Users, "Id", "Id", productReviewReply.Userid);
            return View(productReviewReply);
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
            .Include(p => p.Parentreply!)
                .ThenInclude(pr => pr.User)
            .Include(p => p.Review)

            .FirstOrDefaultAsync(m => m.Replyid == id);

            if (productReviewReply == null)
            {
                return NotFound();
            }

            return View(productReviewReply);
        }

        // POST: Admin/ProductReviewReplies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var productReviewReply = await _context.ProductReviewReplies.FindAsync(id);
            if (productReviewReply != null)
            {
                _context.ProductReviewReplies.Remove(productReviewReply);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductReviewReplyExists(int id)
        {
            return _context.ProductReviewReplies.Any(e => e.Replyid == id);
        }
    }
}
