using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COMICZONE.Data;
using COMICZONE.Models;
using COMICZONE.Models.Enums;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;

namespace COMICZONE.Areas.Blogs.Controllers
{
    [Area("Blogs")]
    public class BlogsController : COMICZONE.Controllers.BaseController
    {
        private readonly ComiczoneContext _context;

        public BlogsController(ComiczoneContext context)
        {
            _context = context;
        }

        // GET: Blogs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var blog = await _context.Blogs
                .Include(b => b.Author)
                .Include(b => b.Categories)
                .Include(b => b.BlogComments)
                    .ThenInclude(c => c.User)
                .Include(b => b.BlogComments)
                    .ThenInclude(c => c.BlogCommentLikes)
                .Include(b => b.BlogComments)
                    .ThenInclude(c => c.BlogCommentReplies)
                        .ThenInclude(r => r.BlogCommentReplyLikes)
                .Include(b => b.BlogComments)
                    .ThenInclude(c => c.BlogCommentReplies)
                        .ThenInclude(r => r.Replytouser)
                .FirstOrDefaultAsync(m => m.Id == id && !m.Isdeleted);

            if (blog == null)
            {
                return NotFound();
            }

            // Đánh dấu thông báo liên quan đến bài viết này là đã đọc nếu người dùng là tác giả
            var currentUserIdStr = CurrentUserId();
            if (!string.IsNullOrEmpty(currentUserIdStr) && int.TryParse(currentUserIdStr, out int userId))
            {
                if (blog.Authorid == userId)
                {
                    string searchPattern = $"/Blogs/Blogs/Details/{blog.Id}";
                    var unreadBlogNotifs = await _context.Notifications
                        .Where(n => n.UserId == userId && 
                               (n.IsRead == false || n.IsRead == null) && 
                               !n.Isdeleted && 
                               n.Link.Contains(searchPattern))
                        .ToListAsync();

                    if (unreadBlogNotifs.Any())
                    {
                        foreach (var n in unreadBlogNotifs) n.IsRead = true;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            // Bài viết liên quan
            ViewBag.RelatedBlogs = await _context.Blogs
                .Where(b => b.Id != id && b.Status == BlogStatus.Approved.ToString() && !b.Isdeleted)
                .Include(b => b.Author)
                .OrderByDescending(b => b.Createdat)
                .Take(3)
                .ToListAsync();

            return View(blog);
        }

        // GET: Blogs/Create
        public async Task<IActionResult> Create()
        {
            if (!IsLoggedIn())
            {
                TempData["LoginRequired"] = "Bạn cần đăng nhập để viết bài.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Categories = await _context.BlogCategories.Where(c => !c.Isdeleted).ToListAsync();
            return View();
        }

        // POST: Blogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Shortdescription,Content")] Blog blog, int[] selectedCategories, IFormFile? thumbnailFile)
        {
            if (!IsLoggedIn())
            {
                TempData["LoginRequired"] = "Bạn cần đăng nhập để viết bài.";
                return RedirectToAction("Index", "Home");
            }

            ModelState.Remove("Author");
            ModelState.Remove("Categories");
            ModelState.Remove("Slug");
            ModelState.Remove("Status");

            if (selectedCategories == null || selectedCategories.Length == 0)
            {
                ModelState.AddModelError("selectedCategories", "Vui lòng chọn ít nhất một danh mục cho bài viết.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle Thumbnail
                    if (thumbnailFile != null && thumbnailFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(thumbnailFile.FileName);
                        var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/blogs");

                        if (!Directory.Exists(folderPath))
                            Directory.CreateDirectory(folderPath);

                        var filePath = Path.Combine(folderPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await thumbnailFile.CopyToAsync(stream);
                        }
                        blog.Thumbnail = fileName;
                    }

                    // Metadata
                    blog.Authorid = int.Parse(CurrentUserId()!);
                    blog.Createdat = DateTime.Now;
                    blog.Updatedat = DateTime.Now;
                    blog.Status = BlogStatus.Pending.ToString();
                    blog.Isdeleted = false;

                    // Generate Slug
                    blog.Slug = GenerateSlug(blog.Title);

                    // Categories
                    if (selectedCategories != null && selectedCategories.Length > 0)
                    {
                        foreach (var categoryId in selectedCategories)
                        {
                            var category = await _context.BlogCategories.FindAsync(categoryId);
                            if (category != null)
                            {
                                blog.Categories.Add(category);
                            }
                        }
                    }

                    _context.Add(blog);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Bài viết của bạn đã được gửi và đang chờ phê duyệt. Cảm ơn bạn đã đóng góp!";
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra khi lưu bài viết: " + ex.Message);
                }
            }

            ViewBag.Categories = await _context.BlogCategories.Where(c => !c.Isdeleted).ToListAsync();
            ViewBag.SelectedCategories = selectedCategories?.ToList() ?? new List<int>();
            return View(blog);
        }

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";

            string slug = title.ToLowerInvariant();

            // Thay thế các ký tự có dấu
            string[] vietnameseSigns = new string[]
            {
                "aAeEoOuUiIdDyY",
                "áàạảãâấầậẩẫăắằặẳẵ",
                "ÁÀẠẢÃÂẤẦẬẨẪĂẮẰẶẲẴ",
                "éèẹẻẽêếềệểễ",
                "ÉÈẸẺẼÊẾỀỆỂỄ",
                "óòọỏõôốồộổỗơớờợởỡ",
                "ÓÒỌỎÕÔỐỒỘỔỖƠỚỜỢỞỠ",
                "úùụủũưứừựửữ",
                "ÚÙỤỦŨƯỨỪỰỬỮ",
                "íìịỉĩ",
                "ÍÌỊỈĨ",
                "đ",
                "Đ",
                "ýỳỵỷỹ",
                "ÝỲỴỶỸ"
            };

            for (int i = 1; i < vietnameseSigns.Length; i++)
            {
                for (int j = 0; j < vietnameseSigns[i].Length; j++)
                {
                    slug = slug.Replace(vietnameseSigns[i][j], vietnameseSigns[0][i - 1]);
                }
            }

            // Xóa các ký tự đặc biệt
            slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
            // Xóa khoảng trắng thừa
            slug = Regex.Replace(slug, @"\s+", " ").Trim();
            // Thay khoảng trắng bằng gạch ngang
            slug = slug.Replace(" ", "-");

            // Đảm bảo unique (trong thực tế nên check DB, ở đây làm đơn giản thêm ID ngẫu nhiên)
            slug += "-" + Guid.NewGuid().ToString().Substring(0, 4);

            return slug;
        }

        // POST: Blogs/Blogs/AddComment
        [HttpPost]
        public async Task<IActionResult> AddComment(int blogId, string content)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để bình luận." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung bình luận không được để trống." });
            }

