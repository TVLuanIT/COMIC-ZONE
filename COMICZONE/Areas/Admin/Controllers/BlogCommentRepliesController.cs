using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogCommentRepliesController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public BlogCommentRepliesController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/BlogCommentReplies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var reply = await _context.BlogCommentReplies
                .Include(r => r.User)
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Blog)
                .Include(r => r.Comment)
                    .ThenInclude(c => c.User)
                .Include(r => r.Replytouser)
                .Include(r => r.Parentreply)
                .Include(r => r.BlogCommentReplyLikes)
                .Include(r => r.InverseParentreply)
                    .ThenInclude(cr => cr.User)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (reply == null) return NotFound();

            return View(reply);
        }

        // GET: Admin/BlogCommentReplies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var reply = await _context.BlogCommentReplies
                .Include(r => r.User)
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Blog)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (reply == null) return NotFound();

            return View(reply);
        }

        // POST: Admin/BlogCommentReplies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogCommentReply model)
        {
            if (id != model.Replyid) return NotFound();

            var reply = await _context.BlogCommentReplies
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Blog)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (reply == null) return NotFound();

            var oldContent = reply.Content;
            var oldIsDeleted = reply.Isdeleted;

            var changes = new List<string>();
            if (oldContent != model.Content) changes.Add($"Nội dung: \"{oldContent}\" ➔ \"{model.Content}\"");
            if (oldIsDeleted != model.Isdeleted) changes.Add($"Trạng thái: {(oldIsDeleted == true ? "Đã ẩn" : "Đang hiển thị")} ➔ {(model.Isdeleted == true ? "Đã ẩn" : "Đang hiển thị")}");

            reply.Content = model.Content;
            reply.Isdeleted = model.Isdeleted;
            reply.Updatedat = DateTime.Now;

            bool statusChanged = oldIsDeleted != model.Isdeleted;
            bool remainsHidden = oldIsDeleted == true && model.Isdeleted == true;

            if (changes.Any() && !remainsHidden)
            {
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                _context.Notifications.Add(new Notification
                {
                    UserId = reply.Userid,
                    Title = statusChanged ? (reply.Isdeleted == true ? "Phản hồi bị ẩn" : "Phản hồi đã được khôi phục") : "Cập nhật phản hồi bình luận",
                    Message = statusChanged
                        ? $"Phản hồi của bạn trong bài viết \"{reply.Comment.Blog.Title}\" đã bị " + (reply.Isdeleted == true ? "ẩn bởi Admin." : "Admin khôi phục thành công.")
                        : $"Phản hồi của bạn trong bài viết \"{reply.Comment.Blog.Title}\" đã được Admin cập nhật:\n- " + string.Join("\n- ", changes),
                    CreatedBy = adminId,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Link = $"/Blog/Details/{reply.Comment.Blog.Slug}"
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var replyFull = await _context.BlogCommentReplies
                    .Include(r => r.User)
                    .Include(r => r.Comment)
                        .ThenInclude(c => c.Blog)
                    .FirstOrDefaultAsync(r => r.Replyid == id);
                return View(replyFull);
            }

            return RedirectToAction("Index", "BlogComments");
        }

        // POST: Admin/BlogCommentReplies/ToggleSoftDelete
        [HttpPost]
        public async Task<IActionResult> ToggleSoftDelete(int id)
        {
            var reply = await _context.BlogCommentReplies
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Blog)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (reply == null) return NotFound();

            reply.Isdeleted = !(reply.Isdeleted ?? false);

            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = reply.Userid,
                Title = reply.Isdeleted == true ? "Phản hồi bị ẩn" : "Phản hồi đã được khôi phục",
                Message = $"Phản hồi của bạn trong bài viết \"{reply.Comment.Blog.Title}\" đã bị " +
                          (reply.Isdeleted == true ? "ẩn bởi Admin do vi phạm chính sách hoặc nội dung không phù hợp." : "Admin khôi phục thành công."),
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = $"/Blog/Details/{reply.Comment.Blog.Slug}"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = reply.Isdeleted });
        }

        // GET: Admin/BlogCommentReplies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var reply = await _context.BlogCommentReplies
                .Include(r => r.User)
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Blog)
                .Include(r => r.Comment)
                    .ThenInclude(c => c.User)
                .Include(r => r.Replytouser)
                .Include(r => r.BlogCommentReplyLikes)
                .Include(r => r.InverseParentreply)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (reply == null) return NotFound();

            return View(reply);
        }

        // POST: Admin/BlogCommentReplies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var reply = await _context.BlogCommentReplies
                .Include(r => r.Comment)
                    .ThenInclude(c => c.Blog)
                .Include(r => r.BlogCommentReplyLikes)
                .Include(r => r.InverseParentreply)
                    .ThenInclude(cr => cr.BlogCommentReplyLikes)
                .FirstOrDefaultAsync(r => r.Replyid == id);

            if (reply != null)
            {
                if (reply.Isdeleted != true)
                {
                    var adminIdStr = HttpContext.Session.GetString("UserId");
                    int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                    _context.Notifications.Add(new Notification
                    {
                        UserId = reply.Userid,
                        Title = "Xóa phản hồi bình luận vĩnh viễn",
                        Message = $"Phản hồi của bạn trong bài viết \"{reply.Comment.Blog.Title}\" đã bị Admin xóa vĩnh viễn khỏi hệ thống.",
                        CreatedBy = adminId,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Xóa lượt thích của phản hồi
                    if (reply.BlogCommentReplyLikes.Any())
                    {
                        _context.BlogCommentReplyLikes.RemoveRange(reply.BlogCommentReplyLikes);
                    }

                    // 2. Xóa phản hồi con (nếu có)
                    if (reply.InverseParentreply.Any())
                    {
                        foreach (var childReply in reply.InverseParentreply)
                        {
                            if (childReply.BlogCommentReplyLikes.Any())
                            {
                                _context.BlogCommentReplyLikes.RemoveRange(childReply.BlogCommentReplyLikes);
                            }
                        }
                        _context.BlogCommentReplies.RemoveRange(reply.InverseParentreply);
                    }

                    // 3. Xóa phản hồi
                    _context.BlogCommentReplies.Remove(reply);

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return RedirectToAction("Index", "BlogComments");
        }
    }
}
