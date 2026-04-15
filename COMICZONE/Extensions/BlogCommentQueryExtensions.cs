using COMICZONE.Models;
using COMICZONE.Areas.Admin.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace COMICZONE.Extensions
{
    public static class BlogCommentQueryExtensions
    {
        public static IQueryable<BlogComment> ApplyBlogCommentFilters(this IQueryable<BlogComment> query, BlogCommentSearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (CommentId, Username, Blog Title, Content)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(c => 
                    c.Id.ToString() == keyword ||
                    c.User.Username.ToLower().Contains(keyword) ||
                    c.Blog.Title.ToLower().Contains(keyword) ||
                    c.Content.ToLower().Contains(keyword)
                );
            }

            // 2. Exact Field Matches
            if (search.CommentId.HasValue)
                query = query.Where(c => c.Id == search.CommentId.Value);

            if (search.BlogId.HasValue)
                query = query.Where(c => c.Blogid == search.BlogId.Value);

            if (search.UserId.HasValue)
                query = query.Where(c => c.Userid == search.UserId.Value);

            // 3. User Context
            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(c => c.User.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.UserEmail))
                query = query.Where(c => c.User.Email != null && c.User.Email.Contains(search.UserEmail));

            // 4. Blog Context
            if (!string.IsNullOrWhiteSpace(search.BlogTitle))
                query = query.Where(c => c.Blog.Title.Contains(search.BlogTitle));

            // 5. Content Keyword
            if (!string.IsNullOrWhiteSpace(search.ContentKeyword))
                query = query.Where(c => c.Content.Contains(search.ContentKeyword));

            // 6. Timelines
            if (search.CreatedFrom.HasValue)
                query = query.Where(c => c.Createdat >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
                query = query.Where(c => c.Createdat <= search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1));

            if (search.UpdatedFrom.HasValue)
                query = query.Where(c => c.Updatedat >= search.UpdatedFrom.Value);

            if (search.UpdatedTo.HasValue)
                query = query.Where(c => c.Updatedat <= search.UpdatedTo.Value.Date.AddDays(1).AddTicks(-1));

            // 7. Flags
            if (search.IsDeleted.HasValue)
                query = query.Where(c => c.Isdeleted == search.IsDeleted.Value);

            if (search.HasReplies == true)
                query = query.Where(c => c.BlogCommentReplies.Any());
            else if (search.HasReplies == false)
                query = query.Where(c => !c.BlogCommentReplies.Any());

            if (search.NoReplyOnly == true)
                query = query.Where(c => !c.BlogCommentReplies.Any());

            // 8. Counts
            if (search.ReplyCountMin.HasValue)
                query = query.Where(c => c.BlogCommentReplies.Count() >= search.ReplyCountMin.Value);

            if (search.ReplyCountMax.HasValue)
                query = query.Where(c => c.BlogCommentReplies.Count() <= search.ReplyCountMax.Value);

            if (search.LikeCountMin.HasValue)
                query = query.Where(c => c.BlogCommentLikes.Count() >= search.LikeCountMin.Value);

            if (search.LikeCountMax.HasValue)
                query = query.Where(c => c.BlogCommentLikes.Count() <= search.LikeCountMax.Value);

            return query;
        }

        public static IQueryable<BlogCommentReply> ApplyBlogCommentReplyFilters(this IQueryable<BlogCommentReply> query, BlogCommentReplySearchModel search)
        {
            if (search == null) return query;

            // 1. Keyword search (ReplyId, CommentId, Username, Content)
            if (!string.IsNullOrWhiteSpace(search.Keyword))
            {
                var keyword = search.Keyword.ToLower();
                query = query.Where(r => 
                    r.Replyid.ToString() == keyword ||
                    r.Commentid.ToString() == keyword ||
                    r.User.Username.ToLower().Contains(keyword) ||
                    r.Content.ToLower().Contains(keyword) ||
                    (r.Comment.Blog.Title != null && r.Comment.Blog.Title.ToLower().Contains(keyword))
                );
            }

            // 2. IDs
            if (search.ReplyId.HasValue)
                query = query.Where(r => r.Replyid == search.ReplyId.Value);

            if (search.CommentId.HasValue)
                query = query.Where(r => r.Commentid == search.CommentId.Value);

            if (search.BlogId.HasValue)
                query = query.Where(r => r.Comment.Blogid == search.BlogId.Value);

            if (search.UserId.HasValue)
                query = query.Where(r => r.Userid == search.UserId.Value);

            if (search.ParentReplyId.HasValue)
                query = query.Where(r => r.Parentreplyid == search.ParentReplyId.Value);

            // 3. User Context
            if (!string.IsNullOrWhiteSpace(search.Username))
                query = query.Where(r => r.User.Username.Contains(search.Username));

            if (!string.IsNullOrWhiteSpace(search.UserEmail))
                query = query.Where(r => r.User.Email != null && r.User.Email.Contains(search.UserEmail));

            if (!string.IsNullOrWhiteSpace(search.BlogTitle))
                query = query.Where(r => r.Comment.Blog.Title.Contains(search.BlogTitle));

            // 4. Target User
            if (search.ReplyToUserId.HasValue)
                query = query.Where(r => r.Replytouserid == search.ReplyToUserId.Value);

            if (!string.IsNullOrWhiteSpace(search.ReplyToUsername))
                query = query.Where(r => r.Replytouser != null && r.Replytouser.Username.Contains(search.ReplyToUsername));

            // 5. Hierarchy flags
            if (search.TopLevelOnly == true)
                query = query.Where(r => r.Parentreplyid == null);

            if (search.ChildReplyOnly == true)
                query = query.Where(r => r.Parentreplyid != null);

            if (search.NoChildReplyOnly == true)
                query = query.Where(r => !r.InverseParentreply.Any());

            // 6. Timelines
            if (search.CreatedFrom.HasValue)
                query = query.Where(r => r.Createdat >= search.CreatedFrom.Value);

            if (search.CreatedTo.HasValue)
                query = query.Where(r => r.Createdat <= search.CreatedTo.Value.Date.AddDays(1).AddTicks(-1));

            if (search.UpdatedFrom.HasValue)
                query = query.Where(r => r.Updatedat >= search.UpdatedFrom.Value);

            if (search.UpdatedTo.HasValue)
                query = query.Where(r => r.Updatedat <= search.UpdatedTo.Value.Date.AddDays(1).AddTicks(-1));

            // 7. Status
            if (search.IsDeleted.HasValue)
                query = query.Where(r => r.Isdeleted == search.IsDeleted.Value);

            // 8. Counts
            if (search.LikeCountMin.HasValue)
                query = query.Where(r => r.BlogCommentReplyLikes.Count() >= search.LikeCountMin.Value);

            if (search.LikeCountMax.HasValue)
                query = query.Where(r => r.BlogCommentReplyLikes.Count() <= search.LikeCountMax.Value);

            return query;
        }
    }
}
