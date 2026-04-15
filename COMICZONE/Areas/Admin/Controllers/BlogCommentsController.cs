using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Extensions;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class BlogCommentsController : AdminBaseController
    {
        private readonly ComiczoneContext _context;

        public BlogCommentsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Admin/BlogComments
        public async Task<IActionResult> Index(BlogCommentSearchModel commentSearch, BlogCommentReplySearchModel replySearch, string activeTab = "Comments")
        {
            var viewModel = new BlogCommentManagementViewModel
            {
                CommentSearch = commentSearch,
                ReplySearch = replySearch,
                ActiveTab = activeTab
            };

            if (activeTab == "Comments")
            {
                var query = _context.BlogComments
                    .Include(c => c.Blog)
                    .Include(c => c.User)
                    .Include(c => c.BlogCommentReplies)
                    .Include(c => c.BlogCommentLikes)
                    .AsQueryable();

                query = query.ApplyBlogCommentFilters(commentSearch);
                commentSearch.TotalCount = await query.CountAsync();

                query = query.ApplySort(commentSearch.SortColumn ?? "Createdat", commentSearch.IsAscending);

                int pageSize = commentSearch.PageSize > 0 ? commentSearch.PageSize : 10;
                int pageNumber = commentSearch.Page > 0 ? commentSearch.Page : 1;

                viewModel.Comments = await query.ApplyPagination(pageNumber, pageSize).ToListAsync();

                commentSearch.Page = pageNumber;
                commentSearch.PageSize = pageSize;
            }
            else // Replies
            {
                var query = _context.BlogCommentReplies
                    .Include(r => r.User)
                    .Include(r => r.Comment)
                        .ThenInclude(c => c.Blog)
                    .Include(r => r.Replytouser)
                    .Include(r => r.BlogCommentReplyLikes)
                    .AsQueryable();

                query = query.ApplyBlogCommentReplyFilters(replySearch);
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

        // GET: Admin/BlogComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.BlogComments
                .Include(c => c.Blog)
                .Include(c => c.User)
                .Include(c => c.BlogCommentReplies)
                    .ThenInclude(r => r.User)
                .Include(c => c.BlogCommentLikes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            return View(comment);
        }

        // GET: Admin/BlogComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.BlogComments
                .Include(c => c.Blog)
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            return View(comment);
        }

        // POST: Admin/BlogComments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BlogComment model)
        {
            if (id != model.Id) return NotFound();

            var comment = await _context.BlogComments
                .Include(c => c.Blog)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            // Lưu giá trị cũ
            var oldContent = comment.Content;
            var oldIsDeleted = comment.Isdeleted;

            var changes = new List<string>();
            if (oldContent != model.Content) changes.Add($"Nội dung: \"{oldContent}\" ➔ \"{model.Content}\"");
            if (oldIsDeleted != model.Isdeleted) changes.Add($"Trạng thái: {(oldIsDeleted == true ? "Đã ẩn" : "Đang hiển thị")} ➔ {(model.Isdeleted == true ? "Đã ẩn" : "Đang hiển thị")}");

            comment.Content = model.Content;
            comment.Isdeleted = model.Isdeleted;
            comment.Updatedat = DateTime.Now;

            // Gửi thông báo
            bool statusChanged = oldIsDeleted != model.Isdeleted;
            bool remainsHidden = oldIsDeleted == true && model.Isdeleted == true;

            if (changes.Any() && !remainsHidden)
            {
                var adminIdStr = HttpContext.Session.GetString("UserId");
                int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                _context.Notifications.Add(new Notification
                {
                    UserId = comment.Userid,
                    Title = statusChanged ? (comment.Isdeleted == true ? "Bình luận bị ẩn" : "Bình luận đã được khôi phục") : "Cập nhật bình luận blog",
                    Message = statusChanged
                        ? $"Bình luận của bạn trong bài viết \"{comment.Blog.Title}\" đã bị " + (comment.Isdeleted == true ? "ẩn bởi Admin." : "Admin khôi phục thành công.")
                        : $"Bình luận của bạn trong bài viết \"{comment.Blog.Title}\" đã được Admin cập nhật:\n- " + string.Join("\n- ", changes),
                    CreatedBy = adminId,
                    CreatedAt = DateTime.Now,
                    IsRead = false,
                    Link = $"/Blog/Details/{comment.Blog.Slug}"
                });
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                var commentFull = await _context.BlogComments
                    .Include(c => c.Blog)
                    .Include(c => c.User)
                    .FirstOrDefaultAsync(c => c.Id == id);
                return View(commentFull);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/BlogComments/ToggleDelete
        [HttpPost]
        public async Task<IActionResult> ToggleDelete(int id)
        {
            var comment = await _context.BlogComments
                .Include(c => c.Blog)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            comment.Isdeleted = !(comment.Isdeleted ?? false);

            // Thêm thông báo
            var adminIdStr = HttpContext.Session.GetString("UserId");
            int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

            _context.Notifications.Add(new Notification
            {
                UserId = comment.Userid,
                Title = comment.Isdeleted == true ? "Bình luận bị ẩn" : "Bình luận đã được khôi phục",
                Message = $"Bình luận của bạn trong bài viết \"{comment.Blog.Title}\" đã bị " +
                          (comment.Isdeleted == true ? "ẩn bởi Admin do vi phạm chính sách hoặc nội dung không phù hợp." : "Admin khôi phục thành công."),
                CreatedBy = adminId,
                CreatedAt = DateTime.Now,
                IsRead = false,
                Link = $"/Blog/Details/{comment.Blog.Slug}"
            });

            await _context.SaveChangesAsync();

            return Json(new { success = true, isDeleted = comment.Isdeleted });
        }

        // GET: Admin/BlogComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var comment = await _context.BlogComments
                .Include(c => c.Blog)
                .Include(c => c.User)
                .Include(c => c.BlogCommentReplies)
                    .ThenInclude(r => r.User)
                .Include(c => c.BlogCommentLikes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment == null) return NotFound();

            return View(comment);
        }

        // POST: Admin/BlogComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var comment = await _context.BlogComments
                .Include(c => c.Blog)
                .Include(c => c.BlogCommentLikes)
                .Include(c => c.BlogCommentReplies)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comment != null)
            {
                // Thông báo trước khi xóa
                if (comment.Isdeleted != true)
                {
                    var adminIdStr = HttpContext.Session.GetString("UserId");
                    int? adminId = int.TryParse(adminIdStr, out var parsedId) ? parsedId : null;

                    _context.Notifications.Add(new Notification
                    {
                        UserId = comment.Userid,
                        Title = "Xóa bình luận blog vĩnh viễn",
                        Message = $"Bình luận của bạn trong bài viết \"{comment.Blog.Title}\" đã bị Admin xóa vĩnh viễn khỏi hệ thống.",
                        CreatedBy = adminId,
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    });
                }

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // 1. Xóa lượt thích của bình luận
                    if (comment.BlogCommentLikes.Any())
                    {
                        _context.BlogCommentLikes.RemoveRange(comment.BlogCommentLikes);
                    }

                    // 2. Xóa các phản hồi và dữ liệu liên quan
                    var replyIds = comment.BlogCommentReplies.Select(r => r.Replyid).ToList();

                    if (replyIds.Any())
                    {
                        // Xóa lượt thích của các phản hồi
                        var replyLikes = await _context.BlogCommentReplyLikes
                            .Where(rl => replyIds.Contains(rl.Replyid))
                            .ToListAsync();
                        if (replyLikes.Any()) _context.BlogCommentReplyLikes.RemoveRange(replyLikes);

                        // Xóa các phản hồi
                        _context.BlogCommentReplies.RemoveRange(comment.BlogCommentReplies);
                    }

                    // 3. Xóa bình luận
                    _context.BlogComments.Remove(comment);

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
    }
}