            try
            {
                var comment = new BlogComment
                {
                    Blogid = blogId,
                    Userid = int.Parse(CurrentUserId()!),
                    Content = content,
                    Createdat = DateTime.Now,
                    Isdeleted = false
                };

                _context.BlogComments.Add(comment);
                await _context.SaveChangesAsync();

                // Nạp thêm thông tin User để trả về cho Client
                await _context.Entry(comment).Reference(c => c.User).LoadAsync();

                return Json(new
                {
                    success = true,
                    message = "Thêm bình luận thành công.",
                    comment = new
                    {
                        id = comment.Id,
                        username = comment.User?.Username ?? "User",
                        avatar = COMICZONE.Extensions.StringExtensions.AvatarOrDefault(comment.User?.Avatar),
                        content = comment.Content,
                        createdAt = comment.Createdat.ToString("dd/MM/yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã xảy ra lỗi: " + ex.Message });
            }
        }
        // POST: Blogs/Blogs/ToggleCommentLike
        [HttpPost]
        public async Task<IActionResult> ToggleCommentLike(int commentId, bool isLike)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này." });
            }

            var userId = int.Parse(CurrentUserId()!);
            var comment = await _context.BlogComments
                .Include(c => c.BlogCommentLikes)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.Isdeleted != true);

            if (comment == null)
            {
                return Json(new { success = false, message = "Bình luận không tồn tại hoặc đã bị xóa." });
            }

            var existingLike = comment.BlogCommentLikes.FirstOrDefault(l => l.Userid == userId);

            if (existingLike != null)
            {
                if (existingLike.Islike == isLike)
                {
                    // Toggle off if same reaction
                    _context.BlogCommentLikes.Remove(existingLike);
                }
                else
                {
                    // Switch reaction (e.g. from like to dislike)
                    existingLike.Islike = isLike;
                    existingLike.Createdat = DateTime.Now;
                    _context.BlogCommentLikes.Update(existingLike);
                }
            }
            else
            {
                // Create new reaction
                var newLike = new BlogCommentLike
                {
                    Commentid = commentId,
                    Userid = userId,
                    Islike = isLike,
                    Createdat = DateTime.Now
                };
                _context.BlogCommentLikes.Add(newLike);
            }

            await _context.SaveChangesAsync();

            // Refresh counts
            var updatedComment = await _context.BlogComments
                .Include(c => c.BlogCommentLikes)
                .FirstOrDefaultAsync(c => c.Id == commentId);

            var likeCount = updatedComment!.BlogCommentLikes.Count(l => l.Islike == true);
            var dislikeCount = updatedComment!.BlogCommentLikes.Count(l => l.Islike == false);
            var currentUserReaction = updatedComment!.BlogCommentLikes.FirstOrDefault(l => l.Userid == userId)?.Islike;

            return Json(new
            {
                success = true,
                likeCount = likeCount,
                dislikeCount = dislikeCount,
                currentUserReaction = currentUserReaction // true, false, or null
            });
        }

        // POST: Blogs/Blogs/AddReply
        [HttpPost]
        public async Task<IActionResult> AddReply(int commentId, string content, int? parentReplyId = null)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để phản hồi." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung phản hồi không được để trống." });
            }

            try
            {
                var reply = new BlogCommentReply
                {
                    Commentid = commentId,
                    Userid = int.Parse(CurrentUserId()!),
                    Content = content,
                    Parentreplyid = parentReplyId,
                    Createdat = DateTime.Now,
                    Isdeleted = false
                };

                if (parentReplyId.HasValue)
                {
                    var parentReply = await _context.BlogCommentReplies.FindAsync(parentReplyId.Value);
                    if (parentReply != null)
                    {
                        reply.Replytouserid = parentReply.Userid;
                    }
                }

                _context.BlogCommentReplies.Add(reply);
                await _context.SaveChangesAsync();

                // Nạp thông tin User và Replytouser
                await _context.Entry(reply).Reference(r => r.User).LoadAsync();
                if (reply.Replytouserid.HasValue)
                {
                    await _context.Entry(reply).Reference(r => r.Replytouser).LoadAsync();
                }

                return Json(new
                {
                    success = true,
                    reply = new
                    {
                        id = reply.Replyid,
                        username = reply.User?.Username ?? "User",
                        avatar = COMICZONE.Extensions.StringExtensions.AvatarOrDefault(reply.User?.Avatar),
                        content = reply.Content,
                        createdAt = reply.Createdat?.ToString("dd/MM/yyyy HH:mm") ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                        replyToUsername = reply.Replytouser?.Username
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lưu phản hồi: " + ex.Message });
            }
        }

        // POST: Blogs/Blogs/ToggleReplyLike
        [HttpPost]
        public async Task<IActionResult> ToggleReplyLike(int replyId, bool isLike)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này." });
            }

            var userId = int.Parse(CurrentUserId()!);
            var reply = await _context.BlogCommentReplies
                .Include(r => r.BlogCommentReplyLikes)
                .FirstOrDefaultAsync(r => r.Replyid == replyId && r.Isdeleted != true);

            if (reply == null)
            {
                return Json(new { success = false, message = "Phản hồi không tồn tại." });
            }

            var existingLike = reply.BlogCommentReplyLikes.FirstOrDefault(l => l.Userid == userId);

            if (existingLike != null)
            {
                if (existingLike.Islike == isLike)
                {
                    _context.BlogCommentReplyLikes.Remove(existingLike);
                }
                else
                {
                    existingLike.Islike = isLike;
                    existingLike.Createdat = DateTime.Now;
                    _context.BlogCommentReplyLikes.Update(existingLike);
                }
            }
            else
            {
                var newLike = new BlogCommentReplyLike
                {
                    Replyid = replyId,
                    Userid = userId,
                    Islike = isLike,
                    Createdat = DateTime.Now
                };
                _context.BlogCommentReplyLikes.Add(newLike);
            }

            await _context.SaveChangesAsync();

            var updatedReply = await _context.BlogCommentReplies
                .Include(r => r.BlogCommentReplyLikes)
                .FirstOrDefaultAsync(r => r.Replyid == replyId);

            var likeCount = updatedReply!.BlogCommentReplyLikes.Count(l => l.Islike == true);
            var currentUserReaction = updatedReply!.BlogCommentReplyLikes.FirstOrDefault(l => l.Userid == userId)?.Islike;

            return Json(new
            {
                success = true,
                likeCount = likeCount,
                currentUserReaction = currentUserReaction
            });
        }
        // POST: Blogs/Blogs/UpdateComment
        [HttpPost]
        public async Task<IActionResult> UpdateComment(int commentId, string content)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung bình luận không được để trống." });
            }

            var userId = int.Parse(CurrentUserId()!);
            var comment = await _context.BlogComments.FirstOrDefaultAsync(c => c.Id == commentId && !c.Isdeleted.Value);

            if (comment == null)
            {
                return Json(new { success = false, message = "Bình luận không tồn tại." });
            }

            if (comment.Userid != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa bình luận này." });
            }

            try
            {
                comment.Content = content;
                comment.Updatedat = DateTime.Now;
                _context.BlogComments.Update(comment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật bình luận thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        // POST: Blogs/Blogs/DeleteComment
        [HttpPost]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này." });
            }

            var userId = int.Parse(CurrentUserId()!);
            var comment = await _context.BlogComments
                .Include(c => c.BlogCommentReplies)
                .FirstOrDefaultAsync(c => c.Id == commentId && !c.Isdeleted.Value);

            if (comment == null)
            {
                return Json(new { success = false, message = "Bình luận không tồn tại." });
            }

            if (comment.Userid != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa bình luận này." });
            }

            try
            {
                comment.Isdeleted = true;
                comment.Updatedat = DateTime.Now;
                
                // Cũng đánh dấu các phản hồi là đã xóa nếu cần (tùy logic, ở đây ta chỉ ẩn bình luận cha)
                foreach(var r in comment.BlogCommentReplies) r.Isdeleted = true;

                _context.BlogComments.Update(comment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa bình luận thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }

        // POST: Blogs/Blogs/UpdateReply
        [HttpPost]
        public async Task<IActionResult> UpdateReply(int replyId, string content)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này." });
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                return Json(new { success = false, message = "Nội dung phản hồi không được để trống." });
            }

            var userId = int.Parse(CurrentUserId()!);
            var reply = await _context.BlogCommentReplies.FirstOrDefaultAsync(r => r.Replyid == replyId && !r.Isdeleted.Value);

            if (reply == null)
            {
                return Json(new { success = false, message = "Phản hồi không tồn tại." });
            }

            if (reply.Userid != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền chỉnh sửa phản hồi này." });
            }

            try
            {
                reply.Content = content;
                reply.Updatedat = DateTime.Now;
                _context.BlogCommentReplies.Update(reply);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật phản hồi thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật: " + ex.Message });
            }
        }

        // POST: Blogs/Blogs/DeleteReply
        [HttpPost]
        public async Task<IActionResult> DeleteReply(int replyId)
        {
            if (!IsLoggedIn())
            {
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này." });
            }

            var userId = int.Parse(CurrentUserId()!);
            var reply = await _context.BlogCommentReplies.FirstOrDefaultAsync(r => r.Replyid == replyId && !r.Isdeleted.Value);

            if (reply == null)
            {
                return Json(new { success = false, message = "Phản hồi không tồn tại." });
            }

            if (reply.Userid != userId)
            {
                return Json(new { success = false, message = "Bạn không có quyền xóa phản hồi này." });
            }

            try
            {
                reply.Isdeleted = true;
                reply.Updatedat = DateTime.Now;
                _context.BlogCommentReplies.Update(reply);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Xóa phản hồi thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa: " + ex.Message });
            }
        }
    }
}
